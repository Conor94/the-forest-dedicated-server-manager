namespace DataAccess.Schemas
{
    public class ShutdownServiceDataSchema
    {
        public const string TABLE_NAME = "ShutdownServiceData";

        public static readonly string COLUMN_DEFINITIONS = $"{ID} INTEGER PRIMARY KEY," +
                                                           $"{IS_SHUTDOWN_SCHEDULED} INTEGER DEFAULT 0," +
                                                           $"{IS_MACHINE_SHUTDOWN} INTEGER DEFAULT 0," +
                                                           $"{SHUTDOWN_TIME} TEXT DEFAULT ''";

        public const string ID = "Id";
        public const string IS_SHUTDOWN_SCHEDULED = "IsShutdownScheduled";
        public const string IS_MACHINE_SHUTDOWN = "IsMachineShutdown";
        public const string SHUTDOWN_TIME = "ShutdownTime";

        public const int ID_DEFAULT_VALUE = 1;
    }
}
