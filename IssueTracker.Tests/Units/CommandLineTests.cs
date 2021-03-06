using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IssueTracker.Tests.Units
{
    /// <summary>
    /// Asserts that the command line arguments invoke the correct target methods but fully ignores what those commands actually do.
    /// </summary>
    [TestFixture]
    public class CommandLineTests
    {
        [Test]
        public void TestInvalidCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var arguments = new[] { "invalid_command" };

                var args = ExecuteTestCommand(path, arguments);
                args.Should().BeNull();
            });
        }

        [Test]
        public void TestHelpCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var arguments = new[] { "help" };

                var args = ExecuteTestCommand(path, arguments);
                args.Should().BeNull();
            });
        }

        [Test]
        public void TestInitCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var arguments = new[] { "init" };

                var args = ExecuteTestCommand(path, arguments);
                args.Should().BeEmpty();
            });
        }

        [Test]
        public void TestListCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");

                var arguments = new[] { "list" };

                var args = ExecuteTestCommand(path, arguments);
                // if no filters are provided a default filter of "open issues only" is applied
                args.Should().ContainKey("filters");

                var filters = (List<FilterValue>)args["filters"];
                filters.Should().HaveCount(1);
                var state = filters.FirstOrDefault(f => f.FilterType == Filter.IssueState);
                state.Should().NotBeNull();
                state.Value.Should().Be(IssueState.Open);
            });
        }

        [Test]
        public void TestListCommandWithFilters()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");

                // filter order shouldn't matter
                var permutations = new[]
                {
                    new[] {"list", "open", "user:me", "tag:foo"},
                    new[] {"list", "user:me", "state:open", "tag:foo"},
                    new[] {"list", "user:me", "tag:foo", "--state:open"},
                    new[] {"list", "/tag:foo", "user:me", "open"},
                    new[] {"list", "tag:foo", "open", "user:me"}
                };

                foreach (var p in permutations)
                {
                    var args = ExecuteTestCommand(path, p);
                    args.Should().ContainKey("filters");

                    var filters = (List<FilterValue>)args["filters"];
                    filters.Should().HaveCount(3);
                    var state = filters.FirstOrDefault(f => f.FilterType == Filter.IssueState);
                    state.Should().NotBeNull();
                    var user = filters.FirstOrDefault(f => f.FilterType == Filter.User);
                    user.Should().NotBeNull();
                    var tag = filters.FirstOrDefault(f => f.FilterType == Filter.Tag);
                    tag.Should().NotBeNull();

                    state.Value.Should().Be(IssueState.Open);
                    user.Value.Should().Be("me");
                    var tags = (Tag[])tag.Value;
                    tags.Should().HaveCount(1);
                    tags[0].Name.Should().Be("foo");
                }
            });
        }

        [Test]
        public void TestAddCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");

                var arguments = new[] { "add", "-t", "hello world", "-m", "foobar", "tag:bug" };

                var args = ExecuteTestCommand(path, arguments);
                // if no filters are provided a default filter of "open issues only" is applied
                args.Should().ContainKey("title");
                args.Should().ContainKey("message");
                args.Should().ContainKey("tags");

                args["title"].Should().Be("hello world");
                args["message"].Should().Be("foobar");
                ((Tag[])args["tags"]).Should().Contain(new Tag("bug"));
            });
        }

        [Test]
        public void TestCloseCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");
                Directory.CreateDirectory(Path.Combine(path, "#1"));
                var text = new[]
                {
                    "[Issue]",
                    "Title=hello",
                    "Message=",
                    "Tags=",
                    "PostDate=131416916119635597",
                    "Author=Contoso",
                    "State=Open",
                    "CommentCount=0",
                    "LastStateChangeCommentIndex=-1"
                };
                File.WriteAllLines(Path.Combine(path, "#1\\issue.ini"), text);

                var arguments = new[] { "close", "1" };

                var args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
            });
        }

        [Test]
        public void TestReopenCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");
                Directory.CreateDirectory(Path.Combine(path, "#1"));
                var text = new[]
                {
                    "[Issue]",
                    "Title=hello",
                    "Message=",
                    "Tags=",
                    "PostDate=131416916119635597",
                    "Author=Contoso",
                    "State=Open",
                    "CommentCount=0",
                    "LastStateChangeCommentIndex=-1"
                };
                var close = new[]
                {
                    "[Comment]",
                    "Message = Closed the issue.",
                    "CommentDate=131416926100176451",
                    "Author=Contoso",
                    "Editable=False",
                    "ChangedStateTo=Closed"
                };
                File.WriteAllLines(Path.Combine(path, "#1\\issue.ini"), text);
                File.WriteAllLines(Path.Combine(path, "#1\\comment-001.ini"), close);

                var arguments = new[] { "reopen", "1" };

                var args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
            });
        }

        [Test]
        public void TestEditTags()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");
                Directory.CreateDirectory(Path.Combine(path, "#1"));
                var text = new[]
                {
                    "[Issue]",
                    "Title=hello",
                    "Message=",
                    "Tags=",
                    "PostDate=131416916119635597",
                    "Author=Contoso",
                    "State=Open",
                    "CommentCount=0",
                    "LastStateChangeCommentIndex=-1"
                };
                File.WriteAllLines(Path.Combine(path, "#1\\issue.ini"), text);


                // first add a few tags
                var arguments = new[] { "edit", "1", "tag:foo,bar,baz" };

                var args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
                ((Tag[])args["add"]).Should().Contain(new[] { new Tag("foo"), new Tag("bar"), new Tag("baz") });
                ((Tag[])args["remove"]).Should().BeEmpty();

                // next remove one
                arguments = new[] { "edit", "1", "tag:-foo" };

                args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
                ((Tag[])args["add"]).Should().BeEmpty();
                ((Tag[])args["remove"]).Should().Contain(new[] { new Tag("foo") });

                // finally add one and remove 2
                arguments = new[] { "edit", "1", "tag:bug,-bar,-baz" };

                args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
                ((Tag[])args["add"]).Should().Contain(new[] { new Tag("bug") });
                ((Tag[])args["remove"]).Should().Contain(new[] { new Tag("bar"), new Tag("baz") });
            });
        }

        [Test]
        public void TestAddCommentCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");
                Directory.CreateDirectory(Path.Combine(path, "#1"));
                var text = new[]
                {
                    "[Issue]",
                    "Title=hello",
                    "Message=",
                    "Tags=",
                    "PostDate=131416916119635597",
                    "Author=Contoso",
                    "State=Open",
                    "CommentCount=0",
                    "LastStateChangeCommentIndex=-1"
                };
                File.WriteAllLines(Path.Combine(path, "#1\\issue.ini"), text);

                var arguments = new[] { "comment", "1", "-m", "my comment" };

                var args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
            });
        }

        [Test]
        public void TestShowIssueCommand()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                File.WriteAllText(Path.Combine(path, ".issues"), "");
                Directory.CreateDirectory(Path.Combine(path, "#1"));
                var text = new[]
                {
                    "[Issue]",
                    "Title=hello",
                    "Message=",
                    "Tags=",
                    "PostDate=131416916119635597",
                    "Author=Contoso",
                    "State=Open",
                    "CommentCount=0",
                    "LastStateChangeCommentIndex=-1"
                };
                File.WriteAllLines(Path.Combine(path, "#1\\issue.ini"), text);

                var arguments = new[] { "show", "1" };

                var args = ExecuteTestCommand(path, arguments);

                args.Should().ContainKey("id");
                args["id"].Should().Be(1);
            });
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