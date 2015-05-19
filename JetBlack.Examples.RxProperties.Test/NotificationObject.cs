using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace JetBlack.Examples.RxProperties.Test
{
    public class NotificationObject : INotifyPropertyChanged
    {
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = PropertyChangeExtensions.ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
