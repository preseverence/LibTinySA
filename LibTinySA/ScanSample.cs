namespace LibTinySA
{
  public struct ScanSample
  {
    /// <summary>
    /// Keeps the sample frequency, Hz.
    /// </summary>
    public readonly ulong Frequency;
    /// <summary>
    /// Keeps the sample power, dBm
    /// </summary>
    public readonly double Power;

    public ScanSample(ulong frequency, double power)
    {
      Frequency = frequency;
      Power = power;
    }

    public override string ToString()
    {
      return $"{Frequency:N0} Hz, {Power} dBm";
    }
  }
}
