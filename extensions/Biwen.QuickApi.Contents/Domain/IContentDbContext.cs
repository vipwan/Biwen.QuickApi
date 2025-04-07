// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 21:57:09 IDbContext.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Biwen.QuickApi.Contents.Domain;

/// <summary>
/// IDbContext
/// </summary>
public interface IContentDbContext : ICurrentDbContext
{
    DbSet<Content> Contents { get; set; }

    DbSet<ContentAuditLog> ContentAuditLogs { get; set; }

    DbSet<ContentVersion> ContentVersions { get; set; }

}
