using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Input;
using MetroRx;

namespace MetroRx.Xaml
{
    /// <summary>
    /// IReactiveCommand is an Rx-enabled version of ICommand that is also an
    /// Observable. Its Observable fires once for each invocation of
    /// ICommand.Execute and its value is the CommandParameter that was
    /// provided.
    /// </summary>
    public class ReactiveCommand : ICommand, IObservable<object>, IEnableLogger, IDisposable
    {
        /// <summary>
        /// Creates a new ReactiveCommand object.
        /// </summary>
        /// <param name="canExecute">An Observable, often obtained via
        /// ObservableFromProperty, that defines when the Command can
        /// execute.</param>
        /// <param name="scheduler">The scheduler to publish events on - default
        /// is RxApp.DeferredScheduler.</param>
        public ReactiveCommand(IObservable<bool> canExecute = null, IScheduler scheduler = null)
        {
            scheduler = scheduler ?? RxApp.DeferredScheduler;

            var ceSubj = (canExecute ?? Observable.Return(true))
                .Multicast(new ScheduledSubject<bool>(scheduler));

            CanExecuteObservable = ceSubj.DistinctUntilChanged();
            ceSubj.Subscribe(
                x => {
                    if (x == _latestCanExecute) return;
                    _latestCanExecute = x;
                    if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
                },
                ex => { this.Log().Warn("CanExecute threw", ex); _latestCanExecute = false; });

            _executeSubject = new ScheduledSubject<object>(scheduler);

            _inner = ceSubj.Connect();
        }

        /// <summary>
        /// Fires whenever the CanExecute of the ICommand changes. 
        /// </summary>
        public IObservable<bool> CanExecuteObservable { get; protected set; }

        bool _latestCanExecute = true;
        public virtual bool CanExecute(object parameter)
        {
            return _latestCanExecute;
        }

        public event EventHandler CanExecuteChanged;

        ScheduledSubject<object> _executeSubject;
        IDisposable _inner;

        public void Execute(object parameter)
        {
            this.Log().InfoFormat("{0:X}: Executed", this.GetHashCode());
            _executeSubject.OnNext(parameter);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _executeSubject.Subscribe(observer);
        }

        public void Dispose()
        {
            var inner = Interlocked.Exchange(ref _inner, null);
            if (inner != null) {
                inner.Dispose();
            }
        }
    }

    public static class ReactiveCommandMixins
    {
        /// <summary>
        /// ToCommand is a convenience method for returning a new
        /// ReactiveCommand based on an existing Observable chain.
        /// </summary>
        /// <param name="scheduler">The scheduler to publish events on - default
        /// is RxApp.DeferredScheduler.</param>
        /// <returns>A new ReactiveCommand whose CanExecute Observable is the
        /// current object.</returns>
        public static ReactiveCommand ToCommand(this IObservable<bool> This, IScheduler scheduler = null)
        {
            return new ReactiveCommand(This, scheduler);
        }

        /// <summary>
        /// A utility method that will pipe an Observable to an ICommand (i.e.
        /// it will first call its CanExecute with the provided value, then if
        /// the command can be executed, Execute() will be called)
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        /// <returns>An object that when disposes, disconnects the Observable
        /// from the command.</returns>
        public static IDisposable InvokeCommand<T>(this IObservable<T> This, ICommand command)
        {
            return This.Subscribe(x => {
                if (!command.CanExecute(x)) {
                    return;
                }
                command.Execute(x);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static ReactiveCommand AsProxyCommand(this ICommand This, IScheduler scheduler = null)
        {
            var ce = Observable.Create<Unit>(subj => {
                EventHandler eh = (o, e) => subj.OnNext(Unit.Default);
                This.CanExecuteChanged += eh;
                return () => This.CanExecuteChanged -= eh;
            });

            var ret = new ReactiveCommand(ce.Select(_ => This.CanExecute(null)), scheduler);
            ret.Subscribe(x => This.Execute(x));
            return ret;
        }
    }
}

// vim: tw=120 ts=4 sw=4 et :