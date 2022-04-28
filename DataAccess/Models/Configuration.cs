using DataAccess.Validators;
using PrismMvvmBase.Bindable;

namespace DataAccess.Models
{
    public class Configuration : DataErrorBindableBase
    {
        private string mId;
        private string mServerExecutetablePath;
        private string mServerProcessName;
        private string mServerArguments;
        private bool mIsSetup;

        public Configuration()
        {
            AddValidator(nameof(ServerExecutablePath), new DataErrorValidator<string>(AppConfigurationValidator.ValidateServerExePath));
        }

        public string Id
        {
            get => mId;
            set => SetProperty(ref mId, value);
        }

        public string ServerExecutablePath
        {
            get => mServerExecutetablePath;
            set => SetProperty(ref mServerExecutetablePath, value);
        }

        public string ServerProcessName
        {
            get => mServerProcessName;
            set => SetProperty(ref mServerProcessName, value);
        }

        public string ServerArguments
        {
            get => mServerArguments;
            set => SetProperty(ref mServerArguments, value);
        }

        public bool IsSetup
        {
            get => mIsSetup;
            set => SetProperty(ref mIsSetup, value);
        }
    }
}
