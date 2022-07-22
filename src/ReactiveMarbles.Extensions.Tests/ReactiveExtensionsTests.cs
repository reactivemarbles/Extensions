// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using Xunit;

namespace ReactiveMarbles.Extensions.Tests;

/// <summary>
/// Tests Reactive Extensions.
/// </summary>
public class ReactiveExtensionsTests
{
    /// <summary>
    /// Tests the WhereIsNotNull extension.
    /// </summary>
    [Fact]
    public void GivenNull_WhenWhereIsNotNull_ThenNoNotification()
    {
        // Given, When
        bool? result = null;
        using var disposable = Observable.Return<bool?>(null).WhereIsNotNull().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .BeNull();
    }

    /// <summary>
    /// Tests the WhereIsNotNull extension.
    /// </summary>
    [Fact]
    public void GivenValue_WhenWhereIsNotNull_ThenNotification()
    {
        // Given, When
        bool? result = null;
        using var disposable = Observable.Return<bool?>(false).WhereIsNotNull().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .BeFalse();
    }

    /// <summary>
    /// Tests the AsSignal extension.
    /// </summary>
    [Fact]
    public void GivenObservable_WhenAsSignal_ThenNotifiesUnit()
    {
        // Given, When
        Unit? result = null;
        using var disposable = Observable.Return<bool?>(false).AsSignal().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .Be(Unit.Default);
    }

    /// <summary>
    /// Tests WhereFalse does notifies false values.
    /// </summary>
    [Fact]
    public void GivenFalse_WhenWhereFalse_ThenNotifiesValue()
    {
        // Given, When
        bool? result = null;
        using var disposable = Observable.Return(false).WhereFalse().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .BeFalse();
    }

    /// <summary>
    /// Tests WhereFalse does not notify true values.
    /// </summary>
    [Fact]
    public void GivenTrue_WhenWhereFalse_ThenNoNotifications()
    {
        // Given, When
        bool? result = null;
        using var disposable = Observable.Return(true).WhereFalse().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .BeNull();
    }

    /// <summary>
    /// Tests WhereTrue notifies true values.
    /// </summary>
    [Fact]
    public void GivenTrue_WhenWhereTrue_ThenNotifiesValue()
    {
        // Given, When
        bool? result = null;
        using var disposable = Observable.Return(true).WhereTrue().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .BeTrue();
    }

    /// <summary>
    /// Tests WhereTrue does not notify false values.
    /// </summary>
    [Fact]
    public void GivenFalse_WhenWhereTrue_ThenNoNotifications()
    {
        // Given, When
        bool? result = null;
        using var disposable = Observable.Return(false).WhereTrue().Subscribe(x => result = x);

        // Then
        result
            .Should()
            .BeNull();
    }
}
