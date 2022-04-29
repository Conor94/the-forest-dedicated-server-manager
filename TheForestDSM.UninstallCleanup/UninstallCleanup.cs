using System;
using System.Diagnostics;
using System.IO;
using TheForestDSM.Utilities;

namespace TheForestDSM.UninstallCleanup
{
    public class UninstallCleanup
    {
        static void Main(string[] args)
        {
            try
            {
                string[] paths = new string[]
                {
                    new PathResolver().GetApplicationDataPath(),
                    $@"{Environment.GetEnvironmentVariable("ProgramFiles(x86)")}\The Forest Dedicated Server Manager"
                };

                foreach (string path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("The Forest Dedicated Server Manager",
                                    "An error occurred while uninstalling The Forest Dedicated Server Manager.\n\n" +
                                    $"Description: {e.Message}\n\n" +
                                    $"Exception info: {e.StackTrace}",
                                    EventLogEntryType.Error);
            }
        }
    }
}
