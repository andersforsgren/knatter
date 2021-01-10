using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Knatter.Core
{
   public sealed class SingleInstance : IDisposable
   {
      private Mutex mutex;

      private SingleInstance(Mutex mutex)
      {
         this.mutex = mutex;
      }

      public static SingleInstance Create(string id, int timeout)
      {         
         string mutexId = string.Format("Global\\{{{0}}}", id);

         var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
         var securitySettings = new MutexSecurity();
         securitySettings.AddAccessRule(allowEveryoneRule);
         bool hasHandle;
         Mutex mutex = null;
         try
         {
            mutex = new Mutex(false, mutexId, out bool createdNew, securitySettings);
            hasHandle = mutex.WaitOne(timeout, false);
         }
         catch (AbandonedMutexException)
         {
            hasHandle = true;
         }

         if (mutex != null && hasHandle)
            return new SingleInstance(mutex);
         else return null;
      }

      public void Dispose()
      {
         var m = mutex;
         if (m != null)
         {
            m.ReleaseMutex();
            mutex = null;
         }         
      }
   }
}
