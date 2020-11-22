using CommandLine;
using SqlServer.Rules.Report;
using System;

namespace SqlServer.Rules.SolutionGenerator
{
    class CmdLineOptions
    {

        [Option('s', "solution", Required = true, HelpText = "The solution to run the sql checks against.")]
        public String SolutionPath { get; set; }

        [Option('b', "build", Required = false, DefaultValue = false, HelpText = "Whether to build the solution before running the sql checks.")]
        public bool Build { get; set; }

        [Option('c', "config", Required = false, HelpText = "The build configuration to use when building the solution. Defaults to Release.")]
        public string BuildConfiguration { get; set; } = "Release";

        [Option('p', "platform", Required = false, HelpText = "The build configuration plaform to use when building the solution. Defaults to 'Any CPU'.")]
        public string BuildPlatform { get; set; } = "Any CPU";


        [Option('t', "toolsVersion", Required = false, HelpText = "The tools version to use when building the solution. Defaults to 4.0.")]
        public string ToolsVersion { get; set; } = "4.0";

        [Option('d', "reportDirectory", Required = false, HelpText = "The directory to create the generated reports in.")]
        public string ReportDirectory { get; set; }

        [Option('o', "reportOutputType", Required = false, HelpText = "The type of report to generate, either XML, or CSV.")]
        public ReportOutputType ReportOutputType { get; set; } = ReportOutputType.XML;
        //[HelpOption]
        //public string GetUsage()
        //{
        //    // this without using CommandLine.Text
        //    //  or using HelpText.AutoBuild
        //    var usage = new StringBuilder();
        //    usage.AppendLine("Quickstart Application 1.0");
        //    usage.AppendLine("Read user manual for usage instructions...");
        //    return usage.ToString();
        //}

    }

}
