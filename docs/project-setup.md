# Project Setup

## Requirements
- git
- Visual Studio

## Downloading repository to your local computer

### 🍴 Create a fork

First things first we need to create a fork of the SaveIT repository `TerrestrialSoft/SaveIT`.

Forks allows us to propose changes and after that create a pull request to submit changes to an original repository.

1. Click on the button **Fork** located in the upper right corner under the page header with your profile picture.
1. Deselect **Copy the `main` branch only** option
1. Click **Create fork** 
    - This will create a copy of SaveIT repository in your profile.

So now you have a remote copy of `SaveIT` repository.

> More information about forking a repository in [Documentation](https://docs.github.com/en/get-started/quickstart/fork-a-repo#forking-a-repository)

### 👥 Cloning the Fork

For now the fork only exists remotely. We need to download it (clone it) in order to be able to work on it on our computer.

1. Navigate to your forked repository
1. Click on **Code** button located next to the **About section** in the right.
1. Select one of the options to clone. (SSH is highly preferable option. More about SSH cloning [here](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/adding-a-new-ssh-key-to-your-github-account))
1. Copy the the link to your remote repository
1. Open terminal on your local machine
1. Navigate to a folder where the project should be cloned to
1. type `git clone` and paste your **repository link**
    - For example: `git clone git@github.com:YourName/SaveIT.git`

> For more information about cloning the forked repository, please visit the [Documentation](https://docs.github.com/en/get-started/quickstart/fork-a-repo#cloning-your-forked-repository).


### 🔛 Syncing the fork

This is the most important step for every forked repository. Remember the forked repository is only the copy of the main repository. There are no automatic branch pulls so your fork won't contain newest changes unless you pull them from the original repository. This can be done in following steps

1. Navigate to the root folder of your cloned repository using command line
    - To step into folder use command `cd folderName`, to step out of the folder use `cd ..`.
1. Verify you are located in a git repository using command: `git status`.
    - Expected output should start with: `On branch ...`
1. Print your current remotes using command `git remote -v`
    - Your output should like something like this:
    ```git
    origin  git@github.com:USERNAME/YOUR_FORK_NAME.git (fetch)
    origin  git@github.com:USERNAME/YOUR_FORK_NAME.git (push)
    ```
1. Add new upstream using command `git remote add`. This is used to create a link between your fork and the original repository
    - `git remote add upstream https://github.com/ORIGINAL_OWNER/SaveIT.git`
    - Steps to obtain above url
        1. Navigate to the original repository page
        1. Click **Code** button
        1. Select **HTTPS**
        1. Copy the link
1. Perform synchronization using `git pull upstream/main`

> For mote information about syncing a fork visit the [Documentation](https://docs.github.com/en/get-started/quickstart/fork-a-repo#configuring-git-to-sync-your-fork-with-the-original-repository)

❗❗ **From now on, everytime you want to create a new branch make sure you pull the newest state of original repository so you avoid unnecessary git conflicts.** ❗❗


## Preparing the Development environment

1. Download Visual Studio 2022 [here](https://visualstudio.microsoft.com/)
    - It comes in three editions. The Community (free) edition will work perfectly for this project.
1. Open Visual Studio installer and install Visual Studio
    - Recommended modules:
        - ASP.NET Development
        - .NET Multi-Platform App UI
        - Data storage and processing
    - Recommended components (section Individual Components)
        - Git for Windows
1. Click on Install

## Windows setup

In order to develop any Windows MAUI application (as well as SaveIT Windows application) it is required to enable Windows Developer Mode. This policy is was set by Microsoft and the Developer mode allows installing application from unknown sources.

*This setting doesn't affect your security as long as you don't download files from unknown sources and install them.*

1. Open Windows Settings
1. Developers
1. Developer Mode section
1. Enable