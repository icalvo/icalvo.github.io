# Development

There is a Development Container configuration for Visual Studio Code. If you open that container you can just run the site with Ctrl+P -> `task serve`.

# Tools

+ `copyWork.ps1 -work <WORK_NAME>`: Copies the latest version of a work to the site (all stuff except Sibelius files)
+ `generateBackground.ps1 -work <WORK_NAME>`: Generates background image for a work
+ `commitWork.ps1 -work <WORK_NAME>`: Commits a work to the GitHub Pages repo. It DOESN'T push.
+ `generateDefaultBackground.ps1`: Generates the default background
+ `publishWork.ps1 -work <WORK_NAME> [-skipPendingCommitsCheck]`: Calls:
    - `copyWork.ps1`
    - `generateBackground.ps1`
    - `commitWork.ps1`
    - And pushes to git, **so eventually the files will be publicly available** after GitHub processes the push.
+ `rollbackWork.ps1 -work <WORK_NAME>`: Undoes a `copyWork.ps1` call by resetting to the HEAD of the branch on the work's folder.

# Normal flow
After editing a score and have it ready to be published, the required steps are:

- Export the PDF file (full score and parts in a single file) to: `<WORK_NAME>_full_parts.pdf`
- Export the MP3 file to: `<WORK_NAME>.mp3`
- Edit the `index.markdown` file to add a changelog line (you can skip this since the publish script will ask you for this line if it is not present.)
- Execute `tools/publishWork.ps1 <WORK_NAME>`.
