using System.Globalization;

namespace Lab3
{
    internal static class NumberFormatter
    {
        // -0 (например, 0 * -5) печатаем как 0, а точку вместо запятой - для вставки в онлайн-калькулятор.
        public static string Format(double value) =>
            (value is 0 ? 0 : value).ToString("0.##", CultureInfo.InvariantCulture);
    }
}
