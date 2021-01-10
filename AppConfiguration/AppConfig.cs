using System.Configuration;

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
        [ConfigurationProperty("ServerArguments")]
        public string ServerArguments
        {
            get => (string)this["ServerArguments"];
            private set => this["ServerArguments"] = value;
        }
        #endregion

        #region Constructors
        public AppConfig()
        {
            //! hardcoded values for now
            ServiceName = "TheForestDedicatedServerManagerService";
            TheForestServerManagerExecutablePath = @"C:\Program Files (x86)\Steam\steamapps\common\TheForestDedicatedServer\TheForestDedicatedServer.exe";
            ServerArguments = "-batchmode -dedicated -nographics -nosteamclient";
        }
        #endregion
    }
}
