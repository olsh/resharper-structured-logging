using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using NuGet.Versioning;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.NUnit;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.Tools.NUnit.NUnitTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    protected override void OnBuildInitialized()
    {
        SdkVersion = XDocument
            .Load((RootDirectory / "Directory.Build.props").ToString())
            .Descendants()
            .Single(x => x.Name.LocalName == "SdkVersion")
            .Value;
        SdkVersion.NotNull("Unable to detect SDK version");

        var versionMatch = Regex.Match(SdkVersion, @"(?<version>[\d\.]+)(?<suffix>-.*)?");

        SdkVersionWithoutSuffix = versionMatch.Groups["version"]
            .ToString();
        SdkVersionSuffix = versionMatch.Groups["suffix"]
            .ToString();

        ExtensionVersion = GitHubActions == null
            ? SdkVersion
            : $"{versionMatch.Groups["version"]}.{GitHubActions.RunNumber}{versionMatch.Groups["suffix"]}";
        var sdkMatch = Regex.Match(SdkVersion, @"\d{2}(\d{2}).(\d).*");
        WaveMajorVersion = int.Parse(sdkMatch.Groups[1]
            .Value + sdkMatch.Groups[2]
            .Value);
        WaveVersionsRange = $"{WaveMajorVersion}.0";

        base.OnBuildInitialized();
    }

    [CI] readonly GitHubActions GitHubActions;

    [Parameter] readonly string Configuration = "Release";

    [Parameter] readonly bool IsRiderHost;

    [Parameter] [Secret] readonly string MarketplaceToken;

    [Parameter] readonly AbsolutePath RunIdeSolution;

    [Parameter("Adopt this SDK version instead of the one the wave policy picks")]
    readonly string SdkVersionOverride;

    [LocalPath("./gradlew.bat")] readonly Tool Gradle;

    [NuGetPackage(
        packageId: "dotnet-cleanup",
        packageExecutable: "DotnetCleanup.dll")]
    readonly Tool DotNetCleanup;

    // Every project pins its SDK package to $(SdkVersion), and JetBrains does not always push the
    // four of them at the same minute, so a version counts as available only once all of them have it
    static readonly string[] SdkPackageIds =
    {
        "jetbrains.resharper.sdk",
        "jetbrains.rider.sdk",
        "jetbrains.resharper.sdk.tests",
        "jetbrains.rider.sdk.tests",
    };

    string RiderPackagePath => RootDirectory / $"rider-structured-logging-{ExtensionVersion}.zip";

    string ReSharperPackagePath => RootDirectory / $"{ProjectName}.{ExtensionVersion}.nupkg";

    // JetBrains is not very consistent in versioning
    // https://github.com/olsh/resharper-structured-logging/issues/35#issuecomment-892764206
    string RiderProductVersion
    {
        get
        {
            // A zero patch is dropped, so 2025.1.0 is 2025.1 there, but 2026.2.1 stays as it is
            var productVersion = SdkVersionWithoutSuffix.EndsWith(".0", StringComparison.Ordinal)
                ? SdkVersionWithoutSuffix.Substring(0, SdkVersionWithoutSuffix.Length - ".0".Length)
                : SdkVersionWithoutSuffix;

            if (!string.IsNullOrEmpty(SdkVersionSuffix))
            {
                // -eap01 is -EAP1 there. The leading zeros go one number at a time, so that -eap10
                // does not collapse onto -EAP1
                var suffix = Regex.Replace(SdkVersionSuffix, @"\d+", x => int.Parse(x.Value).ToString());
                productVersion += $"{suffix.ToUpperInvariant()}-SNAPSHOT";
            }

            return productVersion;
        }
    }

    // EAP builds must not reach the stable channel of the Marketplace
    string PluginChannel => string.IsNullOrEmpty(SdkVersionSuffix) ? "default" : "eap";

    string ProjectName => IsRiderHost
        ? "ReSharper.Structured.Logging.Rider"
        : "ReSharper.Structured.Logging";

    string TestProjectName => $"{ProjectName}.Tests";

    AbsolutePath Project => RootDirectory / "src" / "ReSharper.Structured.Logging" / $"{ProjectName}.csproj";

    AbsolutePath TestProject => RootDirectory / "test" / "src" / $"{TestProjectName}.csproj";

    AbsolutePath OutputDirectory =>
        RootDirectory / "src" / "ReSharper.Structured.Logging" / "bin" / ProjectName / Configuration;

    AbsolutePath TestProjectOutputDirectory => RootDirectory / "test" / "src" / "bin" / TestProjectName / Configuration;

    string ExtensionVersion { get; set; }

    string SdkVersion { get; set; }

    string SdkVersionSuffix { get; set; }

    string SdkVersionWithoutSuffix { get; set; }

    string WaveVersionsRange { get; set; }

    int WaveMajorVersion { get; set; }

    Target Clean => _ => _
        .Executes(() =>
        {
            // The build project is excluded because the running NUKE process is loaded from its own output
            DotNetCleanup($"{RootDirectory} -y --exclude build/** --verbosity normal");
        });

    Target Compile => _ => _
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Project)
                .SetConfiguration(Configuration)
                .SetVersionPrefix(ExtensionVersion)
                .SetOutputDirectory(OutputDirectory));
        });

    Target Test => _ => _
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(TestProject)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(TestProjectOutputDirectory));

            NUnit3(s => s.SetInputFiles(TestProjectOutputDirectory / $"{TestProjectName}.dll"));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Requires(() => !IsRiderHost)
        .Executes(() =>
        {
            NuGetPack(s => s
                .SetTargetPath(BuildProjectDirectory / "ReSharper.Structured.Logging.nuspec")
                // A ReSharper extension ships its assembly in dotFiles rather than lib, which package
                // analysis reports as NU5100
                .SetNoPackageAnalysis(true)
                .SetVersion(ExtensionVersion)
                .SetBasePath(OutputDirectory)
                .AddProperty("project", ProjectName)
                .AddProperty("waveVersion", WaveVersionsRange)
                // The base path is the compiled output directory, so the logo is passed as an
                // absolute path rather than resolved relative to it
                .AddProperty("logoPath", RootDirectory / "images" / "logo.png")
                .SetOutputDirectory(RootDirectory));

            PublishExtensionVersion();
        });

    Target PackRiderPlugin => _ => _
        .DependsOn(Compile)
        .Requires(() => IsRiderHost)
        .Executes(() =>
        {
            Gradle(
                $"buildPlugin -PPluginVersion={ExtensionVersion} -PProductVersion={RiderProductVersion} -PDotNetOutputDirectory={OutputDirectory} -PDotNetProjectName={ProjectName}",
                logger:
                (_, s) =>
                {
                    // Gradle writes warnings to stderr
                    // By default logger will write stderr as errors
                    // Keep Gradle warnings from being reported as CI build errors
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    Serilog.Log.Debug(s);
                });

            (RootDirectory / "gradle-build" / "distributions" / $"rider-structured-logging-{ExtensionVersion}.zip")
                .Copy(RiderPackagePath, ExistsPolicy.FileOverwrite);

            PublishExtensionVersion();
        });

    Target PublishReSharperPlugin => _ => _
        .DependsOn(Pack)
        .Requires(() => !IsRiderHost)
        .Requires(() => MarketplaceToken)
        .Executes(() =>
        {
            NuGetPush(s => s
                .SetTargetPath(ReSharperPackagePath)
                .SetSource("https://plugins.jetbrains.com/")
                .SetApiKey(MarketplaceToken));
        });

    Target PublishRiderPlugin => _ => _
        .DependsOn(PackRiderPlugin)
        .Requires(() => IsRiderHost)
        .Requires(() => MarketplaceToken)
        .Executes(() =>
        {
            // NUKE logs tool arguments, so the token travels through the environment instead
            // Seeding from Variables is required because this replaces the child process environment
            var environmentVariables = new Dictionary<string, string>(Variables, StringComparer.OrdinalIgnoreCase)
            {
                ["PUBLISH_TOKEN"] = MarketplaceToken,
            };

            Gradle(
                $"publishPlugin -PPluginVersion={ExtensionVersion} -PProductVersion={RiderProductVersion} -PDotNetOutputDirectory={OutputDirectory} -PDotNetProjectName={ProjectName} -PPluginChannel={PluginChannel}",
                environmentVariables: environmentVariables,
                logger:
                (_, s) =>
                {
                    // Gradle writes warnings to stderr
                    // By default logger will write stderr as errors
                    // Keep Gradle warnings from being reported as CI build errors
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    Serilog.Log.Debug(s);
                });
        });

    Target RunIde => _ => _
        .DependsOn(Compile)
        .Requires(() => IsRiderHost)
        .Executes(() =>
        {
            var arguments =
                $"runIde -PPluginVersion={ExtensionVersion} -PProductVersion={RiderProductVersion} -PDotNetOutputDirectory={OutputDirectory} -PDotNetProjectName={ProjectName}";

            if (RunIdeSolution != null)
            {
                arguments += $" -PRunIdeSolution={RunIdeSolution}";
            }

            Gradle(
                arguments,
                logger:
                (_, s) =>
                {
                    // Gradle and the sandboxed IDE write plenty of noise to stderr
                    // Keep it visible without failing the build
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    Serilog.Log.Information(s);
                });
        });

    Target UpdateSdkVersion => _ => _
        .Executes(async () =>
        {
            var availableVersions = await GetPublishedSdkVersions();
            var currentVersion = NuGetVersion.Parse(SdkVersion);

            NuGetVersion targetVersion;
            if (SdkVersionOverride != null)
            {
                var requestedVersion = NuGetVersion.Parse(SdkVersionOverride);
                targetVersion = availableVersions
                    .FirstOrDefault(x => x.Equals(requestedVersion))
                    .NotNull($"{SdkVersionOverride} is not published for every JetBrains SDK package");
            }
            else
            {
                targetVersion = SelectSdkUpdate(currentVersion, availableVersions);
            }

            if (targetVersion == null || targetVersion.Equals(currentVersion))
            {
                Serilog.Log.Information("The JetBrains SDK {Version} is up to date", SdkVersion);
                PublishGitHubOutput("sdk-update-available", "false");

                return;
            }

            var propsFile = RootDirectory / "Directory.Build.props";
            // A regex rather than XDocument.Save, so that the MSBuild namespace declaration, the
            // attribute order and the comment in the file all survive untouched
            propsFile.WriteAllText(Regex.Replace(
                propsFile.ReadAllText(),
                "<SdkVersion>[^<]*</SdkVersion>",
                $"<SdkVersion>{targetVersion}</SdkVersion>"));

            Serilog.Log.Information("Updated the JetBrains SDK from {Current} to {Target}", SdkVersion, targetVersion);
            ReportSummary(_ => _.AddPair("SDK", $"{SdkVersion} -> {targetVersion}"));

            PublishGitHubOutput("sdk-update-available", "true");
            PublishGitHubOutput("sdk-version", targetVersion.ToString());
            PublishGitHubOutput("previous-sdk-version", SdkVersion);
        });

    static async Task<IReadOnlyCollection<NuGetVersion>> GetPublishedSdkVersions()
    {
        using var client = new HttpClient();

        HashSet<NuGetVersion> versions = null;
        foreach (var packageId in SdkPackageIds)
        {
            var index = await client.GetStringAsync($"https://api.nuget.org/v3-flatcontainer/{packageId}/index.json");

            using var document = JsonDocument.Parse(index);
            var published = document.RootElement
                .GetProperty("versions")
                .EnumerateArray()
                .Select(x => NuGetVersion.Parse(x.GetString()))
                .ToList();

            if (versions == null)
            {
                versions = new HashSet<NuGetVersion>(published, VersionComparer.Default);
            }
            else
            {
                versions.IntersectWith(published);
            }
        }

        return versions;
    }

    // A same wave patch is already covered by the Wave dependency range the package declares, so it
    // is not worth a release; the next wave is, and it shows up as an EAP first. Once the adopted
    // version is a prerelease the whole train is followed instead: eap01 -> rc01 -> the stable release
    static NuGetVersion SelectSdkUpdate(NuGetVersion currentVersion, IEnumerable<NuGetVersion> availableVersions)
    {
        return availableVersions
            .Where(x => x > currentVersion)
            .Where(x => currentVersion.IsPrerelease
                        || x.Major > currentVersion.Major
                        || (x.Major == currentVersion.Major && x.Minor > currentVersion.Minor))
            .OrderBy(x => x)
            .LastOrDefault();
    }

    // Replaces AppVeyor's UpdateBuildVersion, which used to display the extension version on the
    // build page. GitHub Actions evaluates run-name before any step runs, so the version cannot go
    // there; it goes to the job summary instead, and to the workflow environment so that the upload
    // steps can name the artifacts after it.
    void PublishExtensionVersion()
    {
        ReportSummary(_ => _.AddPair("Version", ExtensionVersion));

        if (GitHubActions == null)
        {
            return;
        }

        var environmentFile = (AbsolutePath)GetVariable("GITHUB_ENV");
        if (environmentFile != null)
        {
            environmentFile.AppendAllLines(new[] { $"EXTENSION_VERSION={ExtensionVersion}" });
        }

        // Both pack targets call this, and a publish run packs a second time, but the version is
        // the same. The variable exported above is visible to every later step of the same job, so
        // its absence means this is the first pack and the only one that should report the version
        var summaryFile = GitHubActions.StepSummaryFile;
        if (!IsRiderHost && summaryFile != null && GetVariable("EXTENSION_VERSION") == null)
        {
            summaryFile.AppendAllLines(new[] { $"### Version `{ExtensionVersion}`" });
        }
    }

    // Hands a value to the later steps of the same job, which is how the SDK update workflow learns
    // what UpdateSdkVersion decided. Outside of GitHub Actions there is nowhere to write it
    static void PublishGitHubOutput(string name, string value)
    {
        var outputFile = (AbsolutePath)GetVariable("GITHUB_OUTPUT");
        if (outputFile == null)
        {
            return;
        }

        outputFile.AppendAllLines(new[] { $"{name}={value}" });
    }
}
