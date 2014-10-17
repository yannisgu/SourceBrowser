using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using System.IO;

namespace SourceBrowser.Generator.Extensions
{
    public static class SolutionExtensions
    {
        /// <summary>
        /// Returns the name of the solution without the ".sln" suffix.
        /// </summary>
        public static string GetName(this Solution solution)
        {
            const string SOLUTION_SUFFIX = ".sln";
            var fileName = Path.GetFileName(solution.FilePath);

            if (fileName.EndsWith(SOLUTION_SUFFIX))
                fileName = fileName.Remove(fileName.IndexOf(SOLUTION_SUFFIX), SOLUTION_SUFFIX.Length); 

            return fileName;
        }
    }
}
