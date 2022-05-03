using PrismMvvmBase.Bindable;
using System.Windows;

namespace TheForestDSM.ViewModels.ScheduleShutdown
{
    public class ScheduleShutdownInput<TValue> : DataErrorBindableBase
    {
        private TValue mValue;
        private Visibility mVisibility;

        public void RaisePropertyChangedForValue()
        {
            RaisePropertyChanged(nameof(Value));
        }

        public ScheduleShutdownInput(TValue value, Visibility visibility, IDataErrorValidator validator = null)
        {
            Value = value;
            Visibility = visibility;

            if (validator != null)
            {
                AddValidator(nameof(Value), validator);
            }
        }

        public TValue Value
        {
            get => mValue;
            set => SetProperty(ref mValue, value);
        }
        public Visibility Visibility
        {
            get => mVisibility;
            set => SetProperty(ref mVisibility, value);
        }
    }
}
