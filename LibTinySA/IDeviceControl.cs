using System;
using System.Threading.Tasks;

namespace LibTinySA
{
  public interface IDeviceControl: IComReceiver
  {
    /// <summary>
    /// Opens the device connection and performs the startup tasks.
    /// </summary>
    /// <returns>Task which finishes when the operation is completed.</returns>
    /// <seealso cref="IComSender.Open"/>
    Task Open();

    /// <summary>
    /// Closes the device connection.
    /// </summary>
    /// <seealso cref="IComSender.Close"/>
    void Close();

    /// <summary>
    /// Gets the TinySA device version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the TinySA device info.
    /// </summary>
    string Info { get; }

    /// <summary>
    /// Gets the LNA value.
    /// </summary>
    /// <remarks>The API does not allow to get it, so the value will be revealed upon setting.</remarks>
    /// <seealso cref="SetLNA"/>
    bool? LNA { get; }

    /// <summary>
    /// Sets the LNA value. 
    /// </summary>
    /// <param name="value">Value to set: enabled or disabled.</param>
    /// <returns>Task which finishes when the value is set.</returns>
    /// <remarks>The change will be applied on next scan finish.</remarks>
    /// <seealso cref="TinySAControl.LNA"/>
    Task SetLNA(bool value);

    /// <summary>
    /// Gets the RBW value, kHz.
    /// </summary>
    /// <seealso cref="SetRBW"/>
    float RBW { get; }

    /// <summary>
    /// Gets the minimum RBW value, kHz.
    /// </summary>
    /// <seealso cref="SetRBW"/>
    float MinRBW { get; }

    /// <summary>
    /// Gets the maximum RBW value, kHz.
    /// </summary>
    /// <seealso cref="SetRBW"/>
    float MaxRBW { get; }

    /// <summary>
    /// Sets the RBW value, kHz.
    /// </summary>
    /// <param name="value">Value to set. See <see cref="TinySAControl.MinRBW"/> and <see cref="TinySAControl.MaxRBW"/> for range. Use negative (&lt;0) values for auto.</param>
    /// <returns>Task which finishes whtn the value is set.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is out of range.</exception>
    /// <seealso cref="TinySAControl.RBW"/>
    Task SetRBW(float value = -1);

    /// <summary>
    /// Gets the SPUR value.
    /// </summary>
    /// <remarks>The API does not allow to get it, so the value will be revealed upon setting.</remarks>
    /// <seealso cref="SetSpur"/>
    bool? Spur { get; }

    /// <summary>
    /// Sets the SPUR value.
    /// </summary>
    /// <param name="value">Value to set: <c>true</c> to enable and <c>false</c> to disable.</param>
    /// <returns>Task which finishes when the value is set.</returns>
    /// <seealso cref="TinySAControl.Spur"/>
    Task SetSpur(bool value);

    /// <summary>
    /// Gets the zero reference level, dBm.
    /// </summary>
    /// <seealso cref="SetZeroReference"/>
    int ZeroReference { get; }

    /// <summary>
    /// Sets the zero reference level, dBm.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns>Task which finishes when the value is set.</returns>
    /// <seealso cref="TinySAControl.ZeroReference"/>
    Task SetZeroReference(int value);

    /// <summary>
    /// Get the battery voltage, V.
    /// </summary>
    /// <seealso cref="UpdateBatteryVoltage"/>
    float BatteryVoltage { get; }

    /// <summary>
    /// Updates the battery voltage from the device.
    /// </summary>
    /// <returns>Task which completes when the data is received. The data is battery voltage, V.</returns>
    /// <seealso cref="TinySAControl.BatteryVoltage"/>
    Task<float> UpdateBatteryVoltage();

    /// <summary>
    /// Gets the TinySA autosweep status.
    /// </summary>
    /// <seealso cref="SetStatus"/>
    TinySAStatus Status { get; }

    /// <summary>
    /// Gets the sweep start, Hz.
    /// </summary>
    /// <seealso cref="SetSweep"/>
    ulong SweepStart { get; }

    /// <summary>
    /// Gets the sweep end, Hz.
    /// </summary>
    /// <seealso cref="SetSweep"/>
    ulong SweepStop { get; }

    /// <summary>
    /// Gets the sweep points number.
    /// </summary>
    /// <seealso cref="SetSweep"/>
    ushort SweepPoints { get; }

    /// <summary>
    /// Sets the sweep values for automatic scanning.
    /// </summary>
    /// <param name="start">Start frequency, Hz.</param>
    /// <param name="stop">Stop frequency, Hz.</param>
    /// <param name="points">Points number.</param>
    /// <returns>Task which finishes when the values is set.</returns>
    /// <remarks>Consider rereading properties after call, because the device may alter the input.</remarks>
    /// <seealso cref="SweepStart"/>
    /// <seealso cref="SweepStop"/>
    /// <seealso cref="SweepPoints"/>
    Task SetSweep(ulong start, ulong stop, ushort points);

    /// <summary>
    /// Raised then the device screenshot has arrived. The params are: pixel data (2 byte per pixel), image width (pixels), image height (pixels).
    /// </summary>
    event Action<ushort[,], int, int> ImageAcquired;

    /// <summary>
    /// Gets the current markers values.
    /// </summary>
    /// <returns>Task which finishes when the data arrives.</returns>
    Task<Marker[]> UpdateMarkers();

    /// <summary>
    /// Loads the device settings from the numbered slot.
    /// </summary>
    /// <param name="number">Number of slot to load, 0 is the startup slot.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid slot number.</exception>
    Task LoadSlot(int number);

    /// <summary>
    /// Saves the device settings to the numbered slot.
    /// </summary>
    /// <param name="number">Number of slot to save, 0 is the startup slot.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown on invalid slot number.</exception>
    Task SaveSlot(int number);

    /// <summary>
    /// Sets the autorefresh status of the device.
    /// </summary>
    /// <param name="value"><c>true</c> to enable and <c>false</c> to disable.</param>
    /// <seealso cref="TinySAControl.Status"/>
    /// <returns>Task which completes when the operation completes.</returns>
    Task SetStatus(bool value);

    /// <summary>
    /// Raised when scanning progress update has been received. The param is normalized (0..1) progress value.
    /// </summary>
    event Action<float> ScanningProgress;

    /// <summary>
    /// Performs one-time scanning with given parameters.
    /// </summary>
    /// <param name="start">Start frequency, Hz.</param>
    /// <param name="stop">Stop frequency, Hz.</param>
    /// <param name="points">Points count.</param>
    /// <returns>Task which completes when the operation completes.</returns>
    /// <remarks>Using this pauses the automatic scanning if any. <see cref="TinySAControl.ScanningProgress"/> notifications will not appear.</remarks>
    Task<ScanPoint[]> Scan(ulong start, ulong stop, ushort points);
  }
}
