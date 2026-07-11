using System;
using System.Security.Cryptography;
using AuthMicroservice.Core.Interfaces;

namespace AuthMicroservice.Application.Helpers;

public class RestoreCodeHelper : IRestoreCodeHelper
{
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";

    public string GenerateRestoreCode()
    {
        var randomBytes = new byte[4];
        RandomNumberGenerator.Fill(randomBytes);

        var letter1 = Letters[randomBytes[0] % Letters.Length];
        var letter2 = Letters[randomBytes[1] % Letters.Length];
        var digit1 = Digits[randomBytes[2] % Digits.Length];
        var digit2 = Digits[randomBytes[3] % Digits.Length];

        var chars = new[] { letter1, letter2, digit1, digit2 };

        RandomNumberGenerator.Fill(randomBytes);
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = randomBytes[i] % (i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
