using System;
using System.Reflection;

namespace TheForestDSM.Utilities
{
    /// <summary>Gets folder paths for the application.</summary>
    public class PathResolver
    {
        /// <summary>Gets the path to the location where application data is stored.</summary>
        /// <remarks>Examples of application data are database files and logs.</remarks>
        /// <returns>The path to the application data.</returns>
        public string GetApplicationDataPath()
        {
            return $@"{Environment.GetEnvironmentVariable("PROGRAMDATA")}\TheForestDSM";
        }
    }
}
