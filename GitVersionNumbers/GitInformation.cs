//-----------------------------------------------------------------------
// <copyright file="GitInformation.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;

    /// <summary>
    /// Git Information Class
    /// </summary>
    public class GitInformation
    {
        /// <summary>
        /// Gets or sets the Last commitment hash or sha1 value
        /// </summary>
        public string LastCommitHash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Date of the last commitment
        /// </summary>
        public DateTime LastCommitDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version number
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the commit number.
        /// </summary>
        public string CommitNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the git branch name.
        /// </summary>
        public string BranchName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the repository has modifications.
        /// </summary>
        public bool HasModifications
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of added files.
        /// </summary>
        public int Added
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of missing files.
        /// </summary>
        public int Missing
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of modified files.
        /// </summary>
        public int Modified
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of removed files.
        /// </summary>
        public int Removed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of stages files.
        /// </summary>
        public int Staged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of changes (added, missing, modified, removed, staged).
        /// </summary>
        public int Changes
        {
            get;
            set;
        }
    }
}
