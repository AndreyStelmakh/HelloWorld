using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Archive.UserControls.Mvvm.Extended
{
    /// <summary>
    /// Реализация уведомления об изменениях
    /// </summary>
    public abstract class ExtendedNotify : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged<T>(Expression<Func<T>> action)
        {
            RaisePropertyChanged(GetPropertyName(action));
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;

            return expression.Member.Name;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}