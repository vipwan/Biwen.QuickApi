// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:55:45 SaveChangesResult.cs

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// Represent operation result for SaveChanges.
/// </summary>
public sealed class SaveChangesResult
{
    /// <summary>
    /// Ctor
    /// </summary>
    public SaveChangesResult() => Messages = new List<string>();

    /// <inheritdoc />
    public SaveChangesResult(string message) : this() => AddMessage(message);

    /// <summary>
    /// Last Exception you can find here
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Is Exception occupied while last operation execution
    /// </summary>
    public bool IsOk => Exception == null;

    /// <summary>
    /// Adds new message to result
    /// </summary>
    /// <param name="message"></param>
    public void AddMessage(string message) => Messages.Add(message);

    /// <summary>
    /// List of the error should appear there
    /// </summary>
    private List<string> Messages { get; }
}