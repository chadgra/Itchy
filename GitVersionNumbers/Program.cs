//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Torion Technologies">
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
            if (args.Count() <= 1)
            {
                Console.WriteLine("Missing Main Project and Solution Directory Name");
                return;
                ////throw new Exception("Missing Main Project and Solution Directory Name");
            }

            // walk the arguments and see if we can get the project and solution values
            string solutionDirectory = string.Empty;
            string projectDirectory = string.Empty;
            for (int i = 0; i < args.Count(); i++)
            {
                if (args[i].Contains("-s") || args[i].Contains("-S"))
                {
                    if (args[i].Length > 2)
                    {
                        // incase the value was passed like -sC:\projects
                        solutionDirectory = args[i].Substring(2, (args[i].Length - 2)).Trim();
                    }
                    else if (args.Count() > (i + 1))
                    {
                        solutionDirectory = args[(i + 1)].Trim();
                    }
                }
                else if (args[i].Contains("-f") || args[i].Contains("-F"))
                {
                    if (args[i].Length > 2)
                    {
                        // incase the value was passed like -phomer
                        projectDirectory = args[i].Substring(2, (args[i].Length - 2)).Trim();
                    }
                    else if (args.Count() > (i + 1))
                    {
                        projectDirectory = args[(i + 1)].Trim();
                    }
                }
            }

            Console.WriteLine("     Using Solution: " + solutionDirectory);
            Console.WriteLine("     Changing File:  " + projectDirectory);
            if (!Directory.Exists(solutionDirectory))
            {
                Console.WriteLine("Invalid Solution Directory " + solutionDirectory);
                return;
                ////throw new Exception("Invalid Solution Directory " + solutionDirectory);
            }

            GitVersionNumbers.GitEmulation git = new GitEmulation(solutionDirectory);
            GitInformation info = git.GitInfo();
            if (!String.IsNullOrEmpty(info.Version) && !String.IsNullOrEmpty(info.LastCommitHash))
            {
                ChangeAssemblyInfo change = new ChangeAssemblyInfo(projectDirectory, info);
                change.UpdateAssembly();
            }
        }       
    }
}
