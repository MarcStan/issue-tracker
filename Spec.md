# Requirements

Currently pulls username from  %home%\\.gitconfig (checks for line name=). So git itself isn't needed but that file must exist.

# Special files

Issues are added as directories "#\<int:id>" to the current working directory.

The working directory must be an issue project (otherwise all commands except init will fail).

Init command will make the directory an issue project, by adding a ".issues" file.

## .issues file

The file serves as a marker and currently only contains "owner=\<name>". The name being pulled from .gitconfig.

# Usage

1. Put IssueTracker.exe in windows PATH or target directory. Rename it to whatever you want (issue, issues, it, tracker, ...). From here on out it is assumed the exe is called "issue.exe".
2. Run 
   > issue init

in an empty directory.
   
Sets up a new repository.

**All further issue commands must be issued within the same directory.**

3. Use it.

## Create new issue

> issue add -t "my first issue" -m "this is the body of the issue" [tag:feature]

Tagging is optional.

## Tag format

Tags can be added (either when creating or editing an issue) or removed (when editing an issue).

The tag format is tag:\<value> where value can either be a single tag name ("bug", "feature-request", ..) or a list of tags (seperator: ',').

> tag:bug,feature,foo,bar

This adds the four tags "bug", "feature", "foo" and "bar".

In order to remove tags, the tagname must start with "-".

Remove the bug tag from an issue. (Only possible when editing an issue). The tag must already exist on the issue in order for it to be removed.
> tag:-bug

Remove 2 and add 3 tags:
> tag:-foo,-bar,buzz,lorem,ipsum

## Tag limitation

No tag may start with "-" (as it indicates removal of a tag).

Tags may also not contain "," (considered seperator on the commandline).

## List issues

Show all open issues:
> issue list

### With filters

Optionally filters may be added:

- name:\<value>
- state:\<value> (either all|open|closed)
- tag:\<value> (one or more tags that must be on the issue)

Only created by user <my_name>:
> issue list user:<my_name>

Show only closed issues:
> issue list state:closed

Show only closed bugs:
> issue list tag:bug state:closed

## Show issue

Displays an issue with all its comments:
> issue show 1

## Comment on issue

> issue comment 1 -c "this is fixed in release v2.0"

Change tags of an issue (either add or remove):
> issue edit 1 tag:bug

Close the issue:
>issue close 1

Reopen it:
>issue reopen 1