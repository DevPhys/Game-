using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

class MathSTR()
{
    public double Main(string Expr)
    {
        string expr = Expr;   // уже без скобок, 11 = 3+8
        double result = Evaluate(expr);

        return result;
    }
    static double Evaluate(string expression)
    {
        var numbers = new List<double>();
        var ops = new List<char>();

        int i = 0;
        bool expectNumber = true;   // флаг: ждём число (начало строки или после оператора)

        while (i < expression.Length)
        {
            // Унарный минус
            if (expression[i] == '-' && expectNumber)
            {
                i++;
                int start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    i++;
                double val = double.Parse(expression[start..i], CultureInfo.InvariantCulture);
                numbers.Add(-val);
                expectNumber = false;
                continue;
            }

            // Обычное число
            if (char.IsDigit(expression[i]) || expression[i] == '.')
            {
                int start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    i++;
                numbers.Add(double.Parse(expression[start..i], CultureInfo.InvariantCulture));
                expectNumber = false;
                continue;
            }

            // Бинарный оператор
            ops.Add(expression[i]);
            expectNumber = true;
            i++;
        }

        // Первый проход: * и /
        for (int j = 0; j < ops.Count; j++)
        {
            if (ops[j] == '*' || ops[j] == '/')
            {
                double a = numbers[j];
                double b = numbers[j + 1];
                double res = ops[j] == '*' ? a * b : a / b;

                numbers[j] = res;
                numbers.RemoveAt(j + 1);
                ops.RemoveAt(j);
                j--;
            }
        }

        // Второй проход: + и -
        double result = numbers[0];
        for (int j = 0; j < ops.Count; j++)
        {
            if (ops[j] == '+')
                result += numbers[j + 1];
            else
                result -= numbers[j + 1];
        }

        return result;
    }


    public void StaticTime(List<TimeSpan> time, string name = "Статистика времени выполнения")
    {
        int count = time.Count;
        TimeSpan total = TimeSpan.FromTicks(time.Sum(t => t.Ticks)); // Суммарное время 
        TimeSpan average = TimeSpan.FromTicks((long)time.Average(t => t.Ticks));  // Среднее время
        TimeSpan min = time.Min();  // Минимальное время
        TimeSpan max = time.Max();  // Максимальное время
        var sorted = time.OrderBy(t => t).ToList();  // Медиана (среднее значение по порядку)
        TimeSpan median;

        if (count % 2 == 0)
        {
            // Чётное количество — среднее двух центральных
            var mid1 = sorted[count / 2 - 1];
            var mid2 = sorted[count / 2];
            median = TimeSpan.FromTicks((mid1.Ticks + mid2.Ticks) / 2);
        }
        else
        {
            // Нечётное — центральный элемент
            median = sorted[count / 2];
        }

        TimeSpan range = max - min;  // Размах (макс - мин)

        // Стандартное отклонение
        // Стандартное отклонение
        double meanTicks = time.Average(t => t.Ticks);
        double sumOfSquares = time.Sum(t => Math.Pow(t.Ticks - meanTicks, 2));
        double stdDevTicks = Math.Sqrt(sumOfSquares / count);
        TimeSpan stdDev = TimeSpan.FromTicks((long)stdDevTicks);

        // Дисперсия — теперь правильно
        double varianceTicks = sumOfSquares / count;
        double varianceMs = varianceTicks / 100_000_000.0; // 10^8 тиков² в 1 мс²
        double varianceNs = varianceTicks / 10_000.0;      // или в наносекундах², если удобнее

        // Дисперсия
        TimeSpan variance = TimeSpan.FromTicks((long)(sumOfSquares / count));

        // ========== ПРОЦЕНТИЛИ ==========

        // 95-й процентиль (95% замеров быстрее этого значения)
        int percentile95Index = (int)Math.Ceiling(count * 0.95) - 1;
        TimeSpan percentile95 = sorted[Math.Min(percentile95Index, count - 1)];

        // 99-й процентиль
        int percentile99Index = (int)Math.Ceiling(count * 0.99) - 1;
        TimeSpan percentile99 = sorted[Math.Min(percentile99Index, count - 1)];

        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine($"{name}");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"Количество замеров:        {count:N0}");
        Console.WriteLine($"Суммарное время:           {total.TotalMilliseconds:F2} мс");
        Console.WriteLine($"Среднее время:             {average.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Минимальное время:         {min.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Максимальное время:        {max.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Медиана:                   {median.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Размах (max-min):          {range.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Стандартное отклонение:    {stdDev.TotalMilliseconds:F4} мс");
        Console.WriteLine($"Дисперсия:                 {varianceMs:F6} мс²");
        Console.WriteLine($"Дисперсия (в тиках²):      {varianceTicks:F0}");
        Console.WriteLine($"95-й процентиль:           {percentile95.TotalMilliseconds:F4} мс");
        Console.WriteLine($"99-й процентиль:           {percentile99.TotalMilliseconds:F4} мс");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════");
    }
}