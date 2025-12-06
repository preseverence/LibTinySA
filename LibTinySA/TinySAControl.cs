using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibTinySA
{
  public partial class TinySAControl: IComReceiver
  {
    private readonly Encoding encoding = Encoding.ASCII;
    private readonly Dictionary<string, TaskCompletionSource<string>> waitCommands = new Dictionary<string, TaskCompletionSource<string>>();
    private ReceiveBuffer receiveBuffer = new ReceiveBuffer(32768);
    private SpinLock commandsLock = new SpinLock();

    private static readonly char[] DELIMITER = "ch> ".ToCharArray();

    public TinySAControl(IComSender sender)
    {
      Sender = sender;
    }

    public IComSender Sender { get; }

    public async Task Open()
    {
      Sender.Receiver = this;
      Sender.Open();

      Version = await SendCommand("version", "version\r\n");
      Info = await SendCommand("info", "info\r\n");
      await UpdateStatus();
      await UpdateRBW();
      await UpdateZeroReference();
      await UpdateBatteryVoltage();
      await UpdateSweep();
    }

    private void AddWaitCommand(string command, TaskCompletionSource<string> tcs)
    {
      bool lockTaken = false;
      try
      {
        commandsLock.Enter(ref lockTaken);
        waitCommands.Add(command, tcs);
      }
      finally
      {
        if (lockTaken)
          commandsLock.Exit(false);
      }
    }

    private bool PeekWaitCommand(string command, out TaskCompletionSource<string> tcs)
    {
      bool lockTaken = false;
      try
      {
        commandsLock.Enter(ref lockTaken);

        if (!waitCommands.TryGetValue(command, out tcs))
          return false;

        waitCommands.Remove(command);
        return true;
      }
      finally
      {
        if (lockTaken)
          commandsLock.Exit(false);
      }
    }

    private Task<string> SendCommand(string command, string message)
    {
      TaskCompletionSource<string> tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

      AddWaitCommand(command, tcs);

      byte[] bytes = encoding.GetBytes(message);
      Sender.SendData(bytes, bytes.Length);

      return tcs.Task;
    }

    public void OnDataReceived(byte[] buffer, int length)
    {
      // collect the data
      receiveBuffer.Append(buffer, length);

      // loop through the data
      while (true)
      {
        // find complete block
        BufferBlock? block = receiveBuffer.GetBufferBlock(DELIMITER);
        if (block == null) // no? then wait for more
          break;

        // handle it
        HandleResponse(block.Value);

        // and remove from buffer
        receiveBuffer.Cut(block.Value.EndIndex);
      }
    }

    private void HandleResponse(BufferBlock block)
    {
      string raw = block.GetAsString(encoding);

      string token = block.GetToken(encoding);
      if (token == null)
        return;

      TaskCompletionSource<string> tcs;
      if (PeekWaitCommand(token, out tcs))
      {
        string content = block.GetAsString(encoding, DELIMITER.Length);
        if (content == token + "?")
          tcs.SetException(new TinySAException($"Command not recognized by the device: {token}"));
        else
          tcs.SetResult(content);
      }
      else
        HandleUnexpectedResponse(token, block);
    }

    private void HandleUnexpectedResponse(string token, BufferBlock block)
    {
      if (token == "bulk")
      {
        ushort x = block.ReadUShort();
        ushort y = block.ReadUShort();
        ushort width = block.ReadUShort();
        ushort height = block.ReadUShort();
        
        UpdateImage(x, y, width, height, ref block);
        return;
      }
      if (token == "fill")
      {
        ushort x = block.ReadUShort();
        ushort y = block.ReadUShort();
        ushort width = block.ReadUShort();
        ushort height = block.ReadUShort();
        ushort color = block.ReadUShort();

        UpdateFill(color, x, width);
        FinishImage();

        return;
      }
    }

    private int imageWidth = -1;
    private int imageHeight = -1;
    private ushort[,] image;
    private bool isImageDirty;

    public event Action<ushort[,], int, int> ImageAcquired;

    private void FinishImage()
    {
      // do we collected the image dimensions?
      if (image == null && imageWidth > 0 && imageHeight > 0)
      {
        image = new ushort[imageHeight, imageWidth];
      }
      else
      {
        if (!isImageDirty)
          return;

        if (ImageAcquired != null)
          Task.Run(() => ImageAcquired(image, imageWidth, imageHeight));
        isImageDirty = false;
      }
    }

    private void UpdateImage(ushort x, ushort y, ushort width, ushort height, ref BufferBlock block)
    {
      if (image == null)
      {
        // collect image dimensions statistics
        int w = x + width;
        int h = y + height;
        if (imageWidth < w)
          imageWidth = w;
        if (imageHeight < h)
          imageHeight = h;

        return;
      }

      // read the data to the image buffer
      for (int iy = 0; iy < height; iy++)
        for (int ix = 0; ix < width; ix++)
          image[iy + y, ix + x] = block.ReadUShort();
      isImageDirty = true;
    }
  }
}
