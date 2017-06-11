﻿using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace IssueTracker
{
    public class Program
    {
        private static readonly ValueArgument<string> _addTitle = new ValueArgument<string>('t', "title", "sets the title for the issue");
        private static readonly ValueArgument<string> _addMessage = new ValueArgument<string>('m', "message", "adds a message");
        private static readonly ValueArgument<string> _tag = new ValueArgument<string>("tag")
        {
            Description = "Allows adding/removing tags. Tags with '-' prefix will be removed. Multiple tags are supported via ',' seperator"
        };
        private static readonly ValueArgument<int> _commentOnIssue = new ValueArgument<int>('c', "comment", "adds a comment to the specific issue.");
        private static readonly ValueArgument<string> _user = new ValueArgument<string>("user")
        {
            Description = "Filters the list for the specific user"
        };
        private static readonly EnumeratedValueArgument<string> _stateArgument = new EnumeratedValueArgument<string>('s', "state", "Filters the list for the specific state.", new[] { "open", "closed", "all" });

        /// <summary>
        /// A helper dicitionary definiting the relationship between first class and secondclass arguments.
        /// Each firstclass argument may allow multiple second class arguments (or none).
        /// </summary>
        private static readonly Dictionary<string, Argument[]> _allowedArguments = new Dictionary<string, Argument[]>
        {
            {"add", new Argument[] {_addTitle, _addMessage, _tag} },
            {"edit", new Argument[] {_tag} },
            {"comment", new Argument[] {_addMessage} },
            {"show", new Argument[0] },
            {"close", new Argument[0] },
            {"reopen", new Argument[0] }
        };

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Use 'help' for more information");
                return;
            }

            var parser = new CommandLineParser.CommandLineParser
            {
                AcceptEqualSignSyntaxForValueArguments = true,

            };

            parser.Arguments.Add(_addTitle);
            parser.Arguments.Add(_addMessage);
            parser.Arguments.Add(_tag);
            parser.Arguments.Add(_commentOnIssue);
            parser.Arguments.Add(_user);
            parser.Arguments.Add(_stateArgument);
            try
            {
                // we asserted that there is at least one argument
                var firstArg = args[0].ToLower();
                var issueTracker = new IssueTracker(Environment.CurrentDirectory);
                switch (firstArg)
                {
                    case "help":
                    case "--help":
                    case "/help":
                    case "-h":
                    case "/h":
                    case "h":
                    case "?":
                    case "/?":
                    case "-?":
                        DisplayHelp(parser);
                        // exit now
                        return;
                    case "init":
                        if (args.Length != 1)
                        {
                            // no further args required
                            Console.WriteLine("Invalid argument for 'init'");
                            DisplayHelp(parser);
                            return;
                        }
                        issueTracker.InitializeNewProject();
                        break;
                    case "add":
                        // add doesn't require an id
                        // parse all except the ones we manually processed
                        parser.ParseCommandLine(args.Skip(1).ToArray());
                        // ensure the user didn't call bullshit on the commandline
                        // e.g. "add user:name -title "foobar"
                        AssertNoWrongArgsWhereParsed(parser, _tag, _addMessage, _addTitle);

                        var title = _addTitle.Value;
                        var message = _addMessage.Parsed ? _addMessage.Value : null;
                        Tag[] a = null;
                        if (_tag.Parsed)
                        {
                            Tag[] r;
                            ParseTags(_tag.Value, out a, out r);
                            if (r.Any())
                                throw new CommandLineException("Cannot remove tags when adding new issue!");
                            if (!a.Any())
                                throw new CommandLineException("No tags to add provided!");
                        }
                        issueTracker.AddIssue(title, message, a);
                        break;
                    case "edit":
                    case "comment":
                    case "show":
                    case "close":
                    case "reopen":
                        ProcessCommandWithIssueId(args, parser, issueTracker);
                        break;
                    case "list":
                        ProcessListCommand(args, parser, issueTracker);
                        break;
                    default:
                        throw new CommandLineException("First argument is not valid. Use 'help' for more information.");
                }
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Parses the command line and runs the list command if it is valid.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parser"></param>
        /// <param name="issueTracker"></param>
        private static void ProcessListCommand(string[] args, CommandLineParser.CommandLineParser parser, IssueTracker issueTracker)
        {
            if (args.Length == 0 || !"list".Equals(args[0], StringComparison.InvariantCultureIgnoreCase))
                throw new NotSupportedException("First arg must be 'list'");

            // list doesn't require an id
            // parse all except the ones we manually processed
            parser.ParseCommandLine(args.Skip(1).ToArray());
            // ensure the user didn't call bullshit on the commandline
            // e.g. "list user:name -title "foobar"
            AssertNoWrongArgsWhereParsed(parser, _tag, _user, _stateArgument);
            var filters = new List<FilterValue>();
            if (_tag.Parsed)
            {
                Tag[] add, remove;
                ParseTags(_tag.Value, out add, out remove);
                if (remove.Any())
                    throw new CommandLineException("Cannot remove tags when listing issues!");
                if (!add.Any())
                    throw new CommandLineException("No tags to filter for provided!");

                filters.Add(new FilterValue(Filter.Tag, add));
            }
            if (_user.Parsed)
            {
                filters.Add(new FilterValue(Filter.User, _user.Value));
            }
            if (_stateArgument.Parsed)
            {
                filters.Add(new FilterValue(Filter.IssueState, _stateArgument.Value));
            }
            issueTracker.ListIssues(filters);
        }

        /// <summary>
        /// Processes all commands that need an issue id as second argument.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parser"></param>
        /// <param name="issueTracker"></param>
        private static void ProcessCommandWithIssueId(string[] args, CommandLineParser.CommandLineParser parser, IssueTracker issueTracker)
        {
            // all those require a issue tracker to exist
            if (!issueTracker.WorkingDirectoryIsIssueTracker)
            {
                Console.WriteLine("Command must run in an existing issue tracker directory.");
                DisplayHelp(parser);
                return;
            }
            // requires a second argument which is the issue id
            if (args.Length < 2)
            {
                Console.WriteLine($"Argument '{args[0]}' requires a second argument (int:issueId)");
                DisplayHelp(parser);
                return;
            }
            int id;
            if (!int.TryParse(args[1], out id) || id < 1)
            {
                Console.WriteLine($"'{args[1]}' is not a valid issue identifier (must be int)!");
                DisplayHelp(parser);
                return;
            }
            // first command is valid
            // now parse the remaining arguments
            // parse all except the ones we manually processed
            parser.ParseCommandLine(args.Skip(2).ToArray());
            // ensure correct usage
            var allowed = _allowedArguments[args[0]];
            AssertNoWrongArgsWhereParsed(parser, allowed);

            // now that correct usage is determined, figure out which command we used again
            switch (args[0].ToLower())
            {
                case "edit":
                    if (!_tag.Parsed)
                        throw new CommandLineException("Tags is required to edit an issue!");

                    Tag[] a, r;
                    ParseTags(_tag.Value, out a, out r);
                    issueTracker.EditTags(id, a, r);
                    break;
                case "comment":
                    if (!_addMessage.Parsed)
                        throw new CommandLineException("Message is required to comment on an issue!");

                    issueTracker.CommentIssue(id, _addMessage.Value);
                    break;
                case "show":
                    issueTracker.ShowIssue(id);
                    break;
                case "close":
                    issueTracker.CloseIssue(id);
                    break;
                case "reopen":
                    issueTracker.ReopenIssue(id);
                    break;
            }
        }

        /// <summary>
        /// Returns the set of tags to be added and removed based on the input string.
        /// Seperator for multiple tags is ',' otherwise a single tag is assumed.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="tagsToAdd"></param>
        /// <param name="tagsToRemove"></param>
        private static void ParseTags(string tags, out Tag[] tagsToAdd, out Tag[] tagsToRemove)
        {
            var remove = new List<Tag>();
            var add = new List<Tag>();
            var allTags = (tags.Contains(",") ? tags.Split(',') : new[] { tags }).ToArray();
            foreach (var t in allTags)
            {
                var tagName = t.Trim();
                if (t.StartsWith("-"))
                {
                    tagName = tagName.Substring(1);
                    if (string.IsNullOrWhiteSpace(tagName))
                        throw new CommandLineException($"'{t}' is not a valid tagname!'");

                    remove.Add(new Tag(tagName));
                }
                else
                {
                    add.Add(new Tag(tagName));
                }
            }
            tagsToAdd = add.ToArray();
            tagsToRemove = remove.ToArray();
        }

        /// <summary>
        /// Ensures that only allowed arguments where parsed.
        /// If any argument was parsed that wasn't part of the allowedArgs an exception is thrown.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="allowedArgs"></param>
        private static void AssertNoWrongArgsWhereParsed(CommandLineParser.CommandLineParser parser, params Argument[] allowedArgs)
        {
            var allArgs = parser.Arguments;

            var parsed = allArgs.Where(a => a.Parsed).ToList();
            if (parsed.Any(p => !allowedArgs.Contains(p)))
            {
                throw new CommandLineException("Invalid command format. Use 'help' for more information.");
            }
        }

        private static void DisplayHelp(CommandLineParser.CommandLineParser parser)
        {
            // TODO: display first class commands
            Console.WriteLine();
            Console.WriteLine("Optional arguments:");
            // these commands are second class only
            parser.ShowUsage();
        }
    }
}
