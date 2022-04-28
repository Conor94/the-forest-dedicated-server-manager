using DataAccess.Validators;
using PrismMvvmBase.Bindable;

namespace DataAccess.Models
{
    public class ShutdownServiceData : DataErrorBindableBase
    {
        private bool mIsMachineShutdown;
        private string mShutdownTime;
        private int mId;
        private bool mIsShutdownScheduled;

        public ShutdownServiceData()
        {
            AddValidator(nameof(ShutdownTime), new DataErrorValidator<string>(ShutdownServiceDataValidator.ValidateShutdownTime));
        }

        public int Id
        {
            get => mId;
            set => SetProperty(ref mId, value);
        }

        public bool IsMachineShutdown
        {
            get => mIsMachineShutdown;
            set => SetProperty(ref mIsMachineShutdown, value);
        }

        public bool IsShutdownScheduled
        {
            get => mIsShutdownScheduled;
            set => SetProperty(ref mIsShutdownScheduled, value);
        }

        public string ShutdownTime
        {
            get => mShutdownTime;
            set => SetProperty(ref mShutdownTime, value);
        }
    }
}
