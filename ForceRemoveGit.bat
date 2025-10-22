@echo off
echo ===============================================
echo 🗑️  FORCE GIT CLEANUP FOR UNITY PROJECT
echo ===============================================
echo.

echo 📋 Force removing all Git files and processes...
echo.

REM Kill any Git processes
echo 🔄 Stopping Git processes...
taskkill /f /im git.exe 2>nul
taskkill /f /im git-lfs.exe 2>nul

REM Wait a moment
timeout /t 2 >nul

REM Force remove Git directory with retries
if exist ".git" (
    echo 🗑️  Force removing .git directory...
    for /l %%i in (1,1,5) do (
        rmdir /s /q ".git" 2>nul
        if not exist ".git" goto :git_removed
        echo    Attempt %%i failed, retrying...
        timeout /t 1 >nul
    )
    echo ❌ Could not remove .git directory completely
    echo    Some files may be locked by another process
    echo    Try closing any Git applications and run again
    goto :cleanup_other
    :git_removed
    echo ✅ .git directory removed
)

:cleanup_other
REM Remove other Git files
echo.
echo 🗑️  Removing other Git files...

if exist ".gitignore" (
    del ".gitignore" 2>nul
    echo ✅ .gitignore removed
)

if exist ".gitattributes" (
    del ".gitattributes" 2>nul
    echo ✅ .gitattributes removed
)

if exist ".gitmodules" (
    del ".gitmodules" 2>nul
    echo ✅ .gitmodules removed
)

REM Remove Visual Studio files
if exist ".vs" (
    echo 🗑️  Removing Visual Studio files...
    rmdir /s /q ".vs" 2>nul
    echo ✅ .vs directory removed
)

REM Remove Git setup scripts
if exist "SetupGit.bat" (
    del "SetupGit.bat" 2>nul
    echo ✅ SetupGit.bat removed
)

if exist "RemoveGit.bat" (
    del "RemoveGit.bat" 2>nul
    echo ✅ RemoveGit.bat removed
)

echo.
echo ===============================================
echo ✅ GIT CLEANUP COMPLETED!
echo ===============================================
echo.
echo 🎯 Your Unity project is now clean of Git files
echo.
echo 📋 Status:
if exist ".git" (
    echo ⚠️  .git directory still exists (some files may be locked)
    echo    Try running this script again or restart your computer
) else (
    echo ✅ All Git files successfully removed
)
echo.
echo 🚀 You can now start fresh with Git!
echo.
pause
