# Issue Tracker (CLI)

Commandline based issue tracker that uses the local filesystem for storage.

No need to setup a Linux/Apache/IIS server with MySQL/SQLite/etc, just use a local directory.

Works perfectly for single developers. Can also work for 2-3 developers if the directory is synced via git or other VCS.

**May** work with cloud sync (OneDrive, GDrive, Box, ..) for solo use. Afterall its all just textfiles.

# Download

Either clone the repo or [go to the tags](https://github.com/MarcStan/IssueTracker/tags) to download precompiled binaries.

# Getting started

msbuild.exe must be in system PATH in order to execute build.cmd (Alternatively just build from Visual Studio).

The msbuild path for VS2017 is "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin" (replace Community with Professional/Enterprise depending on the version you have).

For previous versions the path was: "C:\Program Files (x86)\MSBuild\14.0\Bin" (where 14.0 is VS2015).

Pick the correct path for msbuild and add it to the path by setting the environment variable (Start -> type "edit environment variables for your account").

# Usage

See [Spec.md](Spec.md) for all commands.

## Recommended layout

The default console is only 80 characters wide but the issue tracker works best with 120 characters.

Rightclick on the console titlebar -> Properties. Switch to "Layout" tab and set both "Window Size Width" and "Screen Buffer Size Width" to 120.

Enjoy a better console experience.

If you must absolutely insist on 80 characters, you can edit the titleTrim property of the ".issues" file to further decrease title width when listing all issues.

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

# Roadmap

* Add pretty UI wrapper
* (Possibly) plug into github/gitlab/tfs APIs to pull data and display locally (either cache offline + display or always query live)