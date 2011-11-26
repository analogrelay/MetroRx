using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroRx
{
    /// <summary>
    /// IObservedChange is a generic interface that replaces the non-generic
    /// PropertyChangedEventArgs. Note that it is used for both Changing (i.e.
    /// 'before change') and Changed Observables. In the future, this interface
    /// will be Covariant which will allow simpler casting between specific and
    /// generic changes.
    /// </summary>
    public interface IObservedChange<TSender, TValue>
    {
        /// <summary>
        /// The object that has raised the change.
        /// </summary>
        TSender Sender { get; }

        /// <summary>
        /// The name of the property that has changed on Sender.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// The value of the property that has changed. IMPORTANT NOTE: This
        /// property is often not set for performance reasons, unless you have
        /// explicitly requested an Observable for a property via a method such
        /// as ObservableForProperty. To retrieve the value for the property,
        /// use the Value() extension method.
        /// </summary>
        TValue Value { get; }
    }

    public class ObservedChange<TSender, TValue> : IObservedChange<TSender, TValue>
    {
        public TSender Sender { get; protected set; }
        public string PropertyName { get; protected set; }
        public TValue Value { get; protected set; }

        public ObservedChange(TSender sender, string propertyName, TValue value)
        {
            Sender = sender;
            PropertyName = propertyName;
            Value = value;
        }
    }
}