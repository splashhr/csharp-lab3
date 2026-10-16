using System;
using System.Collections.Generic;
using System.Text;

namespace Lab3
{
    internal sealed class Matrix
    {
        private readonly double[,] _elements;

        /// <summary>Заполняет матрицу по диагоналям, параллельным побочной, начиная с правого нижнего угла.</summary>
        public Matrix(int rowCount, int columnCount, Func<int, int, double> readElement)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rowCount);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columnCount);
            ArgumentNullException.ThrowIfNull(readElement);

            _elements = new double[rowCount, columnCount];
            for (var diagonal = rowCount + columnCount - 2; diagonal >= 0; diagonal--)
            {
                var firstRow = Math.Max(0, diagonal - columnCount + 1);
                var lastRow = Math.Min(rowCount - 1, diagonal);
                for (var row = firstRow; row <= lastRow; row++)
                {
                    var column = diagonal - row;
                    _elements[row, column] = readElement(row, column);
                }
            }
        }

        /// <summary>Выше главной диагонали - числа из (0.5; 1], на ней и ниже - из [0; 0.5].</summary>
        public Matrix(int size, Random random)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
            ArgumentNullException.ThrowIfNull(random);

            _elements = new double[size, size];
            for (var row = 0; row < size; row++)
            {
                for (var column = 0; column < size; column++)
                {
                    _elements[row, column] = column > row ? NextAboveHalf(random) : NextUpToHalf(random);
                }
            }
        }

        /// <summary>Спираль из чисел от size² до 1: от левого верхнего угла вниз, против часовой стрелки.</summary>
        public Matrix(int size)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

            _elements = new double[size, size];
            var value = size * size;
            var top = 0;
            var bottom = size - 1;
            var left = 0;
            var right = size - 1;
            while (value > 0)
            {
                for (var row = top; row <= bottom; row++)
                {
                    _elements[row, left] = value--;
                }

                left++;
                for (var column = left; column <= right; column++)
                {
                    _elements[bottom, column] = value--;
                }

                bottom--;
                for (var row = bottom; row >= top; row--)
                {
                    _elements[row, right] = value--;
                }

                right--;
                for (var column = right; column >= left; column--)
                {
                    _elements[top, column] = value--;
                }

                top++;
            }
        }

        private Matrix(double[,] elements)
        {
            _elements = elements;
        }

        public int RowCount => _elements.GetLength(0);

        public int ColumnCount => _elements.GetLength(1);

        public static Matrix operator *(double scalar, Matrix matrix)
        {
            ArgumentNullException.ThrowIfNull(matrix);

            var result = new double[matrix.RowCount, matrix.ColumnCount];
            for (var row = 0; row < matrix.RowCount; row++)
            {
                for (var column = 0; column < matrix.ColumnCount; column++)
                {
                    result[row, column] = matrix._elements[row, column] * scalar;
                }
            }

            return new Matrix(result);
        }

        public static Matrix operator -(Matrix left, Matrix right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);
            if (left.RowCount != right.RowCount || left.ColumnCount != right.ColumnCount)
            {
                throw new ArgumentException("Для вычитания матрицы должны быть одного размера.", nameof(right));
            }

            var result = new double[left.RowCount, left.ColumnCount];
            for (var row = 0; row < left.RowCount; row++)
            {
                for (var column = 0; column < left.ColumnCount; column++)
                {
                    result[row, column] = left._elements[row, column] - right._elements[row, column];
                }
            }

            return new Matrix(result);
        }

        public Matrix Transpose()
        {
            var result = new double[ColumnCount, RowCount];
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    result[column, row] = _elements[row, column];
                }
            }

            return new Matrix(result);
        }

        /// <summary>
        /// Номера столбцов (с нуля), первый элемент которых больше всех элементов главной и побочной диагоналей.
        /// Если первый элемент сам лежит на диагонали, с собой он не сравнивается.
        /// </summary>
        public IReadOnlyList<int> FindColumnsWithFirstElementAboveDiagonals()
        {
            var columns = new List<int>();
            for (var column = 0; column < ColumnCount; column++)
            {
                if (IsFirstElementAboveDiagonals(column))
                {
                    columns.Add(column);
                }
            }

            return columns;
        }

        public double GetColumnProduct(int column)
        {
            var product = 1.0;
            for (var row = 0; row < RowCount; row++)
            {
                product *= _elements[row, column];
            }

            return product;
        }

        public override string ToString()
        {
            var cells = new string[RowCount, ColumnCount];
            var width = 0;
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    cells[row, column] = NumberFormatter.Format(_elements[row, column]);
                    width = Math.Max(width, cells[row, column].Length);
                }
            }

            var builder = new StringBuilder();
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    var separator = column == 0 ? string.Empty : "  ";
                    builder.Append(separator).Append(cells[row, column].PadLeft(width));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        // Сотые доли, чтобы матрицу можно было без потерь перенести в онлайн-калькулятор.
        private static double NextAboveHalf(Random random) => random.Next(51, 101) / 100.0;

        private static double NextUpToHalf(Random random) => random.Next(0, 51) / 100.0;

        private bool IsFirstElementAboveDiagonals(int column)
        {
            var firstElement = _elements[0, column];
            var diagonalLength = Math.Min(RowCount, ColumnCount);
            for (var row = 0; row < diagonalLength; row++)
            {
                if (!IsAboveOtherCell(firstElement, column, row, row)) return false;
                if (!IsAboveOtherCell(firstElement, column, row, ColumnCount - 1 - row)) return false;
            }

            return true;
        }

        private bool IsAboveOtherCell(double firstElement, int firstElementColumn, int row, int column) =>
            (row == 0 && column == firstElementColumn) || firstElement > _elements[row, column];
    }
}
