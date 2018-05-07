namespace ChangeAnalysis
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using Providers;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.FindSymbols;

    public class Analyzer<T>
    {
        public ISourceControlProvider<T> Provider { get; }
        public string SolutionPath { get; }
        public T Since { get; }
        protected Solution Solution { get; set; }
        private IDictionary<DocumentId, SemanticModel> Models { get; } = new Dictionary<DocumentId, SemanticModel>();

        private IList<IMethodSymbol> _changedMethods = null;

        public Analyzer(ISourceControlProvider<T> provider, string solutionPath, T since)
        {
            this.Provider = provider;
            this.SolutionPath = solutionPath;
            this.Since = since;
        }

        public void OpenSolution()
        {
            var currentWorkspace = MSBuildWorkspace.Create();
            this.Solution = currentWorkspace.OpenSolutionAsync(SolutionPath).Result;
        }

        private SemanticModel GetSemanticModel(Document document)
        {
            SemanticModel result;
            if (!Models.ContainsKey(document.Id))
            {
                result = document.GetSemanticModelAsync().Result;
                Models[document.Id] = result;
            }
            else
            {
                result = Models[document.Id];
            }
            return result;
        }

        public IEnumerable<Change> GetChangedStyles() => this.Provider.GetChanges(this.Since, "*.scss *.less *.css").Distinct();

        public IEnumerable<Change> GetChangedScripts() => this.Provider.GetChanges(this.Since, "*.js").Distinct();

        public IEnumerable<ChangedMethod> GetChangedMethodSignatures()
        {
            foreach (var method in GetChangedMethods())
                yield return new ChangedMethod(method.ToDisplayString(), method.Locations.Select(i => i.GetLineSpan().Path));
        }

        protected IEnumerable<IMethodSymbol> GetChangedMethods()
        {
            if (_changedMethods != null) return _changedMethods;

            _changedMethods = new List<IMethodSymbol>();
            var changes = this.Provider.GetChanges(this.Since, "*.cs");
            foreach (var change in changes)
            {
                switch (change.ChangeType)
                {
                    case ChangeType.Addition:
                        {
                            var document = this.Solution.Projects.SelectMany(i => i.Documents.Where(doc => doc.FilePath == change.FileName)).FirstOrDefault();
                            if (document != null)
                            {
                                var model = GetSemanticModel(document);
                                var methods = document.GetSyntaxRootAsync().Result
                                    .DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>();
                                foreach (var method in methods)
                                {
                                    var symbol = model.GetDeclaredSymbol(method);
                                    if (!_changedMethods.Contains(symbol))
                                        _changedMethods.Add(symbol);
                                }
                            }
                        }
                        break;
                    case ChangeType.Modification:
                        {
                            var document = this.Solution.Projects.ToList().SelectMany(i => i.Documents).ToList().FirstOrDefault(doc => doc.FilePath == change.FileName);
                            if (document != null )
                            {
                                var model = GetSemanticModel(document);
                                var tree = CSharpSyntaxTree.ParseText(change.OldContent);
                                var currMethods = document.GetSyntaxRootAsync().Result
                                    .DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>();
                                var prevMethods = tree.GetRoot()
                                    .DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>();

                                foreach (var currMethod in currMethods)
                                {
                                    var prevMethod = prevMethods.FirstOrDefault(i => i.IsEquivalentTo(currMethod, false));
                                    if (prevMethod == null)
                                    {
                                        var symbol = model.GetDeclaredSymbol(currMethod);
                                        if (!_changedMethods.Contains(symbol))
                                            _changedMethods.Add(symbol);
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return _changedMethods;
        }

        public IDictionary<string, IList<string>> GetReferencingProjects()
        {
            var result = new Dictionary<string, IList<string>>();
            foreach (var method in GetChangedMethods())
            {
                var references = SymbolFinder.FindReferencesAsync(method, this.Solution).Result;
                foreach (var reference in references)
                {
                    var projects = reference.Locations.Select(i => i.Document.Project.Name);
                    foreach (var project in projects)
                    {
                        if (!result.ContainsKey(project))
                        {
                            result.Add(project, new List<string> { method.ToDisplayString() });
                        }
                        else
                        {
                            if (!result[project].Contains(method.ToDisplayString()))
                                result[project].Add(method.ToDisplayString());
                        }
                    }
                }
            }
            return result;
        }
    }
}
