using CommandLine;
using CommandLine.Text;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using SqlServer.Rules.Report;
using SqlServer.Rules.SolutionGenerator.Payloads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace SqlServer.Rules.SolutionGenerator
{
    public enum ErrorLevel
    {
        Success,
        OptionsParseFailure,
        SolutionDoesNotExist,
        BuildFailure,
        ProjectLoadFailure,
        GeneralException
    }


    internal class Program
    {
        private static StringComparer _comparer = StringComparer.OrdinalIgnoreCase;

        private static int Main(string[] args)
        {
            try
            {
                //parse command line
                var cmdParseResponse = ParseOptions(args);

                if (cmdParseResponse.Result == CompletionResult.Failure)
                {
                    Notify(cmdParseResponse.Messages);
                    return (int)ErrorLevel.OptionsParseFailure;
                }

                if (!Path.IsPathRooted(cmdParseResponse.Options.SolutionPath))
                {
                    cmdParseResponse.Options.SolutionPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), cmdParseResponse.Options.SolutionPath);
                }

                if (!File.Exists(cmdParseResponse.Options.SolutionPath))
                {
                    Notify($"Solution {cmdParseResponse.Options.SolutionPath} does not exist.");
                    return (int)ErrorLevel.SolutionDoesNotExist;
                }

                //build if we need to
                if (cmdParseResponse.Options.Build)
                {
                    var response = BuildSolutions(cmdParseResponse.Options, new ProjectCollection());
                    if (response.Result == CompletionResult.Failure)
                    {
                        response.Messages.Add("BUILD FAILED");
                        Notify(response.Messages);
                        return (int)ErrorLevel.BuildFailure;
                    }
                }

                //load the projects
                var loadProjectsResponse = LoadProjects(cmdParseResponse.Options);
                if (loadProjectsResponse.Result == CompletionResult.Failure)
                {
                    loadProjectsResponse.Messages.Add("Couldn't load the projects");
                    Notify(loadProjectsResponse.Messages);
                    return (int)ErrorLevel.ProjectLoadFailure;
                }

                #region PROCESS THE RULES

                var suppressTypesRegex = ConfigurationManager.AppSettings["RulesSuppressionRegex"];
                Notify("Creating analysis requests");
                var requests =
                    (from p in loadProjectsResponse.Projects
                     from i in p.Items
                     where _comparer.Equals(i.ItemType, "SqlTarget")
                     select new ReportRequest
                     {
                         InputPath = FixPath(p.DirectoryPath, Path.GetFileName(i.EvaluatedInclude)), //this needs the right configuration to be set when parsing the solution
                                                                                                     //InputPath = i.EvaluatedInclude, //would love to go straight off the path, but it does not point to the right location in some cases.
                         Solution = cmdParseResponse.Options.SolutionPath,
                         Suppress = p => Regex.IsMatch(p.Problem.RuleId, suppressTypesRegex, RegexOptions.IgnoreCase),
                         SuppressIssueTypes = p => Regex.IsMatch(p.RuleId, suppressTypesRegex, RegexOptions.IgnoreCase),
                         OutputDirectory = cmdParseResponse.Options.ReportDirectory,
                         ReportOutputType = cmdParseResponse.Options.ReportOutputType
                     }).ToList();

                var factory = new ReportFactory();

                factory.Notify += Factory_Notify;

                requests.ForEach(r => factory.Create(r));

                return (int)ErrorLevel.Success;
                #endregion PROCESS THE RULES
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
                return (int)ErrorLevel.GeneralException;
            }
            finally
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Done.");
                    Console.ReadKey();
                }
            }
        }

        private static LoadProjectsResponse LoadProjects(CmdLineOptions options)
        {
            var result = new LoadProjectsResponse
            {
                Result = CompletionResult.Success
            };

            try
            {
                var solution = SolutionFile.Parse(options.SolutionPath);
                var pc = new ProjectCollection();
                pc.SetGlobalProperty("Configuration", options.BuildConfiguration);

                Notify("Loading projects");

                var properties = new Dictionary<string, string>
                {
                    { "Configuration", options.BuildConfiguration },
                    { "Platform", options.BuildPlatform }
                };
                //the version is needed from the command line, else msbuild will throw errors
                AddConfigValue(properties, "VisualStudioVersion", ispath: false);
                AddConfigValue(properties, "SQLDBExtensionsRefPath");
                AddConfigValue(properties, "SqlServerRedistPath");
                AddConfigValue(properties, "MSBuildExtensionsPath");

                foreach (var p in solution.ProjectsInOrder.Where(s => s.RelativePath.EndsWith(".sqlproj")))
                {
                    var proj = pc.LoadProject(p.AbsolutePath, properties, options.ToolsVersion);
                    result.Projects.Add(proj);
                }
            }
            catch (Exception e)
            {
                result.Messages.Add(e.Message);
                result.Result = CompletionResult.Failure;
            }

            return result;
        }

        private static CmdArgumentParseResponse ParseOptions(string[] arguments)
        {
            var parser = new Parser((settings) =>
            {
                settings.CaseSensitive = false;
                settings.HelpWriter = Console.Error;
                settings.IgnoreUnknownArguments = true;
            });

            var result = new CmdArgumentParseResponse
            {
                Result = CompletionResult.Failure
            };

            if (arguments == null || parser == null || result.Options == null)
            {
                result.Messages.Add("Invalid parse request");
                return result;
            }

            //TDC: I had to switch to a local variable for the options, as it would not parse properly otherwise. Not entirely sure why.
            var options = new CmdLineOptions();
            if (!parser.ParseArguments(arguments, options))
            {
                result.Messages.Add(HelpText.AutoBuild(result).ToString());
                return result;
            }

            result.Options = options;
            result.Result = CompletionResult.Success;

            return result;
        }

        #region Notifications

        private static void Factory_Notify(string notificationMessage, NotificationType type = NotificationType.Information)
        {
            Notify(notificationMessage, type);
        }

        private static void Notify(string message, NotificationType type = NotificationType.Information)
        {
            Notify(Enumerable.Repeat(message, 1), type);
        }

        private static void Notify(IEnumerable<string> messages, NotificationType type = NotificationType.Information)
        {
            Notify(type, messages.ToArray());
        }

        private static void Notify(NotificationType type = NotificationType.Information, params string[] messages)
        {
            switch (type)
            {
                case NotificationType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case NotificationType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            foreach (var msg in messages)
            {
                Console.WriteLine(msg);
            }
            Console.ResetColor();
        }

        #endregion Notifications

        private static string FixPath(string directory, string dacpac)
        {
            return (from f in Directory.EnumerateFiles(directory, dacpac, SearchOption.AllDirectories)
                    select new FileInfo(f) into fi
                    orderby fi.LastWriteTime descending
                    select fi).FirstOrDefault()?.FullName;
        }

        private static BuildSolutionResponse BuildSolutions(CmdLineOptions options, ProjectCollection projectCollection)
        {
            var result = new BuildSolutionResponse();

            if (options == null || projectCollection == null)
            {
                result.Messages.Add("Invalid request");
                result.Result = CompletionResult.Failure;
            }

            var properties = new Dictionary<string, string>
            {
                { "Configuration", options.BuildConfiguration },
                { "Platform", options.BuildPlatform },
                { "RunSqlCodeAnalysis", "false" } //skip code analysis for this build as we will run it below
            };
            //the VisualStudioVersion is needed from the command line, else msbuild will throw errors
            AddConfigValue(properties, "VisualStudioVersion", ispath: false);
            AddConfigValue(properties, "SQLDBExtensionsRefPath");
            AddConfigValue(properties, "SqlServerRedistPath");
            AddConfigValue(properties, "MSBuildExtensionsPath");


            //SQL71558: The object reference [...].[...] differs only by case from the object definition [...].[...].
            properties.Add("SuppressTSqlWarnings", "71558"); //comma delimited

            var bp = new BuildParameters(projectCollection);
            var loggers = new List<ILogger> { new ConsoleLogger() };

            bp.Loggers = loggers;

            var buildRequest = new BuildRequestData(options.SolutionPath, properties, options.ToolsVersion, new string[] { /* "Clean", */ "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(bp, buildRequest);

            return new BuildSolutionResponse
            {
                Result = buildResult.OverallResult == BuildResultCode.Success ? CompletionResult.Success : CompletionResult.Failure
            };
        }

        private static void AddConfigValue(Dictionary<string, string> properties, string keyName, string defaultValue = null, bool ispath = true)
        {
            var val = ConfigurationManager.AppSettings[keyName];
            if (string.IsNullOrWhiteSpace(val))
            {
                val = defaultValue;
            }
            if (!string.IsNullOrWhiteSpace(val))
            {
                if (ispath)
                {
                    val = MakePathRelative(val);
                }
                properties.Add(keyName, val);
            }
        }

        private static string MakePathRelative(string path)
        {
            path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);

            return path;
        }
    }
}