namespace LibTinySA
{
  public interface IComSender
  {
    /// <summary>
    /// Sends the data to the COM port.
    /// </summary>
    /// <param name="buffer">Buffer of binary data to send.</param>
    /// <param name="length">Amount of valid data in <paramref name="buffer"/>, bytes.</param>
    void SendData(byte[] buffer, int length);

    /// <summary>
    /// Gets or sets the receiver for COM port.
    /// </summary>
    IComReceiver Receiver { get; set; }

    /// <summary>
    /// Opens the COM connection.
    /// </summary>
    void Open();
  }
}
