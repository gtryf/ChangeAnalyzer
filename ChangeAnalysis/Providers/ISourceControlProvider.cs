namespace ChangeAnalysis.Providers
{
    using System;
    using System.Collections.Generic;
    using Models;

    public interface ISourceControlProvider<T>
    {
        IEnumerable<Change> GetChanges(T since, string extensionFilter);
        DateTime GetTimestamp(T when);
    }
}
