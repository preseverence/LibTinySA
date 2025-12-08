namespace LibTinySA
{
  public struct Marker
  {
    public readonly ulong Frequency;
    public readonly double Power;

    internal Marker(ulong frequency, double power)
    {
      Frequency = frequency;
      Power = power;
    }

    public override string ToString()
    {
      return $"{Frequency/1000} kHz, {Power} dBm";
    }
  }
}