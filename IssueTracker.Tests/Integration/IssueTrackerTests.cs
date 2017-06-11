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
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            it.InitializeNewProject();
            it.WorkingDirectoryIsIssueTracker.Should().BeTrue();

            Directory.Delete(path, true);
        }

        [Test]
        public void CreateNewIssueTrackerProjectInExistingIssueTracker()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);

            it.InitializeNewProject();
            it.WorkingDirectoryIsIssueTracker.Should().BeTrue();

            // does nothing because it aborts
            it.InitializeNewProject();
            it.WorkingDirectoryIsIssueTracker.Should().BeTrue();

            Directory.Delete(path, true);
        }

        [Test]
        public void AddNewIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.AddIssue("foo", "bar", null)).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }

        [Test]
        public void EditIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.EditTags(1, new Tag[0], new Tag[0])).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }

        [Test]
        public void ListIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.ListIssues(new List<FilterValue>())).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }

        [Test]
        public void ShowIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.ShowIssue(1)).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }

        [Test]
        public void CommentIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.CommentIssue(1, "foo")).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }

        [Test]
        public void CloseIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.CloseIssue(1)).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }

        [Test]
        public void ReopenIssueInEmptyDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);

            var it = new IssueTracker(path);
            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            new Action(() => it.ReopenIssue(1)).ShouldThrow<NotSupportedException>();

            it.WorkingDirectoryIsIssueTracker.Should().BeFalse();

            Directory.Delete(path, true);
        }
    }
}