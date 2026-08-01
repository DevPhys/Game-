Main main = new Main();

Parser parser = new Parser(main);
Programm program = new Programm(parser, main);
program.Run(@"JIT.txt", (false, 2000));


// Очистка после первого запуска
main = null;
parser = null;
program = null;

// Принудительная сборка мусора
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();


Main main2 = new Main();

Parser parser2 = new Parser(main2);
Programm program2 = new Programm(parser2, main2);
program2.Run(@"data.txt", (true, 1000));