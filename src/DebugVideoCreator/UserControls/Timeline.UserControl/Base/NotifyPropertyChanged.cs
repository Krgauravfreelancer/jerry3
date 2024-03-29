﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Timeline.UserControls.Base
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            RaisePropertyChanged(PropertyName);
        }

        protected void RaiseAllChanged()
        {
            RaisePropertyChanged("");
        }

        protected bool Set<T>(ref T Field, T Value, [CallerMemberName] string PropertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(Field, Value))
                return false;

            Field = Value;

            RaisePropertyChanged(PropertyName);

            return true;
        }

        protected bool SetAndAlwaysRaisePropertyChanged<T>(ref T Field, T Value, [CallerMemberName] string PropertyName = null)
        {
            

            Field = Value;

            RaisePropertyChanged(PropertyName);

            return true;
        }
    }
}
