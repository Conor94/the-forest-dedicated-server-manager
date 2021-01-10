using System;
using System.Configuration;
using System.IO;

namespace AppConfiguration
{
    public class AppConfig : ConfigurationElement
    {
        #region Fields and properties
        [ConfigurationProperty("ServiceName")]
        public string ServiceName
        {
            get => (string)this["ServiceName"];
            private set => this["ServiceName"] = value;
        }
        [ConfigurationProperty("TheForestServerManagerExecutablePath")]
        public string TheForestServerManagerExecutablePath
        {
            get => (string)this["TheForestServerManagerExecutablePath"];
            private set => this["TheForestServerManagerExecutablePath"] = value;
        }
        [ConfigurationProperty("ServerProcessName")]
        public string ServerProcessName
        {
            get => (string)this["ServerProcessName"];
            private set => this["ServerProcessName"] = value;
        }
        [ConfigurationProperty("ServerArguments")]
        public string ServerArguments
        {
            get => (string)this["ServerArguments"];
            private set => this["ServerArguments"] = value;
        }
        /// <summary>
        /// Determines whether a machine shutdown is scheduled along with a scheduled dedicated server shutdown.
        /// </summary>
        [ConfigurationProperty("IsMachineShutdownScheduled")]
        public bool IsMachineShutdownScheduled
        {
            get => (bool)this["IsMachineShutdownScheduled"];
            set => this["IsMachineShutdownScheduled"] = value;
        }
        [ConfigurationProperty("ShutdownTime")]
        public DateTime ShutdownTime
        {
            get => (DateTime)this["ShutdownTime"];
            set => this["ShutdownTime"] = value;
        }
        #endregion

        #region Constructors
        public AppConfig()
        {
            //! hardcoded values for now
            ServiceName = "TheForestDedicatedServerManagerService";
            TheForestServerManagerExecutablePath = @"C:\Program Files (x86)\Steam\steamapps\common\TheForestDedicatedServer\TheForestDedicatedServer.exe";
            ServerProcessName = Path.GetFileNameWithoutExtension(TheForestServerManagerExecutablePath);
            ServerArguments = "-batchmode -dedicated -nographics -nosteamclient";
        }
        #endregion
    }
}
