# ReproMachine

Automate some common manual steps when dealing with bug reproductions on Windows.

## Usage

- If you don't already have one, create a folder called `C:\repro`
- Run the console app
- Drag and drop a repro `.zip` into `C:\repro`

## What this does

- Extracts the `.zip` 
- Cleans up any duplicate [macOS folders](https://superuser.com/questions/104500/what-is-macosx-folder)
- Strips out intermediate empty folders
- Opens the solution file

By "Strips out intermediate empty folders", we're talking about repros which have a folder structure like this when unzipped:

MyRepro
    - MyRepro
        - MyRepro.sln
        - MyRepro
        - MyRepro.iOS
        - MyRepro.UWP
        - MyRepro.Droid
        
For a structure like this, the top level MyRepro folder will be removed. This makes "path too long" errors from Java less likely. 

For the "Opens the solution" step, it will automatically open the solution file if there's only a single solution file in the project folders. If there are multiple solution files, it will print a message to that effect in the console window and you'll have to figure out on your own which one to open.

## Stuff I might add

- Automatically editing the solution configuration to check the `Deploy` box for Android
- Warning you if you're still likely to get "path too long" errors 
