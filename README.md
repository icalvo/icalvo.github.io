# SWGen Tools

+ `build.ps1`: Builds the site at `src\CommandLine\_public`. Alternatively, open the solution with Rider/VS and run the `CommandLine` project using its Launch Profile.
+ `serve.ps1`: Runs the site with watch so that if you rebuild it will be refreshed in the browser.

# Music Tools

+ `generateBackground.ps1 -work <WORK_NAME>`: Generates background image for a work
+ `commitWork.ps1 -work <WORK_NAME>`: Commits a work to the GitHub Pages repo. It DOESN'T push.
+ `generateDefaultBackground.ps1`: Generates the default background
+ `publishWork.ps1 -work <WORK_NAME> [-skipPendingCommitsCheck]`: Calls:
    - `commitWork.ps1 <WORK_NAME>`
    - And pushes to git, **so eventually the files will be publicly available** after GitHub processes the push.
+ `rollbackWork.ps1 -work <WORK_NAME>`: Undoes a `copyWork.ps1` call by resetting to the HEAD of the branch on the work's folder.

# Normal flow
After editing a score and have it ready to be published, the required steps are:

- Export the PDF file, either:
  - Full score and parts in a single file to: `<WORK_NAME>_full_parts.pdf`
  - Full score only to: `<WORK_NAME>_full.pdf`
- Export the MP3 file to: `<WORK_NAME>.mp3`
- Edit the `index.cshtml` file to add a changelog line.
- Execute `tools/generateBackground.ps1 -work <WORK_NAME>` to (re)generate the background image.
- Execute `tools/build.ps1` to generate the site.
- Execute `tools/serve.ps1` to preview the site.
- Execute `tools/publishWork.ps1 <WORK_NAME>`.
