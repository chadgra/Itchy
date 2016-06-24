//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Torion Technologies">
//     Copyright (c) Torion Technologies.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace GitVersionNumbers
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// main program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// main program
        /// program should be called like
        /// GitVersionNumbers -s {solution directory full path} -f {file in the project to change}
        /// for this project it would be
        /// c:\GitVersionNumbers\GitVersionNumbers.exe -s "$(SolutionDir) " -f "$(ProjectDir)Properties\AssemblyInfo.cs"
        /// THE SPACE AT THE CARROT IS IMPORTANT!                        ^                   
        /// If you do not include the space it the quote is assumed to be part of the dir name
        /// It is nessary to split the solution and project apart so that that homer can update
        /// both the diagnositicgui and homer directories
        /// </summary>
        /// <param name="args">contains the name of the directory to process</param>
        private static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                if (args.Count() <= 1)
                {
                    Console.WriteLine("Missing Main Project and Solution Directory Name");
                    return;
                }

                // walk the arguments and see if we can get the project and solution values
                string solutionDirectory = string.Empty;
                string filePath = string.Empty;
                bool shouldRenameFile = false;
                bool shouldCopyFile = false;
                for (int i = 0; i < args.Count(); i++)
                {
                    var arg1 = args[i].ToUpper().Trim();
                    var arg2 = args.Length > (i + 1) ? args[i + 1].Trim() : string.Empty;

                    switch (arg1)
                    {
                        case "-S":
                        case "-SOLUTION":
                            solutionDirectory = arg2;
                            break;
                        case "-F":
                        case "-FILE":
                            filePath = arg2;
                            break;
                        case "-RN":
                        case "-RENAME":
                            shouldRenameFile = true;
                            break;
                        case "-C":
                        case "-COPY":
                            shouldCopyFile = true;
                            break;
                    }
                }

                Console.WriteLine("     Using Solution: " + solutionDirectory);
                Console.WriteLine("     Changing File:  " + filePath);
                if (!Directory.Exists(solutionDirectory))
                {
                    Console.WriteLine("Invalid Solution Directory " + solutionDirectory);
                    return;
                }

                var git = new GitEmulation(solutionDirectory);
                GitInformation info = git.GitInfo();
                if (!String.IsNullOrEmpty(info.Version) && !String.IsNullOrEmpty(info.LastCommitHash))
                {
                    var change = new ChangeAssemblyInfo(filePath, info);
                    change.UpdateAssembly(shouldRenameFile, shouldCopyFile);
                }
#if !DEBUG
            }
            catch (Exception e)
            {
                while (null != e.InnerException)
                {
                    e = e.InnerException;
                }

                Console.WriteLine(e.Message);
            }
#endif
        }       
    }
}
