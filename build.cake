#reference "NuGet.Packaging"

#load nuget.tool.cake

var target = Argument("target", "default");
var rootPath     = "./";
var srcPath      = rootPath + "src/";
var contractPath = rootPath + "contract/";
var testPath     = rootPath + "test/";
var distPath     = rootPath + "aelf-node/";

var solution     = rootPath + "AElf.sln";
var srcProjects  = GetFiles(srcPath + "**/*.csproj");
var contractProjects  = GetFiles(contractPath + "**/*.csproj");

var nugetTool = NuGetTool.FromCakeContext(Context);

Task("clean")
    .Description("清理项目缓存")
    .Does(() =>
{
    DeleteFiles(distPath + "*.nupkg");
    CleanDirectories(srcPath + "**/bin");
    CleanDirectories(srcPath + "**/obj");
    CleanDirectories(contractPath + "**/bin");
    CleanDirectories(contractPath + "**/obj");
    CleanDirectories(testPath + "**/bin");
    CleanDirectories(testPath + "**/obj");
});

Task("restore")
    .Description("还原项目依赖")
    .Does(() =>
{
    DotNetCoreRestore(solution);
});

Task("build")
    .Description("编译项目")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .Does(() =>
{
    var buildSetting = new DotNetCoreBuildSettings{
        NoRestore = true
    };
     
    DotNetCoreBuild(solution, buildSetting);
});


Task("test")
    .Description("运行测试")
    .IsDependentOn("build")
    .Does(() =>
{
    var testSetting = new DotNetCoreTestSettings{
        NoRestore = true,
        NoBuild = true
    };
    var testProjects = GetFiles("./test/*.Tests/*.csproj");

    foreach(var testProject in testProjects)
    {
        DotNetCoreTest(testProject.FullPath, testSetting);
    }
});

Task("pack")
    .Description("nuget打包")
    .IsDependentOn("test")
    .Does(() =>
{
    var projectFilePaths = srcProjects.Select(_=>_.FullPath).ToList();
    nugetTool.Pack(projectFilePaths, distPath);
});

Task("push")
    .Description("nuget发布")
    .IsDependentOn("pack")
    .Does(() =>
{
    var packageFilePaths = GetFiles(distPath + "*.symbols.nupkg").Select(_=>_.FullPath).ToList();

    nugetTool.Push(packageFilePaths);
});

Task("default")
    .Description("默认-运行测试(-target test)")
    .IsDependentOn("test");

RunTarget(target);
