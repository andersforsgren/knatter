using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Knatter.Application
{
   public static class Resources
   {
      private static readonly ResourceManager resourceMgr;
      private static readonly ResourceSet resourceSet;

      static Resources()
      {
         resourceMgr = new ResourceManager("Knatter.Application.Properties.Resources", Assembly.GetExecutingAssembly());
         resourceSet = resourceMgr.GetResourceSet(CultureInfo.InvariantCulture, true, false);
      }
      
      public static T Get<T>(string name)
      {
         return (T)resourceSet.GetObject(name); 
      }

      public static Icon Mute => Get<Icon>("Mute");
      public static Icon Unmute => Get<Icon>("Unmute");
   }
}
