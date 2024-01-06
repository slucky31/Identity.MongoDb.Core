using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Identity.MongoDb.Core.Test.Utilities;

public class Configuration
{
    public static string YoutubeLikeId()
    {
        Thread.Sleep(1);
        string characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var charSet = characterSet.ToCharArray();
        int targetBase = charSet.Length;
        long ticks = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string output="";

        do
        {
            output += charSet[ticks % targetBase];
            ticks = ticks / targetBase;
        } while (ticks > 0);

        output = new string(output.Reverse().ToArray());

        return Convert
                .ToBase64String(Encoding.UTF8.GetBytes(output))
                .Replace("/", "_", StringComparison.Ordinal)
                .Replace("+", "-", StringComparison.Ordinal)
                .Replace("==", "", StringComparison.Ordinal);
    }

    public string Connection { get; } = "mongodb+srv://api-rest-dev:YTTu6dYjRqFhX4zC@dev.dvd91.azure.mongodb.net/";
    public string DatabaseName { get; } = "idmongodb_tests_" + YoutubeLikeId();
}
