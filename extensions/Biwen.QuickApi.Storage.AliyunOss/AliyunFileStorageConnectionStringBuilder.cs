// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:54 AliyunFileStorageConnectionStringBuilder.cs

namespace Biwen.QuickApi.Storage.AliyunOss;

internal class AliyunFileStorageConnectionStringBuilder : AliyunConnectionStringBuilder
{
    private string? _bucket;

    public AliyunFileStorageConnectionStringBuilder(string connectionString) : base(connectionString)
    {
    }

    public string Bucket
    {
        get => string.IsNullOrEmpty(_bucket) ? "storage" : _bucket;
        set => _bucket = value;
    }

    protected override bool ParseItem(string key, string value)
    {
        if (string.Equals(key, "Bucket", StringComparison.OrdinalIgnoreCase))
        {
            Bucket = value;
            return true;
        }
        return base.ParseItem(key, value);
    }

    public override string ToString()
    {
        string connectionString = base.ToString();
        if (!string.IsNullOrEmpty(_bucket))
            connectionString += "Bucket=" + Bucket + ";";
        return connectionString;
    }
}
