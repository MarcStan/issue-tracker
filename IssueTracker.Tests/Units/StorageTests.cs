using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace IssueTracker.Tests.Units
{
    [TestFixture]
    public class StorageTests
    {
        [Test]
        public void TestSaveIssue()
        {
            var id = 1;
            var dir = "#" + id;

            var date = DateTime.Now;
            var i = new Issue(id, "hello world", "test message", null, date, "me");
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.Exists(dir).Should().BeFalse();
            Storage.SaveIssue(i, true);
            Directory.Exists(dir).Should().BeTrue();
            File.Exists(dir + "\\issue.ini").Should().BeTrue();

            File.ReadAllLines(dir + "\\issue.ini")
                .Should()
                .ContainInOrder(
                    "[Issue]",
                    "Title=hello world",
                    "Message=test message",
                    "Tags=",
                    "PostDate=" + date.ToFileTimeUtc(),
                    "Author=me",
                    "State=Open",
                    "CommentCount=0",
                    "LastStateChangeCommentIndex=-1");

            Directory.Delete(dir, true);
        }

        [Test]
        public void TestLoadIssue()
        {
            var id = 2;
            var dir = "#" + id;

            var date = DateTime.Now;
            var i = new Issue(id, "hello world", "test message", null, date, "me");
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.Exists(dir).Should().BeFalse();
            Storage.SaveIssue(i, true);
            Directory.Exists(dir).Should().BeTrue();
            File.Exists(dir + "\\issue.ini").Should().BeTrue();

            var loaded = Storage.LoadIssue(dir);
            loaded.Title.Should().Be("hello world");
            loaded.Message.Should().Be("test message");
            loaded.Tags.Should().HaveCount(0);
            loaded.CreationDate.Should().Be(date);
            loaded.Author.Should().Be("me");
            loaded.State.Should().Be(IssueState.Open);
            loaded.Comments.Should().HaveCount(0);
            loaded.LastStateChangeCommentIndex.Should().Be(-1);

            Directory.Delete(dir, true);
        }

        [Test]
        public void TestSaveAndLoadIssueWithComments()
        {
            var id = 2;
            var dir = "#" + id;

            var date = DateTime.Now;
            var i = new Issue(id, "hello world", "test message", null, date, "me");
            i.Add(new Comment("c1", "me again", DateTime.Now));
            i.Add(new Comment("c2", "me again", DateTime.Now));
            i.Add(new Comment("c3", "me again", DateTime.Now));
            i.Add(new Comment("c4", "me again", DateTime.Now));
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.Exists(dir).Should().BeFalse();
            Storage.SaveIssue(i, true);
            Directory.Exists(dir).Should().BeTrue();
            File.Exists(dir + "\\issue.ini").Should().BeTrue();

            var loaded = Storage.LoadIssue(dir);
            loaded.Title.Should().Be("hello world");
            loaded.Message.Should().Be("test message");
            loaded.Tags.Should().HaveCount(0);
            loaded.CreationDate.Should().Be(date);
            loaded.Author.Should().Be("me");
            loaded.State.Should().Be(IssueState.Open);
            loaded.Comments.Should().HaveCount(4);
            loaded.Comments[0].Message.Should().Be("c1");
            loaded.Comments[1].Message.Should().Be("c2");
            loaded.Comments[2].Message.Should().Be("c3");
            loaded.Comments[3].Message.Should().Be("c4");
            loaded.LastStateChangeCommentIndex.Should().Be(-1);

            Directory.Delete(dir, true);
        }
    }
}
