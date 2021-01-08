using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyMute
{
   public static class ProgramInfo
   {
      public static string Name => System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
      public static string Version  => $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}.{ThisAssembly.Git.Commits}";
      public static string NameAndVersion => $"{Name} v{Version}";
   }

   static class Program
   {   
      [STAThread]
      static void Main()
      {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         Application.Run(new MuteAppContext());
      }

      private sealed class MuteAppContext : ApplicationContext
      {
         private readonly NotifyIcon trayIcon;
         private readonly MuteForm muteForm;
   
         public MuteAppContext()
         {
            muteForm = new MuteForm();
            trayIcon = new NotifyIcon()
            {
               Icon = Properties.Resources.AppIcon,
               ContextMenu = new ContextMenu(new MenuItem[] {
                  new MenuItem("Preferences", (s, e) => Preferences()),
                  new MenuItem("Exit", (s, e) => Exit())
               }),
               Visible = true,              
            };
            trayIcon.DoubleClick += (s, e) => Preferences();
            trayIcon.Text = ProgramInfo.NameAndVersion;
            muteForm.Show();
         }

         private void Preferences()
         {
            muteForm.Show();
         }

         private void Exit()
         {
            trayIcon.Visible = false;
            muteForm.Close();
            Application.Exit();
         }
      }
   }
}
