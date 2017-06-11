using MadMilkman.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IssueTracker
{
    public class Storage
    {
        /// <summary>
        /// Saves the issue to disk and all comments to disk.
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="isNew">If true will assert that no other issue with this id exists. If false just overwrites the existing one.</param>
        public static void SaveIssue(Issue issue, bool isNew)
        {
            var tDir = "#" + issue.Id;
            if (isNew && Directory.Exists(tDir))
            {
                throw new NotSupportedException($"Issue with id {issue.Id} already exists.");
            }
            Directory.CreateDirectory(tDir);
            var tags = issue.Tags.Select(t => t.Name).ToList();
            File.WriteAllLines(Path.Combine(tDir, "issue.ini"), new[]
            {
                "[Issue]",
                "Title=" + issue.Title,
                "Message=" + issue.Message,
                "Tags=" + string.Join(",", tags),
                "PostDate=" + issue.CreationDate.ToFileTimeUtc(),
                "Author=" + issue.Author,
                "State=" + issue.State,
                "CommentCount=" + issue.Comments.Count,
                "LastStateChangeCommentIndex=" + issue.LastStateChangeCommentIndex
            });
            for (var i = 0; i < issue.Comments.Count; i++)
            {
                var comment = issue.Comments[i];
                int id = i + 1;
                var file = $"comment-{id:000}.txt";
                File.WriteAllLines(Path.Combine(tDir, file), new[]
                {
                    "[Comment]",
                    "Message=" + comment.Message,
                    "CommentDate=" + comment.DateTime.ToFileTimeUtc(),
                    "Author=" + comment.Author,
                    "Editable=" + comment.Editable,
                    "ChangedStateTo=" + (comment.ChangedStateTo?.ToString() ?? "")
                });
            }
        }

        /// <summary>
        /// Loads all issues.
        /// </summary>
        /// <returns></returns>
        public static List<Issue> LoadIssues()
        {
            var issues = new List<Issue>();
            var curr = Path.GetFullPath(".");
            var dirs = Directory.GetDirectories(curr);
            foreach (var d in dirs)
            {
                var i = LoadIssue(d);
                if (i == null)
                {
                    // silent ignore, could be other directories that don't match our format
                    // although we should probably forbid that. It's not like adding custom files into the ".git" directory is going to do any good either..
                    continue;
                }
                issues.Add(i);
            }
            return issues;
        }

        internal static Issue LoadIssue(string directory)
        {
            var dName = new DirectoryInfo(directory).Name;
            if (!dName.StartsWith("#"))
                return null;
            int id;
            if (!int.TryParse(dName.Substring(1), out id))
                return null;

            var root = Path.Combine(directory, "issue.ini");
            if (!File.Exists(root))
                return null;

            var ini = new IniFile();
            ini.Load(root);

            var sec = ini.Sections["Issue"];
            var title = sec.Keys["Title"].Value;
            var message = sec.Keys["Message"].Value;
            var tagList = sec.Keys["Tags"].Value;
            Tag[] tags = null;
            if (!string.IsNullOrWhiteSpace(tagList))
            {
                var t = tagList.Contains(",") ? tagList.Split(',') : new[] { tagList };
                tags = t.Select(v => new Tag(v)).ToArray();
            }
            var dt = sec.Keys["PostDate"].Value;
            var date = DateTime.FromFileTimeUtc(long.Parse(dt)).ToLocalTime();
            var author = sec.Keys["Author"].Value;
            var state = (IssueState)Enum.Parse(typeof(IssueState), sec.Keys["State"].Value);
            var comments = new List<Comment>();
            var commentCount = int.Parse(sec.Keys["CommentCount"].Value);
            for (int i = 0; i < commentCount; i++)
            {
                var cid = i + 1;
                var c = LoadComment(Path.Combine(directory, $"comment-{cid:000}.txt"));
                comments.Add(c);
            }
            int changeIndex = int.Parse(sec.Keys["LastStateChangeCommentIndex"].Value);
            var issue = new Issue(id, title, message, tags, date, author, comments, state, changeIndex);
            return issue;
        }

        /// <summary>
        /// Loads the specific comment.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Comment LoadComment(string path)
        {
            var ini = new IniFile();
            ini.Load(path);

            var sec = ini.Sections["Comment"];
            var message = sec.Keys["Message"].Value;
            var dt = sec.Keys["CommentDate"].Value;
            var date = DateTime.FromFileTimeUtc(long.Parse(dt)).ToLocalTime();
            var author = sec.Keys["Author"].Value;
            var editable = bool.Parse(sec.Keys["Editable"].Value);
            var changedState = sec.Keys["ChangedStateTo"]?.Value;
            var c = new Comment(message, author, date, editable);
            if (!string.IsNullOrEmpty(changedState))
            {
                var target = (IssueState)Enum.Parse(typeof(IssueState), changedState);
                c.ChangedStateTo = target;
            }
            return c;
        }
    }
}