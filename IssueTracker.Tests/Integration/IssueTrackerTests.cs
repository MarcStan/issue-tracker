using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace IssueTracker.Tests.Integration
{
    /// <summary>
    /// Asserts that the output of issue tracker commands is as expected.
    /// </summary>
    [TestFixture]
    public class IssueTrackerTests
    {
        [Test]
        public void CreateNewIssueTrackerProjectInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                it.InitializeNewProject();
                it.WorkingDirectoryIsIssueTracker.Should().BeTrue();
            });
        }

        [Test]
        public void CreateNewIssueTrackerProjectInExistingIssueTracker()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);

                it.InitializeNewProject();
                it.WorkingDirectoryIsIssueTracker.Should().BeTrue();

                // does nothing because it aborts
                it.InitializeNewProject();
                it.WorkingDirectoryIsIssueTracker.Should().BeTrue();
            });
        }

        [Test]
        public void AddNewIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.AddIssue("foo", "bar", null)).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void AddNewIssueInIssueTrackerDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
                it.InitializeNewProject();

                new Action(() => it.AddIssue("foo", "bar", null)).ShouldNotThrow();

                File.Exists(Path.Combine(path, "#1\\issue.ini")).Should().BeTrue();
            });
        }

        [Test]
        public void AddTagToExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                new Action(() => it.EditTags(1, new[] { new Tag("feature") }, new Tag[0])).ShouldNotThrow();

                var tags = new Storage(path).LoadIssue(Path.Combine(path, "#1")).Tags;
                tags.Should().HaveCount(4);
                tags.Should()
                    .Contain(new[]
                    {
                        new Tag("foo"),
                        new Tag("bar"),
                        new Tag("bug"),
                        new Tag("feature")
                    });
            });
        }

        [Test]
        public void RemoveTagFromExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                new Action(() => it.EditTags(1, new Tag[0], new[] { new Tag("bug") })).ShouldNotThrow();

                var tags = new Storage(path).LoadIssue(Path.Combine(path, "#1")).Tags;
                tags.Should().HaveCount(2);
                tags.Should()
                    .Contain(new[]
                    {
                        new Tag("foo"),
                        new Tag("bar"),
                    });
            });
        }

        [Test]
        public void AddAndRemoveTagsFromExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                new Action(() => it.EditTags(1, new[] { new Tag("buzz"), }, new[] { new Tag("bug"), new Tag("foo") })).ShouldNotThrow();

                var tags = new Storage(path).LoadIssue(Path.Combine(path, "#1")).Tags;
                tags.Should().HaveCount(2);
                tags.Should()
                    .Contain(new[]
                    {
                        new Tag("bar"),
                        new Tag("buzz")
                    });
            });
        }

        [Test]
        public void EditIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.EditTags(1, new Tag[0], new Tag[0])).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void ListIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.ListIssues(new List<FilterValue>())).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void ListExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                // can't really test much appart from no crash because it just prints to console
                new Action(() => it.ListIssues(new List<FilterValue>())).ShouldNotThrow();
                new Action(() => it.ListIssues(new List<FilterValue>
                {
                    new FilterValue(Filter.IssueState, IssueState.Open)
                })).ShouldNotThrow();
            });
        }

        [Test]
        public void ShowIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.ShowIssue(1)).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void ShowExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                // can't really test much appart from no crash because it just prints to console
                new Action(() => it.ShowIssue(1)).ShouldNotThrow();
            });
        }

        [Test]
        public void CommentIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.CommentIssue(1, "foo")).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void CommentExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                var c = Path.Combine(path, "#1\\comment-001.ini");
                File.Exists(c).Should().BeFalse();
                new Action(() => it.CommentIssue(1, "dummy")).ShouldNotThrow();

                File.Exists(c).Should().BeTrue();
            });
        }

        [Test]
        public void CloseIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.CloseIssue(1)).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void CloseExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);

                var c1 = Path.Combine(path, "#1\\comment-001.ini");
                var c2 = Path.Combine(path, "#1\\comment-002.ini");
                File.Exists(c1).Should().BeFalse();
                File.Exists(c2).Should().BeFalse();

                new Action(() => it.CloseIssue(1)).ShouldNotThrow();
                new Action(() => it.CloseIssue(1)).ShouldNotThrow();

                // closing twice should only close once
                // so we should only have 1 comment file
                File.Exists(c1).Should().BeTrue();
                File.Exists(c2).Should().BeFalse();
            });
        }

        [Test]
        public void ReopenIssueInEmptyDirectory()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = new IssueTracker(path);
                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

                new Action(() => it.ReopenIssue(1)).ShouldThrow<NotSupportedException>();

                it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            });
        }

        [Test]
        public void ReopenExistingIssue()
        {
            TestHelper.ExecuteInNewDirectory(path =>
            {
                var it = SetupIssueTrackerWithIssues(path);
                it.CloseIssue(1);

                // comment file 1 was created by close issue, so reopen should be file 2
                var c1 = Path.Combine(path, "#1\\comment-002.ini");
                var c2 = Path.Combine(path, "#1\\comment-003.ini");
                File.Exists(c1).Should().BeFalse();
                File.Exists(c2).Should().BeFalse();

                new Action(() => it.ReopenIssue(1)).ShouldNotThrow();
                new Action(() => it.ReopenIssue(1)).ShouldNotThrow();

                // closing twice should only close once
                // so we should only have 1 comment file (about reopening)
                File.Exists(c1).Should().BeTrue();
                File.Exists(c2).Should().BeFalse();
            });
        }

        /// <summary>
        /// Helper that creates issue tracker in the provided directory and adds 3 issues.
        /// </summary>
        /// <param name="path"></param>
        private static IssueTracker SetupIssueTrackerWithIssues(string path)
        {
            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();
            it.InitializeNewProject();

            it.AddIssue("hello world", null, new[] { new Tag("foo"), new Tag("bar"), new Tag("bug") });
            it.AddIssue("hello world", "this one has a message", new[] { new Tag("bug") });
            it.AddIssue("different title", "message", new[] { new Tag("foo"), new Tag("bar"), new Tag("bug") });

            return it;
        }
    }
}