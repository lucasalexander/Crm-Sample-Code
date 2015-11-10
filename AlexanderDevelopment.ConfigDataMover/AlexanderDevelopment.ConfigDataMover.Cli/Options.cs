using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace AlexanderDevelopment.ConfigDataMover.Cli
{
    class Options
    {
        [Option('c', "configfile", Required = true, HelpText = "XML file containing job data")]
        public string ConfigFile { get; set; }

        [Option('s', "source", Required = false, HelpText = "Simplified connection string to CRM source org - optional if connection details are specified in config file")]
        public string Source { get; set; }

        [Option('t', "target", Required = false, HelpText = "Simplified connection string to CRM target org - optional if connection details are specified in config file")]
        public string Target { get; set; }

        [Option('v', "verbose", HelpText = "Print details during execution")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("AlexanderDevelopment.ConfigDataMover.Cli", "1.0.0.0"),
                Copyright = new CopyrightInfo("Lucas Alexander", 2015),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Apache License 2.0");
            help.AddPreOptionsLine("Usage: AlexanderDevelopment.ConfigDataMover.Cli.exe -c configfile.xml -s \"Url=https://xxxx; Domain=xxxx; Username=xxxx; Password=xxxx;\" -t \"Url=https://xxxx; Domain=xxxx; Username=xxxx; Password=xxxx;\"");
            help.AddOptions(this);
            return help;
        }
    }
}
