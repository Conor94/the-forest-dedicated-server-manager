using AppConfiguration;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using System.Windows;
using TheForestDedicatedServerManager.Views;

namespace TheForestDedicatedServerManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindow>(); // don't need this, but it's more explicit
            containerRegistry.Register<IEventAggregator, EventAggregator>();
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
        protected void OnStartup(object sender, StartupEventArgs e)
        {
            AppConfigurationManager.Init(@".\AppConfiguration.exe");

            ////StringBuilder sb = new StringBuilder();
            ////sb.AppendLine($"Type = {sender.GetType()}");
            ////sb.AppendLine($"e.Args = {e.Args}");

            ////if (sender is App a)
            ////{
            ////    if (a == this)
            ////    {
            ////        sb.AppendLine("a == this");
            ////    }
            ////    else
            ////    {
            ////        sb.AppendLine("a != this");
            ////    }
            ////}

            ////MessageBox.Show(sb.ToString());
        }
    }
}
