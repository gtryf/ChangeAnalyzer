namespace ChangeAnalysis.Providers
{
    using System;
    using System.Collections.Generic;
    using Models;
    using System.Text;
    using System.IO;
    using LibGit2Sharp;

    public class GitSourceControlProvider : ISourceControlProvider<string>
    {
        public string WorkingDirectory { get; }

        public GitSourceControlProvider(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
        }

        private string GetFilePath(string filename) => Path.Combine(this.WorkingDirectory, filename.Replace('/', Path.DirectorySeparatorChar));

        public IEnumerable<Change> GetChanges(string since, string extensionFilter)
        {
            using (var repo = new Repository(this.WorkingDirectory)) 
            {
                var tree = repo.Head.Tip.Tree;
                var ancestor = repo.Lookup<Commit>(since).Tree;

                using (var changes = repo.Diff.Compare<TreeChanges>(ancestor, tree, extensionFilter.Split()))
                {
                    foreach (var change in changes) {
                        switch (change.Status) 
                        {
                            case ChangeKind.Added:
                                {
                                    var entry = tree[change.Path];
                                    var blob = (Blob)entry.Target;

                                    var contentStream = blob.GetContentStream();
                                    using (var tr = new StreamReader(contentStream, Encoding.UTF8))
                                    {
                                        string content = tr.ReadToEnd();
                                        yield return new Change(GetFilePath(change.Path), "", content, ChangeType.Addition);
                                    }
                                }
                                break;
                            case ChangeKind.Modified:
                                {
                                    var oldEntry = ancestor[change.OldPath];
                                    var newEntry = tree[change.Path];
                                    var oldBlob = (Blob)oldEntry.Target;
                                    var newBlob = (Blob)newEntry.Target;

                                    var oldContentStream = oldBlob.GetContentStream();
                                    var newContentStream = newBlob.GetContentStream();
                                    using (var oldTr = new StreamReader(oldContentStream, Encoding.UTF8))
                                    using (var newTr = new StreamReader(newContentStream, Encoding.UTF8))
                                    {
                                        string oldContent = oldTr.ReadToEnd();
                                        string newContent = newTr.ReadToEnd();
                                        yield return new Change(GetFilePath(change.Path), oldContent, newContent, ChangeType.Modification);
                                    }
                                }
                                break;
                            case ChangeKind.Deleted:
                                {
                                    var entry = ancestor[change.OldPath];
                                    var blob = (Blob)entry.Target;

                                    var contentStream = blob.GetContentStream();
                                    using (var tr = new StreamReader(contentStream, Encoding.UTF8))
                                    {
                                        string content = tr.ReadToEnd();
                                        yield return new Change(GetFilePath(change.OldPath), "", content, ChangeType.Deletion);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        public DateTime GetTimestamp(string when)
        {
            using (var repo = new Repository(this.WorkingDirectory)) 
            {
                var commit = repo.Lookup<Commit>(when);
                return commit.Committer.When.LocalDateTime;
            }
        }
    }
}
