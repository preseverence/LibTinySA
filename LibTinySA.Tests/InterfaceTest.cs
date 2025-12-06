using System.Threading.Tasks;

using NUnit.Framework;

namespace LibTinySA.Tests
{
  [TestFixture]
  class InterfaceTest
  {
    [Test]
    public async Task FineTest()
    {
      TestInterface i = new TestInterface();
      i.PushExpectation("version\r\n", "version\r\ntest version\r\nch> ");
      i.PushExpectation("info\r\n", "info\r\ntest info\r\nch> ");
      i.PushExpectation("status\r\n", "status\r\nResumed\r\nch> ");
      i.PushExpectation("rbw\r\n", "rbw\r\nusage: rbw 0.3..800|auto\r\n600kHz\r\nch> ");
      i.PushExpectation("zero\r\n", "zero\r\nusage: zero {level}\r\n25dBm\r\nch> ");
      i.PushExpectation("vbat\r\n", "vbat\r\n4251 mV\r\nch> ");
      i.PushExpectation("sweep\r\n", "sweep\r\n800000000 1200000000 450\r\nch> ");
      
      TinySAControl control = new TinySAControl(i);
      await control.Open();

      Assert.AreEqual("test version\r\n", control.Version);
      Assert.AreEqual("test info\r\n", control.Info);
      Assert.AreEqual(TinySAStatus.Resumed, control.Status);
      Assert.AreEqual(0.3f, control.MinRBW);
      Assert.AreEqual(800, control.MaxRBW);
      Assert.AreEqual(600, control.RBW);
      Assert.AreEqual(25, control.ZeroReference);
      Assert.AreEqual(4.251f, control.BatteryVoltage);
      Assert.AreEqual(800000000, control.SweepStart);
      Assert.AreEqual(1200000000, control.SweepStop);
      Assert.AreEqual(450, control.SweepPoints);
      i.AssertNoCommands();
    }

    [Test]
    public async Task ReversedRBWTest()
    {
      TestInterface i = new TestInterface();
      i.PushExpectation("version\r\n", "version\r\ntest version\r\nch> ");
      i.PushExpectation("info\r\n", "info\r\ntest info\r\nch> ");
      i.PushExpectation("status\r\n", "status\r\nResumed\r\nch> ");
      i.PushExpectation("rbw\r\n", "rbw\r\nusage: rbw auto|0.3..800\r\n600kHz\r\nch> ");
      i.PushExpectation("zero\r\n", "zero\r\nusage: zero {level}\r\n25dBm\r\nch> ");
      i.PushExpectation("vbat\r\n", "vbat\r\n4251 mV\r\nch> ");
      i.PushExpectation("sweep\r\n", "sweep\r\n800000000 1200000000 450\r\nch> ");

      TinySAControl control = new TinySAControl(i);
      await control.Open();

      Assert.AreEqual("test version\r\n", control.Version);
      Assert.AreEqual("test info\r\n", control.Info);
      Assert.AreEqual(TinySAStatus.Resumed, control.Status);
      Assert.AreEqual(0.3f, control.MinRBW);
      Assert.AreEqual(800, control.MaxRBW);
      Assert.AreEqual(600, control.RBW);
      Assert.AreEqual(25, control.ZeroReference);
      Assert.AreEqual(4.251f, control.BatteryVoltage);
      Assert.AreEqual(4.251f, control.BatteryVoltage);
      Assert.AreEqual(800000000, control.SweepStart);
      Assert.AreEqual(1200000000, control.SweepStop);
      Assert.AreEqual(450, control.SweepPoints);
      i.AssertNoCommands();
    }
  }
}
