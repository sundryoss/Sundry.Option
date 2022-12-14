using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Git;
using Nuke.Common.ChangeLog;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.NerdbankGitVersioning;

using Octokit;
using Octokit.Internal;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

using ParameterAttribute = Nuke.Common.ParameterAttribute;
using Serilog;

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranches = new[] { "main", "dev" },
    OnPullRequestBranches = new[] { "releases/**" },
    InvokedTargets = new[] {
        nameof(Pack),
   },
    EnableGitHubToken = true,
    ImportSecrets = new[] { nameof(MyGetApiKey), nameof(NuGetApiKey) }
)]

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    static readonly string PackageContentType = "application/octet-stream";

    [Parameter("MyGet Feed Url for Public Access of Pre Releases")]
    readonly string MyGetNugetFeed;
    [Parameter("MyGet Api Key"), Secret]
    readonly string MyGetApiKey;

    [Parameter("Nuget Feed Url for Public Access of Pre Releases")]
    readonly string NugetFeed;
    [Parameter("Nuget Api Key"),Secret]
    readonly string NuGetApiKey;

    [Parameter("Copyright Detils")]
    readonly string Copyright;

    [Parameter("Artifacts Type")]
    readonly string ArtifactsType;

    [Parameter("Excluded Artifacts Type")]
    readonly string ExcludedArtifactsType;

    [GitVersion]
    readonly GitVersion GitVersion;

    [GitRepository]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects =false)]
    readonly Solution Solution;

    static GitHubActions GitHubActions => GitHubActions.Instance;
    static AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";
    static string ChangeLogFile => RootDirectory / "CHANGELOG.md";

    string GithubNugetFeed => GitHubActions != null
         ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json"
         : null;

    Target Clean => _ => _
      .Description($"Cleaning Project.")
      .Before(Restore)
      .Executes(() =>
      {
          DotNetClean(c => c.SetProject(Solution.src.Sundry_Option));
          EnsureCleanDirectory(ArtifactsDirectory);
          Log.Information(GitRepository.Branch);
          Log.Information(GitHubActions.IsPullRequest.ToString());
      });
    Target Restore => _ => _
        .Description($"Restoring Project Dependencies.")
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(
                r => r.SetProjectFile(Solution.src.Sundry_Option));
        });

    Target Compile => _ => _
        .Description($"Building Project with the version.")
        .DependsOn(Restore)
        .Executes(() =>
        {
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
    .Description($"Packing Project with the version.")
   .Produces(ArtifactsDirectory / ArtifactsType)
   .DependsOn(Compile)
   .Triggers(PublishToGithub,PublishToMyGet, PublishToNuGet, CreateRelease)
   .Executes(() =>
   {
       DotNetPack(p =>
           p
               .SetProject(Solution.src.Sundry_Option)
               .SetConfiguration(Configuration)
               .SetOutputDirectory(ArtifactsDirectory)
               .EnableNoBuild()
               .EnableNoRestore()
               .SetCopyright(Copyright)
               .SetVersion(GitVersion.NuGetVersionV2)
               .SetAssemblyVersion(GitVersion.AssemblySemVer)
               .SetInformationalVersion(GitVersion.InformationalVersion)
               .SetFileVersion(GitVersion.AssemblySemFileVer));
   });

    Target PublishToGithub => _ => _
       .Description($"Publishing to Github for Development only.")
       .Requires(() => Configuration.Equals(Configuration.Release))
       .OnlyWhenStatic(() => GitRepository.IsOnDevelopBranch() || GitHubActions.IsPullRequest)
       .Executes(() =>
       {
           GlobFiles(ArtifactsDirectory, ArtifactsType)
               .Where(x => !x.EndsWith(ExcludedArtifactsType))
               .ForEach(x =>
               {
                   DotNetNuGetPush(s => s
                       .SetTargetPath(x)
                       .SetSource(GithubNugetFeed)
                       .SetApiKey(GitHubActions.Token)
                       .SetSkipDuplicate(true)
                   );
               });
       });

    Target PublishToMyGet => _ => _
       .Description($"Publishing to MyGet for PreRelese only.")
       .Requires(() => Configuration.Equals(Configuration.Release))
       .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch())
       .Executes(() =>
       {
           GlobFiles(ArtifactsDirectory, ArtifactsType)
               .Where(x => !x.EndsWith(ExcludedArtifactsType))
               .ForEach(x =>
               {
                   DotNetNuGetPush(s => s
                       .SetTargetPath(x)
                       .SetSource(MyGetNugetFeed)
                       .SetApiKey(MyGetApiKey)
                       .SetSkipDuplicate(true)
                   );
               });
       });
    Target PublishToNuGet => _ => _
       .Description($"Publishing to NuGet with the version.")
       .Requires(() => Configuration.Equals(Configuration.Release))
       .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
       .Executes(() =>
       {
           GlobFiles(ArtifactsDirectory, ArtifactsType)
               .Where(x => !x.EndsWith(ExcludedArtifactsType))
               .ForEach(x =>
               {
                   DotNetNuGetPush(s => s
                       .SetTargetPath(x)
                       .SetSource(NugetFeed)
                       .SetApiKey(NuGetApiKey)
                       .SetSkipDuplicate(true)
                   );
               });
       });

    Target CreateRelease => _ => _
       .Description($"Creating release for the publishable version.")
       .Requires(() => Configuration.Equals(Configuration.Release))
       .OnlyWhenStatic(()=> GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
       .Executes(async () =>
       {
           var credentials = new Credentials(GitHubActions.Token);
           GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild)),
               new InMemoryCredentialStore(credentials));

           var (owner, name) = (GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName());

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
                                       .Release.Create(owner, name, newRelease);

           GlobFiles(ArtifactsDirectory, ArtifactsType)
              .Where(x => !x.EndsWith(ExcludedArtifactsType))
              .ForEach(async x =>
              {
                  await UploadReleaseAssetToGithub(createdRelease, x);
              });

           await GitHubTasks
                      .GitHubClient
                      .Repository
                      .Release
              .Edit(owner, name, createdRelease.Id, new ReleaseUpdate { Draft = false });
       });


    private static async Task UploadReleaseAssetToGithub(Release release, string asset)
    {
        using var artifactStream = File.OpenRead(asset);
        var fileName = Path.GetFileName(asset);
        var assetUpload = new ReleaseAssetUpload
        {
            FileName = fileName,
            ContentType = PackageContentType,
            RawData = artifactStream,
        };
        await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, assetUpload);
    }
}
