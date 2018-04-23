namespace Analyze
{
    using Analyze.Formatters;
    using ChangeAnalysis;
    using ChangeAnalysis.Providers;
    using CommandLine;
    using System;
    using System.IO;

    class Program
    {
        class Options
        {
            [Option('w', Default = ".", HelpText = "The working directory")]
            public string WorkingDirectory { get; set; }
            [Option('s', Required = true, HelpText = "Path to the solution file (if not absolute, then relative to the working directory")]
            public string SolutionPath { get; set; }
            [Option('c', Required = true, HelpText = "git commit against which to compare")]
            public string CommitId { get; set; }
            [Option('m', Required = false, HelpText = "Include unified list of signatures of changed methods")]
            public bool IncludeChangedMethodSignatures { get; set; }
            [Option('o', Required = false, HelpText = "Report output file")]
            public string OutputPath { get; set; }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => 
                {
                    if (!Directory.Exists(opts.WorkingDirectory))
                    {
                        Console.Error.WriteLine($"Directory {opts.WorkingDirectory} is invalid.");
                        return;
                    }

                    string solutionPath = opts.SolutionPath;
                    if (!Path.IsPathRooted(opts.SolutionPath))
                        solutionPath = Path.Combine(opts.WorkingDirectory, opts.SolutionPath);

                    if (!File.Exists(solutionPath))
                    {
                        Console.Error.WriteLine($"File {solutionPath} does not exist.");
                        return;
                    }

                    IOutputFormatter formatter = new ConsoleOutputFormatter();
                    if (!string.IsNullOrEmpty(opts.OutputPath))
                        formatter = new PrettyOutputFormatter(opts.OutputPath);

                    Console.WriteLine("Analyzing changes...");
                    DateTime now = DateTime.Now;
                    var provider = new GitSourceControlProvider("git", opts.WorkingDirectory);
                    var analyzer = new Analyzer<string>(provider, solutionPath, opts.CommitId);
                    analyzer.OpenSolution();

                    formatter.WriteHeading(opts.SolutionPath, opts.CommitId);

                    var changedStyles = analyzer.GetChangedStyles();
                    formatter.WriteChangedAssets("stylesheets", changedStyles);
                    formatter.AppendSectionBreak();

                    var changedScripts = analyzer.GetChangedScripts();
                    formatter.WriteChangedAssets("JavaScripts", changedScripts);
                    formatter.AppendSectionBreak();

                    if (opts.IncludeChangedMethodSignatures)
                    {
                        var changedMethods = analyzer.GetChangedMethodSignatures();
                        formatter.WriteChangedMethods(changedMethods);
                        formatter.AppendSectionBreak();
                    }

                    var referencingProjects = analyzer.GetReferencingProjects();
                    formatter.WriteReferencingProjects(referencingProjects);

                    formatter.WriteFooter();

                    Console.WriteLine($"Elapsed time: {DateTime.Now - now}");
                });
        }
    }
}
