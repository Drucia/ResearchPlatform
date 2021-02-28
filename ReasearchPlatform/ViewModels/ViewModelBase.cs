using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ResearchPlatform.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T Storage, T Value, [CallerMemberName] string Propertname = null)
        {
            if (EqualityComparer<T>.Default.Equals(Storage, Value)) return false;

            Storage = Value;
            OnPropertyChanged(Propertname);
            return true;
        }
    }
}
