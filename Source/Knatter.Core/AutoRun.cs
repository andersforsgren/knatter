using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Knatter.Core
{
   public static class AutoRun
   {
      private const string RegKey = @"Software\Microsoft\Windows\CurrentVersion\Run";      
      private static readonly string RegValue = ProgramInfo.Name;
      
      public static bool? IsEnabled()
      {
         try
         {
            return Registry.GetValue($"HKEY_CURRENT_USER\\{RegKey}", RegValue, null) is string;
         } 
         catch (Exception ex)
         {
            Debug.WriteLine("Error reading autorun key: "+ex);
            return null;
         }
      }
      
      public static bool? Enable(bool enabled, string commandLine)
      {
         try
         {
            bool? isEnabled = IsEnabled();
            if (!isEnabled.HasValue)
               return isEnabled;

            if (isEnabled.Value == enabled)
               return isEnabled;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegKey, true))
            {
               if (enabled)
               {
                  Debug.WriteLine($"Adding regkey for autostart: {RegValue}='{commandLine}'");
                  key.SetValue(RegValue, commandLine);
               }
               else
               {
                  Debug.WriteLine($"Removing regkey for autostart: {RegValue}");
                  key.DeleteValue(RegValue);
               }
               key.Dispose();
               key.Flush();
            }
            return enabled;
         }
         catch (Exception ex)
         {
            Debug.WriteLine("Error writing autorun key: " + ex);
            return null;
         }
      }

   }
}
