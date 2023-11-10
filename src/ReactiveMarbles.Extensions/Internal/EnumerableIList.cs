﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace ReactiveMarbles.Extensions.Internal;

internal static class EnumerableIList
{
    public static EnumerableIList<T> Create<T>(IList<T> list) => new(list);
}
