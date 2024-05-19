using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Interaction logic for TimepickerControl.xaml
    /// </summary>
    public partial class TimepickerControl : UserControl
    {
        private TextBox SelectedTextbox;

        private EventStartTimeModel startTime;

        public TimepickerControl()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(TimepickerControl_Loaded);

            OnFocusActions();
            OnKeyDownActions();


            

        }

        private void OnFocusActions()
        {
            TbHour.GotFocus += (sender, e) =>
            {
                SelectedTextbox = TbHour;
            };

            TbMinute.GotFocus += (sender, e) =>
            {
                SelectedTextbox = TbMinute;
            };

            TbSecond.GotFocus += (sender, e) =>
            {
                SelectedTextbox = TbSecond;
            };
        }


        private void OnKeyDownActions()
        {

            TbHour.TextChanged += (sender, e) =>
            {
                if (!String.IsNullOrEmpty(TbHour.Text))
                    if (!IsValid24Hour(Convert.ToDouble(TbHour.Text)))
                        TbHour.Text = "00";

            };

            TbHour.PreviewTextInput += (sender, e) =>
            {
                bool isValidNum = double.TryParse(e.Text, out double num);
                e.Handled = !(isValidNum);
            };


            TbMinute.TextChanged += (sender, e) =>
            {
                if (!String.IsNullOrEmpty(TbMinute.Text))
                    if (!IsValidMinute(Convert.ToDouble(TbMinute.Text)))
                        TbMinute.Text = "00";

            };

            TbMinute.PreviewTextInput += (sender, e) =>
            {
                bool isValidNum = double.TryParse(e.Text, out double num);
                e.Handled = !(isValidNum);
            };

            TbSecond.TextChanged += (sender, e) =>
            {
                if (!String.IsNullOrEmpty(TbSecond.Text))
                    if (!IsValidSecond(Convert.ToDouble(TbSecond.Text)))
                        TbSecond.Text = "00";

            };

            TbSecond.PreviewTextInput += (sender, e) =>
            {
                bool isValidNum = double.TryParse(e.Text, out double num);
                e.Handled = !(isValidNum);
            };


        }

        private void TimepickerControl_Loaded(object sender, RoutedEventArgs e)
        {
            startTime = this.DataContext as EventStartTimeModel;
        }

        private void Increment(object sender, RoutedEventArgs e)
        {
            if (SelectedTextbox != null)
            {

                if (SelectedTextbox.Name == "TbHour")
                {
                    double tempNewValue = startTime.Hour + 1;
                    if (IsValid24Hour(tempNewValue))
                        startTime.Hour++;
                }

                else if (SelectedTextbox.Name == "TbMinute")
                {
                    double tempNewValue = startTime.Minute + 1;
                    if (IsValidMinute(tempNewValue))
                        startTime.Minute++;
                }

                else if (SelectedTextbox.Name == "TbSecond")
                {
                    double tempNewValue = startTime.Second + 1;
                    if (IsValidSecond(tempNewValue))
                        startTime.Second++;
                }

            }

        }

        private void Decrement(object sender, RoutedEventArgs e)
        {
            if (SelectedTextbox != null)
            {

                if (SelectedTextbox.Name == "TbHour")
                {
                    double tempNewValue = startTime.Hour - 1;
                    if (IsValid24Hour(tempNewValue))
                        startTime.Hour--;
                }

                else if (SelectedTextbox.Name == "TbMinute")
                {
                    double tempNewValue = startTime.Minute - 1;
                    if (IsValidMinute(tempNewValue))
                        startTime.Minute--;
                }

                else if (SelectedTextbox.Name == "TbSecond")
                {
                    double tempNewValue = startTime.Second - 1;
                    if (IsValidSecond(tempNewValue))
                        startTime.Second--;
                }

            }
        }


        private bool IsValid24Hour(double hour)
        {
            return hour >= 0 && hour <= 23;
        }

        private bool IsValidMinute(double minute)
        {
            return minute >= 0 && minute <= 59;
        }

        private bool IsValidSecond(double second)
        {
            return second >= 0 && second <= 59;
        }

       
    }
}
