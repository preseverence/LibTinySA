using System;
using System.Text;

namespace LibTinySA
{
  internal struct BufferBlock
  {
    private int index;

    public readonly byte[] Buffer;
    public readonly int EndIndex;

    public BufferBlock(byte[] buffer, int index, int endIndex)
    {
      this.index = index;
      Buffer = buffer;
      EndIndex = endIndex;
    }

    public int Index
    {
      get { return index; }
    }

    public int Length
    {
      get { return EndIndex - index; }
    }

    public string GetToken(Encoding encoding)
    {
      for (int i = index; i < EndIndex - 1; i++)
        if (Buffer[i] == 13 && Buffer[i + 1] == 10)
        {
          index = i + 2;
          return encoding.GetString(Buffer, 0, i);
        }

      return null;
    }

    public string GetAsString(Encoding encoding, int postfixLength = 0)
    {
      return encoding.GetString(Buffer, index, EndIndex - index - postfixLength);
    }

    public ushort ReadUShort()
    {
      ushort result = BitConverter.ToUInt16(Buffer, index);
      index += sizeof(ushort);
      return result;
    }
  }
}
