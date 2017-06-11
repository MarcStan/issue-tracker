# Issue Tracker (CLI)

Commandline based issue tracker that uses the local filesystem for storage.

No need to setup a Linux/Apache/IIS server with MySQL/SQLite/etc, just use a local directory.

Works perfectly for single developers. Can also work for 2-3 developers if the directory is synced via git or other VCS.

**May** work with cloud sync (OneDrive, GDrive, Box, ..) for solo use. Afterall its all just textfiles.

# Usage

See [Spec.md](Spec.md) for all commands.

## Quickstart

Place the issue tracker exe somewhere, add it to windows PATH and name it the way you want (issue.exe, it.exe, tracker.exe, ...). For now it is assumed to be called "issue.exe".

(In an empty directory) run:

Initialize:
> issue init

Add issue:
> issue add -t "hello world" -m "my first bug report" tag:bug

List all (open) issues:
> issue list

Comment on isse:
>issue 1 comment -m "cannot reproduce"

Close issue:
> issue 1 close

# Changelog

See [Changelog.md](Changelog.md) for all versions/changes.