public class Main
{
    public string nameFile = @"data.txt";
    public string nameJIT = @"JIT.txt";

    public Var[] variables = new Var[1000];
    public int[] loopMemory = new int[1000];
    public Dictionary<string, Func<bool>> commandsValidate;
    public double[][] cacheNumInt =new double[1000][];

    public List<string> cache = new List<string>();
    public Action[] teamsArray;

    public ValidateLine validateLine = new ValidateLine();
    public Teams teams;
    public Tokenes tokenes = new Tokenes();

    public Main()
    {
        teams = new Teams(variables, cache, cacheNumInt);

        commandsValidate = new Dictionary<string, Func<bool>> {
            { "write", validateLine.Print },
            { "input", validateLine.Input  },

            { "calc", validateLine.Sum },

            { "int", validateLine.Int  },
            { "float", validateLine.Float  },
            { "write_var", validateLine.PrintVar },};
        teamsArray = new Action[]{
            teams.Print, teams.Input,
            teams.Int, teams.Float,

            teams.Calc,
            teams.Comment,

            teams.PrintVar,
            teams.Sum,};

    }
}