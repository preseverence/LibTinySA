using System;

namespace LibTinySA
{
  internal class TinySAException: Exception
  {
    public TinySAException(string message)
      : base(message)
    {
    }

    public TinySAException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
