using System.Diagnostics;
using System.Runtime;

Main main = new Main();
MathSTR math = new MathSTR();

GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

Tokenes tokenes = new Tokenes();
tokenes.Token(@"data.txt");

ByteCode ByteCode = new ByteCode(tokenes.Tokenizer());
double[][] byteCode = ByteCode.CreateByteCod(main.stringsArray);

// Находим максимальную длину числа во всём массиве
int maxWidth = 0;
for (int i = 0; i < byteCode.Length; i++)
{
    if (byteCode[i] != null)
    {
        foreach (var num in byteCode[i])
        {
            int width = num.ToString().Length;
            if (width > maxWidth)
                maxWidth = width;
        }
    }
}

// Выводим с выравниванием по правому краю
for (int i = 0; i < byteCode.Length; i++)
{
    if (byteCode[i] != null)
    {
        var formatted = byteCode[i].Select(x => x.ToString().PadLeft(maxWidth));
        Console.WriteLine("| " + string.Join(" | ", formatted) + " |");
    }
    else
    {
        Console.WriteLine("null!");
    }
}

// Выводим линейно через запятую
List<string> allNumbers = new List<string>();

for (int i = 0; i < byteCode.Length; i++)
{
    if (byteCode[i] != null)
    {
        foreach (var num in byteCode[i])
        {
            allNumbers.Add(num.ToString());
        }
    }
}

Console.WriteLine(string.Join(", ", allNumbers));

Console.WriteLine();

List<TimeSpan> time = new List<TimeSpan>();
List<TimeSpan> timeCode = new List<TimeSpan>();

for (int i = 0; i < 200; i++)
{
    Main main2 = new Main();

    Tokenes tokenes2 = new Tokenes();
    tokenes2.Token(@"JIT.txt");

    ByteCode ByteCode2 = new ByteCode(tokenes2.Tokenizer());
    double[][] byteCode2 = ByteCode.CreateByteCod(main2.stringsArray);

    P prog = new P(byteCode2, main2);
    prog.Cache();
    prog.RunCode();

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

for (int i = 0; i < 100; i++)
{
    var sw = Stopwatch.StartNew();

    Main main2 = new Main();

    Tokenes tokenes2 = new Tokenes();
    tokenes2.Token(@"data.txt");

    ByteCode ByteCode2 = new ByteCode(tokenes2.Tokenizer());
    double[][] byteCode2 = ByteCode.CreateByteCod(main2.stringsArray);

    P prog = new P(byteCode2, main2);
    prog.Cache();

    var swCode = Stopwatch.StartNew();
    prog.RunCode();
    swCode.Stop();

    main2.teams.End();
    sw.Stop();

    time.Add(sw.Elapsed);
    timeCode.Add(swCode.Elapsed);

    Console.WriteLine($"Цикл {i}. Время выполнения: {sw.Elapsed.TotalNanoseconds:F4} нс ({sw.Elapsed.TotalMilliseconds:F4} мс)");

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}
math.StaticTime(time, name: "Статистика выполнения всей программы");
math.StaticTime(timeCode, name: "Статистика выполнения кода");

public class P
{
    private CancellationTokenSource GcTokenSource = new CancellationTokenSource();

    Teams teams;
    Main main;

    double[][] byteCode;
    int[] loopMemory;
    public P (double[][] ByteCode, Main mains)
    {
        main = mains;
        teams = mains.teams;

        loopMemory = mains.loopMemory;
        byteCode = ByteCode;
    }
    public void Cache()
    {
        for (int lNum = 0; lNum < byteCode.Length - 1; lNum++)
        {
            //Console.WriteLine(byteCode[lNum] + "   " + lNum);

            if (byteCode[lNum][0] == 9)
            {
                for (int i = lNum; i < byteCode.Length; i++)
                {

                    if (byteCode[i][0] == 10)
                    {
                        loopMemory[lNum] = i;
                        break;
                    }
                }
            }
        }
    }
    public void RunCode()
    {
        //GCstart();

        int lineNum = 0;
        Action[] teamsArray = main.teamsArray;

        double[][] localByteCode = byteCode;
        while (lineNum < localByteCode.Length)
        {
            double[] line = localByteCode[lineNum];
            teams.UpdateLine(line, lineNum);

            if (line[0] == 9) // loop
            {
                int numIf = (int)line[1];
                int loopEnd = loopMemory[lineNum];  // ← сохраните в переменную

                //Console.WriteLine($"lineNum={lineNum}, loopMemory[lineNum]={loopEnd}");
                //Console.WriteLine(numIf);

                for (int num = 0; num < numIf; num++)
                {
                    //Console.WriteLine($"Внешний цикл, итерация {num}");  // <- пишится

                    for (int i = lineNum; i < loopMemory[lineNum]; i++)
                    {
                        double[] l = localByteCode[i];
                        teams.UpdateLine(l, i);

                        //Console.WriteLine($"Внутренний цикл, i={i}");  // <- а эта уже НЕ пишется

                        if (l[0] != 9 && l[0] != 10)
                            teamsArray[(int)l[0]]();
                    }
                }
            }
            else
            {
                if (line[0] != 9 && line[0] != 10 && line[0] != 404)
                    teamsArray[(int)line[0]]();
            }

            lineNum++;
        }
        //GCstop();
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
                    await Task.Delay(50, GcTokenSource.Token);

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
}