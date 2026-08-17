using System;
using System.Collections.Generic;
using System.Text;

public class Tokenes
{
    List<string> tokens = new List<string>();

    public List<string> Token(string NameFile)
    {
        string[] lines = File.ReadAllLines(NameFile);

        foreach (string line in lines)
        {
            int i = 0;
            while (i < line.Length)
            {
                char c = line[i];

                if (c == ' ' || c == '\t')  // пробел ИЛИ табуляция
                {
                    i++;
                    continue;
                }

                // Буквы — собираем слово
                if (char.IsLetter(c) || line[i] == '_')
                {
                    int start = i;
                    while (i < line.Length && (char.IsLetter(line[i]) || line[i] == '_' || char.IsDigit(line[i])))
                    {
                        i++;
                    }
                    tokens.Add(line.Substring(start, i - start));
                }
                // Цифры — собираем число
                else if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < line.Length && (char.IsDigit(line[i]) || line[i] == '.'))
                    {
                        i++;
                    }
                    tokens.Add(line.Substring(start, i - start));
                }
                else // Остальные символы по одному
                {
                    if (c != '[' && c != ']')
                        tokens.Add(c.ToString());
                    i++;
                }
            }
        }

        return tokens;
    }
    public Dictionary<int, List<string>> Tokenizer()
    {
        int lineNum = 0;
        Dictionary<int, List<string>> lines = new Dictionary<int, List<string>>();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (lines.ContainsKey(lineNum))
            {
                if (tokens[i] != ";")
                {
                    lines[lineNum].Add(tokens[i]);
                }
                else
                {
                    lineNum++;
                }
            }
            else
            {
                lines[lineNum] = [];
                i--;
                continue;
            }
        }

        return lines;
    }
}