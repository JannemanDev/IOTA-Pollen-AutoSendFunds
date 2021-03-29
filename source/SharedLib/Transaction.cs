using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public Command Command { get; set; }
    }

    public class Command
    {
        public string Filename { get; set; }
        public string Arguments { get; set; }
        public string StandardOutput { get; set; }
        public string StandardError { get; set; }
    }
}
