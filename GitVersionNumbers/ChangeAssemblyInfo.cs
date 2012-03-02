//-----------------------------------------------------------------------
// <copyright file="ChangeAssemblyInfo.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
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
                ////throw new Exception("Did not send project path or git info to UpdateAssembly");
            }

            string outputFile = string.Empty; // ouput file
            string inputFile = string.Empty; // input file
            string backupFile = string.Empty; // backup file name
            string inputContents = string.Empty; // input file contents
            
            if (!File.Exists(this.ProjectPath))
            {
                Console.WriteLine("Project Does Not Exist " + this.ProjectPath);
                return;
                ////throw new Exception("Project Does Not Exist " + this.ProjectPath);
            }

            inputFile = this.ProjectPath + ".git";
            outputFile = this.ProjectPath;
            backupFile = this.ProjectPath + ".bak";
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
            Console.WriteLine("     GITMODS         = " + this.GitInfo.Modifications);

            string pattern = @"\$GITMODS\?(.+):(.+)\$";
            MatchCollection matches = Regex.Matches(inputContents, pattern);
            foreach (Match match in matches)
            {
                inputContents = inputContents.Replace(
                    match.Value,
                    this.GitInfo.Modifications ? match.Groups[1].Value : match.Groups[2].Value);
            }

            inputContents = inputContents.Replace("$GITBRANCHNAME$", this.GitInfo.BranchName.Trim());
            inputContents = inputContents.Replace("$GITCOMMITDATE$", this.GitInfo.LastCommitDate.ToShortDateString());
            inputContents = inputContents.Replace("$GITHASH$", this.GitInfo.LastCommitHash.Trim());
            inputContents = inputContents.Replace("$GITVERSION$", this.GitInfo.Version.Trim());
            inputContents = inputContents.Replace("$GITCOMMITNUMBER$", this.GitInfo.CommitNumber.Trim());

            // write out the changes
            try
            {
                File.WriteAllText(outputFile, inputContents);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can Not Write Output to " + outputFile);
                Console.WriteLine(ex.ToString());
                return;
                ////throw new Exception("Can Not Write Output to " + outputFile, ex);
            }
        }

        #endregion

        #region private methods
        #endregion
    }
}
