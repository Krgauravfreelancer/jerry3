using System;
using System.Windows;
using System.Windows.Controls;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Interaction logic for EventBlockTooltipControl.xaml
    /// </summary>
    public partial class EventBlockTooltipControl : UserControl
    {
        public EventBlockTooltipControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        // Dependency property for EventHover text
        public static readonly DependencyProperty EventBlockTooltipTextProperty =
            DependencyProperty.Register("EventBlockTooltipText", typeof(string), typeof(EventBlockTooltipControl), new PropertyMetadata(String.Empty));

        public string EventBlockTooltipText
        {
            get { return (string)GetValue(EventBlockTooltipTextProperty); }
            set { SetValue(EventBlockTooltipTextProperty, value); }
        }
    }
}
