**1.1.1**

* made comment time relative to now instead of issue creation date
* issues are now correctly sorted by ID

**1.1.0**

Added header format when displaying single issue (better visibility where the issue starts).

Added support for short commands (show, comment and edit are supported).

No need to write "show 1":
> issue.exe 1

No need to write "comment 1 -m "message"":
> issue.exe 1 -m "my comment"

No need to write "edit 1 tag:+,-":
> issue.exe 1 tag:foo

**1.0.0**

Initial release