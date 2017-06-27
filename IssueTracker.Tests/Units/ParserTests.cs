using FluentAssertions;
using NUnit.Framework;
using System;

namespace IssueTracker.Tests.Units
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestShortShowCommand()
        {
            // only a number should convert to "show 1"
            TestShortcutCommand(new[] { "1" }, "show");
        }

        [Test]
        public void TestShortTagCommand()
        {
            // number + tag should be edit command
            TestShortcutCommand(new[] { "1", "tag:foo" }, "edit");
        }

        [Test]
        public void TestShortCommentCommand()
        {
            // number + "-m" should be edit command
            TestShortcutCommand(new[] { "1", "-m", "my-text" }, "comment");
            TestShortcutCommand(new[] { "1", "--message=my-text" }, "comment");
        }

        /// <summary>
        /// Helper that creates a temporary issue project with a single issue and executes the provided set of arguments.
        /// Using the mock tracker it checks that the specific command (<see cref="expectedCommand"/> is actually executed).
        /// </summary>
        /// <param name="args"></param>
        /// <param name="expectedCommand"></param>
        private static void TestShortcutCommand(string[] args, string expectedCommand)
        {
            TestHelper.ExecuteInNewDirectory(dir =>
            {
                var real = new IssueTracker(dir);
                // all mock queries require real project dir
                real.InitializeNewProject();
                real.AddIssue("dummy", null, null);
                var mock = new MockIssueTracker(dir);

                var parser = new ArgumentParser();

                // assert the correct command is called
                bool commandRan = false;
                mock.OnMethodCalled += (sender, e) =>
                {
                    if (e.Command == expectedCommand)
                    {
                        commandRan = true;
                    }
                    else
                    {
                        throw new NotSupportedException("Another command was called than we expected");
                    }
                };
                parser.ParseAndExecute(args, mock);
                // should be true when the method was called above
                // if the mock method was called with wrong command, we would get the NotSupportedException above
                // so really the only possibility to get to false here is when the command failed (e.g. "issue not found", "canot add comment to closed issue", or similar)
                commandRan.Should().BeTrue("because using a shorthand command should be correctly resolved to the longform by the parser.");
            });
        }
    }
}