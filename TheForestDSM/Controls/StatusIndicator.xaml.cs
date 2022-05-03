using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TheForestDSM.Controls
{
    public partial class StatusIndicator : UserControl
    {
        public static DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(bool), typeof(StatusIndicator),
            new PropertyMetadata(false, StatusChanged));

        public static DependencyProperty PositiveFillColorProperty =
            DependencyProperty.Register("PositiveFillColor", typeof(Brush), typeof(StatusIndicator),
            new PropertyMetadata(new SolidColorBrush(Colors.Lime)));

        public static DependencyProperty PositiveStrokeColorProperty =
            DependencyProperty.Register("PositiveStrokeColor", typeof(Brush), typeof(StatusIndicator),
            new PropertyMetadata(new SolidColorBrush(Colors.Green)));

        public static DependencyProperty NegativeFillColorProperty =
            DependencyProperty.Register("NegativeFillColor", typeof(Brush), typeof(StatusIndicator),
            new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public static DependencyProperty NegativeStrokeColorProperty =
            DependencyProperty.Register("NegativeStrokeColor", typeof(Brush), typeof(StatusIndicator),
            new PropertyMetadata(new SolidColorBrush(Colors.DarkRed)));

        public static DependencyProperty IndicatorFillColorProperty =
            DependencyProperty.Register("IndicatorFillColor", typeof(Brush), typeof(StatusIndicator),
            new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public static DependencyProperty IndicatorStrokeColorProperty =
            DependencyProperty.Register("IndicatorStrokeColor", typeof(Brush), typeof(StatusIndicator),
            new PropertyMetadata(new SolidColorBrush(Colors.DarkRed)));

        public bool Status
        {
            get => (bool)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public Brush PositiveFillColor
        {
            get => (Brush)GetValue(PositiveFillColorProperty);
            set => SetValue(PositiveFillColorProperty, value);
        }

        public Brush NegativeFillColor
        {
            get => (Brush)GetValue(NegativeFillColorProperty);
            set => SetValue(NegativeFillColorProperty, value);
        }

        protected Brush IndicatorFillColor
        {
            get => (Brush)GetValue(IndicatorFillColorProperty);
            set => SetValue(IndicatorFillColorProperty, value);
        }

        protected Brush IndicatorStrokeColor
        {
            get => (Brush)GetValue(IndicatorStrokeColorProperty);
            set => SetValue(IndicatorStrokeColorProperty, value);
        }

        public StatusIndicator()
        {
            InitializeComponent();
        }

        protected static void StatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                d.SetValue(IndicatorFillColorProperty, d.GetValue(PositiveFillColorProperty));
                d.SetValue(IndicatorStrokeColorProperty, d.GetValue(PositiveStrokeColorProperty));
            }
            else
            {
                d.SetValue(IndicatorFillColorProperty, d.GetValue(NegativeFillColorProperty));
                d.SetValue(IndicatorStrokeColorProperty, d.GetValue(NegativeStrokeColorProperty));
            }
        }
    }
}
