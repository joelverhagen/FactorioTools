[CmdletBinding(DefaultParameterSetName = "AllSteps")]
param (
    [Parameter(ParameterSetName = "SpecificSteps")]
    [switch] $CopyCoreSystem,

    [Parameter(ParameterSetName = "SpecificSteps")]
    [switch]$CompileFactorioTools
)

$All = $PsCmdlet.ParameterSetName -eq "AllSteps"

$repoDir = Resolve-Path (Join-Path $PSScriptRoot "../..")
$compilerDir = Join-Path $repoDir "submodules/CSharp.lua/CSharp.lua.Launcher"
$baseOutputDir = Join-Path $repoDir "src/lua"

function Copy-CoreSystem() {
    Write-Host "Copying CoreSystem Lua"
    $coreSystemDestDir = Join-Path $baseOutputDir "CoreSystem"
    if (Test-Path $coreSystemDestDir) {
        Remove-Item $coreSystemDestDir -Recurse -Force
        New-Item $coreSystemDestDir -ItemType Directory | Out-Null
    }

    $coreSystemSrcDir = Join-Path $repoDir "submodules/CSharp.lua/CSharp.lua/CoreSystem.Lua/CoreSystem"
    $coreSystemDependencies = Get-Content (Join-Path $baseOutputDir "CoreSystem.lua") | `
        ForEach-Object { if ($_ -match "^ *load\(`"([^`"]+)`"\)") { $Matches[1] } }
    foreach ($dependency in $coreSystemDependencies) {
        $srcFile = $dependency.Replace(".", "/") + ".lua"
        $srcPath = Join-Path $coreSystemSrcDir $srcFile
        $destPath = Join-Path $coreSystemDestDir $srcFile
        $destDir = Split-Path -Path $destPath
        if (!(Test-Path $destDir)) {
            New-Item $destDir -Type Directory -Force | Out-Null
        }
        Write-Host "  Copying $dependency"
        Copy-Item $srcPath $destPath -Recurse
    }
}

function Publish-CompiledLua($projectDir, $referenceNames, $filesFirst) {
    $projectDir = Join-Path $repoDir $projectDir
    $projectName = (Get-ChildItem $projectDir -Filter "*.csproj").BaseName

    $outputDir = Join-Path $baseOutputDir $projectName

    if ($All) {
        Write-Host "Building $projectName"
        dotnet build --configuration Release $projectDir
    }

    Write-Host "Compiling $projectName to $outputDir"

    if (Test-Path $outputDir) {
        Write-Host "  Removing $outputDir"
        Remove-Item $outputDir -Recurse -Force
    }

    $references = @()
    if ($referenceNames) {
        $binDir = Join-Path $projectDir "bin/Release"
        foreach ($referenceName in $referenceNames) {
            $referencePath = Get-ChildItem $binDir -Recurse -Filter $referenceName
            Write-Host "  Reference: $($referencePath.FullName)"
            # The "!" suffix makes CSharp.lua consider the library a Lua module
            $references += @($referencePath.FullName + "!")
        }
    }

    $childDirectories = Get-ChildItem $projectDir -Exclude @("bin", "obj") -Directory
    $childFiles = Get-ChildItem $projectDir "*.cs"
    $recursiveFiles = $childDirectories | Get-ChildItem -Filter "*.cs" -Recurse
    $files = $($childFiles; $recursiveFiles)
    Write-Host "  File count: $($files.Count)"

    $libArg = if ($references) { @("-l"; $references -join ";") } else { @() }
    $sourceList = ($files | ForEach-Object { $_.FullName } | Sort-Object) -join ";"
    
    dotnet run --no-build --configuration Release --project $compilerDir -- -c -p -csc "-define:ENABLE_GRID_TOSTRING" @libArg -s $sourceList -d $outputDir
    if ($LASTEXITCODE -ne 0) {
        throw "The CSharp.lua compiler failed with exit code $LASTEXITCODE."
    }

    # workaround for https://github.com/yanghuan/CSharp.lua/issues/492
    if ($filesFirst) {
        $manifestPath = Join-Path $outputDir "manifest.lua"
        $manifest = Get-Content $manifestPath -Raw
        $startMarker = "files = {"
        $filesStartIndex = $manifest.IndexOf($startMarker) + $startMarker.Length
        $filesEndIndex = $manifest.IndexOf("}", $filesStartIndex)
        $originalFiles = $manifest.Substring($filesStartIndex, $filesEndIndex - $filesStartIndex).Split(",") | `
            ForEach-Object { $_.Trim().Trim('"') }
        $modifiedFiles = @($filesFirst)
        foreach ($file in $originalFiles) {
            if ($modifiedFiles -notcontains $file) {
                $modifiedFiles += @($file)
            }
        }
        $modifiedFiles = ($modifiedFiles | ForEach-Object { [Environment]::NewLine + '      "' + $_ + '"' }) -join ","

        $newManifest = $manifest.Substring(0, $filesStartIndex) + $modifiedFiles + [Environment]::NewLine + '    ' + $manifest.Substring($filesEndIndex)
        $newManifest | Out-File $manifestPath -Encoding utf8
    }
}

if ($All) {
    Write-Host "Building the CSharp.lua compiler"
    dotnet build --configuration Release $compilerDir
}

if ($All -or $CopyCoreSystem) {
    Copy-CoreSystem
}

if ($All) {
    Publish-CompiledLua "submodules/delaunator-sharp/DelaunatorSharp"
}

if ($All) {
    Publish-CompiledLua "submodules/FluteSharp/src/FluteSharp"
}

if ($All -or $CompileFactorioTools) {
    Publish-CompiledLua "src/FactorioTools" @("DelaunatorSharp.dll", "Knapcode.FluteSharp.dll") @("LocationIntDictionary")
}
