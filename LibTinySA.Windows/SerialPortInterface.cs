using System;
using System.IO.Ports;

namespace LibTinySA.Windows
{
  /// <summary>
  /// TinySA COM interface based on <see cref="SerialPort"/>
  /// </summary>
  public class SerialPortInterface: IComSender
  {
    private const int BUFFER_SIZE = 16384;
    private SerialPort serialPort;
    private readonly byte[] receiveBuffer = new byte[BUFFER_SIZE];
    private readonly object receiverLock = new object();

    public string PortName { get; set; }

    public void Open()
    {
      if (string.IsNullOrWhiteSpace(PortName))
        throw new InvalidOperationException("PortName must be set before opening");

      serialPort = new SerialPort();
      serialPort.PortName = PortName;
      serialPort.BaudRate = 115200;
      serialPort.ReadTimeout = 500;
      serialPort.WriteTimeout = 500;
      serialPort.DataReceived += SerialPort_DataReceived;
      serialPort.ReadBufferSize = BUFFER_SIZE;

      serialPort.Open();

      if (serialPort.BytesToRead > 0)
        Receive();
    }

    public void Close()
    {
      if (serialPort == null)
        return;

      serialPort.Close();
      serialPort = null;
    }

    public bool IsOpen
    {
      get { return serialPort != null; }
    }

    private void Receive()
    {
      lock (receiverLock)
      {
        int bytesToRead = serialPort.BytesToRead;
        if (bytesToRead == 0)
          return;

        bytesToRead = serialPort.Read(receiveBuffer, 0, bytesToRead);

        //Console.WriteLine(">>" + Encoding.ASCII.GetString(receiveBuffer, 0, bytesToRead));

        Receiver?.OnDataReceived(receiveBuffer, bytesToRead);
      }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      Receive();
    }

    public void SendData(byte[] buffer, int length)
    {
      serialPort.Write(buffer, 0, length);
    }

    public IComReceiver Receiver { get; set; }
  }
}
