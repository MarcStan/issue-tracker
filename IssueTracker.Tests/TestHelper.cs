using System;
using System.IO;

namespace IssueTracker.Tests
{
    public static class TestHelper
    {
        /// <summary>
        /// Executes the action in a new directory and deletes it regardless of test sucess or failure.
        /// The full path to the directory is provided as the action argument.
        /// The directory is guaranteed to be empty but exist.
        /// </summary>
        /// <param name="runInDir"></param>
        public static void ExecuteInNewDirectory(Action<string> runInDir)
        {
            var path = Path.Combine(Path.GetTempPath(), typeof(TestHelper).Namespace, Guid.NewGuid().ToString());
            try
            {
                Directory.CreateDirectory(path);
                runInDir(path);

            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}