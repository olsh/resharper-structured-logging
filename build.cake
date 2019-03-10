#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.3.1"

#addin "nuget:?package=Cake.Sonar&version=1.1.18"

var target = Argument("target", "Default");
var buildConfiguration = Argument("buildConfig", "Debug");
var waveVersion = Argument("wave", "[183.0]");
var extensionsVersion =  Argument("Version", "2018.3.0");

var solutionName = "ReSharper.Structured.Logging";
var projectName = solutionName;

var solutionFile = string.Format("./src/{0}.sln", solutionName);
var solutionFolder = string.Format("./src/{0}/", solutionName);
var projectFile = string.Format("{0}{1}.csproj", solutionFolder, projectName);

Task("AppendBuildNumber")
  .WithCriteria(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
  .Does(() =>
{
    var buildNumber = BuildSystem.AppVeyor.Environment.Build.Number;
    extensionsVersion = string.Format("{0}.{1}", extensionsVersion, buildNumber);
});

Task("UpdateBuildVersion")
  .IsDependentOn("AppendBuildNumber")
  .WithCriteria(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
  .Does(() =>
{
    BuildSystem.AppVeyor.UpdateBuildVersion(extensionsVersion);
});

Task("NugetRestore")
  .Does(() =>
{
    NuGetRestore(solutionFile);
});

Task("Build")
  .IsDependentOn("NugetRestore")
  .Does(() =>
{
    MSBuild(solutionFile, new MSBuildSettings {
        Configuration = buildConfiguration
    });
});

Task("NugetPack")
  .IsDependentOn("AppendBuildNumber")
  .IsDependentOn("Build")
  .Does(() =>
{
     var buildPath = string.Format("./src/{0}/bin/{1}", solutionName, buildConfiguration);

     var files = new List<NuSpecContent>();
     files.Add(new NuSpecContent {Source = string.Format("{0}/{1}.dll", buildPath, projectName), Target = "dotFiles"});
     files.Add(new NuSpecContent {Source = string.Format("{0}/{1}.pdb", buildPath, projectName), Target = "dotFiles"});

     var nuGetPackSettings   = new NuGetPackSettings {
                                     Id                      = projectName,
                                     Version                 = extensionsVersion,
                                     Title                   = "Structured Logging",
                                     Authors                 = new[] { "Oleg Shevchenko" },
                                     Owners                  = new[] { "Oleg Shevchenko" },
                                     Description             = "Provides support for Serilog",
                                     ProjectUrl              = new Uri("https://github.com/olsh/resharper-structured-logging"),
                                     LicenseUrl              = new Uri("https://github.com/olsh/resharper-structured-logging/raw/master/LICENSE"),
                                     Tags                    = new [] { "resharper", "serilog", "nlog", "logging", "structurelogging" },
                                     RequireLicenseAcceptance= false,
                                     Symbols                 = false,
                                     NoPackageAnalysis       = true,
                                     Files                   = files,
                                     OutputDirectory         = ".",
                                     Dependencies            = new [] { new NuSpecDependency() { Id = "Wave", Version = waveVersion } },
                                     ReleaseNotes            = new [] { "https://github.com/olsh/resharper-structured-logging/releases" }
                                 };

     NuGetPack(nuGetPackSettings);
});

Task("SonarBegin")
  .Does(() => {
     SonarBegin(new SonarBeginSettings {
        Url = "https://sonarcloud.io",
        Login = EnvironmentVariable("sonar:apikey"),
        Key = "resharper-structured-logging",
        Name = "ReSharper Structured Logging",
        ArgumentCustomization = args => args
            .Append($"/o:olsh-github"),
        Version = "1.0.0.0"
     });
  });

Task("SonarEnd")
  .Does(() => {
     SonarEnd(new SonarEndSettings {
        Login = EnvironmentVariable("sonar:apikey")
     });
  });

Task("Sonar")
  .IsDependentOn("SonarBegin")
  .IsDependentOn("Build")
  .IsDependentOn("SonarEnd");

Task("CreateArtifact")
  .IsDependentOn("UpdateBuildVersion")
  .IsDependentOn("NugetPack")
  .WithCriteria(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
  .Does(() =>
{
    var artifactFile = string.Format("{0}.{1}.nupkg", projectName, extensionsVersion);
    BuildSystem.AppVeyor.UploadArtifact(artifactFile);
});

Task("CI")
    .IsDependentOn("Sonar")
    .IsDependentOn("CreateArtifact");

Task("Default")
    .IsDependentOn("NugetPack");

RunTarget(target);
