﻿// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 IdGenerator.cs

namespace Biwen.QuickApi;

/// <summary>
/// IdGenerator
/// </summary>
[SuppressType]
public static class IdGenerator
{
    private static readonly char[] _encode32Chars = "0123456789abcdefghjkmnpqrstvwxyz".ToCharArray();

    /// <summary>
    /// 生成26位长度的随机Id
    /// </summary>
    /// <returns></returns>
    public static string GenerateId()
    {
        var guid = Guid.NewGuid().ToByteArray();

        return string.Create(26, guid, (buffer, guid) =>
        {
            var hs = BitConverter.ToInt64(guid, 0);
            var ls = BitConverter.ToInt64(guid, 8);

            // Using a local copy prevents additional bound checks by the JIT.
            var encode32Chars = _encode32Chars;

            // A char array allows a long as the indexer, so without any cast.
            buffer[0] = encode32Chars[(hs >> 60) & 31];
            buffer[1] = encode32Chars[(hs >> 55) & 31];
            buffer[2] = encode32Chars[(hs >> 50) & 31];
            buffer[3] = encode32Chars[(hs >> 45) & 31];
            buffer[4] = encode32Chars[(hs >> 40) & 31];
            buffer[5] = encode32Chars[(hs >> 35) & 31];
            buffer[6] = encode32Chars[(hs >> 30) & 31];
            buffer[7] = encode32Chars[(hs >> 25) & 31];
            buffer[8] = encode32Chars[(hs >> 20) & 31];
            buffer[9] = encode32Chars[(hs >> 15) & 31];
            buffer[10] = encode32Chars[(hs >> 10) & 31];
            buffer[11] = encode32Chars[(hs >> 5) & 31];
            buffer[12] = encode32Chars[hs & 31];

            buffer[13] = encode32Chars[(ls >> 60) & 31];
            buffer[14] = encode32Chars[(ls >> 55) & 31];
            buffer[15] = encode32Chars[(ls >> 50) & 31];
            buffer[16] = encode32Chars[(ls >> 45) & 31];
            buffer[17] = encode32Chars[(ls >> 40) & 31];
            buffer[18] = encode32Chars[(ls >> 35) & 31];
            buffer[19] = encode32Chars[(ls >> 30) & 31];
            buffer[20] = encode32Chars[(ls >> 25) & 31];
            buffer[21] = encode32Chars[(ls >> 20) & 31];
            buffer[22] = encode32Chars[(ls >> 15) & 31];
            buffer[23] = encode32Chars[(ls >> 10) & 31];
            buffer[24] = encode32Chars[(ls >> 5) & 31];
            buffer[25] = encode32Chars[ls & 31];
        });
    }
}
