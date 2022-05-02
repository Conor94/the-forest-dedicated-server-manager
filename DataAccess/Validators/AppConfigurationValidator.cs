using System.IO;

namespace DataAccess.Validators
{
    public static class AppConfigurationValidator
    {
        public static bool ValidateServerExePath(string exePath, out string errorMessage)
        {
            errorMessage = "";

            if (!File.Exists(exePath) && exePath != "")
            {
                errorMessage = "This file does not exist.";
                return false;
            }

            return true;
        }
    }
}
