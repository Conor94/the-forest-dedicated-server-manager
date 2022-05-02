using DataAccess.Models;
using DataAccess.Schemas;
using DataAccess.Validators;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using PrismMvvmBase.Bindable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TheForestDSM.Dialogs;
using TheForestDSM.Events;
using Unity;

namespace TheForestDSM.ViewModels.ScheduleShutdown
{
    public class ScheduleShutdownViewModel : ViewModelBase
    {
        private ShutdownFormat mSelectedShutdownFormat;

        private TimeFormat mSelectedTimeFormat;
        private Visibility mSelectedTimeFormatVisiblity;

        private ScheduleShutdownInput<DateTime> mDateInput;
        private ScheduleShutdownInput<int> mMinutesInput;
        private ScheduleShutdownInput<double> mHoursInput;
        private bool mIsMachineShutdown;

        private DelegateCommand mScheduleShutdownCommand;

        public ShutdownFormat SelectedShutdownFormat
        {
            get => mSelectedShutdownFormat;
            set
            {
                SetProperty(ref mSelectedShutdownFormat, value);

                switch (value)
                {
                    case ShutdownFormat.@in:
                        DateInput.Visibility = Visibility.Collapsed;
                        SelectedTimeFormatVisiblity = Visibility.Visible;

                        SwitchTimeFormat(SelectedTimeFormat);
                        break;
                    case ShutdownFormat.at:
                        DateInput.Visibility = Visibility.Visible;
                        SelectedTimeFormatVisiblity = Visibility.Collapsed;

                        MinutesInput.Visibility = Visibility.Collapsed;
                        HoursInput.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }
        public IEnumerable<ShutdownFormat> ShutdownFormatValues
        {
            get => Enum.GetValues(typeof(ShutdownFormat)).Cast<ShutdownFormat>();
        }

        public TimeFormat SelectedTimeFormat
        {
            get => mSelectedTimeFormat;
            set
            {
                SetProperty(ref mSelectedTimeFormat, value);

                SwitchTimeFormat(value);
            }
        }
        public IEnumerable<TimeFormat> TimeFormatValues
        {
            get => Enum.GetValues(typeof(TimeFormat)).Cast<TimeFormat>();
        }
        public Visibility SelectedTimeFormatVisiblity
        {
            get => mSelectedTimeFormatVisiblity;
            set => SetProperty(ref mSelectedTimeFormatVisiblity, value);
        }

        public ScheduleShutdownInput<DateTime> DateInput
        {
            get => mDateInput;
            set => SetProperty(ref mDateInput, value);
        }
        public DateTime MinimumDate
        {
            get
            {
                DateTime currentTime = DateTime.Now;

                return currentTime.AddMinutes(1);
            }
        }
        public ScheduleShutdownInput<int> MinutesInput
        {
            get => mMinutesInput;
            set => SetProperty(ref mMinutesInput, value);
        }
        public ScheduleShutdownInput<double> HoursInput
        {
            get => mHoursInput;
            set => SetProperty(ref mHoursInput, value);
        }
        public bool IsMachineShutdown
        {
            get => mIsMachineShutdown;
            set => SetProperty(ref mIsMachineShutdown, value);
        }

        public DelegateCommand ScheduleShutdownCommand
        {
            get => mScheduleShutdownCommand ?? (mScheduleShutdownCommand = new DelegateCommand(ScheduleShutdownExecute));
        }

        [InjectionConstructor]
        public ScheduleShutdownViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            Title = "Schedule Shutdown";

            DateInput = new ScheduleShutdownInput<DateTime>(DateTime.Now.AddMinutes(30), Visibility.Visible);
            MinutesInput = new ScheduleShutdownInput<int>(1, Visibility.Collapsed);
            HoursInput = new ScheduleShutdownInput<double>(1, Visibility.Collapsed);

            SelectedShutdownFormat = ShutdownFormat.at;
            SelectedTimeFormatVisiblity = Visibility.Collapsed; // Shutdown "at" doesn't use time format

            DateInput.PropertyChanged += DateInput_PropertyChanged;
        }

        private void DateInput_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ScheduleShutdownCommand.RaiseCanExecuteChanged();
        }

        private void SwitchTimeFormat(TimeFormat selectedTimeFormat)
        {
            if (selectedTimeFormat == TimeFormat.minutes)
            {
                MinutesInput.Visibility = Visibility.Visible;
                HoursInput.Visibility = Visibility.Collapsed;
            }
            else if (selectedTimeFormat == TimeFormat.hours)
            {
                MinutesInput.Visibility = Visibility.Collapsed;
                HoursInput.Visibility = Visibility.Visible;
            }
        }

        private void ScheduleShutdownExecute()
        {
            if (!ShutdownServiceDataValidator.ValidateShutdownTime(DateInput.Value, out string _))
            {
                new MessageDialog(AppStrings.InvalidShutdownTime_DialogTitle,
                                  string.Format(AppStrings.InvalidShutdownTime_DialogContent, DateTime.Now, DateInput.Value),
                                  MessageDialogType.Warn).ShowDialog();
            }

            if (new MessageDialog(AppStrings.ScheduleShutdownConfirmation_Title, string.Format(AppStrings.ScheduleShutdownConfirmation_Content, DateInput.Value), MessageDialogType.Question, "Yes", "No").ShowDialog() == true)
            {
                // Close the window and send shutdown information to HomePageViewModel
                EventAggregator.GetEvent<ScheduleShutdownViewCloseRequest>().Publish();

                ShutdownServiceData data = new ShutdownServiceData()
                {
                    Id = ShutdownServiceDataSchema.ID_DEFAULT_VALUE,
                    IsMachineShutdown = IsMachineShutdown,
                    IsShutdownScheduled = true
                };

                if (SelectedShutdownFormat == ShutdownFormat.@in)
                {
                    DateTime shutdownTime;
                    if (SelectedTimeFormat == TimeFormat.hours)
                    {
                        shutdownTime = DateTime.Now.AddHours(HoursInput.Value);
                    }
                    else
                    {
                        shutdownTime = DateTime.Now.AddMinutes(MinutesInput.Value);
                    }

                    data.ShutdownTime = shutdownTime.ToString();
                }
                else if (SelectedShutdownFormat == ShutdownFormat.at)
                {
                    data.ShutdownTime = DateInput.Value.ToString();
                }

                EventAggregator.GetEvent<ShutdownScheduledEvent>().Publish(data);
                EventAggregator.GetEvent<ScheduleShutdownViewCloseRequest>().Publish();
            }
        }
    }
}
