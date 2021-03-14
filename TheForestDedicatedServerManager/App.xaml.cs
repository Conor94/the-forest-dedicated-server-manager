using AppConfigurationManager;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Windows;
using TheForestDedicatedServerManager.ViewModels;
using TheForestDedicatedServerManager.Views;
using TheForestDedicatedServerManagerConfig;
using Unity;

namespace TheForestDedicatedServerManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <inheritdoc/>
        protected override Window CreateShell()
        {
            // Open the setup window if there is no setup information saved
            AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
            if (!config.IsSetupSaved)
            {
                Container.Resolve<SetupView>().ShowDialog();
                // Shutdown the app if the setup window was cancelled
                config = AppConfigManager<AppConfigSection>.GetSection();
                if (!config.IsSetupSaved)
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
            containerRegistry.Register<IContainerProvider, UnityContainerExtension>();
            containerRegistry.Register<IEventAggregator, EventAggregator>();
            containerRegistry.Register<SetupViewModel>();

            // Register singletons
            containerRegistry.RegisterSingleton<MainWindow>(); // don't need this, but it's more explicit
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
            // Initialize the app
            AppConfigManager<AppConfigSection>.Init(Environment.SpecialFolder.LocalApplicationData);
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
