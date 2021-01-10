using System;
using System.Configuration;

namespace AppConfiguration
{
    /// <summary>
    /// Abstract class that is a generic implementation of <see cref="ConfigurationSection"/>.
    /// </summary>
    /// <typeparam name="TConfigurationElement">The type of settings in this section (must inherit from <see cref="ConfigurationElement"/>).</typeparam>
    /// <typeparam name="TDerived">The concrete implementation of <see cref="ConfigurationSectionBase{TSettings, TDerived}"/> 
    /// (i.e. the class that derives from this abstract class).</typeparam>
    public abstract class ConfigurationSectionBase<TConfigurationElement, TDerived> : ConfigurationSection where TConfigurationElement : ConfigurationElement
                                                                                                           where TDerived : ConfigurationSectionBase<TConfigurationElement, TDerived>
    {
        #region Fields
        private static System.Configuration.Configuration mConfig;
        private static string mExeConfigurationPath;
        private static bool mIsInitialized;
        #endregion

        #region Properties
        [ConfigurationProperty("AppConfig")]
        public TConfigurationElement Settings
        {
            get => (TConfigurationElement)this["AppConfig"];
            set => this["AppConfig"] = value;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Static constructor that initializes data members to their default values.
        /// </summary>
        static ConfigurationSectionBase()
        {
            mConfig = null;
            mExeConfigurationPath = "";
            mIsInitialized = false;
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Initializes the section. This method should be called before
        /// calling any other method.
        /// </summary>
        /// <param name="exeConfigurationPath">The path to the executable that has the settings (e.g. ".\AppConfiguration.exe").</param>
        public static void Init(string exeConfigurationPath)
        {
            mExeConfigurationPath = exeConfigurationPath;
            mIsInitialized = true;
        }

        /// <summary>
        /// Retrieves the <see cref="Settings"/> object. The executables configuration file path must be set by calling
        /// <see cref="Init(string)"/> before calling this method.
        /// </summary>
        /// <param name="exeConfigurationPath">Path to the executeable (.exe) that has the settings.</param>
        /// <returns></returns>
        public static TConfigurationElement GetSettings(/*string exeConfigurationPath*/)
        {
            ////mConfig = ConfigurationManager.OpenExeConfiguration(exeConfigurationPath);      
            if (mIsInitialized)
            {
                mConfig = ConfigurationManager.OpenExeConfiguration(mExeConfigurationPath);
                ConfigurationSectionBase<TConfigurationElement, TDerived> appConfig = (ConfigurationSectionBase<TConfigurationElement, TDerived>)mConfig.GetSection(typeof(TDerived).Name);
                return appConfig.Settings;
            }
            else
            {
                throw new Exception($"'{typeof(TDerived).Name}.{nameof(Init)}()' must be called before calling '{typeof(TDerived).Name}.{nameof(GetSettings)}()'.");
            }
        }

        /// <summary>
        /// Saves the configuration file. The configuration file that is saved is specified
        /// by calling the <see cref="Init(string)"/> method.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration file has been saved and
        /// <see langword="false"/> if it has not.</returns>
        public static bool Save()
        {
            bool hasSaved = false;
            if (mConfig != null)
            {
                mConfig.Save();
                hasSaved = true;
            }
            return hasSaved;
        }
        #endregion
    }
}
