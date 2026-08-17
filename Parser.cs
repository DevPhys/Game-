using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

public class Programm
{
    string nameFile = "";

    Tokenes Tokens;
    Parser parser;
    Teams teams;

    List<string> cache;

    private CancellationTokenSource GcTokenSource = new CancellationTokenSource();

    public Programm (Parser parser, Main main)
    {
        Tokens = main.tokenes;
        teams = main.teams;

        this.parser = parser;
        //cache = main.cache;
    }
    private void Start(bool bl)
    {
        parser.tokens = Tokens.Token(nameFile);
        if (bl)
            //Console.WriteLine("| " + string.Join(" | ", parser.tokens) + " |\n");

        parser.Tokenizer();
        //parser.Validates();
    }
    private void Compiler()
    {
        parser.CompilerPase1();

        ByteCode byteCode = new ByteCode(parser.lines);
        parser.commandArray = byteCode.CreateCommandsByte();

        parser.Warmup();
    }
    private void GCstart()
    {
        var oldMode = GCSettings.LatencyMode;

        // Batch для серверной/долгой работы
        GCSettings.LatencyMode = GCLatencyMode.Batch;

        Task.Run(async () =>
        {
            try
            {
                while (!GcTokenSource.Token.IsCancellationRequested)
                {
                    // Проверяем раз в 100 мс, не каждую миллисекунду
                    await Task.Delay(100, GcTokenSource.Token);

                    // Даём GC подсказку собрать Gen 0, если нужно
                    GC.Collect(0, GCCollectionMode.Optimized, false);
                }
            }
            finally
            {
                GCSettings.LatencyMode = oldMode;
            }
        }, GcTokenSource.Token);
    }
    private void GCstop()
    {
        GcTokenSource?.Cancel();
    }
    private (TimeSpan sw, TimeSpan swCod) RunCod(bool WriteConsole)
    {
        var sw = Stopwatch.StartNew();

        // Начало выполнение программы
        Start(WriteConsole);

        // Компиляция
        Compiler();
        GCstart();


        var swCod = Stopwatch.StartNew();

        // Выполнение основного кода
        parser.ExecutionCod();

        swCod.Stop();

        return (sw.Elapsed, swCod.Elapsed);
    }
    public (TimeSpan sw, TimeSpan swCod) Run(string Name, bool WriteConsole)
    {
        nameFile = Name;
        var swTime = RunCod(WriteConsole);

        // Вывод
        if (WriteConsole)
        {
            var sw = Stopwatch.StartNew();

            Console.WriteLine("Вывод:");
            if (parser.isTextWrite)
                Console.Write(cache[0]);
            else
                teams.End();
            GCstop();

            sw.Stop();
            Console.WriteLine($"Время вывода: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)\n");
        }

        return swTime;
    }
}
public class Parser 
{
    int lineNum = 0;
    string word = "";

    bool isValidate = true;
    bool flag = true;

    public bool isTextWrite = true;

    public int[] commandArray = new int[] { };

    public List<string> line = new List<string>();
    public List<string> tokens = new List<string>();
    List<string> cache;

    Action[] commands;

    public Dictionary<int, List<string>> lines = new Dictionary<int, List<string>>();
    Dictionary<string, Func<bool>> commandsValidate;
    Var[] variables;
    int[] loopMemory;

    double[][] cacheNumInt;
    public int[][] cacheIdVar;

    MathSTR math = new MathSTR();

    Teams teams;
    Tokenes t = new Tokenes();
    ValidateLine validateLine;
    public Parser (Main main)
    {
        commands = main.teamsArray;
        commandsValidate = main.commandsValidate;
        teams = main.teams;
        validateLine = main.validateLine;
        variables = main.variables;
        //cache = main.cache;
        loopMemory = main.loopMemory;
        //cacheNumInt = main.cacheNumInt;
        //cacheIdVar = main.cacheIdVar;
    }

    public Dictionary<int, List<string>> Tokenizer()
    {
        line.Clear();
        lineNum = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (i > tokens.Count)
                break;

            if (lines.ContainsKey(lineNum))
            {
                if (tokens[i] != ";")
                {
                    lines[lineNum].Add(tokens[i]);
                    line.Add(tokens[i]);
                }
                else
                {
                    //Console.WriteLine($"Строка {lineNum}: {string.Join(" | ", line)}");
                    //Console.WriteLine($"Создали строку в список! Строка {lineNum}. Список {string.Join(" | ", line)}");
                    lineNum++;
                    line.Clear();
                }
            }
            else
            {
                lines[lineNum] = [];
                i--;
                continue;
            }
        }

        lineNum = 0;
        line.Clear();

        return lines;
    }
    public void Validates()
    {
        while (lines.ContainsKey(lineNum))
        {
            word = lines[lineNum][0];
            validateLine.Update(lines[lineNum]);

            if (word == "#")
            {
                lineNum++;
                continue;
            }

            if (commandsValidate.ContainsKey(word))
            {
                try
                {
                    isValidate = commandsValidate[word]();
                    if (!isValidate)
                    {
                        Console.WriteLine($"Синтаксическая ошибка на строке {lineNum}");
                    }
                }
                catch (Exception ex)
                {
                    isValidate = false;
                    Console.WriteLine($"\nСистемная ошибка команды '{word}' на строке {lineNum}: {ex.Message}\n");
                }
            }
            else
            {
                if (word != "end_programm")
                {
                    isValidate = false;
                    Console.WriteLine($"Ошибка! Неизвестная команда '{word}' -> Строка: {lineNum}");
                }
            }

            // Console.WriteLine($"Номер строки: {lineNum}, длина строки: {lineCount}");

            if (!isValidate && flag)
                flag = false;

            lineNum++;
        }

        isValidate = flag;
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
        return Math.Abs((int)hash) % 100000;
    }
    public void CompilerPase1()
    {
        word = "";
        lineNum = 0;

        while (lines.ContainsKey(lineNum))
        {
            word = lines[lineNum][0];
            cacheIdVar[lineNum] = new int[lines[lineNum].Count];

            if (word != "write") isTextWrite = false;

            if (word == "write")
            {
                string finalStr = "";

                for (int i = 1; i < lines[lineNum].Count - 1; i++)
                {
                    if (lines[lineNum][i] != "'")
                    {
                        if (lines[lineNum][i] == "$")
                            finalStr += lines[lineNum][i];
                        else
                            finalStr += lines[lineNum][i] + " ";
                    }
                }

                cache.Add(finalStr);
                lineNum++;
                continue;
            }
            else if (word == "calc")
            {
                string expression = "";
                string nameVariable = lines[lineNum][lines[lineNum].Count - 1];

                for (int i = 1; i < lines[lineNum].Count - 3; i++)
                {
                    expression += lines[lineNum][i];
                }
                expression = expression.Replace(",", ".");
                double mainVariable = math.Main(expression);

                cacheNumInt[lineNum] = new double[1];
                cacheNumInt[lineNum][0] = mainVariable;

                int id = IdVar(nameVariable);
                variables[id] = mainVariable;
            }
            else if (word == "add")
            {
                double mainVariable = 0.0;

                if (double.TryParse(lines[lineNum][1], NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    mainVariable = parsed;
                }
                else
                {
                    cacheIdVar[lineNum][1] = IdVar(lines[lineNum][1]);
                }

                int nameVarI = lines[lineNum].Count - 1;

                cacheNumInt[lineNum] = new double[lines[lineNum].Count];

                for (int i = 2; i < lines[lineNum].Count - 3; i += 2)
                {
                    string op = lines[lineNum][i];
                    int opNum = 1;

                    if (double.TryParse(lines[lineNum][i + 1].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedNext))
                    {
                        //Console.WriteLine("Добавляем число: " + parsedNext);
                        //Console.WriteLine("До " + string.Join(" | ", cacheNumInt[lineNum]));

                        cacheNumInt[lineNum][i + 1] = parsedNext;

                        //Console.WriteLine("После " + string.Join(" | ", cacheNumInt[lineNum]));
                    }
                    else
                    {
                        cacheIdVar[lineNum][i + 1] = IdVar(lines[lineNum][i + 1]);
                        cacheNumInt[lineNum][i + 1] = 0.0;
                    }

                    if (op == "-") opNum = 2;
                    else if (op == "*") opNum = 3;
                    else if (op == "/") opNum = 4;
                    else if (op == "@") opNum = 5;

                    cacheNumInt[lineNum][i] = opNum;
                }

                Console.WriteLine(string.Join(" | ", cacheNumInt[lineNum]));

                int id = IdVar(lines[lineNum][lines[lineNum].Count - 1]);
                if (variables[id] == null)
                    variables[id] = 0;
            }
            else if (word == "int")
            {
                string nameVariable = lines[lineNum][1];
                int mainVariable = int.Parse(lines[lineNum][lines[lineNum].Count - 1]);

                int id = IdVar(nameVariable);
                variables[id] = mainVariable;
            }
            else if (word == "loop")
            {
                for (int i = lineNum; i < lines.Count; i++)
                {
                    //Console.WriteLine(lines[i][0]);

                    if (lines[i][0] == "end_loop")
                    {
                        loopMemory[lineNum] = i;
                        //Console.WriteLine("Строка 1 " + i);

                        break;
                    }
                }
            }

            // Эта строка выполняется для всех команд КРОМЕ write (у write свой continue)
            cache.Add("Заглушка");
            lineNum++;
        }

        for (int i = 0; i < cacheIdVar.Length - 1; i++)
        {
            if (cacheIdVar[i] != null)
                Console.WriteLine("Массив " + i + " " + string.Join(" | ", cacheIdVar[i]));
        }

        if (isTextWrite)
        {
            for (int i = 0; i < cache.Count; i++)
            {
                if (i != 0)
                    cache[0] += cache[i];
            }
        }
    }
    public void Warmup()
    {
        foreach (var method in typeof(Teams).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        }
        foreach (var method in typeof(ValidateLine).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        }
        foreach (var method in typeof(Programm).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        }
        foreach (var method in typeof(MathSTR).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        }

        var dummyLine = new double [] { };

        teams.UpdateLine(dummyLine, 0);

        // Вызываем каждый тип команды один раз с защитой от ошибок
        try { teams.Print(); } catch { }
        try { teams.PrintVar(); } catch { }
        try { teams.Calc(); } catch { }
        try { teams.Int(); } catch { }
        try { teams.Float(); } catch { }

        // Прогрев ExecutionCod на пустых данных
        var savedLines = lines;
        var savedArray = commandArray;

        try { ExecutionCod(); } catch { }

        lines = savedLines;
        commandArray = savedArray;
        teams._dynamicOutput.Clear();

        teams._dynamicOutput.Clear();
    }
    public void ExecutionCod()
    {
        if (!isValidate)
        {
            Console.WriteLine("Код не запущен. В коде присутствуют синтаксические ошибки.");
            return;
        }
        else if (!isTextWrite)
        {
            // Кешируем делегаты
            var updateLine = teams.UpdateLine;
            var localCommands = commands;
            var localCommandArray = commandArray;
            var localLines = lines;

            // Стек для вложенных циклов (сохраняем позицию возврата)
            Stack<int> loopReturnStack = new Stack<int>();
            Stack<int> loopCounterStack = new Stack<int>();

            //Console.WriteLine("Длина массива " + localCommandArray.Length);
            //Console.WriteLine(string.Join(" | ", localCommandArray));

            // Используем TryGetValue вместо двойного поиска
            lineNum = 0;
            while (localLines.TryGetValue(lineNum, out var l))
            {
                if (lineNum > localCommandArray.Length - 1) Console.WriteLine($"Выход за массив! Строка {lineNum}");

                //Console.WriteLine(string.Join(" | ", l));
                //updateLine(l, lineNum);
                //Console.WriteLine("Команда " + localCommandArray[lineNum]);

                //try
                //{
                if (localCommandArray[lineNum] == 8) // loop
                {
                    int num = 0;
                    int numIf = int.Parse(l[1]);

                    //Console.WriteLine(loopMemory[lineNum] + "  " + numIf + "  " + string.Join(" | ", l));
                    //Console.WriteLine(lineNum + " <- Номер строки");

                    for (int i = lineNum; i < loopMemory[lineNum]; i++)
                    {
                        //updateLine(lines[i], i);

                        if (localCommandArray[i] < 8)
                            localCommands[localCommandArray[i]]();

                        if (i + 1 == loopMemory[lineNum] && num < numIf)
                        {
                            i = lineNum;
                            num++;
                        }
                    }

                    //Console.WriteLine("Кол-во повторений: " + num + " Условие: " + numIf);
                }
                else
                {
                    if (localCommandArray[lineNum] != 9)
                        localCommands[localCommandArray[lineNum]]();
                }
                //}
                //catch (Exception ex)
                //{
                //Console.Write("\nОшибка в команде на строке ");
                //Console.Write(lineNum);
                //Console.Write(": ");
                //Console.WriteLine(ex.Message);
                //}

                //Console.WriteLine("Массив переменных " + string.Join(" | ", variables));
                lineNum++;
            }
        }
    }
}