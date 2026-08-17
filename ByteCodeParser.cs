using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

using System.Linq;

class ByteCode(Dictionary<int, List<string>> Lines)
{
    MathSTR math = new MathSTR();
    int lineNum = 0;

    public int[] CreateCommandsByte()
    {
        // Сразу создаём массив нужного размера
        int[] commandArray = new int[Lines.Count];

        while (Lines.ContainsKey(lineNum))
        {
            string word = Lines[lineNum][0];

            if (word == "write") commandArray[lineNum] = 0;
            else if (word == "input") commandArray[lineNum] = 1;
            else if (word == "int") commandArray[lineNum] = 2;
            else if (word == "float") commandArray[lineNum] = 3;
            else if (word == "calc") commandArray[lineNum] = 4;
            else if (word == "#") commandArray[lineNum] = 5;
            else if (word == "write_var") commandArray[lineNum] = 6;
            else if (word == "add") commandArray[lineNum] = 7;
            else if (word == "loop") commandArray[lineNum] = 8;
            else if (word == "end_loop") commandArray[lineNum] = 9;

            lineNum++;
        }

        return commandArray;
    }

    public double[][] CreateByteCod(string[] StringsArray)
    {
        // Сразу создаём массив нужного размера
        double[][] byteCode = new double[Lines.Count][];
        lineNum = 0;

        while (Lines.ContainsKey(lineNum))
        {
            List<string> line = Lines[lineNum];
            string word = line[0];

            if (word == "write")
            {
                byteCode[lineNum] = new double[2];
                byteCode[lineNum][0] = 0;

                string str = "";

                for (int i = 2; i < line.Count - 1; i++)
                {
                    str += line[i];
                    if (i + 1 != line.Count - 1)
                        str += " ";
                }
                int strId = IdStr(str);

                byteCode[lineNum][1] = strId;
                StringsArray[strId] = str;
            }
            else if (word == "write_var")
            {
                byteCode[lineNum] = new double[2];

                byteCode[lineNum][0] = 1;
                byteCode[lineNum][1] = IdVar(line[1]);
            }

            else if (word == "loop")
            {
                byteCode[lineNum] = new double[2];

                byteCode[lineNum][0] = 9;
                byteCode[lineNum][1] = int.Parse(line[1]);
            }
            else if (word == "end_loop")
            {
                byteCode[lineNum] = new double[1];
                byteCode[lineNum][0] = 10;
            }

            else if (word == "int")
            {
                byteCode[lineNum] = new double[3];

                byteCode[lineNum][0] = 2;
                byteCode[lineNum][1] = IdVar(line[1]);

                string varMain = line[3];
                double mainVar = double.Parse(varMain);

                byteCode[lineNum][2] = mainVar;
            }
            else if (word == "float")
            {
                byteCode[lineNum] = new double[3];

                byteCode[lineNum][0] = 3;
                byteCode[lineNum][1] = IdVar(line[1]);

                string varMain = line[3].Replace(".", ",");
                double mainVar = double.Parse(varMain);

                byteCode[lineNum][2] = mainVar;
            }
            else if (word == "bool")
            {
                byteCode[lineNum] = new double[3];

                byteCode[lineNum][0] = 4;
                byteCode[lineNum][1] = IdVar(line[1]);

                string varMain = line[3];
                if (varMain == "False")
                    byteCode[lineNum][2] = -1;
                else if (varMain == "True")
                    byteCode[lineNum][2] = 1;
                else
                {
                    byteCode[lineNum][2] = IdVar(varMain);
                }
            }
            else if (word == "str")
            {
                byteCode[lineNum] = new double[3];

                byteCode[lineNum][0] = 5;
                byteCode[lineNum][1] = IdVar(line[1]);

                string str = "";

                for (int i = 3; i < line.Count - 1; i++)
                {
                    str += line[i];
                    if (i + 1 != line.Count - 1 && !str.StartsWith("'"))
                        str += " ";
                }

                //Console.WriteLine(str);

                string varMain = str;
                if (varMain.StartsWith("'"))
                {
                    varMain = varMain.Replace("'", "");

                    byteCode[lineNum][2] = IdStr(varMain);
                    StringsArray[IdStr(varMain)] = varMain;
                }
                else
                {
                    varMain = varMain.Replace(" ", "");
                    byteCode[lineNum][2] = IdVar(varMain);
                }
            }

            else if (word == "calc")
            {
                byteCode[lineNum] = new double[line.Count - 2];

                byteCode[lineNum][0] = 6;
                byteCode[lineNum][line.Count - 3] = IdVar(line[line.Count - 1]);

                for (int i = 1; i < line.Count - 3; i++)
                {
                    bool hasNoCommaOrDot = line[i].IndexOfAny(new[] { '+', '-', '*', '/', '$' }) == -1;

                    if (hasNoCommaOrDot)
                    {
                        double parsed = double.Parse(line[i]);

                        if (parsed < 50000)
                        {
                            byteCode[lineNum][i] = parsed;
                        }
                        else
                        {
                            byteCode[lineNum][i] = 50000;
                        }
                    }
                    else
                    {
                        string op = line[i];
                        int opNum = 1;

                        if (op == "-") opNum = 2;
                        else if (op == "*") opNum = 3;
                        else if (op == "/") opNum = 4;
                        else if (op == "@") opNum = 5;

                        byteCode[lineNum][i] = opNum;
                    }
                }

            }
            else if (word == "add")
            {
                byteCode[lineNum] = new double[line.Count - 2];

                byteCode[lineNum][0] = 7;
                byteCode[lineNum][line.Count - 3] = IdVar(line[line.Count - 1]);

                for (int i = 1; i < line.Count - 3; i++)
                {
                    bool hasNoCommaOrDot = line[i].IndexOfAny(new[] { '+', '-', '*', '/', '$' }) == -1;
                    // Исключаем экспоненциальную запись
                    NumberStyles numberStyle = NumberStyles.Float & ~NumberStyles.AllowExponent;

                    if (!double.TryParse(line[i], numberStyle, CultureInfo.InvariantCulture, out double parsed) && hasNoCommaOrDot)
                    {
                        byteCode[lineNum][i] = IdVar(line[i]);
                        //Console.WriteLine($"Встретели переменную! Строка {lineNum}, номер токена в строке {i}");
                    }
                    else if (hasNoCommaOrDot)
                    {
                        if (parsed < 50000)
                        {
                            byteCode[lineNum][i] = parsed;
                        }
                        else
                        {
                            byteCode[lineNum][i] = 50000;
                        }
                    }
                    else
                    {
                        string op = line[i];
                        int opNum = 1;

                        if (op == "-") opNum = 2;
                        else if (op == "*") opNum = 3;
                        else if (op == "/") opNum = 4;
                        else if (op == "@") opNum = 5;

                        byteCode[lineNum][i] = opNum;
                    }
                }
            }

            else if (word == "add_str")
            {
                byteCode[lineNum] = new double[line.Count - 2];

                byteCode[lineNum][0] = 8;
                byteCode[lineNum][line.Count - 3] = IdVar(line[line.Count - 1]);

                for (int i = 1; i < line.Count - 3; i++)
                {
                    if (line[i] != "'")
                    {
                        int idStr = IdStr(line[i]);

                        byteCode[lineNum][i] = idStr;
                        StringsArray[idStr] = line[i];
                    }
                    else
                    {
                        byteCode[lineNum][i] = 255;
                    }
                }
            }
            else if (word == "refect")
            {
                byteCode[lineNum] = new double[3];

                byteCode[lineNum][0] = 11;
                byteCode[lineNum][2] = IdVar(line[line.Count - 1]);

                string varMain = line[1];

                for (int i = 2; i < line.Count - 3; i++)
                {
                    varMain += line[i] + " ";
                }

                if (varMain == "False")
                    byteCode[lineNum][1] = -1;
                else if (varMain == "True")
                    byteCode[lineNum][1] = 1;
                else if (varMain.StartsWith("'"))
                {
                    varMain = varMain.Replace("'", "");

                    byteCode[lineNum][1] = IdStr(varMain);
                    StringsArray[IdStr(varMain)] = varMain;
                }
                else if (!string.IsNullOrEmpty(varMain) && varMain.All(c => char.IsDigit(c) || c == '.'))
                {
                    // Только цифры и точки — это число
                    byteCode[lineNum][1] = double.Parse(varMain);
                }
                else if (!string.IsNullOrEmpty(varMain))
                {
                    // Всё остальное непустое — это переменная
                    byteCode[lineNum][1] = IdVar(varMain);
                }
            }

            else
            {
                byteCode[lineNum] = new double[1];
                byteCode[lineNum][0] = 404;
            }

            lineNum++;
        }
        return byteCode;
    }

    private int IdVar(string nameVariable, int NUM = 999999)
    {
        if (string.IsNullOrEmpty(nameVariable)) return 0;

        uint hash = 2166136261;

        for (int i = 0; i < nameVariable.Length; i++)
        {
            hash ^= nameVariable[i];
            hash *= 16777619;
        }

        // Math.Abs гарантирует, что число не будет отрицательным
        return Math.Abs((int)hash) % NUM;
    }
    private int IdStr(string str, int NUM = 999999)
    {
        if (string.IsNullOrEmpty(str)) return 0;

        uint hash = 2166136261;

        for (int i = 0; i < str.Length; i++)
        {
            hash ^= str[i];
            hash *= 16777619;
        }

        // Math.Abs гарантирует, что число не будет отрицательным
        return Math.Abs((int)hash) % NUM;
    }
}