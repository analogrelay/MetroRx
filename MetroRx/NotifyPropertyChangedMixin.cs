using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MetroRx
{
    public static class ReactiveNotifyPropertyChangedMixin
    {
        /// <summary>
        /// ObservableForProperty returns an Observable representing the
        /// property change notifications for a specific property on a
        /// ReactiveObject. This method (unlike other Observables that return
        /// IObservedChange) guarantees that the Value property of
        /// the IObservedChange is set.
        /// </summary>
        /// <param name="property">An Expression representing the property (i.e.
        /// 'x => x.SomeProperty.SomeOtherProperty'</param>
        /// <param name="beforeChange">If True, the Observable will notify
        /// immediately before a property is going to change.</param>
        /// <returns>An Observable representing the property change
        /// notifications for the given property.</returns>
        public static IObservable<IObservedChange<TSender, TValue>> FromProperty<TSender, TValue>(
                this TSender This,
                Expression<Func<TSender, TValue>> property)
            where TSender : INotifyPropertyChanged
        {
            var propName = RxApp.simpleExpressionToPropertyName(property);
            var pi = RxApp.getPropertyInfoForProperty<TSender>(propName);

            var ret = Observable.Create<PropertyChangedEventArgs>(subj => {
                PropertyChangedEventHandler f = (o,e) => subj.OnNext(e);
                This.PropertyChanged += f;
                return () => This.PropertyChanged -= f;
            });

            return ret
                .Where(x => x.PropertyName == propName)
                .Select(x => new ObservedChange<TSender, TValue>(This, propName, (TValue)pi.GetValue(This)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IObservable<IObservedChange<TSender, object>> Changed<TSender>(this TSender This)
            where TSender : INotifyPropertyChanged
        {
            var ret = Observable.Create<PropertyChangedEventArgs>(subj => {
                PropertyChangedEventHandler f = (o,e) => subj.OnNext(e);
                This.PropertyChanged += f;
                return () => This.PropertyChanged -= f;
            });

            return ret.Select(x => new ObservedChange<TSender, object>(
                This, x.PropertyName, RxApp.getPropertyInfoForProperty(typeof(TSender), x.PropertyName).GetValue(This)));
        }
    }
}

// vim: tw=120 ts=4 sw=4 et :