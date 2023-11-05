@{
    ####################
    ####   Global   ####
    ####################

    # Required. The name of the project/GitHub repository
    Name = 'ChaosLib'

    # Required. The prefix to use for all build environment cmdlets
    CmdletPrefix = 'Chaos'

    # Required. The copyright author and year to display in the build environment
    Copyright = 'lordmilko, 2023'

    # Optional. The name of the Visual Studio Solution. Required when a project contains multiple solutions
    # SolutionName = ''

    # Optional. A wildcard expression indicating the projects that should be built in CI
    # BuildFilter = ''

    # Optional. The target framework that is used in debug mode when the project conditionally multi-targets only on Release
    # DebugTargetFramework = ''

    # Optional. Features to enable in the build environment. By default all features are allowed, and can be negated with ~. Valid values include: Dependency, Build, Test, Coverage, Package, Version
    Features = '~Test','~Package','~Coverage'

    # Optional. Commands to enable in the build environment. By default all commands are allowed, and can be negated with ~. Valid values include: CommandList, Coverage, ClearBuild, GetVersion, GitStatus, InstallDependency, InvokeBuild, InvokePSAnalyzer, InvokeTest, LaunchModule, Log, NewPackage, SimulateCI, OpenWiki, SetVersion, TestResult, UpdateVersion
    # Commands = ''

    # Optional. The value to use for the prompt in the build environment. If not specified, Name will be used
    # Prompt = ''

    # Optional. The name of the folder that the source code is contained in. If not specified, will automatically be calculated
    # SourceFolder = ''

    # Optional. The minimum coverage threshold that must be met under CI
    # CoverageThreshold = ''

    ####################
    ####   CSharp   ####
    ####################

    # Optional. Files to exclude from the C# NuGet Package when building legacy packages
    # CSharpLegacyPackageExcludes = @()

    ####################
    #### PowerShell ####
    ####################

    # Optional. Indicates that a PowerShell package should be built containing both coreclr and fullclr subfolders
    # PowerShellMultiTargeted = $false

    # Optional. The name of the PowerShell module. If not specified, Name will be used
    # PowerShellModuleName = ''

    # Optional. The name of the PowerShell project. If not specified, will automatically be calculated
    # PowerShellProjectName = ''

    # Optional. A ScriptBlock that takes a FileInfo/DirectoryInfo as $_ and returns whether or not to process unit tests for that file/folder
    # PowerShellUnitTestFilter = $null

    ####################
    ####    Test    ####
    ####################

    # Optional. The languages to perform unit tests for. If not specified, C# and PowerShell will be tested
    # TestTypes = @()

    # Optional. The name of the Unit Test project. If not specified, will automatically be calculated
    # UnitTestProjectName = ''

    ####################
    ####   Package  ####
    ####################

    # Optional. The types of packages to produce. If not specified, C#/PowerShell *.nupkg and Redist *.zip files will be produced
    # PackageTypes = @()

    # Optional. The tests to perform for each type of package
    # PackageTests = @{}

    # Optional. The files that are expected to exist in each tyoe of package
    # PackageFiles = @{}
}
