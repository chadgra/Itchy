//-----------------------------------------------------------------------
// <copyright file="GitEmulation.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using GitSharp;

    /// <summary>
    /// Get the information from GIT
    /// </summary>
    public class GitEmulation
    {
        #region globals

        /// <summary>
        /// git information structure
        /// </summary>
        private GitInformation gi = new GitInformation();

        #endregion

        #region constructors

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

        #region properties

        /// <summary>
        /// Gets or sets the project directory to process
        /// </summary>
        public string SolutionDirectory
        {
            get;
            set;
        }

        #endregion

        #region public methods
        
        /// <summary>
        /// Load the Git Information
        /// </summary>
        /// <returns>Git Information values</returns>
        public GitInformation GitInfo()
        {
            Git.DefaultGitDirectory = this.SolutionDirectory;
            Git.DefaultRepository = new Repository(this.SolutionDirectory);
            this.GitLog();
            this.GitDescribe();
            this.GitBranch();
            this.GitModifications();
            return this.gi;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Get the last log entrys shaw number and date submitted
        /// </summary>
        private void GitLog()
        {
            this.gi.LastCommitHash = Git.DefaultRepository.Head.CurrentCommit.Hash;
            this.gi.LastCommitDate = Git.DefaultRepository.Head.CurrentCommit.AuthorDate.DateTime;
        }

        /// <summary>
        /// get the version number from describe
        /// </summary>
        private void GitDescribe()
        {
            int totalCommitNumber = Git.DefaultRepository.Head.CurrentCommit.Ancestors.Count();

            this.gi.Version = "0.0.0." + totalCommitNumber.ToString();
            string info = this.ExecuteCommand("git describe --tags --long --match [0-9].[0-9].[0-9]");
            string[] infoSplit = info.Split(new char[] { '|', '-', ',', '\n' });
            if (infoSplit.Count() >= 2)
            {
                this.gi.Version = infoSplit[0].ToString() + "." + infoSplit[1].ToString();
            }

            info = this.ExecuteCommand("git describe --tags --long --match CN*");
            infoSplit = info.Split(new char[] { '|', '-', ',', '\n' });
            if (infoSplit.Count() >= 2)
            {
                int commitNumber = int.Parse(infoSplit[0].Replace("CN", "")) + int.Parse(infoSplit[1]);
                this.gi.CommitNumber = commitNumber.ToString();
            }
            else
            {
                this.gi.CommitNumber = totalCommitNumber.ToString();
            }
        }

        /// <summary>
        /// get the name of the branch the project is using
        /// </summary>
        private void GitBranch()
        {
            this.gi.BranchName = Git.DefaultRepository.CurrentBranch.Name;
        }

        /// <summary>
        /// Get a value indicating if there are modifications in the current working directory.
        /// </summary>
        private void GitModifications()
        {
            RepositoryStatus status = Git.DefaultRepository.Status;
            this.gi.Modifications = Convert.ToBoolean(status.Added.Count() + status.Missing.Count() + status.Modified.Count() +
                status.Removed.Count() + status.Staged.Count());
        }

        /// <summary>
        /// execute the git command and return the results
        /// </summary>
        /// <param name="command">command to execute</param>
        /// <returns>output stream as a string</returns>
        private string ExecuteCommand(string command)
        {
            if (!Directory.Exists(this.SolutionDirectory))
            {
                Console.WriteLine("Invalid Project Directory " + this.SolutionDirectory);
                return string.Empty;
                ////throw new Exception("Invalid Project Directory " + this.SolutionDirectory);
            }

            Process p = new Process();

            p.StartInfo.WorkingDirectory = this.SolutionDirectory;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c " + command;

            // Redirect the output stream of the child process.  
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();

            string output = p.StandardOutput.ReadToEnd();
            string[] splits = output.Split(new char[] { '\r', '\n' });
            output = string.Empty;
            foreach (string split in splits)
            {
                if (!split.StartsWith(@"C:\"))
                {
                    output += split + "\r\n";
                }
            }

            return output.Trim();
        }

        #endregion
    }
}