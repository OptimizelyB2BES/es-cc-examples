param (
    [Parameter(Mandatory=$true)]
    [string]$themeName,
    [string]$copyFromThemeName
)

function returnError($message)
{
    $initialBG = $Host.Ui.RawUI.BackgroundColor
    $initialFG = $Host.Ui.RawUI.ForegroundColor
    $Host.UI.RawUI.BackgroundColor = "Red"
    write-output $message
    $Host.Ui.RawUI.BackgroundColor = $initialBG
    exit 1
}

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
    }
}

function Get-Root
{
    $scriptDirectory = Get-ScriptDirectory
    if ($scriptDirectory.EndsWith("PowerShellScripts")) {
        return (get-item $scriptDirectory).Parent.Parent.FullName
    }

    return $scriptDirectory
}

$rootDirectory = Get-Root
Write-Output $rootDirectory

if ($themeName -eq "") {
    returnError "Please specify a -themeName"
}

$themePath = $rootDirectory + "\InsiteCommerce.Web\Themes\$themeName"

if (Test-Path $themePath) {
    returnError "The theme $themeName already exists at $themePath"
}

if ($copyFromThemeName -eq "") {
    $sourceThemePath = $rootDirectory + "\InsiteCommerce.Web\_systemResources\themes\responsive"
    if (-not (Test-Path $sourceThemePath)) {
        $sourceThemePath = $rootDirectory + "\Insite.SystemResources\Themes\Responsive"
    }

    $copyFromThemeName = "Responsive"
}
else {
    $sourceThemePath = $rootDirectory + "\InsiteCommerce.Web\themes\$copyFromThemeName"
}

if (-Not(Test-Path $sourceThemePath)) {
    returnError "There was no theme found called $copyFromThemeName at $sourceThemePath found to copy from"
}

New-Item $themePath -type directory > $null

& xcopy $sourceThemePath $themePath /y /s /r > $null

$themeJsonPath = $themePath + "\theme.json"

if (-Not(Test-Path $themeJsonPath)) {
    $themeJsonBakPath = $themePath + "\theme.json.bak"
    if (-Not(Test-Path $themeJsonBakPath)) {
        Remove-Item $themePath -Force -Recurse

        returnError "There was no theme.json or theme.json.bak file found in the source theme."
    }
    Rename-Item -Path $themeJsonBakPath -NewName $themeJsonPath
}

$reader = [System.IO.File]::OpenText($themeJsonPath)
$themeJsonContent = New-Object System.Text.StringBuilder
while($null -ne ($line = $reader.ReadLine())) {
    if ($line.Trim().StartsWith("`"Name`": ")) {
        $line = "  `"Name`": `"$themeName`",";
    }
    elseif ($line.Trim().StartsWith("`"Description`": ")) {
        $line = "  `"Description`": `"The $themeName Theme`",";
    }
    elseif ($line.Trim().StartsWith("`"ParentTheme`": ")) {
        $line = "  `"ParentTheme`": `"$copyFromThemeName`",";
    }
    $themeJsonContent.AppendLine($line) > $null
}

$reader.Close()

Set-Content -Path $themeJsonPath -Value $themeJsonContent.ToString() -Force

$itemGroup = ""

foreach($file in Get-ChildItem $themePath -Recurse) {
    if ($file.FullName.ToLower().EndsWith(".ts")) {
        $itemGroup += "<TypeScriptCompile Include=`"" + $file.FullName.Substring($themePath.Length + 1) + "`" />`n"
    }
    elseif ($file.FullName.ToLower().EndsWith(".css")) {
        $scssPath = $file.FullName.Substring(0, $file.FullName.Length - 4) + ".scss"
        if (-not (Test-Path $scssPath)) {
            $itemGroup += "<Content Include=`"" + $file.FullName.Substring($themePath.Length + 1) + "`" />`n"
        }
    }
    elseif ($file.FullName.ToLower().EndsWith(".cshtml") -or $file.FullName.ToLower().EndsWith(".liquid") -or $file.FullName.ToLower().EndsWith(".json") -or $file.FullName.ToLower().EndsWith(".scss")) {
        $itemGroup += "<Content Include=`"" + $file.FullName.Substring($themePath.Length + 1) + "`" />`n"
    }
}

$projectId = [guid]::NewGuid()

$solutionFilePath = $rootDirectory + "\insitecommerce.sln"

$reader = [System.IO.File]::OpenText($solutionFilePath)
$solutionFileContent = New-Object System.Text.StringBuilder
while($null -ne ($line = $reader.ReadLine())) {
    if ($line -eq "Global") {
        $solutionFileContent.AppendLine("Project(`"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`") = `"$themeName`", `"InSiteCommerce.Web\Themes\$themeName\$themeName.csproj`", `"{$projectId}`"
EndProject") > $null
        $solutionFileContent.AppendLine($line) > $null
    }
    elseif ($line.Trim() -eq "GlobalSection(ProjectConfigurationPlatforms) = postSolution") {
        $solutionFileContent.AppendLine($line) > $null
        $solutionFileContent.AppendLine("		{$projectId}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$projectId}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$projectId}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$projectId}.Release|Any CPU.Build.0 = Release|Any CPU") > $null
    }
    else {
        $solutionFileContent.AppendLine($line) > $null
    }
}

$reader.Close()

Set-Content -Path $solutionFilePath -Value $solutionFileContent.ToString() -Force

$tsConfigFile = "{
  `"compileOnSave`": true,
  `"compilerOptions`": {
    `"module`": `"commonjs`",
    `"target`": `"es5`",
    `"sourceMap`": true
  },
  `"exclude`": [
    `"node_modules`",
    `"bin`"
  ]
}"

$tsConfigFilePath = "$themePath\tsconfig.json"
Set-Content -Path $tsConfigFilePath -Value $tsConfigFile -Force

$projectFile = "<Project Sdk=`"Microsoft.NET.Sdk.Web`">
  <PropertyGroup>
    <TypeScriptToolsVersion>3.7</TypeScriptToolsVersion>
    <OutputType>Library</OutputType>
    <OutputPath>bin\</OutputPath>
    <TargetFramework>net48</TargetFramework>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
  <PropertyGroup Condition=`"'`$(Configuration)|`$(Platform)' == 'Debug|AnyCPU' `">
    <TypeScriptJSXEmit>None</TypeScriptJSXEmit>
    <TypeScriptModuleKind>CommonJS</TypeScriptModuleKind>
    <TypeScriptOutFile />
    <TypeScriptOutDir />
    <TypeScriptNoEmitOnError>False</TypeScriptNoEmitOnError>
    <TypeScriptMapRoot />
    <TypeScriptSourceRoot />
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=`"Microsoft.CSharp`" Version=`"4.7.0`" />
    <PackageReference Include=`"System.ComponentModel.Annotations`" Version=`"5.0.0`" />
    <PackageReference Include=`"System.Data.DataSetExtensions`" Version=`"4.5.0`" />
  </ItemGroup>
</Project>"

$csProject = "$themePath\$themeName.csproj"

Add-Content $csProject $projectFile
