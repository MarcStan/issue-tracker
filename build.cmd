msbuild IssueTracker.sln /t:Build /p:Configuration=Release /nr:false

echo f|xcopy IssueTracker\bin\Release\IssueTracker.exe !Release\issue.exe