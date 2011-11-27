﻿using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Microsoft.Reactive.Testing;
using MetroRx;

namespace MetroRx.Testing
{
    public static class TestUtils
    {
        /// <summary>
        /// WithScheduler overrides the default Deferred and Taskpool schedulers
        /// with the given scheduler until the return value is disposed. This
        /// is useful in a unit test runner to force RxXaml objects to schedule
        /// via a TestScheduler object.
        /// </summary>
        /// <param name="sched">The scheduler to use.</param>
        /// <returns>An object that when disposed, restores the previous default
        /// schedulers.</returns>
        public static IDisposable WithScheduler(IScheduler sched)
        {
            var prevDef = RxApp.DeferredScheduler;
            var prevTask = RxApp.TaskpoolScheduler;

            RxApp.DeferredScheduler = sched;
            RxApp.TaskpoolScheduler = sched;

            return Disposable.Create(() => {
                RxApp.DeferredScheduler = prevDef;
                RxApp.TaskpoolScheduler = prevTask;
            });
        }

        /// <summary>
        /// With is an extension method that uses the given scheduler as the
        /// default Deferred and Taskpool schedulers for the given Func. Use
        /// this to initialize objects that store the default scheduler (most
        /// RxXaml objects).
        /// </summary>
        /// <param name="sched">The scheduler to use.</param>
        /// <param name="block">The function to execute.</param>
        /// <returns>The return value of the function.</returns>
        public static TRet With<TRet>(this IScheduler sched, Func<IScheduler, TRet> block)
        {
            TRet ret;
            using (WithScheduler(sched)) {
                ret = block(sched);
            }
            return ret;
        }

        /// <summary>
        /// With is an extension method that uses the given scheduler as the
        /// default Deferred and Taskpool schedulers for the given Action. 
        /// </summary>
        /// <param name="sched">The scheduler to use.</param>
        /// <param name="block">The action to execute.</param>
        public static void With(this IScheduler sched, Action<IScheduler> block)
        {
            sched.With(x => { block(x); return 0; });
        }

        /// <summary>
        /// With is an extension method that uses the given scheduler as the
        /// default Deferred and Taskpool schedulers for the given Func. Use
        /// this to initialize objects that store the default scheduler (most
        /// RxXaml objects).
        /// </summary>
        /// <param name="sched">The scheduler to use.</param>
        /// <param name="block">The function to execute.</param>
        /// <returns>The return value of the function.</returns>
        public static TRet With<TRet>(this TestScheduler sched, Func<TestScheduler, TRet> block)
        {
            TRet ret;
            using (WithScheduler(sched)) {
                ret = block(sched);
            }
            return ret;
        }

        /// <summary>
        /// With is an extension method that uses the given scheduler as the
        /// default Deferred and Taskpool schedulers for the given Action. 
        /// </summary>
        /// <param name="sched">The scheduler to use.</param>
        /// <param name="block">The action to execute.</param>
        public static void With(this TestScheduler sched, Action<TestScheduler> block)
        {
            sched.With(x => { block(x); return 0; });
        }

        public static long FromTimeSpan(this TestScheduler sched, TimeSpan span)
        {
            return span.Ticks;
        }
    }
}

// vim: tw=120 ts=4 sw=4 et :