#tool "nuget:?package=GitVersion.CommandLine"
#tool nuget:?package=OpenCover
#tool nuget:?package=Codecov
#addin nuget:?package=Cake.Figlet

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var artifactPath = Argument("artifactPath", "./artifacts");
var versionAssemblyInfoFile = Argument("versionAssemblyInfo", "./build/VersionAssemblyInfo.cs");

var buildPath = MakeAbsolute(Directory("./build"));
var slnPath = "./MaltReport.sln";
var artifacts = MakeAbsolute(Directory(artifactPath));
var versionAssemblyInfo = MakeAbsolute(File(versionAssemblyInfoFile));
GitVersion versionInfo = null;


Setup(context =>
{
    Information(Figlet("Sandwych.Reporting"));

    EnsureDirectoryExists(artifacts);
    
    EnsureDirectoryExists(buildPath);
    /*
    var binDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\bin");
    var objDirs = GetDirectories(solutionPath.GetDirectory() +@"\src\**\obj");
    DeleteDirectories(binDirs, true);
    DeleteDirectories(objDirs, true);
    */

});


Task("Update-Version-Info")
    .IsDependentOn("Create-Version-Info")
    .Does(() => 
{
        versionInfo = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true,
            UpdateAssemblyInfoFilePath = versionAssemblyInfo
        });
    if(versionInfo != null) {
        Information("Version: {0}", versionInfo.FullSemVer);
    } else {
        throw new Exception("Unable to determine version");
    }
});


Task("Create-Version-Info")
    .WithCriteria(() => !FileExists(versionAssemblyInfo))
    .Does(() =>
{
    Information("Creating version assembly info");
    CreateAssemblyInfo(versionAssemblyInfo, new AssemblyInfoSettings {
        Version = "0.0.0.0",
        FileVersion = "0.0.0.0",
        InformationalVersion = "",
    });
});


Task("Clean").Does(() =>
{
    CleanDirectories(new[]{"./Sandwych.Reporting/bin", "./Sandwych.Reporting.Demo/bin", "./Sandwych.Reporting.Tests/bin"});
});

Task("Restore")
    .IsDependentOn("Clean")
    .IsDependentOn("Update-Version-Info")
    .Does(() =>
{
    NuGetRestore(slnPath, new NuGetRestoreSettings());
});


Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    MSBuild(slnPath, c => c
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2015)
        .WithProperty("TreatWarningsAsErrors", "true")
        .WithTarget("Build")
    );

});


Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
});


Task("Pack").IsDependentOn("Tests").Does(() =>
{
    var version = GitVersion();
    var nuGetPackSettings   = new NuGetPackSettings {
        Id                      = "MaltReport2",
        Version                 =  versionInfo.NuGetVersionV2,
        NoPackageAnalysis       = true,
        BasePath                = "./Sandwych.Reporting/Bin/Release",
        OutputDirectory         = artifacts,
        Files                   = new [] {
                                    new NuSpecContent {Source = "Sandwych.Reporting.dll", Target = "lib/net45"},
                                  },
     };

     NuGetPack("./MaltReport2.nuspec", nuGetPackSettings);
});

Task("Default").IsDependentOn("Pack");

RunTarget(target);
