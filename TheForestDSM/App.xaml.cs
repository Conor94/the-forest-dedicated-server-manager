using DataAccess;
using DataAccess.Repositories;
using DataAccess.Schemas;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using TheForestDSM.Utilities;
using TheForestDSM.ViewModels;
using TheForestDSM.ViewModels.ScheduleShutdown;
using TheForestDSM.Views;
using Unity;

namespace TheForestDSM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <inheritdoc/>
        protected override Window CreateShell()
        {
            ConfigurationRepository appConfigRepo = Container.Resolve<ConfigurationRepository>();

            // Open the setup window if there is no setup information saved
            if (!appConfigRepo.Read(ConfigurationSchema.ID_DEFAULT_VALUE).IsSetup)
            {
                Container.Resolve<SetupView>().ShowDialog();

                // Shutdown the app if the setup window was cancelled
                if (!appConfigRepo.Read(ConfigurationSchema.ID_DEFAULT_VALUE).IsSetup)
                {
                    Current.Shutdown();
                }
            }

            Window w = Container.Resolve<MainWindow>();
            w.Closed += MainWindow_OnClosed;
            return w;
        }

        /// <inheritdoc/>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register transients
            containerRegistry.Register<SetupViewModel>();
            containerRegistry.Register<ScheduleShutdownViewModel>();

            SQLiteDAO dao = SetupDatabase();

            // Register singletons
            containerRegistry.RegisterSingleton<IContainerProvider, UnityContainerExtension>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<MainWindow>(); // don't need this, but it's more explicit
            containerRegistry.RegisterSingleton<ConfigurationRepository>(() => new ConfigurationRepository(dao));
            containerRegistry.RegisterSingleton<ShutdownServiceDataRepository>(() => new ShutdownServiceDataRepository(dao));
        }

        private SQLiteDAO SetupDatabase()
        {
            string dbFilePath = new PathResolver().GetApplicationDataPath();

            // Create the directory for the database file if it doesn't exist
            if (!Directory.Exists(dbFilePath))
            {
                Directory.CreateDirectory(dbFilePath);
            }

            SQLiteDAO dao = new SQLiteDAO($@"Data Source={dbFilePath}\TheForestDSM.db");

            // Create the database if it doesn't exist
            if (!dao.DatabaseExists($@"{dbFilePath}\TheForestDSM.db"))
            {
                dao.CreateDatabase($@"{dbFilePath}\TheForestDSM.db");

                // Create tables for the database
                dao.CreateTable(ConfigurationSchema.TABLE_NAME, ConfigurationSchema.COLUMN_DEFINITIONS);
                dao.CreateTable(ShutdownServiceDataSchema.TABLE_NAME, ShutdownServiceDataSchema.COLUMN_DEFINITIONS);

                dao.InsertSingle(ConfigurationSchema.TABLE_NAME, new SQLiteParameter[]
                {
                    new SQLiteParameter($"@{ConfigurationSchema.ID}", ConfigurationSchema.ID_DEFAULT_VALUE)
                });
                dao.InsertSingle(ShutdownServiceDataSchema.TABLE_NAME, new SQLiteParameter[]
                {
                    new SQLiteParameter($"@{ShutdownServiceDataSchema.ID}", ShutdownServiceDataSchema.ID_DEFAULT_VALUE)
                });
            }

            return dao;
        }

        /// <summary>
        /// Event handler for the <see cref="Application.Startup"/> event (event is subscribed to 
        /// in App.xaml). It must be called before <see cref="CreateShell"/> so that resources from 
        /// App.xaml are loaded.
        /// </summary>
        /// <remarks>
        /// Discussions on why this event handler is neccessary:
        /// <list type="bullet">
        /// <item>https://stackoverflow.com/questions/2487336/cannot-access-resource-defined-in-app-xaml/4939937</item>
        /// <item>https://stackoverflow.com/questions/543414/app-xaml-file-does-not-get-parsed-if-my-app-does-not-set-a-startupuri</item>
        /// <item>https://social.msdn.microsoft.com/Forums/vstudio/en-US/0d3fd0d8-ea9a-47d9-9e10-89db8b0243c3/applicationresources-not-loaded-if-application-doesnt-have-startupuri-property?forum=wpf</item>
        /// </list>
        /// </remarks>
        /// <param name="sender">Object that invoked the event.</param>
        /// <param name="e">Event data for the event.</param>
        protected virtual void OnStartup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        /// <summary>
        /// Shuts down the application when the main window is closed.
        /// </summary>
        /// <param name="sender">Object that invoked the event.</param>
        /// <param name="e">Event data for the event.</param>
        protected virtual void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Current.Shutdown();
        }
    }
}
