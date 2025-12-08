namespace LibTinySA
{
  public struct ScanPoint
  {
    internal ScanPoint(ulong frequency, double power)
    {
      Frequency = frequency;
      Power = power;
    }

    public ulong Frequency { get; }
    public double Power { get; }

    public override string ToString()
    {
      return $"{Frequency / 1000} kHz, {Power} dBm";
    }
  }
}
