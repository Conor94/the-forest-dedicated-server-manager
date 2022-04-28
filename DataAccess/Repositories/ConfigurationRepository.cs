using DataAccess.Models;
using DataAccess.Schemas;
using System;
using System.Data.SQLite;
using System.IO;

namespace DataAccess.Repositories
{
    public class ConfigurationRepository : SQLiteRepository
    {
        public ConfigurationRepository(SQLiteDAO dao) : base(dao) { }

        public Configuration Read(string id)
        {
            return DAO.Read(ConfigurationSchema.TABLE_NAME, "*", (dataReader) =>
            {
                if (dataReader.Read())
                {
                    return ConvertConfiguration(dataReader);
                }
                else
                {
                    throw new Exception("Attempted to find a configuration that does not exist");
                }
            }, $"Id = '{id}'");
        }

        public void Update(Configuration config)
        {
            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter($"@{ConfigurationSchema.SERVER_EXECUTABLE_PATH}", config.ServerExecutablePath),
                new SQLiteParameter($"@{ConfigurationSchema.SERVER_PROCESS_NAME}", Path.GetFileNameWithoutExtension(config.ServerExecutablePath)),
                new SQLiteParameter($"@{ConfigurationSchema.SERVER_ARGUMENTS}", config.ServerArguments),
                new SQLiteParameter($"@{ConfigurationSchema.IS_SETUP}", config.IsSetup ? 1 : 0),
            };

            if (DAO.UpdateSingle(ConfigurationSchema.TABLE_NAME, parameters, $"Id = '{config.Id}'") != 1)
            {
                throw new Exception("Failed to update configuration");
            }
        }

        private Configuration ConvertConfiguration(SQLiteDataReader dataReader)
        {
            return new Configuration()
            {
                Id = (string)dataReader[ConfigurationSchema.ID],
                ServerExecutablePath = (string)dataReader[ConfigurationSchema.SERVER_EXECUTABLE_PATH],
                ServerProcessName = (string)dataReader[ConfigurationSchema.SERVER_PROCESS_NAME],
                ServerArguments = (string)dataReader[ConfigurationSchema.SERVER_ARGUMENTS],
                IsSetup = Convert.ToInt32(dataReader[ConfigurationSchema.IS_SETUP]) == 1 // Convert to bool
            };
        }
    }
}
