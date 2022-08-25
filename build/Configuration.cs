using System;
using System.ComponentModel;
using System.Linq;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    public static Configuration Release = new Configuration { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}

internal class Solution : Nuke.Common.ProjectModel.Solution
{
    private Nuke.Common.ProjectModel.Solution SolutionFolder => this;
    public _Solution_Items Solution_Items => new(SolutionFolder.GetSolutionFolder("Solution Items"));
    public _src src => new(SolutionFolder.GetSolutionFolder("src"));
    internal class _Solution_Items
    {
        private SolutionFolder SolutionFolder { get; }

        public _Solution_Items(SolutionFolder solutionFolder) => SolutionFolder = solutionFolder;
    }

  
    internal class _src
    {
        private SolutionFolder SolutionFolder { get; }

        public _src(SolutionFolder solutionFolder) => SolutionFolder = solutionFolder;
        public Project Sundry_Option => SolutionFolder.GetProject("Sundry.Option");
    }
}
