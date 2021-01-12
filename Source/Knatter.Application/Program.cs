using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Knatter.Core;
using WinFormsApplication = System.Windows.Forms.Application;

namespace Knatter.Application
{
   static class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         WinFormsApplication.EnableVisualStyles();
         WinFormsApplication.SetCompatibleTextRenderingDefault(false);
         WinFormsApplication.ThreadException += new ThreadExceptionEventHandler((o, e) => ShowCrashDialog(e.Exception));
         AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, e) => ShowCrashDialog(e.ExceptionObject as Exception));
         Startup(args);
      }

      private static void Startup(string[] args)
      {
         bool startMinimized = args.Any(a => a.Equals("/background", StringComparison.OrdinalIgnoreCase));
         var ctx = MuteAppContext.Create(startMinimized);
         if (ctx != null)
            WinFormsApplication.Run(ctx);
         else
            WinFormsApplication.Exit();
      }

      private static void ShowCrashDialog(Exception ex)
      {
         string msg = ex == null ? "Unexpected error" : $"An error occurred: {ex.Message}";
         string detail = ex == null ? "" : ex.ToString();
         MessageBox.Show(text: $"{msg}\n\nPlease report this error at {ProgramInfo.GithubUrl}\n\nError details:\n\n{detail}", caption: "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         WinFormsApplication.Exit();
      }

      private sealed class MuteAppContext : ApplicationContext
      {
         private readonly NotifyIcon trayIcon;
         private readonly MuteForm muteForm;
         private readonly SingleInstance singleInstance;

         public static MuteAppContext Create(bool startMinimized)
         {
            var singleInst = SingleInstance.Create($"{ProgramInfo.Name}-single-instance-mutex", 1000);
            if (singleInst == null)
            {
               Debug.WriteLine("Already running.");
               return null;
            }
            return new MuteAppContext(singleInst, startMinimized);
         }

         private MuteAppContext(SingleInstance singleInstance, bool startMinimized)
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
            muteForm = new MuteForm();
            trayIcon.DoubleClick += (s, e) => Preferences();
            trayIcon.Text = ProgramInfo.NameAndVersion;
            muteForm.muter.MutedChanged += Muter_MutedChanged;
            if (!startMinimized)
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
            WinFormsApplication.Exit();
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
