@echo off
echo ===============================================
echo 🗑️  COMPLETE GIT CLEANUP FOR UNITY PROJECT
echo ===============================================
echo.

echo ⚠️  WARNING: This will completely remove all Git files and history!
echo    This action cannot be undone.
echo.
set /p confirm="Are you sure you want to proceed? (y/N): "
if /i not "%confirm%"=="y" (
    echo ❌ Operation cancelled by user
    pause
    exit /b 0
)

echo.
echo 📋 Starting complete Git cleanup...
echo.

REM Remove Git directory
if exist ".git" (
    echo 🗑️  Removing .git directory...
    rmdir /s /q ".git"
    if %errorlevel% neq 0 (
        echo ❌ Failed to remove .git directory
        echo    You may need to run this script as Administrator
        pause
        exit /b 1
    )
    echo ✅ .git directory removed
) else (
    echo ℹ️  No .git directory found
)

REM Remove Git-related files
echo.
echo 🗑️  Removing Git-related files...

if exist ".gitignore" (
    del ".gitignore"
    echo ✅ .gitignore removed
)

if exist ".gitattributes" (
    del ".gitattributes"
    echo ✅ .gitattributes removed
)

if exist ".gitmodules" (
    del ".gitmodules"
    echo ✅ .gitmodules removed
)

REM Remove Visual Studio Git files
if exist ".vs" (
    echo 🗑️  Removing Visual Studio files...
    rmdir /s /q ".vs"
    echo ✅ .vs directory removed
)

REM Remove any Git LFS files
if exist ".git" (
    echo 🗑️  Removing any remaining Git files...
    rmdir /s /q ".git"
)

REM Clean up any Git-related batch files
if exist "SetupGit.bat" (
    del "SetupGit.bat"
    echo ✅ SetupGit.bat removed
)

echo.
echo ===============================================
echo ✅ GIT CLEANUP COMPLETED SUCCESSFULLY!
echo ===============================================
echo.
echo 🎯 Your Unity project is now completely clean of Git files
echo.
echo 📋 What was removed:
echo    ✅ .git directory (all Git history)
echo    ✅ .gitignore file
echo    ✅ .gitattributes file
echo    ✅ .gitmodules file
echo    ✅ .vs directory (Visual Studio files)
echo    ✅ Git setup scripts
echo.
echo 🚀 You can now start fresh with Git if needed!
echo.
echo 💡 To start a new Git repository later, run:
echo    git init
echo    git add .
echo    git commit -m "Initial commit"
echo.
pause
