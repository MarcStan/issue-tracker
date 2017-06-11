using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;

namespace IssueTracker.Tests
{
    [TestFixture]
    public class CommandLineTests
    {
        [Test]
        public void TestInvalidCommand()
        {
            // setup
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);

            var arguments = new[] { "invalid_command" };

            var args = ExecuteTestCommand(path, arguments);
            args.Should().BeNull();

            Directory.Delete(path, true);
        }

        [Test]
        public void TestHelpCommand()
        {
            // setup
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);

            var arguments = new[] { "help" };

            var args = ExecuteTestCommand(path, arguments);
            args.Should().BeNull();

            Directory.Delete(path, true);
        }

        [Test]
        public void TestInitCommand()
        {
            // setup
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);

            var arguments = new[] { "init" };

            var args = ExecuteTestCommand(path, arguments);
            args.Should().BeEmpty();

            Directory.Delete(path, true);
        }

        /// <summary>
        /// Helper method that executes the commands using the <see cref="MockIssueTracker"/> and reports back the values it returned.
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static Dictionary<string, object> ExecuteTestCommand(string workingDirectory, string[] arguments)
        {

            var mock = new MockIssueTracker(workingDirectory);
            Dictionary<string, object> argsReceived = null;
            mock.OnMethodCalled += (sender, args) =>
            {
                if (!args.Command.Equals(arguments[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new NotSupportedException($"A different method was just called! Expected call to '{arguments[0]}' but '{args.Command}' was called instead.");
                }
                argsReceived = args.Args;
            };
            var parser = new ArgumentParser();
            parser.ParseAndExecute(arguments, mock);

            return argsReceived;
        }
    }
}