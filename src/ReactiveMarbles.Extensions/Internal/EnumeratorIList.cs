﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace ReactiveMarbles.Extensions.Internal;

internal struct EnumeratorIList<T>(IList<T> list) : IEnumerator<T>
{
    private int _index = -1;

    public readonly T Current => list[_index];

    readonly object? IEnumerator.Current => Current;

    public bool MoveNext()
    {
        _index++;

        return _index < list.Count;
    }

    public readonly void Dispose() => list.Clear();

    public void Reset() => _index = -1;
}
