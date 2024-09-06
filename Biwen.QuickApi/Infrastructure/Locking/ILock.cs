// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:00 ILock.cs

namespace Biwen.QuickApi.Infrastructure.Locking
{
    /// <summary>
    /// ILocalLock
    /// </summary>
    public interface ILocalLock : ILock
    {
    }

    public interface ILock
    {
        /// <summary>
        /// Waits indefinitely until acquiring a named lock with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null);

        /// <summary>
        /// Tries to acquire a named lock in a given timeout with a given expiration for the current tenant.
        /// After 'expiration' the lock is auto released, a null value is equivalent to 'TimeSpan.MaxValue'.
        /// </summary>
        Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null);

        /// <summary>
        /// Whether a named lock is already acquired.
        /// </summary>
        Task<bool> IsLockAcquiredAsync(string key);
    }
}
