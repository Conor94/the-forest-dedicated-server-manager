using PrismMvvmBase.Bindable;
using System.Windows;

namespace TheForestDSM.ViewModels.ScheduleShutdown
{
    public class ScheduleShutdownInput<TValue> : DataErrorBindableBase
    {
        private TValue mInput;
        private Visibility mVisibility;

        public ScheduleShutdownInput(TValue value, Visibility visibility)
        {
            Value = value;
            Visibility = visibility;
        }

        public TValue Value
        {
            get => mInput;
            set => SetProperty(ref mInput, value);
        }
        public Visibility Visibility
        {
            get => mVisibility;
            set => SetProperty(ref mVisibility, value);
        }
    }
}
