//-----------------------------------------------------------------------
// <copyright file="GitEmulation.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using LibGit2Sharp;

    /// <summary>
    /// Get the information from GIT
    /// </summary>
    public class GitEmulation
    {
        #region Fields

        /// <summary>
        /// git information structure
        /// </summary>
        private readonly GitInformation gitInfo = new GitInformation();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the GitEmulation class.
        /// </summary>
        public GitEmulation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the GitEmulation class.
        /// </summary>
        /// <param name="solutionDirectory">Full path name to the project directory</param>
        public GitEmulation(string solutionDirectory)
        {
            this.SolutionDirectory = solutionDirectory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the project directory to process
        /// </summary>
        public string SolutionDirectory
        {
            get;
            set;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Load the Git Information
        /// </summary>
        /// <returns>Git Information values</returns>
        public GitInformation GitInfo()
        {
            using (var repo = new Repository(this.SolutionDirectory))
            {
                this.GitLog(repo);
                this.GitDescribe(repo);
                this.GitBranch(repo);
                this.GitModifications(repo);
                return this.gitInfo;
            }
        }

        /// <summary>
        /// Get the last log entrys shaw number and date submitted
        /// </summary>
        private void GitLog(IRepository repo)
        {
            this.gitInfo.LastCommitHash = repo.Head.Tip.Sha;
            this.gitInfo.LastCommitDate = repo.Head.Tip.Committer.When.LocalDateTime;
        }

        /// <summary>
        /// Get the version number from describe
        /// </summary>
        private void GitDescribe(IRepository repo)
        {
            var commits = repo.Head.Commits.ToList();
            var tags = repo.Tags;

            this.gitInfo.Version = "0.0.0" + (commits.Count - 1).ToString(CultureInfo.InvariantCulture);

            var versionTags = tags.Where(t => Regex.IsMatch(t.FriendlyName, @"^\d+\.\d+\.\d+$")).ToList();
            var newVersionTags = tags.Where(t => Regex.IsMatch(t.FriendlyName, @"^\d+\.\d+$")).ToList();
            var steps = 0;
            if (versionTags.Any() || newVersionTags.Any())
            {
                foreach (var commit in commits)
                {
                    var tag = versionTags.FirstOrDefault(t => t.Target.Sha == commit.Sha);
                    if (null != tag)
                    {
                        this.gitInfo.Version = tag.FriendlyName + "." + steps.ToString(CultureInfo.InvariantCulture);
                        break;
                    }

                    var newTag = newVersionTags.FirstOrDefault(t => t.Target.Sha == commit.Sha);
                    if (null != newTag)
                    {
                        this.gitInfo.Version = newTag.FriendlyName + "." + steps.ToString(CultureInfo.InvariantCulture);
                        break;
                    }

                    steps++;
                }
            }

            versionTags = tags.Where(t => Regex.IsMatch(t.FriendlyName, @"^CN.*$")).ToList();
            steps = 0;
            if (versionTags.Any())
            {
                foreach (var commit in commits)
                {
                    var tag = versionTags.FirstOrDefault(t => t.Target.Sha == commit.Sha);
                    if (null != tag)
                    {
                        this.gitInfo.CommitNumber =
                            (int.Parse(tag.FriendlyName.Replace("CN", "")) + steps).ToString(CultureInfo.InvariantCulture);
                        break;
                    }

                    steps++;
                }
            }

            if (string.IsNullOrEmpty(this.gitInfo.CommitNumber))
            {
                this.gitInfo.CommitNumber = commits.Count.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// get the name of the branch the project is using
        /// </summary>
        private void GitBranch(IRepository repo)
        {
            this.gitInfo.BranchName = repo.Head.FriendlyName;
        }

        /// <summary>
        /// Get a value indicating if there are modifications in the current working directory.
        /// </summary>
        private void GitModifications(IRepository repo)
        {
            var status = repo.RetrieveStatus(new StatusOptions());
            this.gitInfo.Added = status.Added.Count();
            this.gitInfo.Missing = status.Missing.Count();
            this.gitInfo.Modified = status.Modified.Count();
            this.gitInfo.Removed = status.Removed.Count();
            this.gitInfo.Staged = status.Staged.Count();
            this.gitInfo.Changes = this.gitInfo.Added + this.gitInfo.Missing + this.gitInfo.Modified
                                   + this.gitInfo.Removed + this.gitInfo.Staged;
            this.gitInfo.HasModifications = Convert.ToBoolean(this.gitInfo.Changes);
        }

        #endregion
    }
}