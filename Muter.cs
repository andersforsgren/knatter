using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Diagnostics;
using System.Threading;

namespace KeyMute
{
   public class Muter
   {
      private const int IntervalMs = 50;
      private const float UnmuteTimeMs = 500.0f;
     
      private bool initiallyMuted;
      private Timer timer;
      private CoreAudioDevice muteDevice;
      private DateTime muteTime;

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

      public void MuteEvent()
      {
         if (muteDevice == null || IsPaused)
            return;
         
         muteTime = DateTime.Now;
         if (!muteDevice.IsMuted)
         {
            Debug.WriteLine($"Muting device at {DateTime.Now}");
            Mute(true);
         }
      }

      public bool DeviceMuted => this.muteDevice != null && muteDevice.IsMuted;

      public bool IsPaused { get; private set; }

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
         this.timer = new Timer(new TimerCallback(TimerElapsed), null, 0, IntervalMs);
      }

      public void Stop()
      {
         muteTime = default;
         if (muteDevice == null)
            return;

         timer?.Dispose();
         muteDevice.Mute(initiallyMuted);
         muteDevice = null;
      }

      private void TimerElapsed(object state)
      {
         if (IsPaused)
         {
            return;
         }
         if (muteTime.Ticks == 0L)
         {
            return;
         }
         else if ((DateTime.Now - muteTime).TotalMilliseconds < UnmuteTimeMs)
         {
            return;
         }
         else
         {
            Debug.WriteLine($"Unmuting at dt={(DateTime.Now - muteTime).TotalMilliseconds:F1}ms)");
            muteTime = default;
            Mute(false);
         }
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
