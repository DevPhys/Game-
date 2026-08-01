using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

class ByteCode(Dictionary<int, List<string>> Lines)
{
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
}