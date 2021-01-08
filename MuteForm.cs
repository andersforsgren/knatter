using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace KeyMute
{

   public partial class MuteForm : Form
   {   
      private readonly GlobalKeyHook keyHook;
      private readonly CoreAudioController audioController;
      private readonly CoreAudioDevice[] captureDevices;

      private readonly Muter muter;

      public MuteForm()
      {
         InitializeComponent();
         this.keyHook = new GlobalKeyHook(k => GlobalKeyPress(k));
         this.audioController = new CoreAudioController();
         this.captureDevices = audioController.GetCaptureDevices().ToArray();
         CoreAudioDevice initialMuteDevice = captureDevices.FirstOrDefault(d => d.IsDefaultCommunicationsDevice) ?? captureDevices.FirstOrDefault(d => !d.IsMuted) ?? captureDevices.FirstOrDefault();
         if (initialMuteDevice == null)
         {
            MessageBox.Show("No capture device found");
            Application.Exit();
            return;
         }
         Text = ProgramInfo.NameAndVersion;
         deviceCombo.Items.AddRange(captureDevices.Select(x => new ComboBoxItem(x, x.FullName)).ToArray());
         pausedCheckbox.CheckedChanged += (s, e) => muter.Pause(pausedCheckbox.Checked);
         deviceCombo.SelectedIndexChanged += (s, e) => muter.SetDevice(((ComboBoxItem)deviceCombo.SelectedItem).Value as CoreAudioDevice);
         this.muter = new Muter();
         this.muter.MutedChanged += (s, e) => InvokeIfRequired(() => Text = muter.DeviceMuted ? $"{ProgramInfo.NameAndVersion}*" : $"{ProgramInfo.NameAndVersion}");
         deviceCombo.SelectedItem = new ComboBoxItem(initialMuteDevice, initialMuteDevice.FullName);
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         Hide();
         e.Cancel = true;
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
