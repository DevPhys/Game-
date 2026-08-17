public class Main
{
    public Var[] variables;
    public int[] loopMemory;
    public Action[] teamsArray;
    public string[] stringsArray;

    public Dictionary<string, Func<bool>> commandsValidate;

    public ValidateLine validateLine = new ValidateLine();
    public Teams teams;
    public Tokenes tokenes = new Tokenes();

    public Main(int NumVar = 999999, int NumLoopMemory = 999999, int NumString = 999999)
    {
        variables = new Var[NumVar];
        loopMemory = new int[NumLoopMemory];
        stringsArray = new string[NumString];

        teams = new Teams(variables, stringsArray);

        commandsValidate = new Dictionary<string, Func<bool>> {
            { "write", validateLine.Print },
            { "input", validateLine.Input  },

            { "calc", validateLine.Sum },

            { "int", validateLine.Int  },
            { "float", validateLine.Float  },
            { "write_var", validateLine.PrintVar },};
        teamsArray = new Action[]{
            teams.Print, teams.PrintVar,

            teams.Int, teams.Float,
            teams.Bool, teams.Str,

            teams.Calc, teams.Sum,
            teams.SumStr,

            teams.Comment,
            teams.Comment,

            teams.Refect,
        };
    }
}