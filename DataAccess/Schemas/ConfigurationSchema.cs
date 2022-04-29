namespace DataAccess.Schemas
{
    public class ConfigurationSchema
    {
        public const string TABLE_NAME = "Configuration";

        public static readonly string COLUMN_DEFINITIONS = $"{ID} TEXT PRIMARY KEY," +
                                                           $"{SERVER_EXECUTABLE_PATH} TEXT DEFAULT ''," +
                                                           $"{SERVER_PROCESS_NAME} TEXT DEFAULT ''," +
                                                           $"{SERVER_ARGUMENTS} TEXT DEFAULT ''," +
                                                           $"{REFRESH_INTERVAL_IN_SECONDS} INTEGER DEFAULT {REFRESH_INTERVAL_IN_SECONDS_DEFAULT_VALUE}," +
                                                           $"{IS_SETUP} INTEGER DEFAULT 0";

        public const string ID = "Id";
        public const string SERVER_EXECUTABLE_PATH = "ServerExecutablePath";
        public const string SERVER_PROCESS_NAME = "ServerProcessName";
        public const string SERVER_ARGUMENTS = "ServerArguments";
        public const string REFRESH_INTERVAL_IN_SECONDS = "RefreshIntervalInSeconds";
        public const string IS_SETUP = "IsSetup";

        public const string ID_DEFAULT_VALUE = "Default";
        public const int REFRESH_INTERVAL_IN_SECONDS_DEFAULT_VALUE = 10;
    }
}
