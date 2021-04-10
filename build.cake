#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.11.1"

#addin "nuget:?package=Cake.Sonar&version=1.1.25"

var target = Argument("target", "Default");
var buildConfiguration = Argument("buildConfig", "Debug");

var waveVersion = Argument("wave", "211");
var waveNugetVersion = $"[{waveVersion}.0]";
var host = Argument("Host", "Resharper");

var solutionName = "ReSharper.Structured.Logging";
var projectName = solutionName;
var riderHost = "Rider";
if (host == riderHost)
{
    projectName = solutionName + ".Rider";
}

var testProjectName = projectName + ".Tests";
var testAssembly = string.Format("./test/src/bin/{0}/{1}/{0}.dll", testProjectName, buildConfiguration);
var solutionFile = string.Format("./src/{0}.sln", solutionName);
var solutionFolder = string.Format("./src/{0}/", solutionName);
var projectFile = string.Format("{0}{1}.csproj", solutionFolder, projectName);

var extensionsVersion = XmlPeek(projectFile, "Project/PropertyGroup[1]/VersionPrefix/text()");

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
    var projects = GetFiles("./**/*.csproj");
    foreach(var project in projects)
    {
        NuGetRestore(project);
    }
});

Task("Build")
  .IsDependentOn("NugetRestore")
  .Does(() =>
{
    MSBuild(solutionFile, s => s.SetConfiguration(buildConfiguration));
});

Task("Test")
  .IsDependentOn("Build")
  .Does(() =>
{
    NUnit3(testAssembly);
});

Task("NugetPack")
  .IsDependentOn("AppendBuildNumber")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .Does(() =>
{
     var buildPath = string.Format("./src/{0}/bin/{1}/{2}", solutionName, projectName, buildConfiguration);

     var files = new List<NuSpecContent>();
     files.Add(new NuSpecContent {Source = string.Format("{0}/{1}.dll", buildPath, projectName), Target = "dotFiles"});
     files.Add(new NuSpecContent {Source = string.Format("{0}/{1}.pdb", buildPath, projectName), Target = "dotFiles"});

     var nuGetPackSettings   = new NuGetPackSettings {
                                     Id                      = projectName,
                                     Version                 = extensionsVersion,
                                     Title                   = "Structured Logging",
                                     Authors                 = new[] { "Oleg Shevchenko" },
                                     Owners                  = new[] { "Oleg Shevchenko" },
                                     Description             = "Provides highlighting for structured logging message templates and contains some useful analyzers",
                                     ProjectUrl              = new Uri("https://github.com/olsh/resharper-structured-logging"),
                                     LicenseUrl              = new Uri("https://github.com/olsh/resharper-structured-logging/blob/master/LICENSE"),
                                     Tags                    = new [] { "resharper", "serilog", "nlog", "logging", "structuredlogging" },
                                     RequireLicenseAcceptance= false,
                                     Symbols                 = false,
                                     NoPackageAnalysis       = true,
                                     Files                   = files,
                                     OutputDirectory         = ".",
                                     Dependencies            = new [] { new NuSpecDependency() { Id = "Wave", Version = waveNugetVersion } },
                                     ReleaseNotes            = new [] { "https://github.com/olsh/resharper-structured-logging/releases" }
                                 };

     NuGetPack(nuGetPackSettings);

     if (host == riderHost)
     {
         var tempDirectory = "./temp/";
         if (DirectoryExists(tempDirectory))
         {
             DeleteDirectory(tempDirectory, new DeleteDirectorySettings { Force = true, Recursive = true });
         }

         var riderMetaFolderName = "rider-structured-logging";
         var riderMetaFolderPath = string.Format("{0}{1}/", tempDirectory, riderMetaFolderName);
         CopyDirectory(string.Format("./src/{0}/", riderMetaFolderName), riderMetaFolderPath);
         var nugetPackage = string.Format("{0}.{1}.nupkg", projectName, extensionsVersion);
         CopyFile(nugetPackage, string.Format("{0}{1}", riderMetaFolderPath, nugetPackage));

         var riderMetaFile = "{0}META-INF/plugin.xml";
         var xmlSettings = new XmlPokeSettings { Encoding = new UTF8Encoding(false) };
         XmlPoke(string.Format(riderMetaFile, riderMetaFolderPath), "idea-plugin/version", extensionsVersion, xmlSettings);
         XmlPoke(string.Format(riderMetaFile, riderMetaFolderPath), "idea-plugin/idea-version/@since-build", waveVersion, xmlSettings);
         XmlPoke(string.Format(riderMetaFile, riderMetaFolderPath), "idea-plugin/idea-version/@until-build", waveVersion + ".*", xmlSettings);

         Zip(tempDirectory, string.Format("./{0}.zip", riderMetaFolderName));
     }
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
    if (host == riderHost)
    {
        artifactFile = string.Format("rider-structured-logging.zip");
    }

    BuildSystem.AppVeyor.UploadArtifact(artifactFile);
});

Task("Default")
    .IsDependentOn("NugetPack");

RunTarget(target);
