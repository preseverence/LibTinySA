using System;

namespace LibTinySA
{
  public struct ScanResult
  {
    /// <summary>
    /// Keeps the samples array.
    /// </summary>
    public readonly ScanSample[] Samples;
    /// <summary>
    /// Keeps the scaning end time.
    /// </summary>
    public readonly DateTime Time;

    public ScanResult(ScanSample[] samples, DateTime time)
    {
      Samples = samples;
      Time = time;
    }

    public ScanResult(ScanSample[] samples)
      : this(samples, DateTime.Now)
    {
    }

    public override string ToString()
    {
      return $"{Samples.Length} samples at {Time}";
    }
  }
}
