using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyCompiler;

public class RegEx
{
    public static List<Match> ValidateTime(string input)
    {
        string pattern = @"(?:[01]\d|2[0-3]:(?:[0-5]\d):(?:[0-5]\d))";
        Regex regex = new Regex(pattern);
        return GetMatchesWithPositions(input, regex);
    }
    public static List<Match> ValidatePhoneNumber(string input)
    {
        string pattern = @"(?:\+7|8)\s?\(?\d{3}\)?[-\s]?\d{3}[-\s]?\d{2}[-\s]?\d{2}";
        Regex regex = new Regex(pattern);
        return GetMatchesWithPositions(input, regex);
    }
    public static List<Match> ValidatePassword(string input)
    {
        string pattern = @"(?=.*[а-я])(?=.*[А-Я])(?=.*\d)(?=.*[#?!|/@/$%\^&*-_])[А-Яа-яA-Za-z\d#?!|/@/$%\^&*-_]{8,}";
        Regex regex = new Regex(pattern);
        return GetMatchesWithPositions(input, regex);
    }
    private static List<Match> GetMatchesWithPositions(string input, Regex regex)
    {
        List<Match> matches = [.. regex.Matches(input).Cast<Match>()];
        return matches;
    }
}
