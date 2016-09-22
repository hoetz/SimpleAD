function Get-VsVersion ()
{
    if ($env:VS140COMNTOOLS -ne $null)
    {
        " /property:VisualStudioVersion=14.0"
    }
    elseif ($env:VS120COMNTOOLS -ne $null)
    {
        " /property:VisualStudioVersion=12.0"
    }
}

function Get-BuildConfiguration()
{
    $env:config='Release';
    " /p:Configuration={0}" -f $env:config
}

function Get-CurrentPackageVersion($pathToAssembly)
{
    [System.Diagnostics.FileVersionInfo]$fv=[System.Diagnostics.FileVersionInfo]::GetVersionInfo($pathToAssembly)
    "{0}.{1}.{2}" -f ($fv.FileMajorPart,$fv.FileMinorPart,$fv.FileBuildPart)
}

$env:nuget="nuget"

Start-Process -NoNewWindow -Wait -FilePath ${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild -ArgumentList ('src\SimpleAD\SimpleAD.csproj', (Get-VsVersion),(Get-BuildConfiguration))

$env:version=Get-CurrentPackageVersion (Resolve-Path -Path (".\src\SimpleAD\bin\{0}\SimpleAD.dll" -f $env:config) -Relative)

Start-Process -NoNewWindow -Wait -FilePath $env:nuget -ArgumentList ('pack','NugetSpecs\SimpleAD.nuspec', ' -NoPackageAnalysis', ' -verbosity detailed',' -o Build', ('-Version {0}' -f $env:version),' -p',('Configuration={0}' -f $env:config))





