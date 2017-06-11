using System;

namespace IssueTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Use 'help' for more information");
                return;
            }


            var issueTracker = new IssueTracker(Environment.CurrentDirectory);

            var parser = new ArgumentParser();
            parser.ParseAndExecute(args, issueTracker);

        }
    }
}
