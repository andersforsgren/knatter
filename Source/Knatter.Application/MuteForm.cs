using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Knatter.Core;

namespace Knatter.Application
{
   public partial class MuteForm : Form
   {
      private const int UnmuteTimeSliderIncrement = 50; // Ms per tick step of slider.
      private readonly GlobalKeyHook keyHook;
      private readonly CoreAudioController audioController;
      private readonly CoreAudioDevice[] captureDevices;

      public readonly Muter muter;

      public MuteForm()
      {
         InitializeComponent();
         this.keyHook = new GlobalKeyHook(k => GlobalKeyPress(k));
         this.audioController = new CoreAudioController();
         this.captureDevices = audioController.GetCaptureDevices().ToArray();
         Guid lastUsedUnmuteDeviceId = Properties.Settings.Default.DeviceId;
         CoreAudioDevice initialMuteDevice = captureDevices.FirstOrDefault(d => !lastUsedUnmuteDeviceId.Equals(Guid.Empty) && lastUsedUnmuteDeviceId.Equals(d.Id))
               ?? captureDevices.FirstOrDefault(d => d.IsDefaultCommunicationsDevice)
               ?? captureDevices.FirstOrDefault(d => !d.IsMuted)
               ?? captureDevices.FirstOrDefault();
         if (initialMuteDevice == null)
         {
            MessageBox.Show("No capture device found");
            System.Windows.Forms.Application.Exit();
            return;
         }
         int unmuteTime = Properties.Settings.Default.UnmuteTimeMs;
         Debug.WriteLine($"Loaded settings, using unmute time {unmuteTime} ms, deviceid={lastUsedUnmuteDeviceId}");

         Text = ProgramInfo.NameAndVersion;
         Icon = Resources.Unmute;
         ShowIcon = true;
         deviceCombo.Items.AddRange(captureDevices.Select(x => new ComboBoxItem(x, x.FullName)).ToArray());
         pausedCheckbox.CheckedChanged += (s, e) => muter.Pause(pausedCheckbox.Checked);
         deviceCombo.SelectedIndexChanged += (s, e) => muter.SetDevice(((ComboBoxItem)deviceCombo.SelectedItem).Value as CoreAudioDevice);
         unmuteTimeTrackbar.ValueChanged += (s, e) =>
         {
            int t = UnmuteTimeSliderIncrement * unmuteTimeTrackbar.Value;
            muter.UnmuteTimeMs = t;
            unmuteTimeLabel.Text = $"{t} ms";
         };

         this.muter = new Muter();

         if (unmuteTime > unmuteTimeTrackbar.Maximum * UnmuteTimeSliderIncrement || unmuteTime < unmuteTimeTrackbar.Minimum * UnmuteTimeSliderIncrement)
            unmuteTime = GetDefaultsettingsInt(nameof(Properties.Settings.UnmuteTimeMs));
         unmuteTimeTrackbar.Value = unmuteTime / UnmuteTimeSliderIncrement;
         deviceCombo.SelectedItem = new ComboBoxItem(initialMuteDevice, initialMuteDevice.FullName);
      }

      internal void SetIcon(Icon icon)
      {
         InvokeIfRequired(() => Icon = icon);
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         Hide();
         e.Cancel = true;
      }

      private int GetDefaultsettingsInt(string setting)
      {
         var prop = Properties.Settings.Default.GetType().GetProperty(setting, BindingFlags.Instance | BindingFlags.Public);
         if (prop == null)
            return 0;
         var defAttr = prop.GetCustomAttribute<DefaultSettingValueAttribute>();
         if (defAttr == null)
            return 0;
         return int.Parse(defAttr.Value, NumberFormatInfo.InvariantInfo);
      }

      internal void SaveSettings()
      {
         Properties.Settings.Default.UnmuteTimeMs = unmuteTimeTrackbar.Value * UnmuteTimeSliderIncrement;
         Properties.Settings.Default.DeviceId = muter.DeviceId;
         Properties.Settings.Default.Save();
      }

      private void InvokeIfRequired(Action a)
      {
         if (this.InvokeRequired)
            BeginInvoke(a);
         else
            a();
      }

      public void GlobalKeyPress(int k)
      {
         muter.MuteEvent();
      }

      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            muter.Stop();
            components?.Dispose();
            keyHook?.Dispose();
         }
         base.Dispose(disposing);
      }
   }
}
