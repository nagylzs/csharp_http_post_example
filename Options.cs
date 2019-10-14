using System.Collections.Generic;
using CommandLine;

namespace test
{

    public class Options
    {
        [CommandLine.Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [CommandLine.Value(0)]
        public string URL { get; set; }

        [CommandLine.Value(1)]
        public IEnumerable<string> InputFiles { get; set; }
    }

}
