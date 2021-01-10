using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Diagnostics;
using System.Threading;

namespace Knatter.Core
{
   public sealed class Muter
   {
      private bool initiallyMuted;
      private Timer timer;
      private CoreAudioDevice muteDevice;

      public event EventHandler MutedChanged;

      public Muter()
      {
      }

      public void SetDevice(CoreAudioDevice newMuteDevice)
      {
         if (newMuteDevice != muteDevice)
         {
            Stop();
            if (newMuteDevice != null)
            {
               this.muteDevice = newMuteDevice;
               Start();
            }
         }
      }

      public int UnmuteTimeMs
      {
         get; set;
      }

      public void MuteEvent()
      {
         if (muteDevice == null || IsPaused)
            return;

         timer.Change(UnmuteTimeMs, Timeout.Infinite);
         if (!muteDevice.IsMuted)
         {
            Debug.WriteLine($"Muting device at {DateTime.Now}");
            Mute(true);
         }
      }

      public bool DeviceMuted => this.muteDevice != null && muteDevice.IsMuted;

      public bool IsPaused { get; private set; }

      public Guid DeviceId => this.muteDevice == null ? Guid.Empty : this.muteDevice.Id;

      public void Pause(bool pause)
      {
         if (this.IsPaused == pause)
            return;
         IsPaused = pause;
         if (!pause)
         {
            Mute(false);
         }
      }

      private void Start()
      {
         if (muteDevice == null)
            return;

         this.initiallyMuted = muteDevice.IsMuted;
         this.timer = new Timer(new TimerCallback(TimerElapsed), null, UnmuteTimeMs, Timeout.Infinite);
      }

      public void Stop()
      {
         if (muteDevice == null)
            return;

         timer?.Dispose();
         muteDevice.Mute(initiallyMuted);
         muteDevice = null;
      }

      private void TimerElapsed(object state)
      {
         if (IsPaused && (muteDevice == null || !muteDevice.IsMuted))
            return;

         Debug.WriteLine($"Unmuting at {DateTime.Now}");
         Mute(false);
      }

      private bool Mute(bool mute)
      {
         if (muteDevice == null)
            return false;

         bool result = muteDevice.Mute(mute);

         MutedChanged?.Invoke(this, EventArgs.Empty);
         return result;
      }
   }
}
