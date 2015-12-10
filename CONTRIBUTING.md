# Development/contribution Process

Our development process is based on [GitHub Flow](http://scottchacon.com/2011/08/31/github-flow.html) (highly recommended read if you're not familiar with it).  Merging to the master branch requires approval by **@pandell/developers**.

Typical steps:

0. Clone the repo and create a branch. 
Commit all of your changes to this branch. 
Commit messages should follow [these guidelines](https://github.com/erlang/otp/wiki/Writing-good-commit-messages). 
Each separate bug or feature you implement should be in a separate branch. 
While you are working on a change, the corresponding issue in GitHub should be moved to the `in progress` label.

0. Make sure you add tests:
    - _Unit tests_ are small and fast tests that have no external dependencies (preferred)

0. Make sure the Build and Test are successful for your branch.

0. Push your branch to origin. 
Submit a pull request on GitHub.com, clearly describing your change and apply the `in review` label and removing the `in progress` label. 
**If your change has an associated issue, please convert the issue to a pull request instead of creating a new pull request** - this can be done with the [hub](https://hub.github.com/) command line tool by executing the command (as long as you’ve pushed your changes and your local branch is tracking your remote) `hub.exe pull-request -i %issue_number%` in your project directory.

0. Wait for peer review and address any issues raised by reviewers.
    - To emphasize the goal of reviews (to make better code), commits addressing review comments can thank the reviewer for their help.  E.g.

    ```
    Added forgotten thingy

    - Thanks, @guywhokeepsfindingproblemswithmycode!
    ```

0. Wait for approval of the code.

0. Rebase your branch on `master`

0. Merge your branch to `master`

0. Delete the branch (both locally and on GitHub server).
