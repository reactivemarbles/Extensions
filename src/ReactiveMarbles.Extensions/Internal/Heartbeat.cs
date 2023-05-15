// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Heart beat.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
internal class Heartbeat<T> : IHeartbeat<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Heartbeat{T}"/> class.
    /// </summary>
    public Heartbeat()
        : this(true, default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Heartbeat{T}"/> class.
    /// </summary>
    /// <param name="update">The update.</param>
    public Heartbeat(T? update)
        : this(false, update)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Heartbeat{T}"/> class.
    /// </summary>
    /// <param name="isHeartbeat">if set to <c>true</c> [is heartbeat].</param>
    /// <param name="update">The update.</param>
    private Heartbeat(bool isHeartbeat, T? update)
    {
        IsHeartbeat = isHeartbeat;
        Update = update;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is heartbeat.
    /// </summary>
    /// <value><c>true</c> if this instance is heartbeat; otherwise, <c>false</c>.</value>
    public bool IsHeartbeat { get; }

    /// <summary>
    /// Gets the update.
    /// </summary>
    /// <value>The update.</value>
    public T? Update { get; }
}
