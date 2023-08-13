using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TestAudio_UserControl
{
    public partial class TestAudioUserControl : UserControl
    {
        private int project_id;

        public TestAudioUserControl()
        {
            InitializeComponent();
        }
        

        #region == Public Functions ==

        public void SetSelectedProjectId(int _project_id)
        {
            project_id = _project_id;
        }

        #endregion
        
    }
}