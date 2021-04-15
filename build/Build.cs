using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Colorful;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.NUnit;
using Nuke.Common.Tools.SonarScanner;

using static Nuke.Common.IO.XmlTasks;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.Tools.NUnit.NUnitTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.SonarScanner.SonarScannerTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    protected override void OnBuildInitialized()
    {
        var sdkNuGetPackage = IsRiderHost ? "JetBrains.Rider.SDK" : "JetBrains.ReSharper.SDK";
        SdkVersion = XmlPeekSingle(
            Project.Path,
            $"//PackageReference[@Include='{sdkNuGetPackage}']/@Version");
        SdkVersion.NotNull("Unable to detect SDK version");

        ExtensionVersion = AppVeyor == null ? SdkVersion : $"{SdkVersion}.{AppVeyor.BuildNumber}";
        var sdkMatch = Regex.Match(SdkVersion, @"\d{2}(\d{2}).(\d).*");
        WaveMajorVersion = int.Parse(sdkMatch.Groups[1]
            .Value + sdkMatch.Groups[2]
            .Value);
        WaveVersionsRange = $"[{WaveMajorVersion}.0, {WaveMajorVersion + 1}.0)";

        base.OnBuildInitialized();
    }

    [CI] readonly AppVeyor AppVeyor;

    [Parameter] readonly string Configuration = "Release";

    [Parameter] readonly bool IsRiderHost;

    [Solution] readonly Solution Solution;

    [LocalExecutable("./gradlew.bat")] readonly Tool Gradle;

    string NuGetPackageFileName => $"{Project.Name}.{ExtensionVersion}.nupkg";

    string NuGetPackagePath => RootDirectory / NuGetPackageFileName;

    string RiderPackagePath => RootDirectory / "rider-structured-logging.zip";

    string SonarQubeApiKey => GetVariable<string>("sonar:apikey");

    Project Project => IsRiderHost
        ? Solution.GetProject("ReSharper.Structured.Logging.Rider")
        : Solution.GetProject("ReSharper.Structured.Logging");

    Project TestProject => Solution.GetProject($"{Project.Name}.Tests");

    AbsolutePath OutputDirectory => Project.Directory / "bin" / Project.Name / Configuration;

    AbsolutePath TestProjectOutputDirectory => TestProject.Directory / "bin" / TestProject.Name / Configuration;

    string ExtensionVersion { get; set; }

    string SdkVersion { get; set; }

    string WaveVersionsRange { get; set; }

    int WaveMajorVersion { get; set; }

    Target UpdateBuildVersion => _ => _
        .Requires(() => AppVeyor)
        .Executes(() =>
        {
            AppVeyor.Instance.UpdateBuildVersion(ExtensionVersion);
        });

    Target Compile => _ => _
        .DependsOn()
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Project.Path)
                .SetConfiguration(Configuration)
                .SetVersionPrefix(ExtensionVersion)
                .SetOutputDirectory(OutputDirectory));
        });

    Target Test => _ => _
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(TestProject.Path)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(TestProjectOutputDirectory));

            NUnit3(s => s.SetInputFiles(TestProjectOutputDirectory / $"{TestProject.Name}.dll"));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .After(Test)
        .Executes(() =>
        {
            NuGetPack(s => s
                .SetTargetPath(BuildProjectDirectory / "ReSharper.Structured.Logging.nuspec")
                .SetVersion(ExtensionVersion)
                .SetBasePath(OutputDirectory)
                .AddProperty("project", Project.Name)
                .AddProperty("waveVersion", WaveVersionsRange)
                .SetOutputDirectory(RootDirectory));
        });

    Target BuildRiderFrontend => _ => _
        .Unlisted()
        .Executes(() =>
        {
            Gradle($"buildPlugin -PPluginVersion={ExtensionVersion} -PProductVersion={SdkVersion}", customLogger:
                (type, s) =>
                {
                    // Gradle writes warnings to stderr
                    // By default logger will write stderr as errors
                    // AppVeyor writes errors as special messages and stops the build if such messages more than 500
                    if (type == OutputType.Err)
                    {
                        Logger.Warn(s);
                    }
                    else
                    {
                        Logger.Info(s);
                    }
                });
        });

    Target PackRiderPlugin => _ => _
        .Unlisted()
        .TriggeredBy(Pack)
        .DependsOn(BuildRiderFrontend)
        .OnlyWhenStatic(() => IsRiderHost)
        .Executes(() =>
        {
            var tempDirectory = RootDirectory / "temp";
            if (DirectoryExists(tempDirectory))
            {
                DeleteDirectory(tempDirectory);
            }

            var gradleBuildDirectory = RootDirectory / "gradle-build";
            var pluginXmlFilePath = gradleBuildDirectory / "patchedPluginXmlFiles"  / "plugin.xml";
            var pluginRootDirectory = tempDirectory / "rider-structured-logging";
            CopyFile(pluginXmlFilePath, pluginRootDirectory / "META-INF"  / "plugin.xml" );

            var pluginJarFile = gradleBuildDirectory / "libs" / "rider-StructuredLogging-2021.1.1.jar";
            CopyFile(pluginJarFile, pluginRootDirectory / "lib"  / "rider-structured-logging.jar" );

            CopyFile(NuGetPackagePath, pluginRootDirectory / NuGetPackageFileName);
            CompressZip(tempDirectory, RiderPackagePath, fileMode: FileMode.Create);
            DeleteFile(NuGetPackagePath);
        });

    Target SonarBegin => _ => _
        .Unlisted()
        .Before(Compile)
        .Executes(() =>
        {
            SonarScannerBegin(s => s
                .SetServer("https://sonarcloud.io")
                .SetFramework("net5.0")
                .SetLogin(SonarQubeApiKey)
                .SetProjectKey("resharper-structured-logging")
                .SetName("ReSharper Structured Logging")
                .SetOrganization("olsh-github")
                .SetVersion("1.0.0.0"));
        });

    Target Sonar => _ => _
        .DependsOn(SonarBegin, Compile)
        .Requires(() => !IsRiderHost)
        .Executes(() =>
        {
            SonarScannerEnd(s => s
                .SetLogin(SonarQubeApiKey)
                .SetFramework("net5.0"));
        });

    Target UploadArtifact => _ => _
        .DependsOn(Pack)
        .After(PackRiderPlugin)
        .Requires(() => AppVeyor)
        .Executes(() =>
        {
            AppVeyor.PushArtifact(IsRiderHost
                ? RiderPackagePath
                : NuGetPackagePath);
        });
}
