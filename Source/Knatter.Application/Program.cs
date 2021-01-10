using System;
using System.Diagnostics;
using System.Windows.Forms;
using Knatter.Core;

namespace Knatter.Application
{
   static class Program
   {
      [STAThread]
      static void Main()
      {
         System.Windows.Forms.Application.EnableVisualStyles();
         System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
         var ctx = MuteAppContext.Create();
         if (ctx != null)
            System.Windows.Forms.Application.Run(ctx);
         else
            System.Windows.Forms.Application.Exit();
      }

      private sealed class MuteAppContext : ApplicationContext
      {
         private readonly NotifyIcon trayIcon;
         private readonly MuteForm muteForm;
         private readonly SingleInstance singleInstance;

         public static MuteAppContext Create()
         {
            var singleInst = SingleInstance.Create($"{ProgramInfo.Name}-single-instance-mutex", 1000);
            if (singleInst == null)
            {
               Debug.WriteLine("Already running.");
               return null;
            }
            return new MuteAppContext(singleInst);
         }

         private MuteAppContext(SingleInstance singleInstance)
         {
            this.singleInstance = singleInstance;
            trayIcon = new NotifyIcon()
            {
               Icon = Resources.Mute,
               ContextMenu = new ContextMenu(new MenuItem[] {
                  new MenuItem("Preferences", (s, e) => Preferences()),
                  new MenuItem("Exit", (s, e) => Exit())
               }),
               Visible = true,
            };
            trayIcon.DoubleClick += (s, e) => Preferences();
            trayIcon.Text = ProgramInfo.NameAndVersion;
            muteForm = new MuteForm();
            muteForm.muter.MutedChanged += Muter_MutedChanged;
            muteForm.Show();
         }

         private void Muter_MutedChanged(object sender, EventArgs e)
         {
            var icon = muteForm.muter.DeviceMuted ? Resources.Mute : Resources.Unmute;
            trayIcon.Icon = icon;
            muteForm.SetIcon(icon);
         }

         private void Preferences()
         {
            muteForm.Show();
         }

         private void Exit()
         {
            trayIcon.Visible = false;
            muteForm.SaveSettings();
            muteForm.Close();
            System.Windows.Forms.Application.Exit();
         }

         protected override void Dispose(bool disposing)
         {
            if (disposing)
               singleInstance?.Dispose();

            base.Dispose(disposing);
         }
      }
   }
}
