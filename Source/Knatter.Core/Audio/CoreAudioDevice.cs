using CoreAudio;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Knatter.Core.Audio
{
   [DebuggerDisplay("{ToString(),nq}")]
   public class CoreAudioDevice
   {
      private readonly MMDevice device;

      public CoreAudioDevice(MMDevice device)
      {
         this.device = device;

         Id = GetGuid(device.ID);
         Debug.WriteLine($"Device '{this}' initialized on thread {Thread.CurrentThread.ManagedThreadId}");
      }

      public string FullName => device.FriendlyName;

      public override string ToString() => FullName;

      public Guid Id { get; }

      public bool Mute(bool muted)
      {
         try
         {
            if (device.State != DEVICE_STATE.DEVICE_STATE_ACTIVE)
               return false;
            device.AudioEndpointVolume.Mute = muted;
            return device.AudioEndpointVolume.Mute == muted;
         }
         catch (Exception ex)
         {
            Debug.WriteLine("Failed to mute: " + ex);
            return false;
         }
      }

      public bool IsDefaultCommunicationsDevice => true;

      public bool IsMuted => device.AudioEndpointVolume.Mute;

      private static Guid GetGuid(string deviceId)
      {
         var r = new Regex(@"([a-fA-F0-9]{8}[-][a-fA-F0-9]{4}[-][a-fA-F0-9]{4}[-][a-fA-F0-9]{4}[-][a-fA-F0-9]{12})");
         var match = r.Match(deviceId);

         if (!match.Success)
            return Guid.Empty;

         return new Guid(match.Value);
      }
   }
}
