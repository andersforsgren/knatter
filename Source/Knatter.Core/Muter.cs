using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Knatter.Core
{
   public sealed class Muter : IDisposable, IObserver<DeviceChangedArgs>
   {
      private bool initiallyMuted;
      private Timer timer;
      private CoreAudioDevice muteDevice;
      private CoreAudioDevice[] captureDevices;
      private readonly GlobalKeyHook keyHook;
      private readonly CoreAudioController audioController;

      public event EventHandler MutedChanged;
      public event EventHandler DeviceListChanged;

      public Muter()
      {
         this.keyHook = new GlobalKeyHook(k => MuteEvent());
         this.audioController = new CoreAudioController();
         InitCaptureDevices();
         this.audioController.AudioDeviceChanged.Subscribe(this);
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
            Debug.WriteLine($"Muting device at {DateTime.Now}");
            Mute(true);
         }
      }

      public bool DeviceMuted => this.muteDevice != null && muteDevice.IsMuted;

      public bool IsPaused { get; private set; }

      public Guid DeviceId => this.muteDevice == null ? Guid.Empty : this.muteDevice.Id;

      public CoreAudioDevice  Device => this.muteDevice;

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
            if (muteDevice.IsMuted == mute)
               return;

            bool result = muteDevice.Mute(mute);                       
            MutedChanged?.Invoke(this, EventArgs.Empty);
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
         audioController.Dispose();
      }

      private void InitCaptureDevices()
      {
         this.captureDevices = audioController.GetCaptureDevices().ToArray();
         Debug.WriteLine("Devices now: " + string.Join(",", captureDevices.Select(d => d.FullName)));
         DeviceListChanged?.Invoke(this, EventArgs.Empty);
      }

      public void OnNext(DeviceChangedArgs value)
      {
         InitCaptureDevices();
      }

      void IObserver<DeviceChangedArgs>.OnError(Exception error) { }
      void IObserver<DeviceChangedArgs>.OnCompleted() { }
   }
}
