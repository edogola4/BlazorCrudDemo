using System.Globalization;

namespace BlazorCrudDemo.Web.Utilities
{
    public static class CurrencyFormatter
    {
        private static readonly CultureInfo KenyanCulture = new CultureInfo("en-KE");

        public static string FormatPrice(decimal price)
        {
            // Use Kenyan Shilling formatting
            return price.ToString("C", KenyanCulture);
        }

        public static string FormatPrice(decimal price, CultureInfo culture)
        {
            return price.ToString("C", culture);
        }

        public static string FormatPrice(decimal price, string currencySymbol)
        {
            // For custom currency symbols if needed
            return $"{currencySymbol}{price:N2}";
        }

        public static CultureInfo GetKenyanCulture()
        {
            return KenyanCulture;
        }
    }
}
