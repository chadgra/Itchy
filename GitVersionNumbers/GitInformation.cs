//-----------------------------------------------------------------------
// <copyright file="GitInformation.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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
        /// Gets or sets the git branch name
        /// </summary>
        public string BranchName
        {
            get;
            set;
        }

        public bool Modifications
        {
            get;
            set;
        }
    }
}
