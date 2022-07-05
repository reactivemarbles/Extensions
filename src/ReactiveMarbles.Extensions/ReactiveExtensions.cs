// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using System.Reactive.Linq;

namespace ReactiveMarbles.Extensions
{
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
        public static IObservable<T> WhereNotNull<T>(this IObservable<T> observable) =>
            observable
                .Where(x => x is not null)
                .Select(x => x!);

        /// <summary>
        /// Will convert an observable so that it's value is ignored and converted into just returning <see cref="Unit"/>.
        /// This allows us just to be notified when the observable signals.
        /// </summary>
        /// <typeparam name="T">The current type of the observable.</typeparam>
        /// <param name="observable">The observable to convert.</param>
        /// <returns>The converted observable.</returns>
        public static IObservable<Unit> AsSignal<T>(this IObservable<T> observable) =>
            observable
                .Select(_ => Unit.Default);
    }
}
