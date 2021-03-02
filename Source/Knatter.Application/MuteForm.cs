using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Knatter.Core;
using Knatter.Core.Audio;

namespace Knatter.Application
{
   public partial class MuteForm : Form
   {
      private const int UnmuteTimeSliderIncrement = 50; // Ms per tick step of slider.

      public readonly Muter muter;

      public MuteForm()
      {
         InitializeComponent();

         this.muter = new Muter(SynchronizationContext.Current);

         Guid lastUsedUnmuteDeviceId = Properties.Settings.Default.DeviceId;
         if (muter.Devices.Count == 0)
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
         SetAutorunState();
         
         muter.DeviceListChanged += (s, e) => InvokeIfRequired(() => LoadDevices(muter.Device.Id));
         autoStartCheckbox.CheckedChanged += (s, e) => { AutoRun.Enable(autoStartCheckbox.Checked, $"\"{Assembly.GetEntryAssembly().Location}\" /background"); SetAutorunState(); };
         pausedCheckbox.CheckedChanged += (s, e) => muter.Pause(pausedCheckbox.Checked);
         deviceCombo.SelectedIndexChanged += (s, e) => muter.SetDevice(((ComboBoxItem)deviceCombo.SelectedItem).Value as CoreAudioDevice);
         unmuteTimeTrackbar.ValueChanged += (s, e) =>
         {
            int t = UnmuteTimeSliderIncrement * unmuteTimeTrackbar.Value;
            muter.UnmuteTimeMs = t;
            unmuteTimeLabel.Text = $"{t} ms";
         };

         if (unmuteTime > unmuteTimeTrackbar.Maximum * UnmuteTimeSliderIncrement || unmuteTime < unmuteTimeTrackbar.Minimum * UnmuteTimeSliderIncrement)
            unmuteTime = GetDefaultsettingsInt(nameof(Properties.Settings.UnmuteTimeMs));
         unmuteTimeTrackbar.Value = unmuteTime / UnmuteTimeSliderIncrement;

         LoadDevices(lastUsedUnmuteDeviceId);
      }

      private void SetAutorunState()
      {
         bool? autoRun = AutoRun.IsEnabled();
         autoStartCheckbox.Enabled = autoRun.HasValue;
         autoStartCheckbox.Checked = autoRun ?? false;
      }

      private void LoadDevices(Guid? idOfPrefderredDevice)
      {
         deviceCombo.Items.Clear();
         deviceCombo.Items.AddRange(muter.Devices.Select(x => new ComboBoxItem(x, x.FullName)).ToArray());
         if (muter.Device != null)
            deviceCombo.SelectedItem = new ComboBoxItem(muter.Device, muter.Device.FullName);

         CoreAudioDevice selectedMuteDevice = muter.Devices.FirstOrDefault(d => !idOfPrefderredDevice.Equals(Guid.Empty) && idOfPrefderredDevice.Equals(d.Id))
            ?? muter.Devices.FirstOrDefault(d => d.IsDefaultCommunicationsDevice)
            ?? muter.Devices.FirstOrDefault(d => !d.IsMuted)
            ?? muter.Devices.FirstOrDefault();

         deviceCombo.SelectedItem = new ComboBoxItem(selectedMuteDevice, selectedMuteDevice.FullName);
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

      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            muter.Dispose();
            components?.Dispose();
         }
         base.Dispose(disposing);
      }
   }
}
