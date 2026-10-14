using System.Windows;
using System.Windows.Controls;

namespace HaruApp.Controls
{
    public partial class ForecastRow : UserControl
    {
        public static readonly DependencyProperty HumidityLabelProperty = DependencyProperty.Register(
            "HumidityLabel", typeof(string), typeof(ForecastRow),
            new PropertyMetadata(null, (d, e) => ((ForecastRow)d).HumidityLabelText.Text = (string)e.NewValue));

        public static readonly DependencyProperty PrecipitationLabelProperty = DependencyProperty.Register(
            "PrecipitationLabel", typeof(string), typeof(ForecastRow),
            new PropertyMetadata(null, (d, e) => ((ForecastRow)d).PrecipitationLabelText.Text = (string)e.NewValue));

        public static readonly DependencyProperty WindLabelProperty = DependencyProperty.Register(
            "WindLabel", typeof(string), typeof(ForecastRow),
            new PropertyMetadata(null, (d, e) => ((ForecastRow)d).WindLabelText.Text = (string)e.NewValue));

        public string HumidityLabel
        {
            get { return (string)GetValue(HumidityLabelProperty); }
            set { SetValue(HumidityLabelProperty, value); }
        }

        public string PrecipitationLabel
        {
            get { return (string)GetValue(PrecipitationLabelProperty); }
            set { SetValue(PrecipitationLabelProperty, value); }
        }

        public string WindLabel
        {
            get { return (string)GetValue(WindLabelProperty); }
            set { SetValue(WindLabelProperty, value); }
        }

        public ForecastRow()
        {
            InitializeComponent();
        }
    }
}
