# M1 冒烟脚本：无界面清单与桌面语言/菜单钩子
#
# 用法：pwsh -File scripts/qa/smoke-m1.ps1 [-Configuration Debug]
# 前置：先 dotnet build src/ImageLyre.slnx；脚本不负责构建。
# 判据：三条各输出 PASS，任一失败以退出码 1 结束，失败原因打印到 stderr。

param(
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$app = Join-Path $repoRoot "src/App/ImageLyre.App/bin/$Configuration/net10.0/ImageLyre.App.exe"

if (-not (Test-Path $app)) {
    [Console]::Error.WriteLine("找不到 $app；先运行 dotnet build src/ImageLyre.slnx。")
    exit 1
}

function Invoke-App {
    param(
        [string[]]$Arguments,
        [hashtable]$Environment
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $app
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $startInfo.StandardErrorEncoding = [System.Text.Encoding]::UTF8

    foreach ($argument in $Arguments) {
        $startInfo.ArgumentList.Add($argument)
    }

    if ($Environment) {
        foreach ($key in $Environment.Keys) {
            $startInfo.Environment[$key] = [string]$Environment[$key]
        }
    }

    $process = [System.Diagnostics.Process]::Start($startInfo)
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    return [pscustomobject]@{
        ExitCode = $process.ExitCode
        StdOut   = $stdout
        StdErr   = $stderr
    }
}

$failures = [System.Collections.Generic.List[string]]::new()
$passed = 0

function Assert-Contains {
    param(
        [string]$Name,
        [string]$Text,
        [string]$Expected
    )

    if ($Text.Contains($Expected, [System.StringComparison]::Ordinal)) {
        Write-Host "PASS $Name：$Expected"
        $script:passed++
        return
    }

    $failures.Add("$Name 缺少「$Expected」")
    Write-Host "FAIL $Name：缺少「$Expected」"
}

# 1. 无界面宿主：Feature 被过滤，stdout 只有清单，退出码 0。
$headless = Invoke-App -Arguments @('--plugins') -Environment @{}
if ($headless.ExitCode -ne 0) {
    $failures.Add("无界面 --plugins 退出码为 $($headless.ExitCode)，应为 0")
}
Assert-Contains -Name '无界面 Feature 过滤' -Text $headless.StdOut -Expected 'Feature 0 个'
Assert-Contains -Name '无界面扫描路线' -Text $headless.StdErr -Expected 'HasUiRoute=False'

# 2. 桌面英文：窗口打开后展开菜单并切到 en-US。
$english = Invoke-App -Arguments @() -Environment @{
    LYRE_SMOKE_CLOSE_MS = '2500'
    LYRE_SMOKE_OPEN_MENU = '1'
    LYRE_SMOKE_LANGUAGE = 'en-US'
}
if ($english.ExitCode -ne 0) {
    $failures.Add("桌面英文冒烟退出码为 $($english.ExitCode)，应为 0")
}
Assert-Contains -Name '桌面菜单展开' -Text $english.StdErr -Expected '冒烟：菜单子项已展开。'
Assert-Contains -Name '桌面英文切换' -Text $english.StdErr -Expected '冒烟：语言已切换为 en-US'
Assert-Contains -Name '关于功能注册' -Text $english.StdErr -Expected '关于 Feature 已启动，帮助菜单已注册。'

# 3. 桌面中文：再切回 zh-CN，证明切换可逆且不需要重启。
$chinese = Invoke-App -Arguments @() -Environment @{
    LYRE_SMOKE_CLOSE_MS = '2500'
    LYRE_SMOKE_LANGUAGE = 'zh-CN'
}
if ($chinese.ExitCode -ne 0) {
    $failures.Add("桌面中文冒烟退出码为 $($chinese.ExitCode)，应为 0")
}
Assert-Contains -Name '桌面中文切换' -Text $chinese.StdErr -Expected '冒烟：语言已切换为 zh-CN'

if ($failures.Count -gt 0) {
    Write-Host ''
    Write-Host "结果：$($failures.Count) 项失败。"
    foreach ($failure in $failures) {
        [Console]::Error.WriteLine($failure)
    }

    exit 1
}

Write-Host ''
Write-Host "结果：$passed 项全部 PASS。"
exit 0
