using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Serilog;

namespace IOTA_Pollen_AutoSendFunds
{
    class CommandLine
    {
        private string standardOutput;
        private string errorOutput;

        public BufferedCommandResult Result { get; set; }

        private readonly string filename;
        private readonly string arguments;
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

            Log.Logger.Debug("COMMAND:");
            Log.Logger.Debug($"{command.TargetFilePath} {command.Arguments}");
            Log.Logger.Debug("OUTPUT:");
            Log.Logger.Debug(standardOutput);

            if (errorOutput.Trim() != "")
            {
                Log.Logger.Debug("ERROR:");
                Log.Logger.Debug(errorOutput);
            }
        }
    }
}
