using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public class Teams(Var[] Variables, string[] StringsArray)
{
    private double[] line = new double[] { };
    public StringBuilder _dynamicOutput = new StringBuilder(65536);

    private int lineNum = 0;

    public void UpdateLine(double[] Line, int LineNum)
    {
        line = Line;
        lineNum = LineNum;
    }
    public void Print()
    {
        //Console.WriteLine("Добавляем строку " + Cache[lineNum] + " для вывода");
        //Console.WriteLine("Строка в коде " + lineNum);
        //Console.WriteLine("Строка из кэша" + Cache[lineNum]);
        //Console.WriteLine("Кэш " + string.Join(" | ", Cache));
        //Console.WriteLine((int)line[1]);

        _dynamicOutput.Append(StringsArray[(int)line[1]]);
    }
    public void PrintVar()
    {
        Var nameVar = Variables[(int)line[1]];
        _dynamicOutput.Append(nameVar.GetValue()).Append("\n");
    }

    public void Calc()
    {
        double mainVariable = line[1];

        for (int i = 2; i < line.Length - 1; i += 2)
        {
            double nextNum = line[i + 1];
            double op = line[i];

            if (op == 1) mainVariable += nextNum;
            else if (op == 2) mainVariable -= nextNum;
            else if (op == 3) mainVariable *= nextNum;
            else if (op == 4) mainVariable /= nextNum;
            else if (op == 5) mainVariable = (int)Math.Pow(mainVariable, nextNum);
        }

        int idVar = (int)line[line.Length - 1];
        Variables[idVar] = mainVariable;
    }
    public void Sum()
    {
        //Console.WriteLine("Складываем!");
        double mainVariable = line[1];

        Var v;
        v = Variables[(int)mainVariable];

        if (v != null)
        {
            if (v.Type == VarType.Int)
                mainVariable = v._intValue;
            else
                mainVariable = v._floatValue;
        }

        for (int i = 2; i < line.Length - 2; i += 2)
        {
            double nextNum = line[i + 1];
            double op = line[i];

            v = Variables[(int)nextNum];
            if (v != null)
            {
                if (v.Type == VarType.Int)
                    nextNum = v._intValue;
                else
                    nextNum = v._floatValue;
            }

            if (op == 1) mainVariable += nextNum;
            else if (op == 2) mainVariable -= nextNum;
            else if (op == 3) mainVariable *= nextNum;
            else if (op == 4) mainVariable /= nextNum;
            else if (op == 5) mainVariable = (int)Math.Pow(mainVariable, nextNum);
        }

        int idVar = (int)line[line.Length - 1];
        Variables[idVar] = mainVariable;
    }
    public void SumStr()
    {
        string text = "";
        for (int i = 1; i < line.Length - 1; i++)
        {
            int id = (int)line[i];
            if (id != 255)
            {
                Var v = Variables[(int)id];

                if (v != null)
                {
                    text += v.ToString();
                }
                else
                {
                    text += StringsArray[id];
                }

                text += " ";
            }
        }

        int idVar = (int)line[line.Length - 1];
        Variables[idVar] = text;
    }

    public void Int() 
    {
        int mainVar = (int)line[2];
        int idVar = (int)line[1];

        Var v = Variables[mainVar];
        if (v != null)
        {
            Variables[idVar] = Variables[mainVar];
        }
        else
        {
            Variables[idVar] = mainVar;
        }
    }
    public void Float()
    {
        double mainVar = line[2];
        int idVar = (int)line[1];

        Var v = Variables[(int)mainVar];
        if (v != null)
        {
            Variables[idVar] = Variables[(int)mainVar];
        }
        else
        {
            Variables[idVar] = mainVar;
        }
    }
    public void Bool()
    {
        int mainVar = (int)line[2];
        int idVar = (int)line[1];

        if (mainVar == 1)
        {
            Variables[idVar] = true;
        }
        else if (mainVar == -1)
        {
            Variables[idVar] = false;
        }
        else
        {
            Variables[idVar] = Variables[mainVar];
        }
    }
    public void Str()
    {
        int mainVar = (int)line[2];
        int idVar = (int)line[1];

        Var v = Variables[mainVar];
        //Console.WriteLine("Переменная " + v);

        if (v != null)
        {
            Variables[idVar] = Variables[mainVar];
        }
        else
        {
            Variables[idVar] = StringsArray[mainVar];
        }
    }

    public void Refect()
    {
        int idVar = (int)line[2];
        int main = (int)line[1];

        Var v = Variables[main];
        string str = StringsArray[main];

        if (main == 1)
        {
            Variables[idVar] = true;
        }
        else if (main == -1)
        {
            Variables[idVar] = false;
        }
        else if (v == null)
        {
            if (str != null)
                Variables[idVar] = StringsArray[main];
            else
            {
                Variables[idVar] = main;
            }
        }
        else
        {
            Variables[idVar] = Variables[main];
        }
    }


    public void Comment()
    {

    }
    public void End()
    {
        string text = _dynamicOutput.ToString();

        //Console.WriteLine("Выводим вывод!");

        Console.Write(text.Replace("$", "\n"));
        _dynamicOutput.Clear();
    }
}