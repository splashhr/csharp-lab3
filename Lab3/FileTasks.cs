using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Lab3
{
    internal static class FileTasks
    {
        private const int MinRandomNumber = -50;
        private const int MaxRandomNumber = 50;
        private const int MaxNumbersInRow = 5;
        private const int MinToyPrice = 100;
        private const int MaxToyPrice = 5000;
        private const int MaxToyMinAge = 10;
        private const int MaxToyAgeSpan = 6;
        private const string BuildingSetName = "Конструктор";

        private static readonly XmlSerializer s_toySerializer = new(typeof(Toy));
        private static readonly string[] s_toyNames = ["Кукла", "Мяч", "Кубики", BuildingSetName, "Машинка", "Пазл"];

        public static void FillBinaryFileWithRandomNumbers(string path, int count, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            WriteBinaryNumbers(path, Enumerable.Range(0, count).Select(_ => NextRandomNumber(random)));
        }

        public static IReadOnlyList<int> ReadBinaryNumbers(string path)
        {
            var numbers = new List<int>();
            using var reader = new BinaryReader(File.OpenRead(path));
            while (!IsEndOfFile(reader))
            {
                numbers.Add(reader.ReadInt32());
            }

            return numbers;
        }

        public static IReadOnlyList<long> ReadBinaryProducts(string path)
        {
            var products = new List<long>();
            using var reader = new BinaryReader(File.OpenRead(path));
            while (!IsEndOfFile(reader))
            {
                products.Add(reader.ReadInt64());
            }

            return products;
        }

        public static void WriteAdjacentProducts(string sourcePath, string resultPath)
        {
            using var reader = new BinaryReader(File.OpenRead(sourcePath));
            using var writer = new BinaryWriter(File.Create(resultPath));
            if (IsEndOfFile(reader)) return;

            var previous = reader.ReadInt32();
            while (!IsEndOfFile(reader))
            {
                var current = reader.ReadInt32();
                // Произведение двух int может не поместиться в int, поэтому пишем long.
                writer.Write((long)previous * current);
                previous = current;
            }
        }

        public static void FillToysFile(string path, int count, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            WriteToys(path, Enumerable.Range(0, count).Select(_ => CreateRandomToy(random)));
        }

        public static IReadOnlyList<Toy> ReadToys(string path)
        {
            var toys = new List<Toy>();
            using var reader = new BinaryReader(File.OpenRead(path));
            while (!IsEndOfFile(reader))
            {
                using var text = new StringReader(reader.ReadString());
                using var xml = XmlReader.Create(text);
                toys.Add((Toy)s_toySerializer.Deserialize(xml)!);
            }

            return toys;
        }

        public static IReadOnlyList<Toy> FindBuildingSets(string path) =>
            ReadToys(path)
                .Where(toy => string.Equals(toy.Name, BuildingSetName, StringComparison.OrdinalIgnoreCase))
                .ToList();

        public static void FillTextFileWithRandomNumbers(string path, int count, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            var lines = Enumerable.Range(0, count).Select(_ => FormatRandomNumber(random));
            File.WriteAllLines(path, lines);
        }

        public static void WriteIncrementedNumbers(string sourcePath, string resultPath)
        {
            var incrementedNumbers = File.ReadLines(sourcePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(IncrementNumber);
            File.WriteAllLines(resultPath, incrementedNumbers);
        }

        public static void FillTextFileWithRandomNumberRows(string path, int rowCount, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);

            var rows = Enumerable.Range(0, rowCount).Select(_ => CreateRandomNumberRow(random));
            File.WriteAllLines(path, rows);
        }

        public static long CalculateSumOfFirstAndMinimum(string path)
        {
            long? first = null;
            var minimum = long.MaxValue;
            foreach (var line in File.ReadLines(path))
            {
                foreach (var text in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var number = long.Parse(text, CultureInfo.InvariantCulture);
                    first ??= number;
                    minimum = Math.Min(minimum, number);
                }
            }

            if (first is null) throw new InvalidOperationException("В файле нет ни одного числа.");

            return first.Value + minimum;
        }

        /// <summary>Регистр буквы не учитывается, пробелы в конце строки игнорируются.</summary>
        public static void CopyLinesEndingWith(string sourcePath, string resultPath, char letter)
        {
            var lines = File.ReadLines(sourcePath).Where(line => EndsWithLetter(line, letter));
            File.WriteAllLines(resultPath, lines);
        }

        private static void WriteBinaryNumbers(string path, IEnumerable<int> numbers)
        {
            using var writer = new BinaryWriter(File.Create(path));
            foreach (var number in numbers)
            {
                writer.Write(number);
            }
        }

        // Каждая игрушка сериализуется в XML и записывается в бинарный файл отдельной строкой.
        private static void WriteToys(string path, IEnumerable<Toy> toys)
        {
            using var writer = new BinaryWriter(File.Create(path));
            foreach (var toy in toys)
            {
                using var xml = new StringWriter(CultureInfo.InvariantCulture);
                s_toySerializer.Serialize(xml, toy);
                writer.Write(xml.ToString());
            }
        }

        private static bool IsEndOfFile(BinaryReader reader) =>
            reader.BaseStream.Position >= reader.BaseStream.Length;

        private static int NextRandomNumber(Random random) => random.Next(MinRandomNumber, MaxRandomNumber + 1);

        private static string FormatRandomNumber(Random random) =>
            NextRandomNumber(random).ToString(CultureInfo.InvariantCulture);

        private static string IncrementNumber(string number) =>
            (long.Parse(number, CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture);

        private static string CreateRandomNumberRow(Random random)
        {
            var count = random.Next(1, MaxNumbersInRow + 1);
            return string.Join(' ', Enumerable.Range(0, count).Select(_ => FormatRandomNumber(random)));
        }

        private static Toy CreateRandomToy(Random random)
        {
            var name = s_toyNames[random.Next(s_toyNames.Length)];
            var price = random.Next(MinToyPrice, MaxToyPrice + 1);
            var minAge = random.Next(0, MaxToyMinAge + 1);
            var maxAge = minAge + random.Next(1, MaxToyAgeSpan + 1);
            return new Toy(name, price, minAge, maxAge);
        }

        private static bool EndsWithLetter(string line, char letter)
        {
            var trimmedLine = line.TrimEnd();
            return trimmedLine.Length > 0 && char.ToLowerInvariant(trimmedLine[^1]) == char.ToLowerInvariant(letter);
        }
    }
}
