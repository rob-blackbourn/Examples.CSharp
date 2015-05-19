using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace JetBlack.Examples.RxProperties
{
    public static class PropertyChangeExtensions
    {
        public static IObservable<PropertyChange> ToObservable<TSource>(this TSource viewModel)
            where TSource : INotifyPropertyChanged
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                (h => viewModel.PropertyChanged += h),
                (h => viewModel.PropertyChanged -= h))
                .Select(x =>
                {
                    Expression expr = Expression.Property(Expression.Constant(viewModel), x.EventArgs.PropertyName);
                    var value = Expression.Lambda(expr).Compile().DynamicInvoke();
                    return new PropertyChange(x.EventArgs.PropertyName, value);
                });
        }

        public static IObservable<TProperty> ToObservable<TSource, TProperty>(this TSource viewModel, Expression<Func<TSource, TProperty>> propertyExpression)
            where TSource : INotifyPropertyChanged
        {
            var propertyName = GetPropertyName(propertyExpression);

            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                (h => viewModel.PropertyChanged += h),
                (h => viewModel.PropertyChanged -= h))
                .Where(x => x.EventArgs.PropertyName == propertyName)
                .Select(x => propertyExpression.Compile()(viewModel));
        }

        public static string GetPropertyName<TSource, TProperty>(Expression<Func<TSource, TProperty>> property)
        {
            var lambda = (LambdaExpression)property;

            var memberExpression =
                lambda.Body is UnaryExpression
                ? (MemberExpression)(((UnaryExpression)lambda.Body).Operand)
                : (MemberExpression)lambda.Body;

            return memberExpression.Member.Name;
        }

        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("No member access", "propertyExpression");

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("The expression not a property", "propertyExpression");

            var getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
                throw new ArgumentException("The property cannot be static", "propertyExpression");

            return memberExpression.Member.Name;
        }
    }
}
