# Development/contribution Process

Our development process is based on [GitHub Flow](http://scottchacon.com/2011/08/31/github-flow.html) (highly recommended read if you're not familiar with it).  Merging to the master branch requires approval by **@pandell/developers**; other projects may have different approvers.

Typical steps:

0. Create a private branch. Commit all of your changes to this branch.  Commit messages should follow [these guidelines](https://github.com/pandell/SamplePliWeb/wiki/Writing-good-commit-messages).  Each separate bug or feature you implement should be in a separate branch.  While you are working on a change, the corresponding issue in GitHub should be moved to the `in progress` label.

0. Make sure you add tests for both client-side and server-side code:
    - For client-side code:
         - _Unit tests_ are small and fast tests that do not require a browser environment (preferred)
         - _Integration tests_ should be used to test code that depends on a browser environment (uses `window`, `DOM`, etc)
    - For server-side code:
         - _Unit tests_ are small and fast tests that have no external dependencies (preferred)
         - _Integration tests_ exercise larger parts of the application and have external dependencies (typically SQL Server)

0. Make sure the Build and Test [build configurations](https://build.pandell.com/project.html?projectId=SamplePliWeb) on TeamCity continuous integration server are successful (green) for your branch.

0. Submit a pull request on GitHub.com, clearly describing your change and apply the `in review` label and removing the `in progress` label.  **If your change has an associated issue, please convert the issue to a pull request instead of creating a new pull request** - this can be done with the [hub](https://hub.github.com/) command line tool by executing the command (as long as you’ve pushed your changes and your local branch is tracking your remote) `hub.exe pull-request -i %issue_number%` in your project directory.

0. Wait for peer review and address any issues raised by reviewers.
    - All Pandell developers are required to review pull requests of their peers in their project and strongly encouraged to do the same in the shared Pli projects.
    - To emphasize the goal of reviews (to make better code), commits addressing review comments can thank the reviewer for their help.  E.g.

    ```
    Added forgotten thingy

    - Thanks, @guywhokeepsfindingproblemswithmycode!
    ```

0. Wait for approval of the code.

0. Rebase your private branch on `master`

0. Merge your private branch to `master`

0. Delete the private branch (both locally and on GitHub server).
