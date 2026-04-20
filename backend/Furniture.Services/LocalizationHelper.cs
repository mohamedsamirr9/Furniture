namespace Furniture.Services
{
    public static class LocalizationHelper
    {
        public static string Localize(string? valueEn, string? valueAr, string language)
        {
            if (language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(valueAr))
                return valueAr;

            return valueEn ?? string.Empty;
        }

        public static string? LocalizeNullable(string? valueEn, string? valueAr, string language)
        {
            if (language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(valueAr))
                return valueAr;

            return valueEn;
        }
    }
}
