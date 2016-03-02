//-----------------------------------------------------------------------
// <copyright file="ChangeAssemblyInfo.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// change the assembolyinfo.cs file
    /// </summary>
    public class ChangeAssemblyInfo
    {
        #region fields
        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the ChangeAssemblyInfo class.
        /// </summary>
        public ChangeAssemblyInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChangeAssemblyInfo class.
        /// </summary>
        /// <param name="projectPath">Directory name of the project</param>
        /// <param name="gitInfo">information from git</param>
        public ChangeAssemblyInfo(string projectPath, GitInformation gitInfo)
        {
            this.ProjectPath = projectPath;
            this.GitInfo = gitInfo;
        }

        #endregion

        #region properties
        
        /// <summary>
        /// Gets or sets the project path
        /// </summary>
        public string ProjectPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the GIT information
        /// </summary>
        public GitInformation GitInfo
        {
            get;
            set;
        }

        #endregion

        #region public methods
        
        /// <summary>
        /// update the assembly
        /// </summary>
        public void UpdateAssembly()
        {
            // do we have the values needed?
            if (string.IsNullOrEmpty(this.ProjectPath) || (this.GitInfo == null))
            {
                Console.WriteLine("Did not send project path or git info to UpdateAssembly");
                return;
            }

            string inputContents;
            string inputFile = this.ProjectPath + ".git";
            string outputFile = this.ProjectPath;
            string backupFile = this.ProjectPath + ".bak";

            if (!File.Exists(inputFile))
            {
                return;
            }

            // backup the existing file
            if (File.Exists(outputFile))
            {
                try
                {
                    if (File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                    }

                    File.Move(outputFile, backupFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can Not Create Backup File " + backupFile);
                    Console.WriteLine(ex.ToString());
                    return;
                    ////throw new Exception("Can Not Create Backup File " + backupFile, ex);
                }
            }

            // read the input file
            try
            {
                inputContents = File.ReadAllText(inputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can Not Read " + inputFile);
                Console.WriteLine(ex.ToString());
                return;
                ////throw new Exception("Can Not Read " + inputFile, ex);
            }

            // replace the tags
            Console.WriteLine("     GITBRANCHNAME   = " + this.GitInfo.BranchName.Trim());
            Console.WriteLine("     GITCOMMITDATE   = " + this.GitInfo.LastCommitDate.ToShortDateString());
            Console.WriteLine("     GITHASH         = " + this.GitInfo.LastCommitHash.Trim());
            Console.WriteLine("     GITVERSION      = " + this.GitInfo.Version.Trim());
            Console.WriteLine("     GITCOMMITNUMBER = " + this.GitInfo.CommitNumber.Trim());
            Console.WriteLine("     GITMODS         = " + this.GitInfo.HasModifications);
            Console.WriteLine("     GITMODCOUNT     = " + this.GitInfo.Changes);

            // ReSharper disable LoopCanBeConvertedToQuery
            const string ModsPattern = @"\$GITMODS\?(.*):(.*)\$";
            var matches = Regex.Matches(inputContents, ModsPattern);

            foreach (Match match in matches)
            {
                inputContents = inputContents.Replace(
                    match.Value,
                    this.GitInfo.HasModifications ? match.Groups[1].Value : match.Groups[2].Value);
            }

            const string HashPattern = @"\$GITHASH(\d+)\$";
            matches = Regex.Matches(inputContents, HashPattern);

            foreach (Match match in matches)
            {
                var length = int.Parse(match.Groups[1].Value);
                length = Math.Max(0, length);
                length = Math.Min(length, this.GitInfo.LastCommitHash.Trim().Length);
                inputContents = inputContents.Replace(
                    match.Value,
                    this.GitInfo.LastCommitHash.Trim().Substring(0, length));
            }

            // ReSharper restore LoopCanBeConvertedToQuery
            inputContents = inputContents.Replace("$GITBRANCHNAME$", this.GitInfo.BranchName.Trim());
            inputContents = inputContents.Replace("$GITCOMMITDATE$", this.GitInfo.LastCommitDate.ToShortDateString());
            inputContents = inputContents.Replace("$GITHASH$", this.GitInfo.LastCommitHash.Trim());
            inputContents = inputContents.Replace("$GITVERSION$", this.GitInfo.Version.Trim());
            inputContents = inputContents.Replace("$GITCOMMITNUMBER$", this.GitInfo.CommitNumber.Trim());
            inputContents = inputContents.Replace("$GITMODCOUNT$", this.GitInfo.Changes.ToString(CultureInfo.InvariantCulture));
            inputContents = inputContents.Replace("$GITBUILDDATE$", DateTime.Now.ToShortDateString());

            // write out the changes
            try
            {
                File.WriteAllText(outputFile, inputContents);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can Not Write Output to " + outputFile);
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region private methods
        #endregion
    }
}
