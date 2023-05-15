// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Stale notification.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
internal class Stale<T> : IStale<T>
{
    /// <summary>
    /// The update.
    /// </summary>
    private readonly T? _update;

    /// <summary>
    /// Initializes a new instance of the <see cref="Stale{T}"/> class.
    /// </summary>
    public Stale()
        : this(true, default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Stale{T}"/> class.
    /// </summary>
    /// <param name="update">The update.</param>
    public Stale(T? update)
        : this(false, update)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Stale{T}"/> class.
    /// </summary>
    /// <param name="isStale">if set to <c>true</c> [is stale].</param>
    /// <param name="update">The update.</param>
    private Stale(bool isStale, T? update)
    {
        IsStale = isStale;
        _update = update;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is stale.
    /// </summary>
    /// <value><c>true</c> if this instance is stale; otherwise, <c>false</c>.</value>
    public bool IsStale { get; }

    /// <summary>
    /// Gets the update.
    /// </summary>
    /// <value>The update.</value>
    /// <exception cref="InvalidOperationException">Stale instance has no update.</exception>
    public T? Update => IsStale ? throw new InvalidOperationException("Stale instance has no update.") : _update;
}
