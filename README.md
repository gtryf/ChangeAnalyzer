# Source Code Change Analyzer

This utility will scan the changes between a point in the past and
the current state of a version controlled .NET solution in order to 
highlight important changes. To accompilish this, we use the version
control functionality to fetch the differences between the two points
in time, and have the Microsoft Compiler Platform
([roslyn](https://github.com/dotnet/roslyn)) open the solution,
enumerate its projects, and retrieve methods that have changed.

## Implementation Information

The solution is divided in two major parts: the `ChangeAnalysis`
project, which contains the main analyzer logic (the `Analyzer` class);
and the `Analyze` project under the `Git` folder. The former project
exposes the `ISourceControlProvider` interface which should be
implemented by clients that wish to provide the service for a
particular source control provider:

```csharp
    public interface ISourceControlProvider<T>
    {
        IEnumerable<Change> GetChanges(T since, string extensionFilter);
        DateTime GetTimestamp(T when);
    }
```

The latter project, which is the command line user interface to the
analyzer, has implemented this interface for the Git source
control system.

## Usage

From the command line, you can run the utility as follows:

```
analyze.exe -s <solution file path> -c <commit object id> [-w <working directory>] [-m] [-o <output file>]
```

The options are:

* `-s <solution file path>` (required): the path to your project's `sln` file;
* `-c <commit object id>` (required): the commit identifier to be diff'ed against `HEAD`;
* `-w <working directory>` (optional): the root of your repository. Assumed to be the current directory if mising. Relative paths are OK;
* `-m` (optional): if present, changed methods are listed separately instead of in the context of their referencing project;
* `-o <output file>` (optional): path to an output file to pretty print the output (in html).

Running the command results in a report of:

1. Changed stylesheets (css, sass or less);
2. Changed scripts (js);
3. (if the `-m` flag is specified) Changed methods;
4. Projects that reference the changed methods and therefore need to be rebuilt - the referenced methods are grouped within the projects.

## TODO

Handle the case of methods that have been removed. Currently, removed
methods are ignored in the comparison.