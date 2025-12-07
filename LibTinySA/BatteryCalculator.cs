namespace LibTinySA
{
  /// <summary>
  /// Helper to transform battery voltage to approximate battery charge percent.
  /// </summary>
  /// <remarks>Powered by ChatGPT5.</remarks>
  public static class BatteryCalculator
  {
    // Lookup table: voltages (V) descending and corresponding percents (0..100)
    // You can refine points if you have a specific cell chemistry curve.
    private static readonly float[] Voltages = new float[]
    {
      4.20f, 4.10f, 3.95f, 3.80f, 3.65f, 3.45f, 3.20f, 3.00f
    };

    private static readonly float[] Percents = new float[]
    {
      1.00f, 0.92f, 0.75f, 0.55f, 0.35f, 0.15f, 0.05f, 0.00f
    };

    /// <summary>
    /// Convert single-cell Li-ion voltage (V) to estimated State Of Charge (%) using linear interpolation.
    /// </summary>
    /// <param name="voltage">Battery voltage, V</param>
    public static float VoltageToLevel(float voltage)
    {
      if (voltage >= Voltages[0])
        return 1f;
      if (voltage <= Voltages[Voltages.Length - 1])
        return 0f;

      for (int i = 0; i < Voltages.Length - 1; i++)
      {
        float vHi = Voltages[i];
        float vLo = Voltages[i + 1];

        if (!(voltage <= vHi) || !(voltage >= vLo))
          continue;

        float pHi = Percents[i];
        float pLo = Percents[i + 1];

        float t = (voltage - vLo) / (vHi - vLo);
        float level = pLo + t * (pHi - pLo);

        if (level < 0.0)
          level = 0f;
        if (level > 1.0)
          level = 1f;

        return level;
      }

      return 0f;
    }

    public static string ToVoltageString(this float voltage)
    {
      return $"{VoltageToLevel(voltage):P1} V";
    }
  }
}
