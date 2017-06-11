using System;
using System.IO;
using System.Linq;

namespace IssueTracker
{
    public static class GitHelper
    {
        /// <summary>
        /// Pulls the username from the %home%\.gitconfig file.
        /// Throws if the file or name is not found.
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            var cfg = ".gitconfig";
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(userProfile, cfg);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Didn't find .gitconfig in %home%");
            }
            var lines = File.ReadAllLines(path);
            var nameLine = lines.FirstOrDefault(l => l.Replace(" ", "").Replace("\t", "").StartsWith("name="));
            if (nameLine != null)
            {
                var idx = nameLine.IndexOf('=');
                var name = nameLine.Substring(idx + 1).Trim();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            throw new NotSupportedException(".gitconfig in %home% doesn't contain 'name=' line.");
        }
    }
}