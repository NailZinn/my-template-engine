using System.Text;

namespace HTML_Engine_Library
{
    internal static class EnumerableExtensions
    {
        public static string AsString(this IEnumerable<char> source)
        {
            var sb = new StringBuilder();

            foreach (var c in source)
            {
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
