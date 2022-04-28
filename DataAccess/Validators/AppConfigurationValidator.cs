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
                errorMessage = "Server executable path is not valid.";
                return false;
            }

            return true;
        }
    }
}
