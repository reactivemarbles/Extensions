using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveMarbles.Extensions;

/// <summary>
/// Indicator for connection that has become stale.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
public interface IStale<out T>
{
    /// <summary>
    /// Gets a value indicating whether this instance is stale.
    /// </summary>
    /// <value><c>true</c> if this instance is stale; otherwise, <c>false</c>.</value>
    bool IsStale { get; }

    /// <summary>
    /// Gets the update.
    /// </summary>
    /// <value>The update.</value>
    T? Update { get; }
}
