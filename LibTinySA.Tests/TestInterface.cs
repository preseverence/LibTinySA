using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace LibTinySA.Tests
{
  class TestInterface: IComSender
  {
    private readonly Queue<ExpectedCommand> expectedSends = new Queue<ExpectedCommand>();

    public void PushExpectation(ExpectedCommand command)
    {
      expectedSends.Enqueue(command);
    }

    public void PushExpectation(string command, string response)
    {
      PushExpectation(new ExpectedCommand(command, response));
    }

    public void Receive(byte[] response)
    {
      Receiver.OnDataReceived(response, response.Length);
    }

    public void Receive(string response)
    {
      Receive(Encoding.ASCII.GetBytes(response));
    }

    public void SendData(byte[] buffer, int length)
    {
      Assert.IsTrue(expectedSends.Count > 0, "No more commands expected");

      string command = Encoding.ASCII.GetString(buffer, 0, length);

      ExpectedCommand ex = expectedSends.Dequeue();

      Assert.AreEqual(ex.Command, command, "Command expectation");

      Task.Run(() => Receiver.OnDataReceived(ex.Response, ex.Response.Length));
    }

    public IComReceiver Receiver { get; set; }

    public void Open() { }

    public void Close() { }

    public void AssertNoCommands()
    {
      Assert.IsTrue(expectedSends.Count == 0, "No more commands expected");
    }
  }

  class ExpectedCommand
  {
    public string Command { get; }
    public byte[] Response { get; }

    public ExpectedCommand(string command, string response)
    {
      Command = command;
      Response = Encoding.ASCII.GetBytes(response);
    }

    public ExpectedCommand(string command, byte[] response)
    {
      Command = command;
      Response = response;
    }
  }
}
