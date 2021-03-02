using CoreAudio;
using System.Linq;

namespace Knatter.Core.Audio
{
   public class DeviceManager
   {
      private CoreAudioDevice[] devices;
      private readonly MMDeviceEnumerator deviceEnumerator;

      public DeviceManager()
      {
         this.deviceEnumerator = new MMDeviceEnumerator();
         LoadDevices();
      }

      // TODO: Handle device events changing
      internal CoreAudioDevice[] Devices => devices;

      private void LoadDevices()
      {
         this.devices = deviceEnumerator.EnumerateAudioEndPoints(EDataFlow.eCapture, DEVICE_STATE.DEVICE_STATE_ACTIVE).Select(mmd => new CoreAudioDevice(mmd)).ToArray();
      }
   }
}
