using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace AppConfiguration
{
    /// <summary>
    /// Manages a custom <see cref="ConfigurationSection"/>. Inherit from <see cref="ConfigurationSection"/>
    /// to create a custom configuration section.
    /// </summary>
    /// <remarks>
    /// Call <see cref="Init(Environment.SpecialFolder)"/> to initialize this class before calling any other methods.
    /// </remarks>
    public static class AppConfigManager<TSection> where TSection : ConfigurationSection, new()
    {
        #region Fields
        private static readonly string mSectionName;
        private static string mConfigPath;
        private static Configuration mConfig;
        #endregion

        #region Constructor
        /// <summary>
        /// Static constructor for the <see cref="AppConfigManager{TSection}"/> class.
        /// </summary>
        static AppConfigManager()
        {
            mSectionName = typeof(TSection).Name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Allows providing a folder path through the <paramref name="specialFolder"/> parameter
        /// and creates the configuration file if it is not already created. 
        /// <para/>
        /// The file will be created as follows: "{SpecialFolder}\{AssemblyName}\{AssemblyName}.exe.config", 
        /// where {SpecialFolder} is the path to the special system folder and {AssemblyName} is the name
        /// of the assembly that is executing this method.
        /// </summary>
        /// <remarks>
        /// This method must be called before calling <see cref="GetSection"/> or <see cref="Save"/>.
        /// </remarks>
        /// <param name="specialFolder">Path that the configuration file will be saved to.</param>
        public static void Init(Environment.SpecialFolder specialFolder)
        {
            try
            {
                // Get information about the executing assembly
                Assembly assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name;

                // Create the path to the config file
                mConfigPath = Path.Combine(new string[]
                {
                    Environment.GetFolderPath(specialFolder),
                    assemblyName,
                    $"{assemblyName}.exe.config"
                });
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", mConfigPath);


                // Create the configuration file if it hasn't already been created
                mConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // Get the configuration file
                if (mConfig.Sections[mSectionName] == null)
                {
                    TSection section = new TSection();
                    mConfig.Sections.Add(mSectionName, section);
                    mConfig.Save(ConfigurationSaveMode.Full, true);
                }
            }
            catch (ConfigurationErrorsException e)
            {
                Console.WriteLine($"---Exception thrown---\n" +
                                  $"Message: {e.Message}\n" +
                                  $"StackTrace: {e.StackTrace}");
            }
        }

        /// <summary>
        /// Gets the configuration section that is managed by this object <typeparamref name="TSection"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Init(Environment.SpecialFolder)"/> must be called before calling this method.
        /// </remarks>
        /// <returns><typeparamref name="TSection"/> if the section is found and
        /// <see langword="null"/> if the section could not be found.</returns>
        public static TSection GetSection()
        {
            try
            {
                // Get the configuration file
                mConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                return (TSection)mConfig.GetSection(mSectionName);
            }
            catch (ConfigurationErrorsException e)
            {
                Console.WriteLine($"---Exception thrown---\n" +
                                  $"Message: {e.Message}\n" +
                                  $"StackTrace: {e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Saves the configuration file.
        /// </summary>
        /// <remarks>
        /// <see cref="Init(Environment.SpecialFolder)"/> must be called before calling this method.
        /// </remarks>
        /// <returns><see langword="true"/> if the configuration file is saved and <see langword="false"/> if it is not.</returns>
        public static bool Save()
        {
            try
            {
                mConfig.Save(ConfigurationSaveMode.Modified);
                return true;
            }
            catch (ConfigurationErrorsException e)
            {
                Console.WriteLine($"---Exception thrown---\n" +
                                  $"Message: {e.Message}\n" +
                                  $"StackTrace: {e.StackTrace}");
                return false;
            }
        }
        #endregion
    }
}
