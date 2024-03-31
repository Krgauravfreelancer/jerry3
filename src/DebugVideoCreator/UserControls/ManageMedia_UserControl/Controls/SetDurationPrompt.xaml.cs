using ManageMedia_UserControl.Classes;
using ScreenRecorder_UserControl.Models;
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

namespace ManageMedia_UserControl.Controls
{
    /// <summary>
    /// Interaction logic for SetDurationPrompt.xaml
    /// </summary>
    public partial class SetDurationPrompt : UserControl
    {
        Media _Media;

        TimeSpan NewDuration = TimeSpan.FromSeconds(0);

        public SetDurationPrompt(Media media)
        {
            InitializeComponent();
            _Media = media;

            NewDuration = _Media.Duration;

            if (_Media.ImageType != null)
            {
                NameTxt.Text = FirstCharToUpper(_Media.ImageType);
            }
            else
            {
                NameTxt.Text = "";
            }

            if (_Media.mediaType == MediaType.Video)
            {
                OverideDefaultBox.IsChecked = false;
                OverideDefaultBox.IsEnabled = false;
                OverideDefaultTxt.Opacity = 0.5;
            }

            UpdateTime();
        }

        private string FirstCharToUpper(string input)
        {
            if (input.Length > 0)
            {
                return input[0].ToString().ToUpper() + input.Substring(1);
            }
            else
            {
                return input;
            }
        }

        private void UpdateTime()
        {
            StartTxt.Text = _Media.StartTime.ToString(@"mm\:ss\.fff");
            DurationTxt.Text = NewDuration.ToString(@"mm\:ss\.fff");
            DefaultTimeTxt.Text = _Media.OriginalDuration.ToString(@"mm\:ss\.fff");
        }

        private void DurationMinusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_Media != null)
            {
                NewDuration = NewDuration - TimeSpan.FromSeconds(1);
                if (NewDuration < TimeSpan.FromSeconds(1))
                {
                    NewDuration = TimeSpan.FromSeconds(1);
                }

                UpdateTime();
            }
        }

        private void DurationAddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_Media != null)
            {
                NewDuration = NewDuration + TimeSpan.FromSeconds(1);
                UpdateTime();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool Number = IsNumber(e.Text);
            bool ColonCount = ColonCountMaxed(((TextBox)sender).Text + e.Text);
            bool PeriodCount = PeriodCountMaxed(((TextBox)sender).Text + e.Text);


            if (Number == false && ColonCount == false && PeriodCount == false)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private bool ColonCountMaxed(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ':')
                {
                    count++;
                }
            }

            if (count > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool PeriodCountMaxed(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '.')
                {
                    count++;
                }
            }

            if (count > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsNumber(string text)
        {
            char[] AcceptedCharacters = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ':', '.' };

            if (text != null)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    bool foundInAcceptedChar = false;
                    for (int x = 0; x < AcceptedCharacters.Length; x++)
                    {
                        if (AcceptedCharacters[x] == text[i])
                        {
                            foundInAcceptedChar = true;
                            break;
                        }
                    }

                    if (foundInAcceptedChar == false)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool ProcessTextChange = true;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProcessTextChange)
            {
                ProcessTextChange = false;

                string text = ((TextBox)sender).Text;

                if (text.Length > 0)
                {
                    if (text.LastOrDefault() == ':')
                    {
                        string sub = text.Substring(0, text.Length - 1);


                        bool result = int.TryParse(sub, out int value);

                        if (result == true)
                        {
                            if (value > 59)
                            {
                                value = 59;
                            }

                            sub = value.ToString("00");
                        }
                        else
                        {
                            sub = "00";
                        }

                        ((TextBox)sender).Text = sub + ':';
                        ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
                    }

                    if (text.LastOrDefault() == '.')
                    {
                        string sub = text.Substring(0, text.Length - 1);

                        string[] sections = sub.Split(':');

                        if (sections.Length == 2)
                        {
                            bool result = int.TryParse(sections[0], out int value);

                            if (result == true)
                            {
                                if (value > 59)
                                {
                                    value = 59;
                                }

                                sections[0] = value.ToString("00");
                            }
                            else
                            {
                                sections[0] = "00";
                            }

                            bool result2 = int.TryParse(sections[1], out int value1);

                            if (result2 == true)
                            {
                                if (value1 > 59)
                                {
                                    value1 = 59;
                                }

                                sections[1] = value1.ToString("00");
                            }
                            else
                            {
                                sections[1] = "00";
                            }

                            ((TextBox)sender).Text = sections[0] + ':' + sections[1] + '.';
                            ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
                        }
                        else if (sections.Length == 1)
                        {
                            bool result = int.TryParse(sections[0], out int value);

                            if (result == true)
                            {
                                if (value > 59)
                                {
                                    value = 59;
                                }

                                sections[0] = value.ToString("00");
                            }
                            else
                            {
                                sections[0] = "00";
                            }

                            ((TextBox)sender).Text = "00" + ':' + sections[0] + '.';
                            ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
                        }
                    }
                }

                ProcessTextChange = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int Minutes = 0;
            int Seconds = 0;
            int Milliseconds = 0;

            if (sender is TextBox)
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    string[] values = textBox.Text.Split(new char[] { ':', '.' });

                    if (values.Length == 1)
                    {
                        if (values[0].Length == 1)
                        {
                            //This should be seconds
                            bool result = int.TryParse(values[0], out int secondValue);
                            if (result == true)
                            {
                                Seconds = secondValue;
                            }
                        }
                        else if (values[0].Length == 2)
                        {
                            //This should be seconds
                            bool result = int.TryParse(values[0], out int secondValue);
                            if (result == true)
                            {
                                Seconds = secondValue;
                            }
                        }
                        else if (values[0].Length == 3)
                        {
                            //This should be milliseconds
                            bool result = int.TryParse(values[0], out int millisecondsValue);
                            if (result == true)
                            {
                                Milliseconds = millisecondsValue;
                            }
                        }

                    }
                    else if (values.Length == 2)
                    {
                        if (values[1].Length == 2)
                        {
                            //This should be seconds and the first section should be minutes
                            bool MinutesResult = int.TryParse(values[0], out int minuteValue);
                            bool SecondsResult = int.TryParse(values[1], out int secondValue);
                            if (MinutesResult == true)
                            {
                                Minutes = minuteValue;
                            }
                            if (SecondsResult == true)
                            {
                                Seconds = secondValue;
                            }
                        }
                        else if (values[1].Length == 3)
                        {
                            //This should be milliseconds and the first section should be seconds
                            bool MillisecondResult = int.TryParse(values[1], out int millisecondsValue);
                            bool SecondsResult = int.TryParse(values[0], out int secondValue);
                            if (MillisecondResult == true)
                            {
                                Milliseconds = millisecondsValue;
                            }
                            if (SecondsResult == true)
                            {
                                Seconds = secondValue;
                            }
                        }

                    }
                    else if (values.Length == 3)
                    {
                        bool MinutesResult = int.TryParse(values[0], out int minuteValue);
                        bool SecondsResult = int.TryParse(values[1], out int secondValue);
                        bool MillisecondResult = int.TryParse(values[2], out int millisecondsValue);

                        if (MinutesResult == true)
                        {
                            Minutes = minuteValue;
                        }
                        if (MillisecondResult == true)
                        {
                            Milliseconds = millisecondsValue;
                        }
                        if (SecondsResult == true)
                        {
                            Seconds = secondValue;
                        }

                    }

                    TimeSpan timeSpan = new TimeSpan(0, 0, Minutes, Seconds, Milliseconds);
                    NewDuration = timeSpan;
                    UpdateTime();
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveClicked != null)
            {
                if (OverideDefaultBox.IsChecked == true)
                {
                    SaveClicked(this, new SaveDurationEventArgs(_Media,NewDuration, true));
                }
                else
                {
                    SaveClicked(this, new SaveDurationEventArgs(_Media, NewDuration, false));
                }
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
            {
                CancelClicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler<SaveDurationEventArgs> SaveClicked;

        public event EventHandler<EventArgs> CancelClicked;
    }

    public class SaveDurationEventArgs : EventArgs
    {
        public TimeSpan NewDuration { get; set; }
        public bool UpdateOriginalTime { get; set; }
        public Media Media { get; set; }

        public SaveDurationEventArgs(Media media,TimeSpan newDuration, bool updateOriginalTime)
        {
            NewDuration = newDuration;
            UpdateOriginalTime = updateOriginalTime;
            Media = media;
        }
    }
}
