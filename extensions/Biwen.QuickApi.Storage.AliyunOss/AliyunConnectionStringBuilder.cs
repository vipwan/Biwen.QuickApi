// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 17:00:50 AliyunConnectionStringBuilder.cs

namespace Biwen.QuickApi.Storage.AliyunOss
{
    internal abstract class AliyunConnectionStringBuilder
    {
        public string? Endpoint { get; set; }

        public string? AccessKey { get; set; }

        public string? SecretKey { get; set; }

        protected AliyunConnectionStringBuilder(string connectionString)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);
            Parse(connectionString);
        }

        private void Parse(string connectionString)
        {
            foreach (string[] option in connectionString
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(kvp => kvp.Contains('='))
                .Select(kvp => kvp.Split(new[] { '=' }, 2)))
            {
                string optionKey = option[0].Trim();
                string optionValue = option[1].Trim();
                if (!ParseItem(optionKey, optionValue))
                {
                    throw new ArgumentException($"The option '{optionKey}' cannot be recognized in connection string.", nameof(connectionString));
                }
            }
        }

        protected virtual bool ParseItem(string key, string value)
        {
            if (string.Equals(key, "AccessKey", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Access Key", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "AccessKeyId", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Access Key Id", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Id", StringComparison.OrdinalIgnoreCase))
            {
                AccessKey = value;
                return true;
            }
            if (string.Equals(key, "SecretKey", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Secret Key", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "SecretAccessKey", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Secret Access Key", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "AccessKeySecret", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Access Key Secret", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "Secret", StringComparison.OrdinalIgnoreCase))
            {
                SecretKey = value;
                return true;
            }
            if (string.Equals(key, "EndPoint", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "End Point", StringComparison.OrdinalIgnoreCase))
            {
                Endpoint = value;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            string connectionString = string.Empty;
            if (!string.IsNullOrEmpty(AccessKey))
                connectionString += "AccessKey=" + AccessKey + ";";
            if (!string.IsNullOrEmpty(SecretKey))
                connectionString += "SecretKey=" + SecretKey + ";";
            if (!string.IsNullOrEmpty(Endpoint))
                connectionString += "EndPoint=" + Endpoint + ";";
            return connectionString;
        }
    }
}
