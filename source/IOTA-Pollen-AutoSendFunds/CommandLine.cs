using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace IOTA_Pollen_AutoSendFunds
{
    class CommandLine
    {
        private string standardOutput;
        private string errorOutput;

        public BufferedCommandResult Result { get; set; }

        private string filename;
        private string arguments;
        private Command command;

        public CommandLine(string filename, string arguments)
        {
            standardOutput = "";
            this.filename = filename;
            this.arguments = arguments;

            Init();
        }

        private void Init()
        {
            command = Cli.Wrap(filename)
                .WithArguments(arguments)
                .WithValidation(CommandResultValidation.None)
                .WithWorkingDirectory(Path.GetDirectoryName(filename)); //run by default in same path as filename (important so wallet can be found)
        }

        public async Task Run()
        {
            Result = await command.ExecuteBufferedAsync();

            standardOutput = Result.StandardOutput;
            errorOutput = Result.StandardError;

            if (Program.settings.ShowOutputCliWallet)
            {
                Console.WriteLine("COMMAND:");
                Console.WriteLine($"{command.TargetFilePath} {command.Arguments}");
                Console.WriteLine("OUTPUT:");
                Console.WriteLine(standardOutput);
                Console.WriteLine("ERROR:");
                Console.WriteLine(errorOutput);
            }
        }
    }
}
