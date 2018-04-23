namespace ChangeAnalysis.Providers
{
    using System.Collections.Generic;
    using Models;

    public interface ISourceControlProvider<T>
    {
        IEnumerable<Change> GetChanges(T since, string extensionFilter);
    }
}
