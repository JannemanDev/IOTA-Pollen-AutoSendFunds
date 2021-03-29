using System;
using System.IO;
using SharedLib;

namespace LogViewer
{
    class Program
    {
        public static Settings settings;

        static void Main(string[] args)
        {
            Console.WriteLine("Dashboard - IOTA-Pollen-AutoSendFunds v0.1\n");
            Console.WriteLine(" Escape to quit");
            Console.WriteLine(" Space to pause\n");

            //string path = Directory.GetCurrentDirectory();
            string path = "C:\\MyData\\Persoonlijk\\IOTA\\IOTA-Pollen-AutoSendFunds\\source\\IOTA-Pollen-AutoSendFunds\\bin\\Debug\\net5.0";
            settings = MiscUtil.LoadSettings(path);

            //show sorted on transactionID/starttime or endtime
            while (true)
            {
                bool pause = false;
                while (Console.KeyAvailable || (pause))
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Escape) return;
                    if (cki.Key == ConsoleKey.Spacebar)
                    {
                        if (pause) pause = false; //continue
                        else
                        {
                            Console.WriteLine("Pausing... Press <space> to continue, B = Balance, <escape> to quit!");
                            pause = true;
                        }
                    }

                    if (cki.Key == ConsoleKey.S) //sort on starttime
                    {
                        Console.WriteLine("S");
                    }

                    if (cki.Key == ConsoleKey.E) //sort on starttime
                    {
                    }
                }
            }
        }
    }
}
