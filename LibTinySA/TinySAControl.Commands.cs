using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibTinySA
{
  public partial class TinySAControl
  {
    #region LNA

    /// <summary>
    /// Sets the LNA value. 
    /// </summary>
    /// <param name="value">Value to set: enabled or disabled.</param>
    /// <returns>Task which finishes when the value is set.</returns>
    /// <remarks>The change will be applied on next scan finish.</remarks>
    /// <seealso cref="LNA"/>
    public Task SetLNA(bool value)
    {
      LNA = value;
      return SendCommand("lna", value ? "lna on\r\n" : "lna off\r\n");
    }

    /// <summary>
    /// Gets the LNA value.
    /// </summary>
    /// <remarks>The API does not allow to get it, so the value will be revealed upon setting.</remarks>
    /// <seealso cref="SetLNA"/>
    public bool? LNA { get; private set; }

    #endregion

    #region RBW

    private static readonly Regex REGEX_RBW = new Regex(
      @"usage: rbw (?<min>[\d\.]+)\.\.(?<max>[\d\.]+)\|auto\r\n(?<val>[\d\.]+)kHz|usage: rbw auto\|(?<min>[\d\.]+)\.\.(?<max>[\d\.]+)\r\n(?<val>[\d\.]+)kHz",
      RegexOptions.Multiline | RegexOptions.ECMAScript
    );

    /// <summary>
    /// Sets the RBW value, kHz.
    /// </summary>
    /// <param name="value">Value to set. See <see cref="MinRBW"/> and <see cref="MaxRBW"/> for range. Use negative (&lt;0) values for auto.</param>
    /// <returns>Task which finishes whtn the value is set.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is out of range.</exception>
    /// <seealso cref="RBW"/>
    public Task SetRBW(float value = -1)
    {
      if (value > 0 && (value < MinRBW || value > MaxRBW))
        throw new ArgumentOutOfRangeException(nameof(value),
          "Invalid RBW value. Must be either within (MinRBW; MaxRBW) or <0 for auto");

      return SendCommand("rbw",
        value < 0 ? "rbw auto\r\n" : $"rbw {value.ToString(CultureInfo.InvariantCulture)}\r\n");
    }

    private async Task UpdateRBW()
    {
      string content = await SendCommand("rbw", "rbw\r\n");
      Match match = REGEX_RBW.Match(content);

      if (!match.Success)
        return;

      MinRBW = float.Parse(match.Groups["min"].Value, CultureInfo.InvariantCulture);
      MaxRBW = float.Parse(match.Groups["max"].Value, CultureInfo.InvariantCulture);
      RBW = float.Parse(match.Groups["val"].Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the RBW value, kHz.
    /// </summary>
    /// <seealso cref="SetRBW"/>
    public float RBW { get; private set; }

    /// <summary>
    /// Gets the minimum RBW value, kHz.
    /// </summary>
    /// <seealso cref="SetRBW"/>
    public float MinRBW { get; private set; } = 3;

    /// <summary>
    /// Gets the maximum RBW value, kHz.
    /// </summary>
    /// <seealso cref="SetRBW"/>
    public float MaxRBW { get; private set; } = 600;

    #endregion

    #region SPUR

    /// <summary>
    /// Gets the SPUR value.
    /// </summary>
    /// <remarks>The API does not allow to get it, so the value will be revealed upon setting.</remarks>
    /// <seealso cref="SetSpur"/>
    public bool? Spur { get; private set; }

    /// <summary>
    /// Sets the SPUR value.
    /// </summary>
    /// <param name="value">Value to set: <c>true</c> to enable and <c>false</c> to disable.</param>
    /// <returns>Task which finishes when the value is set.</returns>
    /// <seealso cref="Spur"/>
    public Task SetSpur(bool value)
    {
      Spur = value;

      return SendCommand("spur", value ? "spur on\r\n" : "spur off\r\n");
    }

    #endregion

    #region Zero Reference

    private static readonly Regex REGEX_ZERO = new Regex(
      @"usage: zero {level}\r\n(\d+)dBm",
      RegexOptions.Multiline | RegexOptions.ECMAScript
    );

    /// <summary>
    /// Gets the zero reference level, dBm.
    /// </summary>
    /// <seealso cref="SetZeroReference"/>
    public int ZeroReference { get; private set; }

    private async Task<int> UpdateZeroReference()
    {
      string content = await SendCommand("zero", "zero\r\n");

      Match match = REGEX_ZERO.Match(content);

      if (!match.Success)
        return -1;

      ZeroReference = int.Parse(match.Groups[1].Value);
      return ZeroReference;
    }

    /// <summary>
    /// Sets the zero reference level, dBm.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>Task which finishes when the value is set.</returns>
    /// <seealso cref="ZeroReference"/>
    public Task SetZeroReference(int value)
    {
      ZeroReference = value;
      return SendCommand("zero", $"zero {value}\r\n");
    }

    #endregion

    #region Battery Voltage

    /// <summary>
    /// Get the battery voltage, V.
    /// </summary>
    /// <seealso cref="UpdateBatteryVoltage"/>
    public float BatteryVoltage { get; private set; }

    /// <summary>
    /// Updates the battery voltage from the device.
    /// </summary>
    /// <returns>Task which completes when the data is received. The data is battery voltage, V.</returns>
    /// <seealso cref="BatteryVoltage"/>
    public async Task<float> UpdateBatteryVoltage()
    {
      string content = await SendCommand("vbat", "vbat\r\n");
      int index = content.IndexOf(' ');
      if (index == -1)
        return 0;

      BatteryVoltage = int.Parse(content.Substring(0, index)) / 1000f;
      return BatteryVoltage;
    }

    #endregion

    #region Markers

    private static readonly Regex REGEX_MARKER = new Regex(
      @"\d+\s+\d+\s+(\d+)\s+(.+)",
      RegexOptions.ECMAScript
    );

    /// <summary>
    /// Gets the current markers values.
    /// </summary>
    /// <returns>Task which finishes when the data arrives.</returns>
    public async Task<Marker[]> UpdateMarkers()
    {
      string content = await SendCommand("marker", "marker\r\n");

      string[] values = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

      Marker[] result = new Marker[values.Length];
      for (int i = 0; i < values.Length; i++)
      {
        Match match = REGEX_MARKER.Match(values[i]);
        result[i] = new Marker(
          ulong.Parse(match.Groups[1].Value),
          double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
        );
      }

      return result;
    }

    #endregion

    #region Slots

    /// <summary>
    /// Loads the device settings from the numbered slot.
    /// </summary>
    /// <param name="number">Number of slot to load, 0 is the startup slot.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid slot number.</exception>
    public Task LoadSlot(int number)
    {
      if (number < 0 || number > 4)
        throw new ArgumentOutOfRangeException(nameof(number));

      return SendCommand("load", $"load {number}\r\n");
    }

    /// <summary>
    /// Saves the device settings to the numbered slot.
    /// </summary>
    /// <param name="number">Number of slot to save, 0 is the startup slot.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid slot number.</exception>
    public Task SaveSlot(int number)
    {
      if (number < 0 || number > 4)
        throw new ArgumentOutOfRangeException(nameof(number));

      return SendCommand("save", $"save {number}\r\n");
    }

    #endregion

    #region Status

    /// <summary>
    /// Gets the TinySA autosweep status.
    /// </summary>
    /// <seealso cref="SetStatus"/>
    public TinySAStatus Status { get; private set; }

    private async Task UpdateStatus()
    {
      string value = await SendCommand("status", "status\r\n");
      TinySAStatus s;
      Status = Enum.TryParse(value, true, out s) ? s : TinySAStatus.Invalid;
    }

    /// <summary>
    /// Sets the autorefresh status of the device.
    /// </summary>
    /// <param name="value"><c>true</c> to enable and <c>false</c> to disable.</param>
    /// <seealso cref="Status"/>
    /// <returns>Task which completes when the operation completes.</returns>
    public Task SetStatus(bool value)
    {
      if (value)
      {
        Status = TinySAStatus.Resumed;
        return SendCommand("resume", "resume\r\n");
      }
      else
      {
        Status = TinySAStatus.Paused;
        return SendCommand("pause", "pause\r\n");
      }
    }

    #endregion

    #region Sweep

    /// <summary>
    /// Gets the sweep start, Hz.
    /// </summary>
    /// <seealso cref="SetSweep"/>
    public ulong SweepStart { get; private set; }

    /// <summary>
    /// Gets the sweep end, Hz.
    /// </summary>
    /// <seealso cref="SetSweep"/>
    public ulong SweepStop { get; private set; }

    /// <summary>
    /// Gets the sweep points number.
    /// </summary>
    /// <seealso cref="SetSweep"/>
    public ushort SweepPoints { get; private set; }

    private async Task UpdateSweep()
    {
      string content = await SendCommand("sweep", "sweep\r\n");

      string[] values = content.Split(' ');
      SweepStart = ulong.Parse(values[0]);
      SweepStop = ulong.Parse(values[1]);
      SweepPoints = ushort.Parse(values[2]);
    }

    public async Task SetSweep(ulong start, ulong stop, ushort points)
    {
      await SendCommand($"sweep {start} {stop} {points}", $"sweep {start} {stop} {points}\r\n");

      // call this one more time to see real values set by the device
      await UpdateSweep();
    }

    #endregion

    #region Progress

    private int progressBaseX = -1;

    private void UpdateFill(ushort color, ushort x, ushort width)
    {
      if (ScanningProgress == null)
        return; // don't waste resources if noone is listening

      // color-based detection is total crap, but I see no other way
      if (color == 0)
      {
        // black rectangle, background

        if (progressBaseX == -1)
          return; // not enough data, come in next time

        // x relative to progrss bar start
        int relativeX = x - progressBaseX;
        // total width or progress bar
        int totalWidth = relativeX + width;

        // and then get normalized progress as x/width
        float progress = (float)relativeX / totalWidth;

        // do it in background to don't slow down the handling
        Task.Run(() => ScanningProgress(progress));
      }
      else
      {
        // colored rectangle, foreground
        // we may use it only to determine base x offset of the progressbar
        progressBaseX = x;
      }
    }

    public event Action<float> ScanningProgress;

    #endregion

    /// <summary>
    /// Gets the TinySA device version.
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// Gets the TinySA device info.
    /// </summary>
    public string Info { get; private set; }

    /// <summary>
    /// Enables or disables autorefresh - automatic dumps of screen during scanning.
    /// </summary>
    /// <param name="value"><c>true</c> to enable and <c>false</c> to disable.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    public Task SetAutoRefresh(bool value)
    {
      return SendCommand("refresh", value ? "refresh on\r\n" : "refresh off\r\n");
    }

    /// <summary>
    /// Performs one-time scanning with given parameters.
    /// </summary>
    /// <param name="start">Start frequency, Hz.</param>
    /// <param name="stop">Stop frequency, Hz.</param>
    /// <param name="points">Points count.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    /// <remarks>Using this pauses the automatic scanning if any. <see cref="ScanningProgress"/> notifications will not appear.</remarks>
    public async Task<ScanPoint[]> Scan(ulong start, ulong stop, ushort points)
    {
      string content = await SendCommand($"scan {start} {stop} {points} 3", $"scan {start} {stop} {points} 3\r\n");

      string[] lines = content.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

      ScanPoint[] result = new ScanPoint[lines.Length];
      for (int i = 0; i < lines.Length; i++)
      {
        string line = lines[i];

        string[] values = line.Split(' ');

        ulong freq = ulong.Parse(values[0]);
        double power = values[1].IndexOf(':') != -1 ? -10d : double.Parse(values[1], CultureInfo.InvariantCulture);

        result[i] = new ScanPoint(freq, power);
      }

      Status = TinySAStatus.Paused;
      return result;
    }
  }

  public enum TinySAStatus
  {
    Invalid = -2,
    Undefined = -1,
    Resumed,
    Paused,
  }
}
