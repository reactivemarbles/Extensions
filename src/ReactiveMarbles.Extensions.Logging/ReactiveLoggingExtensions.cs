// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace ReactiveMarbles.Extensions.Logging;

/// <summary>
/// Extension methods to log information from observable pipelines.
/// </summary>
public static class ReactiveLoggingExtensions
{
    /// <summary>
    /// Log a Debug event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="messageTemplate">Message template describing the event. <br />
    ///  use {@Property} to access the property value off of the <see cref="TSource"/>.
    /// </param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogDebug<TSource>(this IObservable<TSource> source, ILogger logger, string messageTemplate)
        => source.LogWrite(logger, LogLevel.Debug, messageTemplate);

    /// <summary>
    /// Log a Debug event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="propertyFactory">The property from the source object.</param>
    /// <param name="messageTemplate">Message template describing the event. <br />
    ///  use {@Property} to access the property value off of the <see cref="TSource"/>.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The property type of the property in the source instance.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogDebug<TSource, TProperty>(
        this IObservable<TSource> source,
        ILogger logger,
        Func<TSource, TProperty> propertyFactory,
        string messageTemplate) => source.LogWrite(logger, propertyFactory, LogLevel.Debug, messageTemplate);

    /// <summary>
    /// Log an Information event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogInformation<TSource>(this IObservable<TSource> source, ILogger logger, string messageTemplate)
        => source.LogWrite(logger, LogLevel.Information, messageTemplate);

    /// <summary>
    /// Log an Information event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="propertyFactory">The property from the source object.</param>
    /// <param name="messageTemplate">Message template describing the event. <br />
    ///  use {@Property} to access the property value off of the <see cref="TSource"/>.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The property type of the property in the source instance.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogInformation<TSource, TProperty>(
        this IObservable<TSource> source,
        ILogger logger,
        Func<TSource, TProperty> propertyFactory,
        string messageTemplate) => source.LogWrite(logger, propertyFactory, LogLevel.Information, messageTemplate);

    /// <summary>
    /// Log a Warning event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogWarning<TSource>(this IObservable<TSource> source, ILogger logger, string messageTemplate)
        => source.LogWrite(logger, LogLevel.Warning, messageTemplate);

    /// <summary>
    /// Log a Warning event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="propertyFactory">The property from the source object.</param>
    /// <param name="messageTemplate">Message template describing the event. <br />
    ///  use {@Property} to access the property value off of the <see cref="TSource"/>.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The property type of the property in the source instance.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogWarning<TSource, TProperty>(
        this IObservable<TSource> source,
        ILogger logger,
        Func<TSource, TProperty> propertyFactory,
        string messageTemplate) => source.LogWrite(logger, propertyFactory, LogLevel.Warning, messageTemplate);

    /// <summary>
    /// Log an Error event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogError<TSource>(this IObservable<TSource> source, ILogger logger, string messageTemplate)
        => source.LogWrite(logger, LogLevel.Error, messageTemplate);

    /// <summary>
    /// Log a Error event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="propertyFactory">The property from the source object.</param>
    /// <param name="messageTemplate">Message template describing the event. <br />
    ///  use {@Property} to access the property value off of the <see cref="TSource"/>.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The property type of the property in the source instance.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogError<TSource, TProperty>(
        this IObservable<TSource> source,
        ILogger logger,
        Func<TSource, TProperty> propertyFactory,
        string messageTemplate) => source.LogWrite(logger, propertyFactory, LogLevel.Error, messageTemplate);

    /// <summary>
    /// Log a Trace event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogTrace<TSource>(this IObservable<TSource> source, ILogger logger, string messageTemplate)
        => source.LogWrite(logger, LogLevel.Trace, messageTemplate);

    /// <summary>
    /// Log a Trace event.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="propertyFactory">The property from the source object.</param>
    /// <param name="messageTemplate">Message template describing the event. <br />
    ///  use {@Property} to access the property value off of the <see cref="TSource"/>.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The property type of the property in the source instance.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogTrace<TSource, TProperty>(
        this IObservable<TSource> source,
        ILogger logger,
        Func<TSource, TProperty> propertyFactory,
        string messageTemplate) => source.LogWrite(logger, propertyFactory, LogLevel.Trace, messageTemplate);

    /// <summary>
    /// Log a log event with the specified level.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogWrite<TSource>(
        this IObservable<TSource> source,
        ILogger logger,
        LogLevel level,
        string messageTemplate) =>
        source.Do(
            property => logger.Log(level, message: messageTemplate, property),
            exception => logger.LogError(exception, "Exception within the observable pipeline"));

    /// <summary>
    /// Log a log event with the specified level.
    /// </summary>
    /// <param name="source">Source sequence.</param>
    /// <param name="logger">The target <see cref="ILogger"/>.</param>
    /// <param name="propertyFactory">The property from the source object.</param>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TProperty">The property.</typeparam>
    /// <returns>The source sequence.</returns>
    public static IObservable<TSource> LogWrite<TSource, TProperty>(
        this IObservable<TSource> source,
        ILogger logger,
        Func<TSource, TProperty> propertyFactory,
        LogLevel level,
        string messageTemplate) =>
        source.Do(
            instance => logger.Log(level, message: messageTemplate, propertyFactory.Invoke(instance)),
            exception => logger.LogError(exception, "Exception within the observable pipeline"));

    /// <summary>
    /// You can append this function liberally to your Rx pipelines while you are developing them to see what's happening:
    /// This observable will only light up if the debugger is attached. It will not add anything to Test or Production logs.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="logger">The logger.</param>
    /// <typeparam name="T">The source type.</typeparam>
    /// <remarks>https://stackoverflow.com/a/20220756/124069.</remarks>
    /// <returns>The source sequence.</returns>
    public static IObservable<T> Spy<T>(this IObservable<T> source, ILogger logger) =>
        Spy(source, logger, "N/A");

    /// <summary>
    /// You can append this function liberally to your Rx pipelines while you are developing them to see what's happening:
    /// This observable will only light up if the debugger is attached. It will not add anything to Test or Production logs.
    /// </summary>
    /// <param name="source">The source observable.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="message">The log message.</param>
    /// <typeparam name="T">The source type.</typeparam>
    /// <remarks>https://stackoverflow.com/a/20220756/124069.</remarks>
    /// <returns>The source sequence.</returns>
    public static IObservable<T> Spy<T>(this IObservable<T> source, ILogger logger, string message)
    {
        if (!Debugger.IsAttached)
        {
            return source;
        }

        var correlationId = Guid.NewGuid();

        logger.LogTrace(
            "[SPY:Init] {@SpyDetails}",
            new { Thread = Thread.CurrentThread.ManagedThreadId, Message = message, CorrelationId = correlationId });

        return Observable.Create<T>(
            obs =>
            {
                var onNextInvocationCount = 0;

                logger.LogTrace(
                    "[SPY:Subscribe] {@SpyDetails}",
                    new
                    {
                        Thread = Thread.CurrentThread.ManagedThreadId,
                        Message = message,
                        CorrelationId = correlationId
                    });

                try
                {
                    var subscription = source
                        .Do(
                            _ => logger.LogTrace(
                                "[SPY:OnNext] {@SpyDetails}",
                                new
                                {
                                    Count = onNextInvocationCount++,
                                    Thread = Thread.CurrentThread.ManagedThreadId,
                                    Message = message,
                                    CorrelationId = correlationId
                                }),
                            ex => logger.LogTrace(
                                ex,
                                "[SPY:OnError] {@SpyDetails}",
                                new
                                {
                                    Thread = Thread.CurrentThread.ManagedThreadId,
                                    Message = message,
                                    CorrelationId = correlationId
                                }),
                            () => logger.LogTrace(
                                "[SPY:OnCompleted] {@SpyDetails}",
                                new
                                {
                                    Thread = Thread.CurrentThread.ManagedThreadId,
                                    Message = message,
                                    CorrelationId = correlationId
                                }))
                        .Subscribe(obs);

                    return new CompositeDisposable(
                        subscription,
                        Disposable.Create(
                            () => logger.LogTrace(
                                "[SPY:Disposed] {@SpyDetails}",
                                new
                                {
                                    Thread = Environment.CurrentManagedThreadId,
                                    Message = message,
                                    CorrelationId = correlationId
                                })));
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "[SPY:Failed] {@SpyDetails}",
                        new
                        {
                            Thread = Thread.CurrentThread.ManagedThreadId,
                            Message = message,
                            CorrelationId = correlationId
                        });

                    return Disposable.Empty;
                }
            });
    }
}
