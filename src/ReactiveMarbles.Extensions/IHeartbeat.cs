﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Heart beat.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public interface IHeartbeat<out T>
{
    /// <summary>
    /// Gets a value indicating whether this instance is heartbeat.
    /// </summary>
    /// <value><c>true</c> if this instance is heartbeat; otherwise, <c>false</c>.</value>
    bool IsHeartbeat { get; }

    /// <summary>
    /// Gets the update.
    /// </summary>
    /// <value>The update.</value>
    T? Update { get; }
}
