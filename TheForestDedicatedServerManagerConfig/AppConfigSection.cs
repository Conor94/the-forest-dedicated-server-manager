using System;
using System.Configuration;
using System.IO;

namespace TheForestDedicatedServerManagerConfig
{
    /// <summary>
    /// Class that implements <see cref="ConfigurationSection"/>.
    /// </summary>
    public class AppConfigSection : ConfigurationSection
    {
        #region Fields and properties
        [ConfigurationProperty("ServiceName", DefaultValue = "TheForestDedicatedServerManagerService")]
        public string ServiceName
        {
            get => (string)this["ServiceName"];
            private set => this["ServiceName"] = value;
        }
        [ConfigurationProperty("TheForestServerManagerExecutablePath")]
        public string TheForestServerManagerExecutablePath
        {
            get => (string)this["TheForestServerManagerExecutablePath"];
            set => this["TheForestServerManagerExecutablePath"] = value;
        }
        [ConfigurationProperty("ServerProcessName")]
        public string ServerProcessName
        {
            get => Path.GetFileNameWithoutExtension(TheForestServerManagerExecutablePath);
            private set => this["ServerProcessName"] = value;
        }
        [ConfigurationProperty("ServerArguments")]
        public string ServerArguments
        {
            get => (string)this["ServerArguments"];
            set => this["ServerArguments"] = value;
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
        [ConfigurationProperty("IsSetupSaved", DefaultValue = false)]
        public bool IsSetupSaved
        {
            get => (bool)this["IsSetupSaved"];
            set => this["IsSetupSaved"] = value;
        }
        [ConfigurationProperty("ShutdownTime")]
        public DateTime ShutdownTime
        {
            get => (DateTime)this["ShutdownTime"];
            set => this["ShutdownTime"] = value;
        }
        #endregion
    }
}
