// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Extension methods for <see cref="System.Reactive"/>.
/// </summary>
public static class ReactiveExtensions
{
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
    /// Returns false values.
    /// </summary>
    /// <param name="observable">The observable that can contain a false value.</param>
    /// <returns>All false values.</returns>
    public static IObservable<bool> WhereFalse(this IObservable<bool> observable) => observable.Where(x => !x);

    /// <summary>
    /// Returns true values.
    /// </summary>
    /// <param name="observable">The observable that can contain a truth value.</param>
    /// <returns>All truth values.</returns>
    public static IObservable<bool> WhereTrue(this IObservable<bool> observable) => observable.Where(x => x);

    /// <summary>
    /// Returns true values.
    /// </summary>
    /// <typeparam name="T">The current type of the observable.</typeparam>
    /// <param name="observable">The observable that can contain a truth value.</param>
    /// <param name="predicate">The predicate to determine all case.</param>
    /// <returns>All that match the predicate.</returns>
    public static IObservable<IEnumerable<T>> WhereAll<T>(
        this IObservable<IEnumerable<T>> observable,
        Func<T, bool> predicate) => observable.Where(enumerable => enumerable.All(predicate));

    /// <summary>
    /// Returns true values.
    /// </summary>
    /// <typeparam name="T">The current type of the observable.</typeparam>
    /// <param name="observable">The observable that can contain a truth value.</param>
    /// <param name="predicate">The predicate to determine any case.</param>
    /// <returns>Any that match the predicate.</returns>
    public static IObservable<IEnumerable<T>> WhereAny<T>(
        this IObservable<IEnumerable<T>> observable,
        Func<T, bool> predicate) => observable.Where(enumerable => enumerable.Any(predicate));

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
}
