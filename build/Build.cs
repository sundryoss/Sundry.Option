using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;

using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;

using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Octokit;
using System.Reflection;
using Nuke.Common.ChangeLog;
using Nuke.Common.Tools.GitHub;
using Octokit.Internal;
using ParameterAttribute = Nuke.Common.ParameterAttribute;
using Nuke.Common.Git;
using System.IO;
using System.Threading.Tasks;

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranches = new[] { "main","dev" },
    OnPullRequestBranches = new[] { "release"},
    InvokedTargets = new[] {
        nameof(Pack),
   },
    EnableGitHubToken = true
)]

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion]
    readonly GitVersion GitVersion;

    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects = false)] readonly Solution Solution;
    GitHubActions GitHubActions => GitHubActions.Instance;
    AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";
    string ChangeLogFile => RootDirectory / "CHANGELOG.md";

    string GithubNugetSource => GitHubActions != null
         ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json"
         : null;


    Target Clean => _ => _
      .Before(Restore)
      .Executes(() =>
      {
          DotNetClean(c => c.SetProject(Solution));
          EnsureCleanDirectory(ArtifactsDirectory);

          Type type = GitVersion.GetType();
          PropertyInfo[] props = type.GetProperties();
      });
    Target Restore => _ => _
        .Description("Restoring the solution dependencies")
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(
                r => r.SetProjectFile(Solution.src.Sundry_Option));
        });

    Target Compile => _ => _
        .Description("Building the solution with the version")
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information(Solution);
            Log.Information(Configuration);


            DotNetBuild(b => b
                .SetProjectFile(Solution.src.Sundry_Option)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .EnableNoRestore());
        });

    Target Pack => _ => _
   .Produces(ArtifactsDirectory / "*.nupkg")
   .DependsOn(Compile)
   .Triggers(PublishToGithub, CreateRelease)
   .Executes(() =>
   {
       DotNetPack(p =>
           p
               .SetProject(Solution.src.Sundry_Option)
               .SetConfiguration(Configuration)
               .SetOutputDirectory(ArtifactsDirectory)
               .EnableNoBuild()
               .EnableNoRestore()
               .SetCopyright($"©SunDryOSS {DateTime.Now.Year}")
               .SetVersion(GitVersion.NuGetVersionV2)
               .SetAssemblyVersion(GitVersion.AssemblySemVer)
               .SetInformationalVersion(GitVersion.InformationalVersion)
               .SetFileVersion(GitVersion.AssemblySemFileVer));
   });

    Target PublishToGithub => _ => _
       .Requires(() => Configuration.Equals(Configuration.Release))
       .Executes(() =>
       {
           GlobFiles(ArtifactsDirectory, "*.nupkg")
               .Where(x => !x.EndsWith("symbols.nupkg"))
               .ForEach(x =>
               {
                   DotNetNuGetPush(s => s
                       .SetTargetPath(x)
                       .SetSource(GithubNugetSource)
                       .SetApiKey(GitHubActions.Token)
                       .SetSkipDuplicate(true)
                   );
               });
       });

    Target CreateRelease => _ => _
       .Requires(() => Configuration.Equals(Configuration.Release))
       .Executes(async () =>
       {
           var credentials = new Credentials(GitHubActions.Token);
           GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild)),
               new InMemoryCredentialStore(credentials));


           var releaseTag = GitVersion.NuGetVersionV2;
           var changeLogSectionEntries = ChangelogTasks.ExtractChangelogSectionNotes(ChangeLogFile);
           var latestChangeLog = changeLogSectionEntries
               .Aggregate((c, n) => c + Environment.NewLine + n);

           var newRelease = new NewRelease(releaseTag)
           {
               TargetCommitish = GitVersion.Sha,
               Draft = true,
               Name = $"v{releaseTag}",
               Prerelease = !string.IsNullOrEmpty(GitVersion.PreReleaseTag),
               Body = latestChangeLog
           };

           var createdRelease = await GitHubTasks
                                       .GitHubClient
                                       .Repository
                                       .Release.Create(GitRepository.GetGitHubOwner(),GitRepository.GetGitHubName(), newRelease);

           GlobFiles(ArtifactsDirectory, "*.nupkg")
              .Where(x => !x.EndsWith("symbols.nupkg"))
              .ForEach(async x =>
              {
                  await UploadReleaseAssetToGithub(createdRelease, x);
              });

           await GitHubTasks
                      .GitHubClient
                      .Repository
                      .Release
              .Edit(GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName(), createdRelease.Id, new ReleaseUpdate { Draft = false });
       });


    private static async Task UploadReleaseAssetToGithub(Release release, string asset)
    {
        using var artifactStream = File.OpenRead(asset);
        var fileName = Path.GetFileName(asset);
        var assetUpload = new ReleaseAssetUpload
        {
            FileName = fileName,
            ContentType = "application/octet-stream",
            RawData = artifactStream,
        }; 
        await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, assetUpload);
    }
}
