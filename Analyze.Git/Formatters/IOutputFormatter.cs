namespace Analyze.Formatters
{
    using System;
    using System.Collections.Generic;
    using ChangeAnalysis.Models;

    public interface IOutputFormatter
    {
        void WriteHeading(string solutionPath, string commitId, DateTime commitDate);
        void WriteChangedAssets(string assetType, IEnumerable<Change> assets);
        void WriteChangedMethods(IEnumerable<ChangedMethod> changes);
        void WriteReferencingProjects(IDictionary<string, IList<string>> projects);
        void AppendSectionBreak();
        void WriteFooter();
    }
}
