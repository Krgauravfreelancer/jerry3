using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimePicker_UserControl
{
    /// <summary>
    /// Interaction logic for TimePicker_UserControl.xaml
    /// </summary>
    public partial class TimePicker_UserControl : UserControl
	{
        public TimePicker_UserControl()
        {
            this.InitializeComponent();
            Set("00:00:00");
        }

        public TimePicker_UserControl(string initialTime = "00:00:00")
		{
			this.InitializeComponent();
			Set(initialTime);
		}


		public void Set(string time)
		{
			if(!string.IsNullOrEmpty(time))
			{
				var arr = time.Split('.');
				if (arr.Length > 0)
				{
                    var TimeBreakupArray = arr[0].Split(':');
                    if (TimeBreakupArray.Length == 3)
                    {
                        HrTxt.Text = TimeBreakupArray[0];
                        MinTxt.Text = TimeBreakupArray[1];
                        SecTxt.Text = TimeBreakupArray[2];
                        millisecTxt.Text = arr.Length > 1 ? arr[1] : "000";
                    }
                }
            }
			
		}

		public string Get()
		{
			return $"{HrTxt.Text}:{MinTxt.Text}:{SecTxt.Text}.{millisecTxt.Text}";
		}


		private void BtnUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (HrBtn.IsChecked == true)
			{
				int hour = int.Parse(HrTxt.Text);
				
				if (hour == 12)
					hour = hour - hour;
				
				hour++;

                if (hour.ToString().Length == 1)
                    HrTxt.Text = "0" + hour.ToString();
				else
					HrTxt.Text = hour.ToString();
			}
			else if (MinBtn.IsChecked == true)
			{
				int min = int.Parse(MinTxt.Text);
				
				if (min == 59)
					min = -1;
				
				min++;
				
				if (min.ToString().Length == 1)
				{
					MinTxt.Text = "0" + min.ToString();
				}
				else
				{
					MinTxt.Text = min.ToString();
				}
			}
			else if (SecBtn.IsChecked == true)
			{
				int sec = int.Parse(SecTxt.Text);
				
				if (sec == 59)
					sec = -1;
				
				sec++;
				
				if (sec.ToString().Length == 1)
				{
					SecTxt.Text = "0" + sec.ToString();
				}
				else
				{
					SecTxt.Text = sec.ToString();
				}
			}
            else if (millisecBtn.IsChecked == true)
            {
                int ms = int.Parse(millisecTxt.Text);

                if (ms == 999)
                    ms = -1;

                ms++;

                if (ms.ToString().Length == 1)
                {
                    millisecTxt.Text = "00" + ms.ToString();
                }
				else if (ms.ToString().Length == 2)
                {
                    millisecTxt.Text = "0" + ms.ToString();
                }
                else
                {
                    millisecTxt.Text = ms.ToString();
                }
            }
            else 
			{
				MessageBox.Show("Please select either Hour, Minute or Seconds to Toggle", "Warning", MessageBoxButton.OK, MessageBoxImage.Stop);
			}
		}

		private void btnDown_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			if (HrBtn.IsChecked == true)
			{
				int hour = int.Parse(HrTxt.Text);

				if (hour == 1)
					hour = 13;

				hour--;

				HrTxt.Text = hour.ToString();
			}
			else if (MinBtn.IsChecked == true)
			{
				int min = int.Parse(MinTxt.Text);

				if (min == 0)
					min = 60;

				min--;


				if (min.ToString().Length == 1)
				{
					MinTxt.Text = "0" + min.ToString();
				}
				else
				{
					MinTxt.Text = min.ToString();
				}
			}
			else if (SecBtn.IsChecked == true)
			{
				int sec = int.Parse(SecTxt.Text);

				if (sec == 0)
					sec = 60;

				sec--;


				if (sec.ToString().Length == 1)
				{
					SecTxt.Text = "0" + sec.ToString();
				}
				else
				{
					SecTxt.Text = sec.ToString();
				}

			}
            else if (millisecBtn.IsChecked == true)
            {
                int ms = int.Parse(millisecTxt.Text);

                if (ms == 0)
                    ms = 1000;

                ms--;

                if (ms.ToString().Length == 1)
                {
                    millisecTxt.Text = "00" + ms.ToString();
                }
                else if (ms.ToString().Length == 2)
                {
                    millisecTxt.Text = "0" + ms.ToString();
                }
                else
                {
                    millisecTxt.Text = ms.ToString();
                }
            }
            else
			{
				MessageBox.Show("Please select either Hour, Minute or Seconds to Toggle", "Warning", MessageBoxButton.OK, MessageBoxImage.Stop);
			}
        }
	}
}