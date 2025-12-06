using System;

namespace LibTinySA
{
  internal struct ReceiveBuffer
  {
    private int index;
    public readonly byte[] Buffer;

    public ReceiveBuffer(int bufferSize = 16384)
    {
      Buffer = new byte[bufferSize];
      index = 0;
    }

    public void Append(byte[] buffer, int length)
    {
      Array.Copy(buffer, 0, Buffer, index, length);
      index += length;
    }

    public void Reset()
    {
      index = 0;
    }

    public int Count
    {
      get { return index; }
    }

    public void Cut(int length)
    {
      Array.Copy(Buffer, length, Buffer, 0, index - length);
      index -= length;
    }

    public BufferBlock? GetBufferBlock(char[] delimiter)
    {
      int maxScan = index - delimiter.Length + 1;
      if (maxScan <= 0)
        return null;

      for (int i = 0; i < maxScan; i++)
      {
        bool isEnd = true;

        for (int j = 0; j < delimiter.Length; j++)
        {
          if (Buffer[i + j] == delimiter[j])
            continue;

          isEnd = false;
          break;

        }

        if (isEnd)
          return new BufferBlock(Buffer, 0, i + delimiter.Length);
      }

      return null;
    }
  }
}
