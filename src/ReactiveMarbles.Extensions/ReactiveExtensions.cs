// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Extension methods for <see cref="System.Reactive"/>.
/// </summary>
public static class ReactiveExtensions
{
    private static readonly Dictionary<TimeSpan, Lazy<IConnectableObservable<DateTime>>> _timerList = new();

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
        if (!_timerList.ContainsKey(timeSpan))
        {
            _timerList.Add(timeSpan, new Lazy<IConnectableObservable<DateTime>>(() => Observable.Timer(TimeSpan.FromMilliseconds(0), timeSpan).Timestamp().Select(x => x.Timestamp.DateTime).Publish()));
            _timerList[timeSpan].Value.Connect();
        }

        return _timerList[timeSpan].Value;
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
    /// Gets the maximum.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="sources">The sources.</param>
    /// <returns>A Value.</returns>
    public static IObservable<T?> GetMax<T>(this IObservable<T?> @this, params IObservable<T?>[] sources)
        where T : struct
    {
        List<IObservable<T?>> source = new() { @this };
        source.AddRange(sources);
        return source.CombineLatest().Select(x => x.Max());
    }

    /// <summary>
    /// Selects a unit.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="self">The self.</param>
    /// <returns>A Value.</returns>
    public static IObservable<Unit> ToUnit<T>(this IObservable<T?> self) => self.Select(_ => Unit.Default);

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
}
