$psversionTable | out-string | write-host

. "$PSScriptRoot\..\src\targets\tests\utils\MSBuild.ps1"

properties {
  $buildConfiguration = "Release"
  $testFilePattern = "$PSScriptRoot\..\src\targets\tests"
  $toolsDir = "$PSScriptRoot\.tools"
  $tasksSolutionPath = "$PSScriptRoot\..\src\tasks\GerwimFeiken.Publishing.sln"
  $artifactDir = "$PSScriptRoot\..\bin"

  if ($env:CI) {
    $xunitPath = "$env:xunit20\xunit.console"
    $packageVersion = $env:APPVEYOR_BUILD_VERSION
  } else {
    $xunitPath = "$PSScriptRoot\..\src\tasks\packages\xunit.runner.console.2.4.1\tools\net461\xunit.console.exe"

    if (-not $packageVersion) {
      throw "packageVersion must be provided"
    }
  }

  $nugetPath = "$toolsDir\nuget.exe"
}

task default -depends Test,Pack

task GetNuget {
  if (-not (Test-Path $nugetPath)) {
    if (-not (Test-Path $toolsDir)) {
        mkdir $toolsDir | Out-Null
    }

    Invoke-WebRequest -OutFile "$toolsDir\nuget.exe" -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
  }
}

task Restore -depends GetNuGet {
  if ($env:CI) {
    return
  }
  & $nugetPath help # To show version
  & $nugetPath restore $tasksSolutionPath
}

task BuildTasks -depends Restore {
  & (Get-MSBuildExePath) "$PSScriptRoot\..\src\tasks\GerwimFeiken.Publishing.sln" "/P:Configuration=$buildConfiguration" "/m" "/v:m"
}

task TestTasks -depends CreateArtifactDir,BuildTasks {
  $testResultsPath = Join-Path $artifactDir tasks-test-results.xml
  & $xunitPath "$PSScriptRoot\..\src\tasks\GerwimFeiken.Publishing.Tasks.Tests\bin\$buildConfiguration\GerwimFeiken.Publishing.Tasks.Tests.dll" -nunit $testResultsPath
}

task Test -depends TestTasks

task CreateArtifactDir {
  mkdir $artifactDir -Force
}

function Set-NuspecReleaseNotes([string]$path, [string]$releaseNotes)
{
  $xml = New-Object System.Xml.XmlDocument
  $xml.PreserveWhitespace = $true
  $xml.Load($path)

  if (([string]$xml.package.metadata.releaseNotes) -eq $releaseNotes)
  {
    Write-Warning "Release notes have not been updated"
  }

  $nsmgr = New-Object System.Xml.XmlNamespaceManager $xml.NameTable
  $nsmgr.AddNamespace('nuspec','http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd')

  $xml.SelectSingleNode("//nuspec:package/nuspec:metadata/nuspec:releaseNotes", $nsmgr).InnerText = $releaseNotes
  $xml.Save($path)
}

task UpdateReleaseNotes {
  $releaseNotes = Get-Content "$PSScriptRoot\..\CHANGELOG"

  Get-ChildItem "$PSScriptRoot\..\src\*.nuspec" | Foreach-Object {
    Set-NuspecReleaseNotes $_.FullName $releaseNotes
  }
}

task Pack -depends GetNuget,BuildTasks,CreateArtifactDir {
  Get-ChildItem "$PSScriptRoot\..\src\*.nuspec" | Foreach-Object {
    & $nugetPath pack $_.FullName -Version $packageVersion -OutputDirectory $artifactDir
  }
}

task Push -depends Pack {
  Get-ChildItem "$PSScriptRoot\..\bin\*.$packageVersion.nupkg" | Foreach-Object {
    & $nugetPath push $_.FullName -ApiKey $nugetApiKey -Source "https://api.nuget.org/v3/index.json"
  }
}
