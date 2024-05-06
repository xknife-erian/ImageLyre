<#
.SYNOPSIS
    一个命令完成还原、构建与测试。

.DESCRIPTION
    本地循环的单一入口，见 docs/design/lukan/DESIGN-0002-cangming-engineering-base.md §2.4。
    任何一步失败即以非零码退出，不吞掉后面的步骤。

.PARAMETER SkipTest
    跳过测试，只做还原与构建，用于纯编译期排错。

.EXAMPLE
    pwsh ./build.ps1
    pwsh ./build.ps1 -SkipTest
#>
[CmdletBinding()]
param(
    [switch] $SkipTest
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $root 'src/ImageLyre.slnx'

Write-Host '== restore ==' -ForegroundColor Cyan
dotnet restore $solution
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host '== build ==' -ForegroundColor Cyan
dotnet build $solution -c Debug --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipTest) {
    Write-Host '== test ==' -ForegroundColor Cyan
    dotnet test $solution -c Debug --no-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host '== done ==' -ForegroundColor Green
