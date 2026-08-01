using System;
using System.Collections.Generic;
using System.Text;

public class ValidateLine()
{
    private int lineCount = 0;
    private List<string> line = new List<string>();

    public void Update(List<string> Line)
    {
        line = Line;
        lineCount = line.Count - 1;
    }

    public bool Print()
    {
        if (line[1] != "'") return false;
        if (line[lineCount] != "'") return false;

        return true;
    }
    public bool PrintVar()
    {
        if (!IsValidName(line[1])) return false;
        if (line.Count != 2) return false;

        return true;
    }
    public bool Input()
    {
        if (line[lineCount - 3] != "'") return false;
        if (line[1] != "'") return false;
        if (line[lineCount - 1] != ">") return false;
        if (line[lineCount - 2] != "-") return false;

        return true;
    }

    public bool Sum()
    {
        if (line[lineCount - 1] != ">") return false;
        if (line[lineCount - 2] != "-") return false;

        return true;
    }

    public bool Int()
    {
        if (lineCount > 3) return false;
        if (line[3].Contains('.')) return false;
        if (line[2] != "=") return false;

        if (!IsValidName(line[1])) return false;

        return true;
    }
    public bool Float()
    {
        bool bl = !line[3].Contains('.');

        if (lineCount > 3) return false;
        if (bl) return false;
        if (line[2] != "=") return false;

        if (!IsValidName(line[1])) return false;

        return true;
    }



    private bool IsValidName(string name)
    {
        for (int i = 0; i < name.Length; i++)
        {
            if ((name[i] != '_' && !char.IsLetterOrDigit(name[i])) || char.IsDigit(name[0])) return false;
        }

        return true;
    }
}