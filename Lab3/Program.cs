using System;
using System.IO;
using System.Text;

namespace Lab3
{
    internal static class Program
    {
        private const int MaxMatrixSize = 10;
        private const int MaxFileItemCount = 30;
        private const char LetterBe = 'б';
        private const string BinaryNumbersPath = "task4_numbers.bin";
        private const string BinaryProductsPath = "task4_products.bin";
        private const string ToysPath = "task5_toys.bin";
        private const string NumbersPath = "task6_numbers.txt";
        private const string IncrementedNumbersPath = "task6_incremented.txt";
        private const string NumberRowsPath = "task7_numbers.txt";
        private const string TextPath = "task8_text.txt";
        private const string LinesEndingWithBePath = "task8_result.txt";
        private const string Menu = """
            Лабораторная работа №3. Вариант 15
            1 - заполнение двумерных массивов
            2 - произведение столбцов
            3 - выражение 7*A - (Bт - 3*C)
            4 - бинарный файл: произведения соседних чисел
            5 - бинарный файл: возраст для конструктора
            6 - текстовый файл: увеличить каждое число на 1
            7 - текстовый файл: сумма первого и минимального
            8 - текстовый файл: строки, оканчивающиеся на «б»
            0 - выход
            """;

        private static readonly string[] s_sampleText =
        [
            "Утром мы пошли в лес за грибами",
            "Под старой елью нашёлся гриб",
            "Рядом росли брусника и черника",
            "На берегу реки стоял высокий дуб",
            "Корабль медленно отходил от причала",
            "СУГРОБ",
            "На ветке сидел голубь",
            "Бабушка испекла свежий хлеб   ",
            "",
            "Конец рассказа.",
        ];

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            while (true)
            {
                Console.WriteLine(Menu);
                var taskNumber = ConsoleInput.ReadInt32("Номер задания: ", 0, 8);
                if (taskNumber == 0) return;

                RunTask(taskNumber);
            }
        }

        private static void RunTask(int taskNumber)
        {
            Action task = taskNumber switch
            {
                1 => FillArrays,
                2 => MultiplyColumns,
                3 => EvaluateExpression,
                4 => MultiplyAdjacentNumbers,
                5 => FindBuildingSetAge,
                6 => IncrementNumbers,
                7 => SumFirstAndMinimum,
                8 => CopyLinesEndingWithBe,
                _ => throw new ArgumentOutOfRangeException(nameof(taskNumber)),
            };

            Console.WriteLine();
            try
            {
                task();
            }
            catch (IOException exception)
            {
                Console.WriteLine($"Ошибка при работе с файлом: {exception.Message}");
            }

            Console.WriteLine();
        }

        private static void FillArrays()
        {
            Console.WriteLine("Задание 1. Заполнение двумерных массивов");
            var rowCount = ConsoleInput.ReadInt32("n = ", 1, MaxMatrixSize);
            var columnCount = ConsoleInput.ReadInt32("m = ", 1, MaxMatrixSize);
            var first = ReadMatrix("a", rowCount, columnCount);

            PrintMatrix("Первый массив:", first);
            PrintMatrix("Второй массив:", new Matrix(rowCount, Random.Shared));
            PrintMatrix("Третий массив:", new Matrix(rowCount));
        }

        private static void MultiplyColumns()
        {
            Console.WriteLine("Задание 2. Произведение столбцов, у которых первый элемент больше элементов диагоналей");
            var rowCount = ConsoleInput.ReadInt32("n = ", 1, MaxMatrixSize);
            var columnCount = ConsoleInput.ReadInt32("m = ", 1, MaxMatrixSize);
            var matrix = ReadMatrix("a", rowCount, columnCount);
            PrintMatrix("Массив:", matrix);

            var columns = matrix.FindColumnsWithFirstElementAboveDiagonals();
            if (columns.Count == 0)
            {
                Console.WriteLine("Таких столбцов нет.");
                return;
            }

            var totalProduct = 1.0;
            foreach (var column in columns)
            {
                var product = matrix.GetColumnProduct(column);
                totalProduct *= product;
                Console.WriteLine($"Столбец {column + 1}: произведение = {NumberFormatter.Format(product)}");
            }

            Console.WriteLine($"Произведение всех найденных столбцов = {NumberFormatter.Format(totalProduct)}");
        }

        private static void EvaluateExpression()
        {
            Console.WriteLine("Задание 3. Вычисление 7*A - (Bт - 3*C)");
            var size = ConsoleInput.ReadInt32("Размер матриц n = ", 1, MaxMatrixSize);
            var matrixA = ReadMatrix("A", size, size);
            var matrixB = new Matrix(size, Random.Shared);
            var matrixC = new Matrix(size);

            var result = 7 * matrixA - (matrixB.Transpose() - 3 * matrixC);

            PrintMatrix("A:", matrixA);
            PrintMatrix("B:", matrixB);
            PrintMatrix("C:", matrixC);
            PrintMatrix("7*A - (Bт - 3*C):", result);
        }

        private static void MultiplyAdjacentNumbers()
        {
            Console.WriteLine("Задание 4. Произведения соседних компонент бинарного файла");
            var count = ConsoleInput.ReadInt32("Количество чисел в файле: ", 1, MaxFileItemCount);
            FileTasks.FillBinaryFileWithRandomNumbers(BinaryNumbersPath, count, Random.Shared);
            FileTasks.WriteAdjacentProducts(BinaryNumbersPath, BinaryProductsPath);

            var numbers = FileTasks.ReadBinaryNumbers(BinaryNumbersPath);
            var products = FileTasks.ReadBinaryProducts(BinaryProductsPath);
            Console.WriteLine($"{BinaryNumbersPath}: {string.Join(' ', numbers)}");
            Console.WriteLine($"{BinaryProductsPath}: {string.Join(' ', products)}");
        }

        private static void FindBuildingSetAge()
        {
            Console.WriteLine("Задание 5. Для детей какого возраста предназначен конструктор");
            var count = ConsoleInput.ReadInt32("Количество игрушек в файле: ", 1, MaxFileItemCount);
            FileTasks.FillToysFile(ToysPath, count, Random.Shared);

            Console.WriteLine($"{ToysPath}:");
            foreach (var toy in FileTasks.ReadToys(ToysPath))
            {
                Console.WriteLine($"{toy.Name,-12} {toy.Price,5} руб.  от {toy.MinAge} до {toy.MaxAge} лет");
            }

            var buildingSets = FileTasks.FindBuildingSets(ToysPath);
            if (buildingSets.Count == 0)
            {
                Console.WriteLine("Конструкторов в файле нет.");
                return;
            }

            foreach (var (_, price, minAge, maxAge) in buildingSets)
            {
                Console.WriteLine($"Конструктор за {price} руб. - для детей от {minAge} до {maxAge} лет");
            }
        }

        private static void IncrementNumbers()
        {
            Console.WriteLine("Задание 6. Увеличить каждое число текстового файла на 1");
            var count = ConsoleInput.ReadInt32("Количество чисел в файле: ", 1, MaxFileItemCount);
            FileTasks.FillTextFileWithRandomNumbers(NumbersPath, count, Random.Shared);
            FileTasks.WriteIncrementedNumbers(NumbersPath, IncrementedNumbersPath);

            Console.WriteLine($"{NumbersPath}: {string.Join(' ', File.ReadLines(NumbersPath))}");
            Console.WriteLine($"{IncrementedNumbersPath}: {string.Join(' ', File.ReadLines(IncrementedNumbersPath))}");
        }

        private static void SumFirstAndMinimum()
        {
            Console.WriteLine("Задание 7. Сумма первого и минимального элементов");
            var rowCount = ConsoleInput.ReadInt32("Количество строк в файле: ", 1, MaxFileItemCount);
            FileTasks.FillTextFileWithRandomNumberRows(NumberRowsPath, rowCount, Random.Shared);

            Console.WriteLine($"{NumberRowsPath}:");
            Console.WriteLine(File.ReadAllText(NumberRowsPath));
            var sum = FileTasks.CalculateSumOfFirstAndMinimum(NumberRowsPath);
            Console.WriteLine($"Сумма первого и минимального = {sum}");
        }

        private static void CopyLinesEndingWithBe()
        {
            Console.WriteLine("Задание 8. Строки, оканчивающиеся буквой «б»");
            if (!File.Exists(TextPath))
            {
                File.WriteAllLines(TextPath, s_sampleText);
            }

            FileTasks.CopyLinesEndingWith(TextPath, LinesEndingWithBePath, LetterBe);

            Console.WriteLine($"{TextPath}:");
            Console.WriteLine(File.ReadAllText(TextPath));
            Console.WriteLine($"{LinesEndingWithBePath}:");
            Console.WriteLine(File.ReadAllText(LinesEndingWithBePath));
        }

        private static Matrix ReadMatrix(string name, int rowCount, int columnCount)
        {
            Console.WriteLine("Ввод по диагоналям, параллельным побочной, начиная с правого нижнего угла:");
            return new Matrix(
                rowCount,
                columnCount,
                (row, column) => ConsoleInput.ReadDouble($"{name}[{row + 1},{column + 1}] = "));
        }

        private static void PrintMatrix(string title, Matrix matrix)
        {
            Console.WriteLine(title);
            Console.WriteLine(matrix);
        }
    }
}
