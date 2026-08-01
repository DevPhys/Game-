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
    List<TimeSpan> time = new List<TimeSpan>();

    private CancellationTokenSource GcTokenSource = new CancellationTokenSource();

    public Programm (Parser parser, Main main)
    {
        Tokens = main.tokenes;
        teams = main.teams;

        this.parser = parser;
        cache = main.cache;
    }
    private void Start(bool bl)
    {
        parser.tokens = Tokens.Token(nameFile);
        if (bl)
            Console.WriteLine("| " + string.Join(" | ", parser.tokens) + " |\n");

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
    private void RunCod((bool WriteConsole, int numRestartCod) main)
    {
        int I = 1;
        if (main.WriteConsole)
            I = main.numRestartCod;

        for (int i = 0; i < I; i++)
        {
            var sw = Stopwatch.StartNew();

            parser.ExecutionCod();

            sw.Stop();

            time.Add(sw.Elapsed);
            if (main.WriteConsole) 
                Console.WriteLine($"Время выполнения кода. Цикл {i}: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)");
        }
    }
    public void Run(string Name, (bool WriteConsole, int numRestartCod) main)
    {
        nameFile = Name;

        // Начало выполнение программы
        var sw = Stopwatch.StartNew();
        Start(main.WriteConsole);
        sw.Stop();
        if (main.WriteConsole)
            Console.WriteLine($"Время токенизации: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)");

        // Компиляция
        sw.Restart();
        Compiler();
        GCstart();
        sw.Stop();
        if (main.WriteConsole)
            Console.WriteLine($"Время кэширования: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)\n");

        // Выполнение основного кода
        sw.Restart();
        RunCod((main.WriteConsole, main.numRestartCod));
        sw.Stop();
        if (main.WriteConsole)
            Console.WriteLine($"\nСуммарное время выполения кода: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)\n");

        // Вывод
        if (main.WriteConsole)
        {
            sw.Restart();
            Console.WriteLine("Вывод:");
            if (parser.isTextWrite)
                Console.Write(cache[0]);
            else
                teams.End();
            GCstop();
            sw.Stop();
            Console.WriteLine($"\nВремя вывода: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)\n");


            int count = time.Count;
            TimeSpan total = TimeSpan.FromTicks(time.Sum(t => t.Ticks)); // Суммарное время 
            TimeSpan average = TimeSpan.FromTicks((long)time.Average(t => t.Ticks));  // Среднее время
            TimeSpan min = time.Min();  // Минимальное время
            TimeSpan max = time.Max();  // Максимальное время
            var sorted = time.OrderBy(t => t).ToList();  // Медиана (среднее значение по порядку)
            TimeSpan median;

            if (count % 2 == 0)
            {
                // Чётное количество — среднее двух центральных
                var mid1 = sorted[count / 2 - 1];
                var mid2 = sorted[count / 2];
                median = TimeSpan.FromTicks((mid1.Ticks + mid2.Ticks) / 2);
            }
            else
            {
                // Нечётное — центральный элемент
                median = sorted[count / 2];
            }

            TimeSpan range = max - min;  // Размах (макс - мин)

            // Стандартное отклонение
            double meanTicks = time.Average(t => t.Ticks);
            double sumOfSquares = time.Sum(t => Math.Pow(t.Ticks - meanTicks, 2));
            double stdDevTicks = Math.Sqrt(sumOfSquares / count);
            TimeSpan stdDev = TimeSpan.FromTicks((long)stdDevTicks);

            // Дисперсия
            TimeSpan variance = TimeSpan.FromTicks((long)(sumOfSquares / count));

            // ========== ПРОЦЕНТИЛИ ==========

            // 95-й процентиль (95% замеров быстрее этого значения)
            int percentile95Index = (int)Math.Ceiling(count * 0.95) - 1;
            TimeSpan percentile95 = sorted[Math.Min(percentile95Index, count - 1)];

            // 99-й процентиль
            int percentile99Index = (int)Math.Ceiling(count * 0.99) - 1;
            TimeSpan percentile99 = sorted[Math.Min(percentile99Index, count - 1)];

            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("          Статистика времени выполнения");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine($"Количество замеров:        {count:N0}");
            Console.WriteLine($"Суммарное время:           {total.TotalMilliseconds:F2} мс");
            Console.WriteLine($"Среднее время:             {average.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Минимальное время:         {min.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Максимальное время:        {max.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Медиана:                   {median.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Размах (max-min):          {range.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Стандартное отклонение:    {stdDev.TotalMilliseconds:F4} мс");
            Console.WriteLine($"Дисперсия:                 {variance.TotalMilliseconds:F4} мс");
            Console.WriteLine($"95-й процентиль:           {percentile95.TotalMilliseconds:F4} мс");
            Console.WriteLine($"99-й процентиль:           {percentile99.TotalMilliseconds:F4} мс");
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════");
        }
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
        cache = main.cache;
        loopMemory = main.loopMemory;
        cacheNumInt = main.cacheNumInt;
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
        // % 1001 сжимает результат строго в рамки от 0 до 1000
        return Math.Abs((int)hash) % 1001;
    }
    public void CompilerPase1()
    {
        word = "";
        lineNum = 0;

        while (lines.ContainsKey(lineNum))
        {
            word = lines[lineNum][0];
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

                int operandCount = (lines[lineNum].Count - 3) / 2 + 1;

                cacheNumInt[lineNum] = new double[operandCount];
                cacheNumInt[lineNum][0] = mainVariable;

                int index = 1;
                for (int i = 2; i < lines[lineNum].Count - 3; i += 2)
                {
                    double nextNum = 0;

                    if (double.TryParse(lines[lineNum][i + 1].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedNext))
                    {
                        nextNum = parsedNext;
                    }

                    cacheNumInt[lineNum][index] = nextNum;
                    index++;
                }

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
                int depth = 0;
                int endLoopLine = -1;

                for (int i = lineNum + 1; i < lines.Count; i++)
                {
                    if (lines[i][0] == "loop")
                        depth++;  // Вошли во вложенный цикл

                    if (lines[i][0] == "end_loop")
                    {
                        if (depth == 0)
                        {
                            endLoopLine = i;  // Нашли ПАРНЫЙ end_loop
                            break;
                        }
                        depth--;  // Выход из вложенного цикла
                    }
                }

                if (endLoopLine == -1)
                    throw new Exception($"Нет парного end_loop для цикла на строке {lineNum}");

                loopMemory[lineNum] = endLoopLine;
            }

            // Эта строка выполняется для всех команд КРОМЕ write (у write свой continue)
            cache.Add("Заглушка");
            lineNum++;
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

        var dummyLine = new List<string> { "", "", "", ";" };

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
                updateLine(l, lineNum);
                //Console.WriteLine("Команда " + localCommandArray[lineNum]);

                //try
                //{
                if (localCommandArray[lineNum] == 8) // loop
                {
                    int loopCount = int.Parse(l[1]);
                    int loopStart = lineNum;
                    int loopEnd = loopMemory[lineNum];

                    // Сохраняем в стек
                    loopReturnStack.Push(loopStart);
                    loopCounterStack.Push(loopCount);

                    // Выполняем тело цикла
                    for (int i = 0; i < loopCount; i++)
                    {
                        for (int j = loopStart + 1; j < loopEnd; j++)
                        {
                            if (localLines.TryGetValue(j, out var loopLine))
                            {
                                updateLine(loopLine, j);
                                int cmdIdx = localCommandArray[j];

                                if (cmdIdx == 8) // Вложенный loop
                                {
                                    // Вложенный цикл обработается рекурсивно через тот же механизм
                                    // Но нам нужно выполнить его целиком
                                    int nestedLoopEnd = loopMemory[j];
                                    int nestedLoopCount = int.Parse(localLines[j][1]);

                                    for (int ni = 0; ni < nestedLoopCount; ni++)
                                    {
                                        for (int nj = j + 1; nj < nestedLoopEnd; nj++)
                                        {
                                            if (localLines.TryGetValue(nj, out var nestedLine))
                                            {
                                                updateLine(nestedLine, nj);
                                                int nestedCmdIdx = localCommandArray[nj];
                                                if (nestedCmdIdx != 9) // не end_loop
                                                    localCommands[nestedCmdIdx]();
                                            }
                                        }
                                    }

                                    // Пропускаем уже выполненный вложенный цикл
                                    j = nestedLoopEnd;
                                    continue;
                                }
                                else if (cmdIdx != 9) // не end_loop
                                {
                                    localCommands[cmdIdx]();
                                }
                            }
                        }
                    }

                    // Восстанавливаем стек
                    loopReturnStack.Pop();
                    loopCounterStack.Pop();

                    lineNum = loopEnd + 1;
                    continue;
                }
                else if (localCommandArray[lineNum] == 9) // end_loop
                {
                    // Если есть внешний цикл - возвращаемся
                    if (loopReturnStack.Count > 0)
                    {
                        lineNum = loopReturnStack.Peek();
                        continue;
                    }
                    else
                    {
                        throw new Exception($"Лишний end_loop на строке {lineNum}");
                    }
                }
                else
                {
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