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
}