using System.Text.RegularExpressions;

class LinearProgramming
{
    public double[] c0; // Вектор коэффициентов целевой функции
    public double[] c1;
    public double[,] A; // Матрица коэффициентов ограничений
    public double[] b; // Вектор ограничений
    public int startPoint;
    public int endPoint;

    public LinearProgramming(double[] c0, double[] c1, double[,] A, double[] b)
    {
        this.c0 = c0;
        this.c1 = c1;
        this.A = A;
        this.b = b;
    }

    // Метод для вывода модели в стандартном виде
    public void PrintStandardForm()
    {
        int originalRows = A.GetLength(0);
        int originalCols = A.GetLength(1);
        Console.WriteLine();
        Console.WriteLine("Математическая модель:");
        Console.WriteLine($"xi >= 0, i = {1} - {c0.Length-1}");
        double[,] tableau = new double[originalRows, originalRows+originalCols];
        for (int i = 0; i < originalRows; i++)
        {
            for (int j = 0; j < originalCols; j++)
            {
                tableau[i, j] = A[i, j];
            }
        }
        for (int i = 0; i < originalRows; i++)
        {
            tableau[i, originalCols + i] = 1;
        }
        for (int i = 0; i < b.Length; i++)
        {
            for (int j = 0; j < c0.Length - 1; j++)
            {
                Console.Write($"{tableau[i, j]}*x{j + 1}");
                if (j < c0.Length - 2)
                    Console.Write(" + ");
            }
            Console.WriteLine($" = {b[i]}");
        }
        Console.WriteLine("Целевая функция:");
        Console.Write("Максимизировать Z = ");
        for (int j = 0; j < c0.Length-1; j++)
        {
            if (c1[j] >= 0)
                Console.Write($"({c0[j]}+{c1[j]}t)*x{j + 1}");
            else
                Console.Write($"({c0[j]}{c1[j]}t)*x{j + 1}");
            if (j < c0.Length - 2)
            {
                if (c1[j] >= 0)
                    Console.Write(" + ");
                else
                    Console.Write(" ");
            }
        }
        Console.WriteLine("\n");
    }

    // Проверка на оптимальность
    static bool IsOptimal(double[] c1)
    {
        int numCols = c1.Length;
        for (int j = 0; j < numCols - 1; j++)
        {
            if (c1[j] < 0)
            {
                return false;
            }
        }
        return true;
    }

    // Выбор опорного столбца
    static int SelectPivotColumn(double[] c0, double[] c1)
    {
        int numCols = c1.Length;
        double minCoefficient = 0;
        int pivotColumn = -1;
        for (int j = 0; j < numCols - 1; j++)
        {
            if (c1[j] < minCoefficient)
            {
                minCoefficient = c1[j];
                pivotColumn = j;
            }
        }
        return pivotColumn;
    }

    // Выбор опорной строки
    static int SelectPivotRow(double[,] tableau, int pivotColumn)
    {
        int numRows = tableau.GetLength(0);
        double minRatio = double.MaxValue;
        int pivotRow = -1;

        for (int i = 0; i < numRows; i++)
        {
            if (tableau[i, pivotColumn] > 0)
            {
                double ratio = tableau[i, tableau.GetLength(1) - 1] / tableau[i, pivotColumn];
                if (ratio < minRatio)
                {
                    minRatio = ratio;
                    pivotRow = i;
                }
            }
        }

        return pivotRow;
    }

    // Обновление симплекс-таблицы
    // Обновление симплекс-таблицы
    static void UpdateTableau(double[,] tableau, int pivotRow, int pivotColumn, double[] c0, double[] c1)
    {
        int numRows = tableau.GetLength(0);
        int numCols = tableau.GetLength(1);
        double pivotElement = tableau[pivotRow, pivotColumn];

        if (pivotElement == 0)
            throw new InvalidOperationException("Проблема в симплекс-методе: опорный элемент равен нулю.");

        // Create a copy of the tableau before updating it
        double[,] tableauCopy = new double[numRows, numCols];
        Array.Copy(tableau, tableauCopy, tableau.Length);

        // Step 1: Divide the pivot row by the pivot element
        for (int j = 0; j < numCols; j++)
        {
            tableau[pivotRow, j] /= pivotElement;
        }

        // Step 2: Update the other rows
        for (int i = 0; i < numRows; i++)
        {
            if (i != pivotRow)
            {
                double rowMultiplier = tableauCopy[i, pivotColumn];
                for (int j = 0; j < numCols; j++)
                {
                    tableau[i, j] -= rowMultiplier * tableau[pivotRow, j];
                }
            }
        }

        // Step 3: Update the cost row (objective function coefficients)
        double[] c0Copy = new double[c0.Length];
        double[] c1Copy = new double[c1.Length];
        Array.Copy(c0, c0Copy, c0.Length);
        Array.Copy(c1, c1Copy, c1.Length);

        for (int j = 0; j < numCols; j++)
        {
            c0[j] -= c0Copy[pivotColumn] * tableau[pivotRow, j];
            c1[j] -= c1Copy[pivotColumn] * tableau[pivotRow, j];
        }
    }

    static void Main()
    {
        Console.Write("Введите количество строк: ");
        int originalRows = int.Parse(Console.ReadLine());

        Console.Write("Введите количество столбцов: ");
        int originalCols = int.Parse(Console.ReadLine());

        // Создание двумерного массива
        double[,] A = new double[originalRows, originalCols];

        // Ввод значений для двумерного массива
        Console.WriteLine();
        for (int i = 0; i < originalRows; i++)
        {
            Console.Write($"Введите коэффициенты при неизвестной X, для {i + 1}-ого уравнения через запятую: ");

            string[] rowElements = Console.ReadLine().Split(',', ' ');
            if (rowElements.Length != originalCols)
                throw new Exception("Вы ввели либо слишком много коэффициентов, либо слишком мало");

            for (int j = 0; j < originalCols; j++)
            {
                A[i, j] = double.Parse(rowElements[j]);
            }
        }

        double[] c = new double[originalCols], b = new double[originalRows]; // Коэффициенты целевой функции
        Console.WriteLine("Введите свободные члены системы уравнений через запятую: ");
        string[] bElements = Console.ReadLine().Split(',', ' ');
        if (bElements.Length != originalRows)
            throw new Exception("Вы ввели либо слишком много коэффициентов, либо слишком мало");
        for (int i = 0; i < originalRows; i++)
            b[i] = double.Parse(bElements[i]); // Ограничения

        // Ввод коэффициентов для переменных, зависящих от параметра t
        Console.WriteLine("Введите коэффициенты переменных для целевой функции через запятую:");
        string input = Console.ReadLine();

        // Разделение вводной строки на отдельные выражения
        string[] expressions = input.Split(',');

        // Создание массива для хранения обработанных выражений
        string[] results = new string[expressions.Length];

        // Регулярное выражение для разбора выражений вида 2+3t, 0+t, 1-2t и т.д.
        string pattern = @"^(\d+)([\+\-])(\d*)t$";
        Regex regex = new Regex(pattern);
        double[] c0 = new double[originalCols + originalRows + 1];
        double[] c1 = new double[originalCols + originalRows + 1];
        // Обработка каждого выражения
        for (int i = 0; i < expressions.Length; i++)
        {
            string expr = expressions[i].Trim();
            Match match = regex.Match(expr);

            if (match.Success)
            {
                // Извлечение значений a, оператора и b
                double f = double.Parse(match.Groups[1].Value);
                string operatorSymbol = match.Groups[2].Value;
                double g = string.IsNullOrEmpty(match.Groups[3].Value) ? 1 : double.Parse(match.Groups[3].Value);

                c0[i] = f;
                c1[i] = operatorSymbol == "+" ? g : -g;
            }
            else
            {
                // Если выражение не соответствует шаблону, сохранить его как есть
                results[i] = expr;
            }
        }

        Console.WriteLine("Целевая функция максимизируется 'max' или минимизируется 'min'?");
        string zFunction = Console.ReadLine();
        if (zFunction == "min")
            for (int j = 0; j < originalCols; j++)
                c[j] = -c[j];
        Console.Write("Введите отрезок в котором находятся значения параметра t через запятую: ");
        string[] a_b = Console.ReadLine().Split(',', ' ');
        int startPoint = int.Parse(a_b[0]);
        int endPoint = int.Parse(a_b[1]);

        LinearProgramming lp = new LinearProgramming(c0, c1, A, b);
        lp.PrintStandardForm();
        double[,] tableau = new double[originalRows, originalCols + originalRows + 1];

        // Копируем исходный массив в новый массив
        for (int i = 0; i < originalRows; i++)
        {
            for (int j = 0; j < originalCols; j++)
            {
                tableau[i, j] = A[i, j];
            }
        }
        for (int j = 0; j < originalCols; j++)
        {
            c0[j] = -c0[j];
            c1[j] = -c1[j];
        }

        // Добавляем балансовые переменные
        for (int i = 0; i < originalRows; i++)
        {
            tableau[i, originalCols + i] = 1;
            tableau[i, tableau.GetLength(1) - 1] = b[i];
        }
        for (int i = originalCols; i < originalCols + originalRows + 1; i++)
        {
            c0[i] = 0;
            c1[i] = 0;
        }

        double startSegment = startPoint, t = 0,endSegment;
        int numb = 0;
        do
        {
            Simplex(tableau, c0, c1, numb, startPoint, endPoint);
            if (numb > 0)
            {
                endSegment = startSegment;
                Console.Write($"{numb}-й отрезок в интервале [{startPoint}, {endPoint}]: ");
                for (int i = 0; i < c0.Length; i++)
                {
                    if (c1[i] != 0)
                    {
                        t = Math.Round(-c0[i] / c1[i], 2);
                        if (t > startSegment && t <= endPoint)
                        {
                            if (endSegment == startSegment || t < endSegment)
                            {
                                endSegment = t;
                            }
                        }
                    }
                }
                if (endSegment != startSegment)
                    Console.WriteLine($"{startSegment} <= t <= {endSegment}");
                else
                    Console.WriteLine($"{startSegment} <= t <= {endPoint}");
                startSegment = endSegment;
                Console.WriteLine();
            }
            numb++;
        }
        while (!IsOptimal(c1));

        Console.WriteLine($"{numb}-я итерация:");
        PrintTableau(tableau, c0, c1);
        OptimalSolution(tableau, c0, c1, numb);
        endSegment = startSegment;
        Console.Write($"{numb}-й отрезок в интервале [{startPoint}, {endPoint}]: ");
        for (int i = 0; i < c0.Length; i++)
        {
            if (c1[i] != 0)
            {
                t = Math.Round(-c0[i] / c1[i], 2);
                if (t > startSegment && t <= endPoint)
                {
                    if (endSegment == startSegment || t < endSegment)
                    {
                        endSegment = t;
                    }
                }
            }
        }
        if (endSegment != startSegment)
            Console.WriteLine($"{startSegment} <= t <= {endSegment}");
        else
            Console.WriteLine($"{startSegment} <= t <= {endPoint}");
        startSegment = endSegment;
        Console.WriteLine();
        Console.ReadLine();
    }

    static void Simplex(double[,] tableau, double[] c0, double[] c1, int numb, int startPoint, int endPoint)
    {
        int numCols = tableau.GetLength(1);
        int numRows = tableau.GetLength(0);
        if (numb == 0)
        {
            double[] Zt = new double[numCols];
            for (int j = 0; j < numCols - numRows - 1; j++)
            {
                Zt[j] = c0[j] + c1[j] * startPoint;
            }
            // Вывод исходной таблицы
            Console.WriteLine("Исходная симплекс таблица: ");
            PrintTableau(tableau, c0, c1);
            for (int j = 0; j < Zt.Length; j++)
            {
                Console.Write($"{Zt[j],14:F2}");
            }
            Console.WriteLine();

            OptimalSolution(tableau, c0, c1, numb);

            int pivotColumn = SelectPivotColumn(c0, Zt);
            int pivotRow = SelectPivotRow(tableau, pivotColumn);

            // Обновление симплекс-таблицы
            UpdateTableau(tableau, pivotRow, pivotColumn, c0, c1);
            numb++;
        }
        else
        {
            Console.WriteLine($"{numb}-я итерация:");
            PrintTableau(tableau, c0, c1);
            OptimalSolution(tableau, c0, c1, numb);

            int pivotColumn = SelectPivotColumn(c0, c1);
            int pivotRow = SelectPivotRow(tableau, pivotColumn);

            // Обновление симплекс-таблицы
            UpdateTableau(tableau, pivotRow, pivotColumn, c0, c1);
            numb++;
        }

    }
    static void OptimalSolution(double[,] tableau, double[] c0, double[] c1, int numb)
    {
        int numCols = tableau.GetLength(1);
        int numRows = tableau.GetLength(0);
        if (numb == 0)
        {
            // Вывод оптимального решения
            double[] X = new double[numCols - 1];
            for (int j = 0; j < numCols - 1; j++)
            {
                X[j] = 0;
            }
            Console.Write($"X*{numb} = (");
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols - 1; j++)
                {
                    if (tableau[i, j] == 1 & (tableau[0, j] == 0 || tableau[numRows - 1, j] == 0))
                    {
                        X[j] = tableau[i, numCols - 1];
                        break;
                    }
                }
            }
            Console.WriteLine(string.Join("; ", X.Select(x => Math.Round(x, 2))) + ");");

            // Final target function output
            if (c1[numCols - 1] >= 0)
                Console.WriteLine($"Z(t)max: {Math.Round(c0[numCols - 1], 2)} + {Math.Round(c1[numCols - 1], 2)}t");
            else
                Console.WriteLine($"Z(t)max: {Math.Round(c0[numCols - 1], 2)} {Math.Round(c1[numCols - 1], 2)}t");
        }
        else
        {
            // Вывод оптимального решения
            double[] X = new double[numCols - 1];
            for (int j = 0; j < numCols - 1; j++)
            {
                X[j] = 0;
            }
            Console.Write($"X*{numb} = (");
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols - 1; j++)
                {
                    if (tableau[i, j] == 1 & (tableau[0, j] == 0 || tableau[numRows - 1, j] == 0))
                    {
                        X[j] = tableau[i, numCols - 1];
                        break;
                    }
                }
            }
            Console.WriteLine(string.Join("; ", X.Select(x => Math.Round(x, 2))) + ");");

            // Final target function output
            if (c1[numCols - 1] >= 0)
                Console.WriteLine($"Z(t)max: {Math.Round(c0[numCols - 1], 2)} + {Math.Round(c1[numCols - 1], 2)}t");
            else
                Console.WriteLine($"Z(t)max: {Math.Round(c0[numCols - 1], 2)} {Math.Round(c1[numCols - 1], 2)}t");
        }
    }
    static void PrintTableau(double[,] tableau, double[] c0, double[] c1)
    {
        int rows = tableau.GetLength(0);
        int cols = tableau.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{tableau[i, j],14:F2} ");
            }
            Console.WriteLine();
        }
        for (int j = 0; j < cols; j++)
        {
            Console.Write($"{c0[j],7:F2} ");
            if (c1[j] < 0)
                Console.Write($" {c1[j],5:F2}t");
            else Console.Write($"+{c1[j],5:F2}t");
        }
        Console.WriteLine();
    }
}