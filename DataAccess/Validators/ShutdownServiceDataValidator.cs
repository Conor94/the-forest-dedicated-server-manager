using System;

namespace DataAccess.Validators
{
    public static class ShutdownServiceDataValidator
    {
        public static bool ValidateShutdownTime(DateTime shutdownTime, out string errorMessage)
        {
            bool isValid = false;

            errorMessage = "";
            if (!string.IsNullOrWhiteSpace(shutdownTime.ToString()))
            {
                if (shutdownTime < DateTime.Now)
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
