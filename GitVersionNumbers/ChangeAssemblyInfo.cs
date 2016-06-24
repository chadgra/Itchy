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
        #region Fields

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ChangeAssemblyInfo class.
        /// </summary>
        public ChangeAssemblyInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ChangeAssemblyInfo class.
        /// </summary>
        /// <param name="filePath">Directory name of the project</param>
        /// <param name="gitInfo">information from git</param>
        public ChangeAssemblyInfo(string filePath, GitInformation gitInfo)
        {
            this.FilePath = filePath;
            this.GitInfo = gitInfo;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string FilePath
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

        #region Public Methods

        /// <summary>
        /// Add the characters that make this a token to the beginning and end of the string.
        /// </summary>
        /// <param name="value">The string that should become a token.</param>
        /// <returns>The original string with characters that make it a token before and after.</returns>
        public static string AsToken(string value)
        {
            string keyCharacters = "[!@#$%^&*]";
            return keyCharacters + value + keyCharacters;
        }

        /// <summary>
        /// Update the assembly
        /// </summary>
        /// <param name="shouldRenameFile">True if the file name should be modified, instead of the file contents.</param>
        /// <param name="shouldCopyFile">True if the file name should be copied instead of moved (only valid if 'shouldRenameFile' is True.</param>
        public void UpdateAssembly(bool shouldRenameFile, bool shouldCopyFile)
        {
            // do we have the values needed?
            if (string.IsNullOrEmpty(this.FilePath) || (this.GitInfo == null))
            {
                Console.WriteLine("Did not send project path or git info to UpdateAssembly");
                return;
            }

            string inputContents;
            var inputFile = string.Empty;
            var outputFile = string.Empty;
            var backupFile = string.Empty;

            if (shouldRenameFile)
            {
                inputContents = this.FilePath;
            }
            else
            {
                inputFile = this.FilePath.EndsWith(".git") ? this.FilePath : this.FilePath + ".git";
                outputFile = inputFile.Remove(inputFile.Length - ".git".Length);
                backupFile = outputFile + ".bak";

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
                }
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
            // Look for GITMODS in input.
            string ModsPattern = AsToken("GITMODS\\?(.*):(.*)");
            var matches = Regex.Matches(inputContents, ModsPattern);

            foreach (Match match in matches)
            {
                inputContents = inputContents.Replace(
                    match.Value,
                    this.GitInfo.HasModifications ? match.Groups[1].Value : match.Groups[2].Value);
            }

            // Look for GITHASH in input.
            string HashPattern = AsToken("GITHASH(\\d+)");
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

            // Look for GITVERSION in input.
            string VersionPattern = AsToken("GITVERSION(.*?)");
            matches = Regex.Matches(inputContents, VersionPattern);

            foreach (Match match in matches)
            {
                var separator = match.Groups[1].Value;
                separator = string.IsNullOrEmpty(separator) ? "." : separator;
                inputContents = inputContents.Replace(
                    match.Value,
                    this.GitInfo.Version.Trim().Replace(".", separator));
            }

            // ReSharper restore LoopCanBeConvertedToQuery
            inputContents = Regex.Replace(inputContents, AsToken("GITBRANCHNAME"), this.GitInfo.BranchName.Trim());
            inputContents = Regex.Replace(inputContents, AsToken("GITCOMMITDATE"), this.GitInfo.LastCommitDate.ToShortDateString());
            inputContents = Regex.Replace(inputContents, AsToken("GITHASH"), this.GitInfo.LastCommitHash.Trim());
            inputContents = Regex.Replace(inputContents, AsToken("GITVERSION"), this.GitInfo.Version.Trim());
            inputContents = Regex.Replace(inputContents, AsToken("GITCOMMITNUMBER"), this.GitInfo.CommitNumber.Trim());
            inputContents = Regex.Replace(inputContents, AsToken("GITMODCOUNT"), this.GitInfo.Changes.ToString(CultureInfo.InvariantCulture));
            inputContents = Regex.Replace(inputContents, AsToken("GITBUILDDATE"), DateTime.Now.ToShortDateString());

            if (shouldRenameFile)
            {
                if (this.FilePath == inputContents)
                {
                    Console.WriteLine("No valid tokens in file name {0}, are you sure -rename (-rn) should be specified?", this.FilePath);
                    return;
                }

                if (File.Exists(inputContents))
                {
                    File.Delete(inputContents);
                }

                if (shouldCopyFile)
                {
                    File.Copy(this.FilePath, inputContents);
                }
                else
                {
                    File.Move(this.FilePath, inputContents);
                }
            }
            else
            {
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
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
