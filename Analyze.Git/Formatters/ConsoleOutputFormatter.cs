namespace Analyze.Formatters
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using ChangeAnalysis.Models;

    public class ConsoleOutputFormatter : IOutputFormatter
    {
        public void WriteHeading(string solutionPath, string commmitId, DateTime commitDate)
        {
            
        }

        public void WriteChangedAssets(string assetType, IEnumerable<Change> assets)
        {
            if (assets.Any())
            {
                var title = $"Changed {assetType}:";
                Console.WriteLine(title);
                Console.WriteLine(new string('-', title.Length));
                Console.WriteLine();
                foreach (var asset in assets)
                {
                    Console.WriteLine(asset);
                }
            }
        }

        public void WriteChangedMethods(IEnumerable<ChangedMethod> changes)
        {
            var title = "Methods that have changed:";
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
            Console.WriteLine();
            foreach (var method in changes)
            {
                Console.WriteLine(method.Symbol);
                foreach (var location in method.Locations)
                    Console.WriteLine($"|--{location}");
            }
        }

        public void WriteReferencingProjects(IDictionary<string, IList<string>> projects)
        {
            var title = $"Projects that need to be recompiled:";
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
            Console.WriteLine();
            foreach (var project in projects)
            {
                Console.WriteLine(project.Key);
                foreach (var method in project.Value)
                {
                    Console.WriteLine($"|-- References method: {method}");
                }
            }
        }

        public void AppendSectionBreak()
        {
            Console.WriteLine();
            Console.WriteLine();
        }

        public void WriteFooter()
        {
            
        }
    }
}
