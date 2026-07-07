using System.Collections.Generic;

namespace AuthMicroservice.Core.Dictionaries;

public static class MessageDictionary
{
    public static readonly Dictionary<string, string> Messages = new()
    {
        { "AUTH001", "User duplicated, please go to signing" },
        { "AUTH002", "User credentials are bad please retry" },
        { "AUTH003", "token malformed, please re-signin" },
        { "AUTH004", "session doesn't exist, please re-signin" },
        { "AUTH005", "session expired, please re-signin" },
        { "AUT000", "An internal error occurred during authentication" },
        
        { "OK001", "User created succesfull thanks and welcome" },
        { "OK002", "Welcome again i'm so happy to see you again" },
        { "OK003", "Session valid" },
        { "OK004", "Have a good day i hope see you again" },

        { "VAL001", "First Name is required" },
        { "VAL002", "Last Name is required" },
        { "VAL003", "User Name or Email is required" },
        { "VAL004", "Invalid Email format" },
        { "VAL005", "Password is required" },
        { "VAL006", "Password must have a minimum length of 8 characters" },
        { "VAL007", "ISO Country is required" },
        { "VAL008", "ISO Country must have a maximum length of 3 characters" }
    };

    public static string GetMessage(string code)
    {
        return Messages.TryGetValue(code, out var message) ? message : "Unknown error";
    }
}
