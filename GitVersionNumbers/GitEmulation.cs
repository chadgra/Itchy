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
            this.GitLog();
            this.GitDescribe();
            this.GitBranch();
            return this.gi;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Get the last log entrys shaw number and date submitted
        /// </summary>
        private void GitLog()
        {
            // get the log entry
            // git log -n1 --pretty=raw
            // -n1 only do the last log entry
            // pretty is used to determine the format
            string info = this.ExecuteCommand("git log -n1 --pretty=\"%H | %ad \" --date=rfc");
            string[] infoSplit = info.Split(new char[] { '|', '-', ',', '\n' });
            if (infoSplit.Count() >= 3)
            {
                this.gi.LastCommitHash = infoSplit[0];
                DateTime dt;
                if (DateTime.TryParse(infoSplit[2], out dt))
                {
                    this.gi.LastCommitDate = dt;
                }
            }
        }

        /// <summary>
        /// get the version number from describe
        /// </summary>
        private void GitDescribe()
        {
            // for info on how to setup describe see git tag -a
            string info = this.ExecuteCommand("git describe --tags");
            string[] infoSplit = info.Split(new char[] { '|', '-', ',', '\n' });
            if (infoSplit.Count() >= 2)
            {
                this.gi.Version = infoSplit[0].ToString() + "." + infoSplit[1].ToString();
            }
        }

        /// <summary>
        /// get the name of the branch the project is using
        /// </summary>
        private void GitBranch()
        {
            string info = this.ExecuteCommand("git branch");
            string[] infoSplit = info.Split(new char[] { '|', '-', ',', '\n' });
            foreach (string str in infoSplit)
            {
                if (str.Contains("*"))
                {
                    this.gi.BranchName = str.Replace("*", string.Empty).Trim();
                }
            }
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
            return output;
        }

        #endregion
    }
}