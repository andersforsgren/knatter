using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyMute
{  
   public sealed class GlobalKeyHook : IDisposable
   {
      private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

      private const int WH_KEYBOARD_LL = 13;
      private const int WM_KEYDOWN = 0x0100;
      private readonly LowLevelKeyboardProc proc;
      private IntPtr hook = IntPtr.Zero;
      private readonly Action<int> userCallback;

      public GlobalKeyHook(Action<int> callback)
      {
         this.userCallback = callback;
         proc = HookCallback;
         this.hook = SetHook(proc);
      }

      public void Dispose()
      {
         var p = hook;
         if (p != IntPtr.Zero)
            UnhookWindowsHookEx(p);
         hook = IntPtr.Zero;
      }

      private IntPtr SetHook(LowLevelKeyboardProc proc)
      {
         using (Process process = Process.GetCurrentProcess())
         {
            using (ProcessModule mainModule = process.MainModule)
            {
               return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(mainModule.ModuleName), 0);
            }
         }
      }
     
      private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
      {
         if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
         {
            int vkCode = Marshal.ReadInt32(lParam);
            userCallback?.Invoke(vkCode);
         }
         return CallNextHookEx(hook, nCode, wParam, lParam);
      }

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool UnhookWindowsHookEx(IntPtr hhk);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

      [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      private static extern IntPtr GetModuleHandle(string lpModuleName);
   }
}
