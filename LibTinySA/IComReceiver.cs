namespace LibTinySA
{
  public interface IComReceiver
  {
    /// <summary>
    /// Processes the data received from the COM port.
    /// </summary>
    /// <param name="buffer">Buffer with data received.</param>
    /// <param name="length">Amount of valid data in <paramref name="buffer"/>, bytes.</param>
    void OnDataReceived(byte[] buffer, int length);
  }
}
