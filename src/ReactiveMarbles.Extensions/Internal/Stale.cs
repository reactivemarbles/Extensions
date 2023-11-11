﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.Extensions;

internal class Stale<T> : IStale<T>
{
    private readonly T? _update;

    public Stale()
        : this(true, default)
    {
    }

    public Stale(T? update)
        : this(false, update)
    {
    }

    private Stale(bool isStale, T? update)
    {
        IsStale = isStale;
        _update = update;
    }

    public bool IsStale { get; }

    public T? Update => IsStale ? throw new InvalidOperationException("Stale instance has no update.") : _update;
}
