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
    }
}