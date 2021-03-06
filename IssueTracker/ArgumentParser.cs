using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IssueTracker
{
    public class ArgumentParser
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
        private static readonly EnumeratedValueArgument<string> _stateArgument = new EnumeratedValueArgument<string>('s', "state", "Filters the list for the specific state (all|open|closed).", new[] { "open", "closed", "all" });

        /// <summary>
        /// A helper dicitionary definiting the relationship between first class and secondclass arguments.
        /// Each firstclass argument may allow multiple second class arguments (or none).
        /// </summary>
        private static readonly Dictionary<string, Argument[]> _allowedArguments = new Dictionary<string, Argument[]>
        {
            {"init", new Argument[0] },
            {"list", new Argument[] { _tag, _user, _stateArgument } },
            {"add", new Argument[] {_addTitle, _addMessage, _tag} },
            {"edit", new Argument[] {_tag} },
            {"comment", new Argument[] {_addMessage} },
            {"show", new Argument[0] },
            {"close", new Argument[0] },
            {"reopen", new Argument[0] }
        };

        /// <summary>
        /// Set of commands that require format "command int:id [optional]".
        /// </summary>
        private static readonly HashSet<string> _commandsWithIssueIdRequiurement = new HashSet<string>
        {
            "edit",
            "comment",
            "show",
            "close",
            "reopen"
        };

        /// <summary>
        /// Parses the command line and executes the commands where valid.
        /// Reports errors or success to console.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="issueTracker"></param>
        public void ParseAndExecute(string[] args, IssueTracker issueTracker)
        {
            if (args == null || args.Length == 0)
                throw new ArgumentException(nameof(args));
            if (issueTracker == null)
                throw new ArgumentNullException(nameof(issueTracker));

            var parser = new CommandLineParser.CommandLineParser
            {
                AcceptEqualSignSyntaxForValueArguments = true
            };

            parser.Arguments.Add(_addTitle);
            parser.Arguments.Add(_addMessage);
            parser.Arguments.Add(_tag);
            parser.Arguments.Add(_commentOnIssue);
            parser.Arguments.Add(_user);
            parser.Arguments.Add(_stateArgument);

            // check if first value is a digit, if so it must be a shortcut command; treat those specially
            if (int.TryParse(args[0], out int _))
            {
                // support for shortcut commands. these all start with the issue # upfront
                //      "issue.exe show 1" -> "issue.exe 1",
                //      "issue.exe edit 1 tag:foo" -> "issue.exe 1 tag:foo",
                //      "issue.exe comment 1 -m "comment"" -> "issue.exe 1 -m "comment""
                // since there is only 3 types, check for them manually
                var newArgs = new string[args.Length + 1];
                Array.Copy(args, 0, newArgs, 1, args.Length);

                // since we only support a limited set of arguments, this basic check will be enough for now

                // message has shortform '-m', so check for any arg starting with m
                var hasComment = args.Any(a => a.TrimStart('-', '/').StartsWith("m"));
                var hasTag = args.Any(a => a.TrimStart('-', '/').StartsWith("tag"));
                if (hasComment)
                {
                    newArgs[0] = "comment";
                }
                else if (hasTag)
                {
                    newArgs[0] = "edit";
                }
                else
                {
                    newArgs[0] = "show";
                }
                args = newArgs;
            }
            args = ConvertArgumentsIntoAcceptableFormat(args, parser.Arguments.ToArray(), 1);
            try
            {
                // we asserted that there is at least one argument
                var firstArg = args[0].ToLower();
                if (IsHelp(firstArg) || !_allowedArguments.ContainsKey(firstArg))
                {
                    DisplayHelp(parser);
                }
                else if (firstArg == "init")
                {
                    if (args.Length != 1)
                    {
                        // no further args required
                        Console.WriteLine("Invalid argument for 'init'");
                        DisplayHelp(parser);
                        return;
                    }
                    issueTracker.InitializeNewProject();
                }
                else
                {
                    // all other arguments require a issue tracker to exist
                    if (!issueTracker.WorkingDirectoryIsIssueTracker)
                    {
                        Console.WriteLine("Command must run in an existing issue tracker directory.");
                        DisplayHelp(parser);
                        return;
                    }
                    if (firstArg == "list")
                    {
                        // requires filter list as further arguments
                        ProcessListCommand(args, parser, issueTracker);
                    }
                    else if (firstArg == "add")
                    {
                        ProcessAddCommand(args, parser, issueTracker);
                    }
                    else
                    {
                        ProcessCommandsWithIssueId(args, parser, issueTracker);
                    }
                }
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Fixes arguments so they pass the commandline libraries arbitrary specification
        /// It wants --tag=foo or /tag=foo but it cannot be tag=foo (no prefix) or --tag:foo (: seperator)
        /// At least it loads --tag foo.
        /// So this adds the prefix where necessary and replaces : with =. It also doesn't modify values such as foo in "--tag foo" if "tag" is one of the supported arguments (checks both long and short form).
        /// additionally I really like the "issues open tag:foo" syntax but that isn't supported either
        /// so instead it has to be "issues --state=open --tag:foo"
        /// -> if we detect any of the state values, prepend with state= as well
        /// </summary>
        /// <param name="args"></param>
        /// <param name="supportedArgs"></param>
        /// <param name="skipProcessingForFirstNValues">Optional. Allows skipping processing values from the start. Affected values will be copied literally.</param>
        private static string[] ConvertArgumentsIntoAcceptableFormat(string[] args, Argument[] supportedArgs, int skipProcessingForFirstNValues = 0)
        {
            var newArgs = new string[args.Length];
            var states = _stateArgument.AllowedValues.ToList();
            bool lastValueWasArgumentRequiringUserValue = false;
            for (int i = 0; i < args.Length; i++)
            {
                // just copy the skip values
                // also copy the values when the lastValueWasArgumentRequiringUserValue is true, it indicates e.g. "-t", "my message" -> prevents "my message" from being appended with "--"
                if (i < skipProcessingForFirstNValues || lastValueWasArgumentRequiringUserValue)
                {
                    newArgs[i] = args[i];
                    lastValueWasArgumentRequiringUserValue = false;
                    continue;
                }
                var current = args[i].Replace(":", "=");

                if (states.Any(s => s.Equals(current, StringComparison.InvariantCultureIgnoreCase)))
                {
                    current = "state=" + current;
                }

                // check if we have any argument with a matching name and only then skip modifications
                var argumentNameOnly = current;
                if (argumentNameOnly.StartsWith("--"))
                    argumentNameOnly = argumentNameOnly.Substring(2);
                else if (argumentNameOnly.StartsWith("-") || argumentNameOnly.StartsWith("/"))
                    argumentNameOnly = argumentNameOnly.Substring(1);

                if (argumentNameOnly.Contains("="))
                    argumentNameOnly = argumentNameOnly.Substring(0, argumentNameOnly.IndexOf('='));

                // now without the prefix and possible value check match any of the user providable arguments
                var match = supportedArgs.FirstOrDefault(
                    a => argumentNameOnly.Equals(a.LongName, StringComparison.InvariantCultureIgnoreCase) ||
                         (a.ShortName.HasValue && argumentNameOnly[0] == a.ShortName.Value));

                // only modify if we did find a matching command, otherwise assume its some user value
                if (match != null && (!current.StartsWith("-") && !current.StartsWith("/")))
                {
                    // figure out whether its a single char command or a long one
                    if (current.Length == 1 || current[1] == '=')
                    {
                        // one char or second char is seperator. must be a short arg
                        current = "-" + current;
                    }
                    else
                    {
                        current = "--" + current;
                    }
                }
                newArgs[i] = current;

                if (!current.Contains("=") &&
                    (current.StartsWith("-") || current.StartsWith("/")))
                {
                    // special case, if the last value was a switch that requires a value (e.g. "-t" "my message")
                    // and the value wasn't integrated (e.g. "-t=my message") then we need to skip the next value, otherwise we end up with "-t" "--my message".

                    if (match != null)
                    {
                        // since we found a match, we need to check whether the provided argument is a switch or not
                        // if it is a switch it means it doesn't require a value
                        // all other arguments should require a value
                        var t = match.GetType();
                        // fickle logic: this might break if someone decides to create another argument not of type switch and makes it implement switch
                        // but who would do such a retarded thing?
                        if (t == typeof(SwitchArgument) || t.IsSubclassOf(typeof(SwitchArgument)))
                        {
                        }
                        else
                        {
                            // not a switch so it must be a value argument (tm)
                            lastValueWasArgumentRequiringUserValue = true;
                        }
                    }
                }
            }
            return newArgs.ToArray();
        }


        /// <summary>
        /// Returns whether the provided argument is any of the possible help arguments.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static bool IsHelp(string arg)
        {
            switch (arg)
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
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Parses the command line for the add command structure "add -t "content" [-m "message"] [tags:foo,bar]
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parser"></param>
        /// <param name="issueTracker"></param>
        private static void ProcessAddCommand(string[] args, CommandLineParser.CommandLineParser parser, IssueTracker issueTracker)
        {
            if (args.Length == 0 || !"add".Equals(args[0], StringComparison.InvariantCultureIgnoreCase))
                throw new NotSupportedException("First arg must be 'add'");

            // add doesn't require an id
            // parse all except the ones we manually processed
            parser.ParseCommandLine(args.Skip(1).ToArray());
            // ensure the user didn't call bullshit on the commandline
            // e.g. "add user:name -title "foobar"
            AssertNoWrongArgsWhereParsed(parser, args[0]);

            if (!_addTitle.Parsed)
                throw new CommandLineException("Title is required for adding a new issue!");

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
            AssertNoWrongArgsWhereParsed(parser, args[0]);

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
                var v = _stateArgument.Value.ToLower();
                if ("all".Equals(v))
                {
                    // no filter
                }
                else if ("open".Equals(v))
                {
                    filters.Add(new FilterValue(Filter.IssueState, IssueState.Open));
                }
                else if ("closed".Equals(v))
                {
                    filters.Add(new FilterValue(Filter.IssueState, IssueState.Closed));
                }
            }
            else
            {
                // default to open issues by default
                filters.Add(new FilterValue(Filter.IssueState, IssueState.Open));
            }

            issueTracker.ListIssues(filters);
        }

        /// <summary>
        /// Processes all commands that need an issue id as second argument.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parser"></param>
        /// <param name="issueTracker"></param>
        private static void ProcessCommandsWithIssueId(string[] args, CommandLineParser.CommandLineParser parser, IssueTracker issueTracker)
        {
            if (!_commandsWithIssueIdRequiurement.Contains(args[0].ToLower()))
                throw new CommandLineException($"Command {args[0]} is not supported!");

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
            // skip command name and issueId
            parser.ParseCommandLine(args.Skip(2).ToArray());

            AssertNoWrongArgsWhereParsed(parser, args[0]);

            // now that correct usage is determined, figure out which command we actually used
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
                default:
                    throw new CommandLineException($"Argument '{args[0]}' is not yet implemented!");
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
        /// Ensures that only valid second class arguments are parsed for the given first class argument.
        /// If any argument was parsed that wasn't part of the allowedArgs an exception is thrown.
        /// Also ensures that no values are unparsed  (e.g definitely-not-a-command=foobar).
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="firstArgument"></param>
        private static void AssertNoWrongArgsWhereParsed(CommandLineParser.CommandLineParser parser, string firstArgument)
        {
            var name = firstArgument.ToLower();
            if (!_allowedArguments.ContainsKey(name))
                throw new CommandLineException("The command isn't registered!");

            var allowedArgs = _allowedArguments[name];
            var allArgs = parser.Arguments;

            var parsed = allArgs.Where(a => a.Parsed).ToList();
            var unsupported = parsed.Where(p => !allowedArgs.Contains(p)).ToList();
            if (unsupported.Any())
            {
                throw new CommandLineException($"Invalid command format. Command{(unsupported.Count > 1 ? "s" : "")} " +
                                               $"'{string.Join(", ", unsupported.Select(p => "--" + p.LongName))}' " +
                                               $"not supported with '{firstArgument}' Use 'help' for more information.");
            }
            if (parser.AdditionalArgumentsSettings.AdditionalArguments.Any())
            {
                throw new CommandLineException($"Found unsupported arguments: {string.Join(", ", parser.AdditionalArgumentsSettings.AdditionalArguments)}");
            }
        }

        private static void DisplayHelp(CommandLineParser.CommandLineParser parser)
        {
            var lines = new List<string[]>();
            foreach (var arg in _allowedArguments)
            {
                lines.Add(new[]
                {
                    arg.Key,
                    _commandsWithIssueIdRequiurement.Contains(arg.Key) ? "id":"",
                    string.Join(", ", arg.Value.Select(a => "--" + a.LongName))
                });
            }
            Console.WriteLine("Supported commands:");

            Console.WriteLine(ConsoleFormatter.PadElementsInLines(lines));

            Console.WriteLine();
            Console.WriteLine("Optional arguments:");
            // these commands are second class only
            parser.ShowUsage();
        }
    }
}