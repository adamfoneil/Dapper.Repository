using System.Text.RegularExpressions;

namespace Dapper.Repository.Test.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceWhitespace(this string input) => Regex.Replace(input, @"\s+", " ");

        public static bool IsEqualWithoutWhitespace(this string input, string compareWith) => ReplaceWhitespace(input).Equals(ReplaceWhitespace(compareWith));
    }
}
