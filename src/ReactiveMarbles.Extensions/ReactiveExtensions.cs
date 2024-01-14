// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReactiveMarbles.Extensions.Internal;

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Extension methods for <see cref="System.Reactive"/>.
/// </summary>
public static class ReactiveExtensions
{
    private static readonly Dictionary<TimeSpan, Lazy<IConnectableObservable<DateTime>>> _timerList = [];

    /// <summary>
    /// Returns only values that are not null.
    /// Converts the nullability.
    /// </summary>
    /// <typeparam name="T">The type of value emitted by the observable.</typeparam>
    /// <param name="observable">The observable that can contain nulls.</param>
    /// <returns>A non nullable version of the observable that only emits valid values.</returns>
    public static IObservable<T> WhereIsNotNull<T>(this IObservable<T> observable) =>
        observable
            .Where(x => x is not null);

    /// <summary>
    /// Change the source observable type to <see cref="Unit"/>.
    /// This allows us to be notified when the observable emits a value.
    /// </summary>
    /// <typeparam name="T">The current type of the observable.</typeparam>
    /// <param name="observable">The observable to convert.</param>
    /// <returns>The signal.</returns>
    public static IObservable<Unit> AsSignal<T>(this IObservable<T> observable) =>
        observable
            .Select(_ => Unit.Default);

    /// <summary>
    /// Synchronized timer all instances of this with the same TimeSpan use the same timer.
    /// </summary>
    /// <param name="timeSpan">The time span.</param>
    /// <returns>An Observable DateTime.</returns>
    public static IObservable<DateTime> SyncTimer(TimeSpan timeSpan)
    {
        if (!_timerList.TryGetValue(timeSpan, out var value))
        {
            value = new Lazy<IConnectableObservable<DateTime>>(() => Observable.Timer(TimeSpan.FromMilliseconds(0), timeSpan).Timestamp().Select(x => x.Timestamp.DateTime).Publish());
            _timerList.Add(timeSpan, value);
            _timerList[timeSpan].Value.Connect();
        }

        return value.Value;
    }

    /// <summary>
    /// Buffers until Start char and End char are found.
    /// </summary>
    /// <param name="this">The this.</param>
    /// <param name="startsWith">The starts with.</param>
    /// <param name="endsWith">The ends with.</param>
    /// <returns>A Value.</returns>
    public static IObservable<string> BufferUntil(this IObservable<char> @this, char startsWith, char endsWith) =>
        Observable.Create<string>(o =>
        {
            StringBuilder sb = new();
            var startFound = false;
            var sub = @this.Subscribe(s =>
            {
                if (startFound || s == startsWith)
                {
                    startFound = true;
                    sb.Append(s);
                    if (s == endsWith)
                    {
                        o.OnNext(sb.ToString());
                        startFound = false;
                        sb.Clear();
                    }
                }
            });
            return new CompositeDisposable(sub);
        });

    /// <summary>
    /// Catch exception and return Observable.Empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> CatchIgnore<TSource>(this IObservable<TSource?> source) =>
        source.Catch(Observable.Empty<TSource?>());

    /// <summary>
    /// Catch exception and return Observable.Empty.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="errorAction">The error action.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> CatchIgnore<TSource, TException>(this IObservable<TSource?> source, Action<TException> errorAction)
        where TException : Exception =>
            source.Catch((TException ex) =>
            {
                errorAction(ex);
                return Observable.Empty<TSource?>();
            });

    /// <summary>
    /// Latest values of each sequence are all false.
    /// </summary>
    /// <param name="sources">The sources.</param>
    /// <returns>A Value.</returns>
    public static IObservable<bool> CombineLatestValuesAreAllFalse(this IEnumerable<IObservable<bool>> sources) =>
        sources.CombineLatest(xs => xs.All(x => !x));

    /// <summary>
    /// Latest values of each sequence are all true.
    /// </summary>
    /// <param name="sources">The sources.</param>
    /// <returns>A Value.</returns>
    public static IObservable<bool> CombineLatestValuesAreAllTrue(this IEnumerable<IObservable<bool>> sources) =>
        sources.CombineLatest(xs => xs.All(x => x));

    /// <summary>
    /// Gets the maximum from all sources.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="sources">The sources.</param>
    /// <returns>A Value.</returns>
    public static IObservable<T?> GetMax<T>(this IObservable<T?> @this, params IObservable<T?>[] sources)
        where T : struct
    {
        List<IObservable<T?>> source = [@this, .. sources];
        return source.CombineLatest().Select(x => x.Max());
    }

    /// <summary>
    /// Gets the minimum from all sources.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="sources">The sources.</param>
    /// <returns>A Value.</returns>
    public static IObservable<T?> GetMin<T>(this IObservable<T?> @this, params IObservable<T?>[] sources)
        where T : struct
    {
        List<IObservable<T?>> source = [@this, .. sources];
        return source.CombineLatest().Select(x => x.Min());
    }

    /// <summary>
    /// Detects when a stream becomes inactive for some period of time.
    /// </summary>
    /// <typeparam name="T">update type.</typeparam>
    /// <param name="source">source stream.</param>
    /// <param name="stalenessPeriod">
    /// if source steam does not OnNext any update during this period, it is declared staled.
    /// </param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>Observable Stale T.</returns>
    public static IObservable<IStale<T>> DetectStale<T>(this IObservable<T> source, TimeSpan stalenessPeriod, IScheduler scheduler) =>
        Observable.Create<IStale<T>>(observer =>
        {
            SerialDisposable timerSubscription = new();
            object observerLock = new();

            void ScheduleStale() =>
                    timerSubscription!.Disposable = Observable.Timer(stalenessPeriod, scheduler)
                    .Subscribe(_ =>
                    {
                        lock (observerLock)
                        {
                            observer.OnNext(new Stale<T>());
                        }
                    });

            var sourceSubscription = source.Subscribe(
                x =>
                {
                    // cancel any scheduled stale update
                    (timerSubscription?.Disposable)?.Dispose();

                    lock (observerLock)
                    {
                        observer.OnNext(new Stale<T>(x));
                    }

                    ScheduleStale();
                },
                observer.OnError,
                observer.OnCompleted);

            ScheduleStale();

            return new CompositeDisposable
            {
                sourceSubscription,
                timerSubscription
            };
        });

    /// <summary>
    /// Applies a conflation algorithm to an observable stream. Anytime the stream OnNext twice
    /// below minimumUpdatePeriod, the second update gets delayed to respect the
    /// minimumUpdatePeriod If more than 2 update happen, only the last update is pushed Updates
    /// are pushed and rescheduled using the provided scheduler.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">the stream.</param>
    /// <param name="minimumUpdatePeriod">minimum delay between 2 updates.</param>
    /// <param name="scheduler">to be used to publish updates and schedule delayed updates.</param>
    /// <returns>Observable T.</returns>
    public static IObservable<T> Conflate<T>(this IObservable<T> source, TimeSpan minimumUpdatePeriod, IScheduler scheduler) =>
        Observable.Create<T>(observer =>
        {
            // indicate when the last update was published
            var lastUpdateTime = DateTimeOffset.MinValue;

            // indicate if an update is currently scheduled
            MultipleAssignmentDisposable updateScheduled = new();

            // indicate if completion has been requested (we can't complete immediately if an
            // update is in flight)
            var completionRequested = false;
            object gate = new();

            return source.ObserveOn(scheduler)
                .Subscribe(
                x =>
                {
                    var currentUpdateTime = scheduler.Now;

                    bool scheduleRequired;
                    lock (gate)
                    {
                        scheduleRequired = currentUpdateTime - lastUpdateTime < minimumUpdatePeriod;
                        if (scheduleRequired && updateScheduled.Disposable != null)
                        {
                            updateScheduled.Disposable.Dispose();
                            updateScheduled.Disposable = null;
                        }
                    }

                    if (scheduleRequired)
                    {
                        updateScheduled.Disposable = scheduler.Schedule(
                    lastUpdateTime + minimumUpdatePeriod,
                    () =>
                    {
                        observer.OnNext(x);

                        lock (gate)
                        {
                            lastUpdateTime = scheduler.Now;
                            updateScheduled.Disposable = null;
                            if (completionRequested)
                            {
                                observer.OnCompleted();
                            }
                        }
                    });
                    }
                    else
                    {
                        observer.OnNext(x);
                        lock (gate)
                        {
                            lastUpdateTime = scheduler.Now;
                        }
                    }
                },
                observer.OnError,
                () =>
                {
                    // if we have scheduled an update we need to complete once the update has been published
                    if (updateScheduled.Disposable != null)
                    {
                        lock (gate)
                        {
                            completionRequested = true;
                        }
                    }
                    else
                    {
                        observer.OnCompleted();
                    }
                });
        });

    /// <summary>
    /// Injects heartbeats in a stream when the source stream becomes quiet:
    /// - upon subscription if the source does not OnNext any update a heartbeat will be pushed
    ///   after heartbeat Period, periodically until source receives an update
    /// - when an update is received it is immediately pushed. After this update, if source does
    ///   not OnNext after heartbeat Period, heartbeats will be pushed.
    /// </summary>
    /// <typeparam name="T">update type.</typeparam>
    /// <param name="source">source stream.</param>
    /// <param name="heartbeatPeriod">The heartbeat period.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>Observable Heartbeat T.</returns>
    public static IObservable<IHeartbeat<T>> Heartbeat<T>(this IObservable<T> source, TimeSpan heartbeatPeriod, IScheduler scheduler) =>
        Observable.Create<IHeartbeat<T>>(observer =>
        {
            MultipleAssignmentDisposable heartbeatTimerSubscription = new();
            object gate = new();

            void ScheduleHeartbeats()
            {
                var disposable = Observable.Timer(heartbeatPeriod, heartbeatPeriod, scheduler)
                .Subscribe(_ => observer.OnNext(new Heartbeat<T>()));

                lock (gate)
                {
                    heartbeatTimerSubscription!.Disposable = disposable;
                }
            }

            var sourceSubscription = source.Subscribe(
                x =>
                {
                    lock (gate)
                    {
                        // cancel any scheduled heartbeat
                        heartbeatTimerSubscription?.Disposable?.Dispose();
                    }

                    observer.OnNext(new Heartbeat<T>(x));

                    ScheduleHeartbeats();
                },
                observer.OnError,
                observer.OnCompleted);

            ScheduleHeartbeats();

            return new CompositeDisposable
                {
                        sourceSubscription,
                        heartbeatTimerSubscription
                };
        });

    /// <summary>
    /// Executes With limited concurrency.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="taskFunctions">The task functions.</param>
    /// <param name="maxConcurrency">The maximum concurrency.</param>
    /// <returns>A Value.</returns>
    public static IObservable<T> WithLimitedConcurrency<T>(this IEnumerable<Task<T>> taskFunctions, int maxConcurrency) =>
        new ConcurrencyLimiter<T>(taskFunctions, maxConcurrency).IObservable;

    /// <summary>
    /// Called when [next].
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="observer">The observer.</param>
    /// <param name="events">The events.</param>
    public static void OnNext<T>(this IObserver<T?> observer, params T?[] events) =>
        FastForEach(observer, events);

    /// <summary>
    /// If the scheduler is not Null, wraps the source sequence in order to run its observer callbacks on the specified scheduler.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Source sequence.</param>
    /// <param name="scheduler">Scheduler to notify observers on.</param>
    /// <returns>The source sequence whose observations happen on the specified scheduler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="scheduler"/> is null.</exception>
    /// <remarks>
    /// This only invokes observer callbacks on a scheduler. In case the subscription and/or unsubscription actions have side-effects
    /// that require to be run on a scheduler, use <see cref="Observable.SubscribeOn{TSource}(IObservable{TSource}, IScheduler)"/>.
    /// </remarks>
    public static IObservable<TSource> ObserveOnSafe<TSource>(this IObservable<TSource> source, IScheduler? scheduler) =>
        scheduler == null ? source : source.ObserveOn(scheduler);

    /// <summary>
    /// Invokes the action asynchronously on the specified scheduler, surfacing the result through an observable sequence.
    /// </summary>
    /// <param name="action">Action to run asynchronously.</param>
    /// <param name="scheduler">If the scheduler is not Null, Scheduler to run the action on.</param>
    /// <returns>An observable sequence exposing a Unit value upon completion of the action, or an exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> or <paramref name="scheduler"/> is null.</exception>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>The action is called immediately, not during the subscription of the resulting sequence.</description></item>
    /// <item><description>Multiple subscriptions to the resulting sequence can observe the action's outcome.</description></item>
    /// </list>
    /// </remarks>
    public static IObservable<Unit> Start(Action action, IScheduler? scheduler)
        => scheduler == null ? Observable.Start(action) : Observable.Start(action, scheduler);

    /// <summary>
    /// Invokes the specified function asynchronously on the specified scheduler, surfacing the result through an observable sequence.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function.</typeparam>
    /// <param name="function">Function to run asynchronously.</param>
    /// <param name="scheduler">Scheduler to run the function on.</param>
    /// <returns>An observable sequence exposing the function's result value, or an exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="function"/> or <paramref name="scheduler"/> is null.</exception>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>The function is called immediately, not during the subscription of the resulting sequence.</description></item>
    /// <item><description>Multiple subscriptions to the resulting sequence can observe the function's result.</description></item>
    /// </list>
    /// </remarks>
    public static IObservable<TResult> Start<TResult>(Func<TResult> function, IScheduler? scheduler)
        => scheduler == null ? Observable.Start(function) : Observable.Start(function, scheduler);

    /// <summary>
    /// Foreach from an Observable array.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>
    /// A Value.
    /// </returns>
    public static IObservable<T> ForEach<T>(this IObservable<IEnumerable<T>> source, IScheduler? scheduler = null) =>
        Observable.Create<T>(observer => source.ObserveOnSafe(scheduler).Subscribe(values => FastForEach(observer, values)));

    /// <summary>
    /// If the scheduler is not null, Schedules an action to be executed otherwise executes the action.
    /// </summary>
    /// <param name="scheduler">Scheduler to execute the action on.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> or <paramref name="action"/> is <c>null</c>.</exception>
    public static IDisposable ScheduleSafe(this IScheduler? scheduler, Action action)
    {
        if (scheduler == null)
        {
            action();
            return Disposable.Empty;
        }

        return scheduler!.Schedule(action);
    }

    /// <summary>
    /// Schedules an action to be executed after the specified relative due time.
    /// </summary>
    /// <param name="scheduler">Scheduler to execute the action on.</param>
    /// <param name="dueTime">Relative time after which to execute the action.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> or <paramref name="action"/> is <c>null</c>.</exception>
    public static IDisposable ScheduleSafe(this IScheduler? scheduler, TimeSpan dueTime, Action action)
    {
        if (scheduler == null)
        {
            Thread.Sleep(dueTime);
            action();
            return Disposable.Empty;
        }

        return scheduler!.Schedule(dueTime, action);
    }

    /// <summary>
    /// Froms the array.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>
    /// A Value.
    /// </returns>
    public static IObservable<T> FromArray<T>(this IEnumerable<T> source, IScheduler? scheduler = null) =>
        Observable.Create<T>(observer => scheduler.ScheduleSafe(() => FastForEach(observer, source)));

    /// <summary>
    /// Using the specified object.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="action">The action.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>
    /// An IObservable of Unit.
    /// </returns>
    public static IObservable<Unit> Using<T>(this T obj, Action<T> action, IScheduler? scheduler = null)
        where T : IDisposable
        => Observable.Using(() => obj, id => Start(() => action?.Invoke(id), scheduler));

    /// <summary>
    /// Usings the specified function.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="function">The function.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An IObservable of TResult.</returns>
    public static IObservable<TResult> Using<T, TResult>(this T obj, Func<T, TResult> function, IScheduler? scheduler = null)
        where T : IDisposable
        => Observable.Using(() => obj, id => Start(() => function.Invoke(id), scheduler));

    /// <summary>
    /// Whiles the specified condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="action">The action.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An IObservable of Unit.</returns>
    public static IObservable<Unit> While(Func<bool> condition, Action action, IScheduler? scheduler = null) =>
        Observable.While(condition, Start(action, scheduler));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, TimeSpan dueTime, IScheduler scheduler) =>
        Observable.Create<T>(observer => scheduler.ScheduleSafe(dueTime, () => observer.OnNext(value)));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, TimeSpan dueTime, IScheduler scheduler) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.ScheduleSafe(dueTime, () => observer.OnNext(value))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, DateTimeOffset dueTime, IScheduler scheduler) =>
        Observable.Create<T>(observer => scheduler.Schedule(dueTime, () => observer.OnNext(value)));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, DateTimeOffset dueTime, IScheduler scheduler) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.Schedule(dueTime, () => observer.OnNext(value))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, TimeSpan dueTime, IScheduler scheduler, Action<T> action) =>
        Observable.Create<T>(observer => scheduler.ScheduleSafe(dueTime, () =>
        {
            action(value);
            observer.OnNext(value);
        }));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, TimeSpan dueTime, IScheduler scheduler, Action<T> action) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.ScheduleSafe(dueTime, () =>
        {
            action(value);
            observer.OnNext(value);
        })));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, DateTimeOffset dueTime, IScheduler scheduler, Action<T> action) =>
        Observable.Create<T>(observer => scheduler.Schedule(dueTime, () =>
        {
            action(value);
            observer.OnNext(value);
        }));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, DateTimeOffset dueTime, IScheduler scheduler, Action<T> action) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.Schedule(dueTime, () =>
        {
            action(value);
            observer.OnNext(value);
        })));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, TimeSpan dueTime, IScheduler scheduler, Action action) =>
        Observable.Create<T>(observer => scheduler.ScheduleSafe(dueTime, () =>
        {
            action();
            observer.OnNext(value);
        }));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, TimeSpan dueTime, IScheduler scheduler, Action action) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.ScheduleSafe(dueTime, () =>
        {
            action();
            observer.OnNext(value);
        })));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, DateTimeOffset dueTime, IScheduler scheduler, Action action) =>
        Observable.Create<T>(observer => scheduler.Schedule(dueTime, () =>
        {
            action();
            observer.OnNext(value);
        }));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, DateTimeOffset dueTime, IScheduler scheduler, Action action) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.Schedule(dueTime, () =>
        {
            action();
            observer.OnNext(value);
        })));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="function">The function.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, IScheduler scheduler, Func<T, T> function) =>
        Observable.Create<T>(observer => scheduler.Schedule(() => observer.OnNext(function(value))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="function">The function.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, IScheduler scheduler, Func<T, T> function) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.Schedule(() => observer.OnNext(function(value)))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="function">The function.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, TimeSpan dueTime, IScheduler scheduler, Func<T, T> function) =>
        Observable.Create<T>(observer => scheduler.Schedule(dueTime, () => observer.OnNext(function(value))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="function">The function.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, TimeSpan dueTime, IScheduler scheduler, Func<T, T> function) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.Schedule(dueTime, () => observer.OnNext(function(value)))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="function">The function.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this T value, DateTimeOffset dueTime, IScheduler scheduler, Func<T, T> function) =>
        Observable.Create<T>(observer => scheduler.Schedule(dueTime, () => observer.OnNext(function(value))));

    /// <summary>
    /// Schedules the specified due time.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The value.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="function">The function.</param>
    /// <returns>An IObservable of T.</returns>
    public static IObservable<T> Schedule<T>(this IObservable<T> source, DateTimeOffset dueTime, IScheduler scheduler, Func<T, T> function) =>
        Observable.Create<T>(observer => source.Subscribe(value => scheduler.Schedule(dueTime, () => observer.OnNext(function(value)))));

    /// <summary>
    /// Filters the specified source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="regexPattern">The pattern.</param>
    /// <returns>A Value.</returns>
    public static IObservable<string> Filter(this IObservable<string> source, string regexPattern) =>
        source.Where(f => Regex.IsMatch(f, regexPattern));

    /// <summary>
    /// Shuffles the specified source.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>An array of values shuffled randomly.</returns>
    public static IObservable<T[]> Shuffle<T>(this IObservable<T[]> source) =>
        Observable.Create<T[]>(observer => source.Subscribe(array =>
            {
                Random random = new(unchecked(Environment.TickCount * 31));
                var n = array.Length;
                while (n > 1)
                {
                    n--;
                    var k = random.Next(n + 1);
                    (array[n], array[k]) = (array[k], array[n]);
                }

                observer.OnNext(array);
            }));

    /// <summary>
    /// <para>Repeats the source observable sequence until it successfully terminates.</para>
    /// <para>This is same as Retry().</para>
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> OnErrorRetry<TSource>(this IObservable<TSource?> source) => source.Retry();

    /// <summary>
    /// When caught exception, do onError action and repeat observable sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="onError">The on error.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> OnErrorRetry<TSource, TException>(this IObservable<TSource?> source, Action<TException> onError)
        where TException : Exception => source.OnErrorRetry(onError, TimeSpan.Zero);

    /// <summary>
    /// When caught exception, do onError action and repeat observable sequence after delay time.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="onError">The on error.</param>
    /// <param name="delay">The delay.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> OnErrorRetry<TSource, TException>(this IObservable<TSource?> source, Action<TException> onError, TimeSpan delay)
where TException : Exception => source.OnErrorRetry(onError, int.MaxValue, delay);

    /// <summary>
    /// When caught exception, do onError action and repeat observable sequence during within retryCount.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="onError">The on error.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> OnErrorRetry<TSource, TException>(this IObservable<TSource?> source, Action<TException> onError, int retryCount)
where TException : Exception => source.OnErrorRetry(onError, retryCount, TimeSpan.Zero);

    /// <summary>
    /// When caught exception, do onError action and repeat observable sequence after delay time
    /// during within retryCount.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="onError">The on error.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="delay">The delay.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> OnErrorRetry<TSource, TException>(this IObservable<TSource?> source, Action<TException> onError, int retryCount, TimeSpan delay)
where TException : Exception => source.OnErrorRetry(onError, retryCount, delay, Scheduler.Default);

    /// <summary>
    /// When caught exception, do onError action and repeat observable sequence after delay
    /// time(work on delayScheduler) during within retryCount.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="onError">The on error.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="delay">The delay.</param>
    /// <param name="delayScheduler">The delay scheduler.</param>
    /// <returns>A Value.</returns>
    public static IObservable<TSource?> OnErrorRetry<TSource, TException>(this IObservable<TSource?> source, Action<TException> onError, int retryCount, TimeSpan delay, IScheduler delayScheduler)
        where TException : Exception => Observable.Defer(() =>
        {
            var dueTime = (delay.Ticks < 0) ? TimeSpan.Zero : delay;
            var empty = Observable.Empty<TSource?>();
            var count = 0;
            IObservable<TSource?>? self = null;
            self = source.Catch((TException ex) =>
            {
                onError(ex);

                return (++count < retryCount)
                        ? (dueTime == TimeSpan.Zero)
                            ? self!.SubscribeOn(Scheduler.CurrentThread)
                            : empty.Delay(dueTime, delayScheduler).Concat(self!).SubscribeOn(Scheduler.CurrentThread)
                        : Observable.Throw<TSource?>(ex);
            });
            return self;
        });

    /// <summary>
    /// Takes the until the predicate is true.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="predicate">The predicate for completion.</param>
    /// <returns>Observable TSource.</returns>
    public static IObservable<TSource> TakeUntil<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate) =>
        Observable.Create<TSource>(observer =>
            source.Subscribe(
            item =>
            {
                observer.OnNext(item);
                if (predicate?.Invoke(item) ?? default)
                {
                    observer.OnCompleted();
                }
            },
            observer.OnError,
            observer.OnCompleted));

    /// <summary>
    /// Synchronizes the asynchronous operations in downstream operations.
    /// Use SubscribeSynchronus instead for a simpler version.
    /// Call Sync.Dispose() to release the lock in the downstream methods.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>An Observable of T and a release mechanism.</returns>
    [SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "To avoid naming conflicts.")]
    public static IObservable<(T Value, IDisposable Sync)> SynchronizeAsync<T>(this IObservable<T> source) =>
        Observable.Create<(T Value, IDisposable Sync)>(observer =>
        {
            var gate = new object();
            return source.Synchronize(gate).Subscribe(item => new Continuation().Lock(item, observer).Wait());
        });

    /// <summary>
    /// Subscribes to the specified source synchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="onNext">The on next.</param>
    /// <param name="onError">The on error.</param>
    /// <param name="onCompleted">The on completed.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    public static IDisposable SubscribeSynchronus<T>(this IObservable<T> source, Func<T, Task> onNext, Action<Exception> onError, Action onCompleted) =>
        source.SynchronizeAsync().Subscribe(
            async observer =>
            {
                await onNext(observer.Value);
                observer.Sync.Dispose();
            },
            onError,
            onCompleted);

    /// <summary>
    /// Subscribes an element handler and an exception handler to an observable sequence synchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <param name="onError">Action to invoke upon exceptional termination of the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    public static IDisposable SubscribeSynchronus<T>(this IObservable<T> source, Func<T, Task> onNext, Action<Exception> onError) =>
        source.SynchronizeAsync().Subscribe(
            async observer =>
            {
                await onNext(observer.Value);
                observer.Sync.Dispose();
            },
            onError);

    /// <summary>
    /// Subscribes an element handler and a completion handler to an observable sequence synchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <param name="onCompleted">Action to invoke upon graceful termination of the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> or <paramref name="onCompleted"/> is <c>null</c>.</exception>
    public static IDisposable SubscribeSynchronus<T>(this IObservable<T> source, Func<T, Task> onNext, Action onCompleted) =>
        source.SynchronizeAsync().Subscribe(
            async observer =>
            {
                await onNext(observer.Value);
                observer.Sync.Dispose();
            },
            onCompleted);

    /// <summary>
    /// Subscribes an element handler to an observable sequence synchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    public static IDisposable SubscribeSynchronus<T>(this IObservable<T> source, Func<T, Task> onNext) =>
        source.SynchronizeAsync().Subscribe(
             async observer =>
             {
                 await onNext(observer.Value);
                 observer.Sync.Dispose();
             });

    private static void FastForEach<T>(IObserver<T> observer, IEnumerable<T> source)
    {
        if (source is List<T> fullList)
        {
            foreach (var item in fullList)
            {
                observer.OnNext(item);
            }
        }
        else if (source is IList<T> list)
        {
            // zero allocation enumerator
            foreach (var item in EnumerableIList.Create(list))
            {
                observer.OnNext(item);
            }
        }
        else if (source is T[] array)
        {
            foreach (var item in array)
            {
                observer.OnNext(item);
            }
        }
        else
        {
            foreach (var item in source)
            {
                observer.OnNext(item);
            }
        }
    }
}
