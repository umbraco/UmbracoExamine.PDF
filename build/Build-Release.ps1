param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("^\d\.\d\.(?:\d\.\d$|\d$)")]
	[string]
	$ReleaseVersionNumber,
	[Parameter(Mandatory=$true)]
	[string]
	[AllowEmptyString()]
	$PreReleaseName
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path);
$RepoRoot = (get-item $PSScriptFilePath).Directory.Parent.FullName;
$SolutionRoot = Join-Path -Path $RepoRoot "src";
$NuGetPackagesPath = Join-Path -Path $SolutionRoot "packages"

#trace
"Solution Root: $SolutionRoot"

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework\v4.0.30319\msbuild.exe";

# Restore NuGet packages
New-Item -ItemType Directory -Force -Path $NuGetPackagesPath
.\NuGet.exe install $SolutionRoot\UmbracoExamine.PDF\packages.config -OutputDirectory  $NuGetPackagesPath

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $RepoRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
	Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
	Remove-Item $ReleaseFolder -Recurse
}
New-Item $ReleaseFolder -Type directory

#trace
"Release path: $ReleaseFolder"

# Set the version number in SolutionInfo.cs
$AssemblyInfoPath = Join-Path -Path $SolutionRoot -ChildPath "UmbracoExamine.PDF\Properties\AssemblyInfo.cs"
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=AssemblyFileVersion\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $AssemblyInfoPath -Encoding UTF8;
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=AssemblyInformationalVersion\(`")[.\w-]*(?=`"\))", "$ReleaseVersionNumber$PreReleaseName" |
	sc -Path $AssemblyInfoPath -Encoding UTF8;
# Set the copyright
$Copyright = "Copyright © Umbraco ".(Get-Date).year
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=AssemblyCopyright\(`")[.\w-]*(?=`"\))", $Copyright |
	sc -Path $AssemblyInfoPath -Encoding UTF8;

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "UmbracoExamine.PDF.sln";

# clean sln for all deploys
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

#build
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

$include = @('UmbracoExamine.PDF.dll','UmbracoExamine.PDF.pdb')
$CoreBinFolder = Join-Path -Path $SolutionRoot -ChildPath "UmbracoExamine.PDF\bin\Release";
Copy-Item "$CoreBinFolder\*.*" -Destination $ReleaseFolder -Include $include

# COPY THE TRANSFORMS OVER
Copy-Item "$BuildFolder\nuget-transforms\ExamineIndex.config.install.xdt" -Destination (New-Item (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\ExamineIndex.config.uninstall.xdt" -Destination (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms");
Copy-Item "$BuildFolder\nuget-transforms\ExamineSettings.config.install.xdt" -Destination (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms");
Copy-Item "$BuildFolder\nuget-transforms\ExamineSettings.config.uninstall.xdt" -Destination (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms");
Copy-Item "$BuildFolder\nuget-transforms\web.config.install.xdt" -Destination (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms");
Copy-Item "$BuildFolder\nuget-transforms\web.config.uninstall.xdt" -Destination (Join-Path -Path $ReleaseFolder -ChildPath "nuget-transforms");

# COPY THE README OVER
Copy-Item "$BuildFolder\Readme.txt" -Destination $ReleaseFolder

# COPY OVER THE CORE NUSPEC AND BUILD THE NUGET PACKAGE
Copy-Item "$BuildFolder\UmbracoCms.UmbracoExamine.PDF.nuspec" -Destination $ReleaseFolder
$CoreNuSpec = Join-Path -Path $ReleaseFolder -ChildPath "UmbracoCms.UmbracoExamine.PDF.nuspec";
$NuGet = Join-Path $BuildFolder -ChildPath "NuGet.exe"
Write-Output "DEBUGGING: " $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName
& $NuGet pack $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

""
"Build $ReleaseVersionNumber$PreReleaseName is done!"
"NuGet packages also created, so if you want to push them just run:"
"  nuget push $CoreNuSpec"

# COPY THE MAANUAL INSTALL README OVER
Copy-Item "$BuildFolder\ReadmeManual.txt" -Destination $ReleaseFolder

# COPY ITEXTSHARP FILES
$iTextSharpPackage = Get-ChildItem $NuGetPackagesPath -Recurse | ?{ $_.PSIsContainer -and $_.Name.StartsWith("iTextSharp", "CurrentCultureIgnoreCase") } | Select-Object -Last 1
$iTextSharpLibPath = Join-Path $iTextSharpPackage.FullName "lib"
Copy-Item $iTextSharpLibPath\*.* $ReleaseFolder

# ZIP UP FOR MANUAL INSTALL
.\7za.exe a -tzip $ReleaseFolder\UmbracoCms.UmbracoExamine.PDF.$ReleaseVersionNumber$PreReleaseName.zip $ReleaseFolder\UmbracoExamine.PDF.pdb  $ReleaseFolder\UmbracoExamine.PDF.dll $ReleaseFolder\iTextSharp.* $ReleaseFolder\ReadmeManual.txt