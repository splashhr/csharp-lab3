using System;
using System.Globalization;
using System.IO;

namespace Lab3
{
    internal static class ConsoleInput
    {
        public static int ReadInt32(string prompt, int minValue, int maxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                var isNumber = int.TryParse(ReadLine(), CultureInfo.InvariantCulture, out var value);
                if (isNumber && value >= minValue && value <= maxValue) return value;

                Console.WriteLine($"Нужно целое число от {minValue} до {maxValue}.");
            }
        }

        public static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                // Принимаем и запятую, и точку, независимо от региональных настроек системы.
                var text = ReadLine().Replace(',', '.');
                var isNumber = double.TryParse(text, CultureInfo.InvariantCulture, out var value);
                if (isNumber && double.IsFinite(value)) return value;

                Console.WriteLine("Нужно число, например 2.5 или -3.");
            }
        }

        private static string ReadLine() =>
            Console.ReadLine() ?? throw new EndOfStreamException("Ввод с клавиатуры закончился.");
    }
}
