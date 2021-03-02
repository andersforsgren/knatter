using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Knatter.Core.Audio;

namespace Knatter.Core
{
   public sealed class Muter : IDisposable
   {
      private bool initiallyMuted;
      private Timer timer;
      private CoreAudioDevice muteDevice;
      private CoreAudioDevice[] captureDevices;
      private readonly GlobalKeyHook keyHook;

      public event EventHandler MutedChanged;

#pragma warning disable CS0067
      // TODO: handle device changes
      public event EventHandler DeviceListChanged;
#pragma warning restore CS0067

      private readonly SynchronizationContext syncContext;

      public Muter(SynchronizationContext syncContext)
      {
         this.syncContext = syncContext;
         this.keyHook = new GlobalKeyHook(k => MuteEvent());
         this.captureDevices = new DeviceManager().Devices;
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

      public IReadOnlyList<CoreAudioDevice> Devices => captureDevices;

      public int UnmuteTimeMs
      {
         get; set;
      }

      private void MuteEvent()
      {
         if (muteDevice == null || IsPaused)
            return;

         timer.Change(UnmuteTimeMs, Timeout.Infinite);
         if (!muteDevice.IsMuted)
         {
            Debug.WriteLine($"Muting device at {DateTime.Now} on thread {Thread.CurrentThread.ManagedThreadId}");
            Mute(true);
         }
      }

      public bool DeviceMuted => this.muteDevice != null && muteDevice.IsMuted;

      public bool IsPaused { get; private set; }

      public Guid DeviceId => this.muteDevice == null ? Guid.Empty : this.muteDevice.Id;

      public CoreAudioDevice Device => this.muteDevice;

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

      private void Mute(bool mute)
      {
         if (muteDevice == null)
            return;

         try
         {
            syncContext.Send(x =>
            {
               if (muteDevice.IsMuted == mute)
                  return;

               bool result = muteDevice.Mute(mute);
               MutedChanged?.Invoke(this, EventArgs.Empty);
            }, null);
         }
         catch (Exception ex)
         {
            Debug.WriteLine($"Exception muting/unmuting: {ex}");

            // Ummute failed: schedule retry.
            if (!mute)
               timer.Change(UnmuteTimeMs, Timeout.Infinite);
         }
      }

      public void Dispose()
      {
         Stop();
         keyHook.Dispose();
      }
   }
}
