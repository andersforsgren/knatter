namespace Knatter.Core
{
   public static class ProgramInfo
   {
      public const string Name = "Knatter";
      public const string GithubUrl = "https://github.com/andersforsgren/knatter";
      public static string Version => $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
      public static string NameAndVersion => $"{Name} v{Version}";
   }
}
