using DataAccess.Models;
using DataAccess.Schemas;
using System;
using System.Data.SQLite;

namespace DataAccess.Repositories
{
    public class ShutdownServiceDataRepository : SQLiteRepository
    {
        public ShutdownServiceDataRepository(SQLiteDAO dao) : base(dao) { }

        public ShutdownServiceData Read(int id)
        {
            return DAO.Read(ShutdownServiceDataSchema.TABLE_NAME, "*", (dataReader) =>
            {
                if (dataReader.Read())
                {
                    return ConvertShutdownServiceData(dataReader);
                }
                else
                {
                    throw new Exception("Failed to retrieve shutdown service data");
                }
            }, $"Id = '{id}'");
        }

        public void Update(ShutdownServiceData data)
        {
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter($"@{ShutdownServiceDataSchema.IS_SHUTDOWN_SCHEDULED}", data.IsShutdownScheduled ? 1 : 0),
                new SQLiteParameter($"@{ShutdownServiceDataSchema.IS_MACHINE_SHUTDOWN}", data.IsMachineShutdown ? 1 : 0),
                new SQLiteParameter($"@{ShutdownServiceDataSchema.SHUTDOWN_TIME}", data.ShutdownTime.ToString())
            };

            if (DAO.UpdateSingle(ShutdownServiceDataSchema.TABLE_NAME, parameters, $"Id = '{data.Id}'") != 1)
            {
                throw new Exception("Failed to update shutdown service data");
            }
        }

        private ShutdownServiceData ConvertShutdownServiceData(SQLiteDataReader dataReader)
        {
            return new ShutdownServiceData()
            {
                Id = Convert.ToInt32(dataReader[ShutdownServiceDataSchema.ID]),
                IsShutdownScheduled = Convert.ToInt32(dataReader[ShutdownServiceDataSchema.IS_SHUTDOWN_SCHEDULED]) == 1, // Convert to bool
                IsMachineShutdown = Convert.ToInt32(dataReader[ShutdownServiceDataSchema.IS_MACHINE_SHUTDOWN]) == 1, // Convert to bool
                ShutdownTime = (string)dataReader[ShutdownServiceDataSchema.SHUTDOWN_TIME]
            };
        }
    }
}
