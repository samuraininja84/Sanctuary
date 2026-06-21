namespace Sanctuary.Extensions
{
    public static class FormattingExtensions
    {
        public static string TryFormat(string input)
        {
            // Initialize the formatted string to an empty value.
            string formatted = string.Empty;

#if UNITY_NEWTONSOFT_JSON
            try
            {
                // Pretty-print the JSON data
                formatted = Newtonsoft.Json.Linq.JToken.Parse(input).ToString(Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                // If parsing fails, just use the raw value
                formatted = input;
            }
#else
            // If Newtonsoft.Json is not available, just use the raw value
            formatted = input;
#endif

            // Return the formatted string.
            return formatted;
        }
    }
}