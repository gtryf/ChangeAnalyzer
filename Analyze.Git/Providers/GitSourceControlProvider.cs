namespace ChangeAnalysis.Providers
{
    using System;
    using System.Collections.Generic;
    using Models;
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using ParseDiff;

    public class GitSourceControlProvider : ISourceControlProvider<string>
    {
        public string GitExecutable { get; }
        public string WorkingDirectory { get; }

        public GitSourceControlProvider(string executable, string workingDirectory)
        {
            GitExecutable = executable;
            WorkingDirectory = workingDirectory;
        }

        private string GetFilePath(string filename) => Path.Combine(this.WorkingDirectory, filename.Replace('/', Path.DirectorySeparatorChar));

        public IEnumerable<Change> GetChanges(string since, string extensionFilter)
        {
            var output = InvokeGit($"diff {since} -- {extensionFilter}");
            if (output.Item1 != 0)
                yield break;

            var files = Diff.Parse(output.Item2, Environment.NewLine);
            foreach (var file in files)
            {
                if (file.Add)
                {
                    var content = InvokeGit($"show HEAD:{file.To}").Item2;
                    yield return new Change(GetFilePath(file.To), "", content, ChangeType.Addition);
                }
                else if (file.Deleted)
                {
                    var content = InvokeGit($"show {since}:{file.From}").Item2;
                    yield return new Change(GetFilePath(file.From), "", content, ChangeType.Deletion);
                }
                else
                {
                    var oldContent = InvokeGit($"show {since}:{file.From}").Item2;
                    var newContent = InvokeGit($"show HEAD:{file.To}").Item2;
                    yield return new Change(GetFilePath(file.To), oldContent, newContent, ChangeType.Modification);
                }
            }
        }

        public DateTime GetTimestamp(string when)
        {
            DateTime result = DateTime.MinValue;
            var output = InvokeGit($"show --pretty=format:%aI -s {when}");
            if (output.Item1 == 0)
                if (!DateTime.TryParse(output.Item2, out result))
                    result = DateTime.MinValue;

            return result;
        }

        private Tuple<int, string> InvokeGit(string args)
        {
            var sb = new StringBuilder();

            var psi = new ProcessStartInfo(GitExecutable, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = this.WorkingDirectory,
            };

            using (var process = new Process())
            {
                process.StartInfo = psi;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => sb.AppendLine(e.Data);
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => sb.AppendLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return Tuple.Create(process.ExitCode, sb.ToString());
            }
        }
    }
}
