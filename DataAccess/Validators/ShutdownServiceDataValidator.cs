using System;

namespace DataAccess.Validators
{
    public static class ShutdownServiceDataValidator
    {
        public static bool ValidateShutdownTime(string shutdownTime, out string errorMessage)
        {
            bool isValid = false;

            errorMessage = "";
            if (!string.IsNullOrWhiteSpace(shutdownTime))
            {
                if (!DateTime.TryParse(shutdownTime, out DateTime tmpShutdownTime))
                {
                    errorMessage = "Shutdown time is not valid.";
                }
                else if (tmpShutdownTime < DateTime.Now)
                {
                    errorMessage = "Shutdown time cannot be in the past.";
                }
                else
                {
                    isValid = true;
                }
            };

            return isValid;
        }
    }
}
