using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public class Teams(Var[] Variables, List<string> Cache, double[][] CacheNumInt)
{
    private List<string> line = new List<string>();
    List<string> finalStr = new List<string>();
    public StringBuilder _dynamicOutput = new StringBuilder(65536);

    private int lineNum = 0;
    public void UpdateLine(List<string> Line, int LineNum)
    {
        line = Line;
        lineNum = LineNum;
    }

    public void Input()
    {
        finalStr.Clear();

        for (int i = 2; i < line.Count - 4; i++)
        {
            finalStr.Add(line[i]);
        }
        Console.Write(string.Join(" ", finalStr));
        string? main = Console.ReadLine() ?? "null";

        Variables[IdVar(line[line.Count - 1])] = main;
    }
    public void Print()
    {
        //Console.WriteLine("Добавляем строку " + Cache[lineNum] + " для вывода");
        //Console.WriteLine("Строка в коде " + lineNum);
        //Console.WriteLine("Строка из кэша" + Cache[lineNum]);

        //Console.WriteLine("Кэш " + string.Join(" | ", Cache));

        _dynamicOutput.Append(Cache[lineNum]);
    }
    public void PrintVar()
    {
        Var nameVar = Variables[IdVar(line[1])];
        _dynamicOutput.Append(nameVar.GetValue()).Append("\n");
    }

    public void Calc()
    {
    }
    public void Sum()
    {
        string nameVariable = line[line.Count - 1];
        double mainVariable = 0.0;
        int id = IdVar(line[1]);

        // Проверяем первый операнд
        if (CacheNumInt[lineNum][0] == 0.0)
        {
            Var v = Variables[id];
            if (v != null)
            {
                if (v.Type == VarType.Int)
                    mainVariable = v._intValue;
                else if (v.Type == VarType.Double)
                    mainVariable = v._floatValue;
            }
        }
        else
        {
            mainVariable = CacheNumInt[lineNum][0];
        }

        for (int i = 1; i < CacheNumInt[lineNum].Length; i++)
        {
            double nextNum = CacheNumInt[lineNum][i];
            string op = line[i * 2];

            if (nextNum == 0.0)
            {
                id = IdVar(line[i * 2 - 1]);
                Var v = Variables[id];

                if (v != null)
                {
                    if (v.Type == VarType.Int)
                        nextNum = v._intValue;
                    else if (v.Type == VarType.Double)
                        nextNum = v._floatValue;
                }
            }

            if (op == "+") mainVariable += nextNum;
            else if (op == "-") mainVariable -= nextNum;
            else if (op == "*") mainVariable *= nextNum;
            else if (op == "/") mainVariable /= nextNum;
            else if (op == "@") mainVariable = (int)Math.Pow(mainVariable, nextNum);
        }

        id = IdVar(nameVariable);
        Variables[id] = mainVariable;
    }

    public void Int() { }
    public void Float()
    {
        string nameVariable = line[1];
        string numberDouble = line[line.Count - 1];
        double mainVariable = double.Parse(numberDouble, CultureInfo.InvariantCulture);

        int id = IdVar(nameVariable);
        Variables[id] = mainVariable;
    }


    public void Comment()
    {

    }
    public void End()
    {
        string text = _dynamicOutput.ToString();

        Console.Write(text.Replace("$", "\n"));
        _dynamicOutput.Clear();
    }

    private int IdVar(string nameVariable)
    {
        if (string.IsNullOrEmpty(nameVariable)) return 0;

        uint hash = 2166136261;

        for (int i = 0; i < nameVariable.Length; i++)
        {
            hash ^= nameVariable[i];
            hash *= 16777619;
        }

        // Math.Abs гарантирует, что число не будет отрицательным
        // % 1001 сжимает результат строго в рамки от 0 до 1000
        return Math.Abs((int)hash) % 1001;
    }
}