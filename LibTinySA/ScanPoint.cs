namespace LibTinySA
{
  public struct ScanPoint
  {
    public readonly ulong Frequency;
    public readonly double Power;

    public ScanPoint(ulong frequency, double power)
    {
      Frequency = frequency;
      Power = power;
    }

    public override string ToString()
    {
      return $"{Frequency / 1000} kHz, {Power} dBm";
    }
  }
}
