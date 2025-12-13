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
      i.PushExpectation("refresh off\r\n", "refresh off\r\nch> ");
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

      // test commands
      i.PushExpectation("load 0\r\n", "load 0\r\nch> ");
      i.PushExpectation("save 2\r\n", "save 2\r\nch> ");
      i.PushExpectation("refresh on\r\n", "refresh on\r\nch> ");
      i.PushExpectation("refresh off\r\n", "refresh off\r\nch> ");
      i.PushExpectation("lna on\r\n", "lna on\r\nch> ");
      i.PushExpectation("lna off\r\n", "lna off\r\nch> ");
      i.PushExpectation("rbw auto\r\n", "rbw auto\r\nch> ");
      i.PushExpectation("rbw\r\n", "rbw\r\nusage: rbw 0.3..800|auto\r\n900kHz\r\nch> ");
      i.PushExpectation("rbw 700\r\n", "rbw 700\r\nch> ");
      i.PushExpectation("rbw\r\n", "rbw\r\nusage: rbw 0.3..800|auto\r\n700kHz\r\nch> ");
      i.PushExpectation("spur on\r\n", "spur on\r\nch> ");
      i.PushExpectation("spur off\r\n", "spur off\r\nch> ");
      i.PushExpectation("resume\r\n", "resume\r\nch> ");
      i.PushExpectation("pause\r\n", "pause\r\nch> ");
      i.PushExpectation("zero 1\r\n", "zero 1\r\nch> ");
      i.PushExpectation("sweep 900000000 1500000000 290\r\n", "sweep 900000000 1500000000 290\r\nch> ");
      i.PushExpectation("sweep\r\n", "sweep\r\n900000000 1400000000 290\r\nch> ");
      i.PushExpectation("marker\r\n", "marker\r\n0 0 819000000 -100\r\n1 0 829000000 -110\r\n2 0 835000000 -150\r\nch> ");

      await control.LoadSlot(0);
      await control.SaveSlot(2);
      await control.SetAutoRefresh(true);
      await control.SetAutoRefresh(false);
      await control.SetLNA(true);
      Assert.AreEqual(true, control.LNA);
      await control.SetLNA(false);
      Assert.AreEqual(false, control.LNA);
      await control.SetRBW();
      Assert.AreEqual(900, control.RBW);
      await control.SetRBW(700);
      Assert.AreEqual(700, control.RBW);
      await control.SetSpur(true);
      Assert.AreEqual(true, control.Spur);
      await control.SetSpur(false);
      Assert.AreEqual(false, control.Spur);
      await control.SetActive(true);
      Assert.AreEqual(TinySAStatus.Resumed, control.Status);
      await control.SetActive(false);
      Assert.AreEqual(TinySAStatus.Paused, control.Status);
      await control.SetZeroReference(1);
      Assert.AreEqual(1, control.ZeroReference);
      await control.SetSweep(900000000, 1500000000, 290);
      Assert.AreEqual(900000000, control.SweepStart);
      Assert.AreEqual(1400000000, control.SweepStop);
      Assert.AreEqual(290, control.SweepPoints);

      Marker[] markers = await control.UpdateMarkers();
      Assert.AreEqual(3, markers.Length);
      Assert.AreEqual(-100, markers[0].Power);
      Assert.AreEqual(819000000, markers[0].Frequency);
      Assert.AreEqual(-110, markers[1].Power);
      Assert.AreEqual(829000000, markers[1].Frequency);
      Assert.AreEqual(-150, markers[2].Power);
      Assert.AreEqual(835000000, markers[2].Frequency);

      i.AssertNoCommands();
    }

    [Test]
    public async Task ReversedRBWTest()
    {
      TestInterface i = new TestInterface();
      i.PushExpectation("refresh off\r\n", "refresh off\r\nch> ");
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
