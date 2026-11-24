<#
.SYNOPSIS
    M1 插件骨架的只读质量检查脚本。

.DESCRIPTION
    在完整树上检查七类约束：引用方向、机制层词表、通配引用计数、工程 README、平台专属符号、壳契约纯度与 Bootstrap 无 UI 引用。
    每项打印 PASS/FAIL 及一行原因；任一项失败时退出码为 1。脚本只读，不修改任何文件。

.NOTES
    运行方式：pwsh scripts/qa/verify-m1.ps1
    退出码：0 表示七项全部 PASS；1 表示至少一项 FAIL。
    新增检查项时同步更新本注释、本脚本与 scripts/qa/README.md。
    ponytail：七类检查是 M1 的最小集；G2 之后扩到依赖树与清单校验。
#>

#Requires -Version 7.0

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

function New-CheckResult {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][bool]$Pass,
        [Parameter(Mandatory)][string]$Reason,
        [string[]]$Details = @()
    )

    [pscustomobject]@{
        Name = $Name
        Pass = $Pass
        Reason = $Reason
        Details = $Details
    }
}

function Get-RepoRelativePath {
    param([Parameter(Mandatory)][string]$Path)

    [System.IO.Path]::GetRelativePath($script:repoRoot, $Path).Replace('\', '/')
}

function Get-ProjectReferenceIncludes {
    param([Parameter(Mandatory)][string]$ProjectPath)

    if (-not (Test-Path -LiteralPath $ProjectPath)) {
        return @()
    }

    $text = Get-Content -LiteralPath $ProjectPath -Raw
    $matches = [regex]::Matches(
        $text,
        '<ProjectReference\b[^>]*\bInclude\s*=\s*"([^"]+)"',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    return @($matches | ForEach-Object { $_.Groups[1].Value })
}

function Get-PackageReferenceIncludes {
    param([Parameter(Mandatory)][string]$ProjectPath)

    if (-not (Test-Path -LiteralPath $ProjectPath)) {
        return @()
    }

    $text = Get-Content -LiteralPath $ProjectPath -Raw
    $matches = [regex]::Matches(
        $text,
        '<PackageReference\b[^>]*\bInclude\s*=\s*"([^"]+)"',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    return @($matches | ForEach-Object { $_.Groups[1].Value })
}

function Remove-CSharpComments {
    param([AllowNull()][string]$Text)

    if ($null -eq $Text) {
        return ''
    }

    $builder = [System.Text.StringBuilder]::new($Text.Length)
    $i = 0
    $state = 'Normal'

    while ($i -lt $Text.Length) {
        $ch = $Text[$i]
        $next = if ($i + 1 -lt $Text.Length) { $Text[$i + 1] } else { [char]0 }

        if ($state -eq 'Normal') {
            if ($ch -eq '/' -and $next -eq '/') {
                [void]$builder.Append('  ')
                $i += 2
                $state = 'LineComment'
                continue
            }
            if ($ch -eq '/' -and $next -eq '*') {
                [void]$builder.Append('  ')
                $i += 2
                $state = 'BlockComment'
                continue
            }
            if ($ch -eq '@' -and $next -eq '"') {
                [void]$builder.Append('@"')
                $i += 2
                $state = 'VerbatimString'
                continue
            }
            if ($ch -eq '"') {
                [void]$builder.Append('"')
                $i++
                $state = 'String'
                continue
            }
            if ($ch -eq "'") {
                [void]$builder.Append("'")
                $i++
                $state = 'Char'
                continue
            }

            [void]$builder.Append($ch)
            $i++
            continue
        }

        if ($state -eq 'LineComment') {
            if ($ch -eq "`r" -or $ch -eq "`n") {
                [void]$builder.Append($ch)
                $state = 'Normal'
            }
            else {
                [void]$builder.Append(' ')
            }

            $i++
            continue
        }

        if ($state -eq 'BlockComment') {
            if ($ch -eq '*' -and $next -eq '/') {
                [void]$builder.Append('  ')
                $i += 2
                $state = 'Normal'
                continue
            }

            if ($ch -eq "`r" -or $ch -eq "`n") {
                [void]$builder.Append($ch)
            }
            else {
                [void]$builder.Append(' ')
            }

            $i++
            continue
        }

        if ($state -eq 'String') {
            [void]$builder.Append($ch)

            if ($ch -eq '\') {
                if ($i + 1 -lt $Text.Length) {
                    [void]$builder.Append($Text[$i + 1])
                    $i += 2
                    continue
                }
            }

            if ($ch -eq '"') {
                $state = 'Normal'
            }

            $i++
            continue
        }

        if ($state -eq 'VerbatimString') {
            [void]$builder.Append($ch)

            if ($ch -eq '"' -and $next -eq '"') {
                [void]$builder.Append($next)
                $i += 2
                continue
            }

            if ($ch -eq '"') {
                $state = 'Normal'
            }

            $i++
            continue
        }

        if ($state -eq 'Char') {
            [void]$builder.Append($ch)

            if ($ch -eq '\') {
                if ($i + 1 -lt $Text.Length) {
                    [void]$builder.Append($Text[$i + 1])
                    $i += 2
                    continue
                }
            }

            if ($ch -eq "'") {
                $state = 'Normal'
            }

            $i++
        }
    }

    return $builder.ToString()
}

function Test-ReferenceDirection {
    $details = [System.Collections.Generic.List[string]]::new()
    $pluginRoot = Join-Path $script:repoRoot 'src\Plugins'
    $pluginProjects = @(
        Get-ChildItem -LiteralPath $pluginRoot -Recurse -Filter '*.csproj' -File |
            Where-Object { $_.FullName -notmatch '[\\/]src[\\/]cangming[\\/]' })

    if ($pluginProjects.Count -eq 0) {
        return New-CheckResult -Name '1. 引用方向' -Pass $false -Reason '未找到 src/Plugins 下的插件工程。'
    }

    foreach ($project in $pluginProjects) {
        foreach ($reference in @(Get-ProjectReferenceIncludes -ProjectPath $project.FullName)) {
            if ($reference -match '(?i)ImageLyre\.(Module|Feature)\.') {
                $details.Add("$(Get-RepoRelativePath $project.FullName) -> $reference")
            }
        }
    }

    $contractRoot = Join-Path $script:repoRoot 'src\Infrastructure\Contracts'
    $contractProjects = @(Get-ChildItem -LiteralPath $contractRoot -Recurse -Filter '*.csproj' -File)
    if ($contractProjects.Count -eq 0) {
        return New-CheckResult -Name '1. 引用方向' -Pass $false -Reason '未找到 src/Infrastructure/Contracts 下的契约工程。'
    }

    foreach ($project in $contractProjects) {
        $relative = Get-RepoRelativePath $project.FullName
        foreach ($package in @(Get-PackageReferenceIncludes -ProjectPath $project.FullName)) {
            if ($package -match '(?i)^(Avalonia|AtomUI|SkiaSharp|Cangming\.)') {
                $details.Add("$relative -> PackageReference $package")
            }
        }

        foreach ($reference in @(Get-ProjectReferenceIncludes -ProjectPath $project.FullName)) {
            if ($reference -match '(?i)ImageLyre\.(Module|Feature)\.') {
                $details.Add("$relative -> ProjectReference $reference")
            }
        }
    }

    if ($details.Count -gt 0) {
        return New-CheckResult -Name '1. 引用方向' -Pass $false `
            -Reason '发现插件反向引用或契约工程引用 UI/机制包/插件。' -Details @($details)
    }

    return New-CheckResult -Name '1. 引用方向' -Pass $true `
        -Reason '插件工程未引用 Module/Feature；三个契约工程无 UI、SkiaSharp、cangming 包或插件引用。'
}

function Test-MechanismVocabulary {
    $details = [System.Collections.Generic.List[string]]::new()
    $roots = @(
        (Join-Path $script:repoRoot 'src\App\ImageLyre.Bootstrap'),
        (Join-Path $script:repoRoot 'src\App\ImageLyre.App'))
    $files = @(
        foreach ($root in $roots) {
            if (Test-Path -LiteralPath $root) {
                Get-ChildItem -LiteralPath $root -Recurse -Filter '*.cs' -File |
                    Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' }
            }
        })

    if ($files.Count -eq 0) {
        return New-CheckResult -Name '2. 机制层词表' -Pass $false -Reason '未找到 App/Bootstrap 的 .cs 源文件。'
    }

    $chineseTerms = @('图像', '选区', '测量', '分析', '通道', '像素', '标尺', '校准')
    $asciiTerms = @('image', 'ROI', 'measure', 'analysis', 'pixel', 'TIFF')

    foreach ($file in $files) {
        $clean = Remove-CSharpComments -Text (Get-Content -LiteralPath $file.FullName -Raw)
        $lines = $clean -split '\r?\n'
        for ($lineNumber = 0; $lineNumber -lt $lines.Length; $lineNumber++) {
            $line = $lines[$lineNumber]
            foreach ($term in $chineseTerms) {
                if ($line.Contains($term)) {
                    $details.Add("$(Get-RepoRelativePath $file.FullName):$($lineNumber + 1): $term")
                }
            }

            foreach ($term in $asciiTerms) {
                $escaped = [regex]::Escape($term)
                if ($line -match "(?i)(?<![A-Za-z0-9_])$escaped(?![A-Za-z0-9_])") {
                    $details.Add("$(Get-RepoRelativePath $file.FullName):$($lineNumber + 1): $term")
                }
            }
        }
    }

    if ($details.Count -gt 0) {
        return New-CheckResult -Name '2. 机制层词表' -Pass $false `
            -Reason 'App/Bootstrap 的标识符或字符串命中机制层禁用词。' -Details @($details)
    }

    return New-CheckResult -Name '2. 机制层词表' -Pass $true `
        -Reason '剥离注释后，App/Bootstrap 未命中图像、测量、像素等禁用词。'
}

function Test-WildcardReferenceCount {
    $appProject = Join-Path $script:repoRoot 'src\App\ImageLyre.App\ImageLyre.App.csproj'
    if (-not (Test-Path -LiteralPath $appProject)) {
        return New-CheckResult -Name '3. 通配引用计数' -Pass $false -Reason '未找到 ImageLyre.App.csproj。'
    }

    $references = @(Get-ProjectReferenceIncludes -ProjectPath $appProject)
    $named = @(
        $references | Where-Object {
            ($_ -match '(?i)ImageLyre\.(Module|Feature)\.') -or
            (($_ -match '(?i)Plugins') -and ($_ -notmatch '\*'))
        })
    $wildcards = @(
        $references | Where-Object {
            ($_ -match '(?i)Plugins') -and ($_ -match '\*')
        })

    $details = @()
    if ($named.Count -gt 0) {
        $details += @($named | ForEach-Object { "按名引用：$_" })
    }
    if ($wildcards.Count -ne 1) {
        $details += "Plugins 通配引用数量：$($wildcards.Count)，预期 1"
    }

    if ($details.Count -gt 0) {
        return New-CheckResult -Name '3. 通配引用计数' -Pass $false `
            -Reason 'App 存在按名插件引用，或 Plugins 通配引用不是恰好 1 条。' -Details $details
    }

    return New-CheckResult -Name '3. 通配引用计数' -Pass $true `
        -Reason 'App 按名引用插件 0 条，指向 Plugins 的通配 ProjectReference 恰为 1 条。'
}

function Test-ReadmePresence {
    $projectFiles = @(
        Get-ChildItem -LiteralPath (Join-Path $script:repoRoot 'src'), (Join-Path $script:repoRoot 'tests') `
            -Recurse -Filter '*.csproj' -File |
            Where-Object { $_.FullName -notmatch '[\\/]src[\\/]cangming[\\/]' })

    if ($projectFiles.Count -eq 0) {
        return New-CheckResult -Name '4. 工程 README' -Pass $false -Reason '未找到 src/tests 下的工程文件。'
    }

    $projectDirs = @($projectFiles | ForEach-Object { $_.DirectoryName } | Sort-Object -Unique)
    $missing = @(
        $projectDirs | Where-Object {
            -not (Test-Path -LiteralPath (Join-Path $_ 'README.md'))
        } | ForEach-Object { Get-RepoRelativePath $_ })

    if ($missing.Count -gt 0) {
        return New-CheckResult -Name '4. 工程 README' -Pass $false `
            -Reason '以下含 csproj 的目录缺少 README.md。' -Details $missing
    }

    return New-CheckResult -Name '4. 工程 README' -Pass $true `
        -Reason "$($projectDirs.Count) 个含 csproj 的工程目录都有 README.md。"
}

function Test-PlatformSymbols {
    $details = [System.Collections.Generic.List[string]]::new()
    $roots = @(
        (Join-Path $script:repoRoot 'src\Infrastructure\Contracts'),
        (Join-Path $script:repoRoot 'src\Plugins'))
    $files = @(
        foreach ($root in $roots) {
            if (Test-Path -LiteralPath $root) {
                Get-ChildItem -LiteralPath $root -Recurse -Filter '*.cs' -File |
                    Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' }
            }
        })

    if ($files.Count -eq 0) {
        return New-CheckResult -Name '5. 平台符号' -Pass $false -Reason '未找到契约、领域或插件源码。'
    }

    $patterns = @(
        [pscustomobject]@{ Name = 'Microsoft.Win32.Registry'; Pattern = 'Microsoft\.Win32\.Registry' },
        [pscustomobject]@{ Name = 'DllImport'; Pattern = '\bDllImport\b' },
        [pscustomobject]@{ Name = 'System.Windows.Forms'; Pattern = 'System\.Windows\.Forms' },
        [pscustomobject]@{ Name = '盘符字面量'; Pattern = '["'']\s*[A-Za-z]:\\' },
        [pscustomobject]@{ Name = '反斜杠拼接'; Pattern = '\+\s*"\\\\"|\+\s*@"\\"' })

    foreach ($file in $files) {
        $clean = Remove-CSharpComments -Text (Get-Content -LiteralPath $file.FullName -Raw)
        $lines = $clean -split '\r?\n'
        for ($lineNumber = 0; $lineNumber -lt $lines.Length; $lineNumber++) {
            $line = $lines[$lineNumber]
            foreach ($pattern in $patterns) {
                if ($line -match $pattern.Pattern) {
                    $details.Add("$(Get-RepoRelativePath $file.FullName):$($lineNumber + 1): $($pattern.Name)")
                }
            }
        }
    }

    if ($details.Count -gt 0) {
        return New-CheckResult -Name '5. 平台符号' -Pass $false `
            -Reason '契约、领域或插件源码命中平台专属符号或路径拼接。' -Details @($details)
    }

    return New-CheckResult -Name '5. 平台符号' -Pass $true `
        -Reason '契约、领域与插件源码未命中注册表、DllImport、WinForms、盘符字面量或反斜杠拼接。'
}

function Test-ShellContractPurity {
    $project = Join-Path $script:repoRoot 'src\Infrastructure\Contracts\ImageLyre.Shell.Contracts\ImageLyre.Shell.Contracts.csproj'
    if (-not (Test-Path -LiteralPath $project)) {
        return New-CheckResult -Name '6. 壳契约纯度' -Pass $false -Reason '未找到 ImageLyre.Shell.Contracts.csproj。'
    }

    $references = @(Get-ProjectReferenceIncludes -ProjectPath $project)
    $packages = @(Get-PackageReferenceIncludes -ProjectPath $project)
    $details = @()
    if ($references.Count -ne 1) {
        $details += "ProjectReference 数量：$($references.Count)，预期 1"
    }
    if ($references.Count -eq 1 -and $references[0] -notmatch '(?i)ImageLyre\.Contracts\.csproj$') {
        $details += "ProjectReference 指向：$($references[0])，预期 ImageLyre.Contracts.csproj"
    }
    if ($packages.Count -gt 0) {
        $details += @($packages | ForEach-Object { "PackageReference：$_" })
    }

    if ($details.Count -gt 0) {
        return New-CheckResult -Name '6. 壳契约纯度' -Pass $false `
            -Reason 'Shell.Contracts 不是只依赖 ImageLyre.Contracts，或存在 PackageReference。' -Details $details
    }

    return New-CheckResult -Name '6. 壳契约纯度' -Pass $true `
        -Reason 'Shell.Contracts 只有一条到 ImageLyre.Contracts 的 ProjectReference，且无 PackageReference。'
}

function Test-BootstrapUiFree {
    $project = Join-Path $script:repoRoot 'src\App\ImageLyre.Bootstrap\ImageLyre.Bootstrap.csproj'
    if (-not (Test-Path -LiteralPath $project)) {
        return New-CheckResult -Name '7. Bootstrap 无 UI 引用' -Pass $false -Reason '未找到 ImageLyre.Bootstrap.csproj。'
    }

    $details = [System.Collections.Generic.List[string]]::new()
    foreach ($reference in @(Get-ProjectReferenceIncludes -ProjectPath $project)) {
        if ($reference -match '(?i)ImageLyre\.Presentation\.') {
            $details.Add("ProjectReference：$reference")
        }
    }

    foreach ($package in @(Get-PackageReferenceIncludes -ProjectPath $project)) {
        if ($package -match '(?i)^(Avalonia|AtomUI)') {
            $details.Add("PackageReference：$package")
        }
    }

    if ($details.Count -gt 0) {
        return New-CheckResult -Name '7. Bootstrap 无 UI 引用' -Pass $false `
            -Reason 'Bootstrap 引用了 Avalonia/AtomUI 或 Presentation 工程。' -Details @($details)
    }

    return New-CheckResult -Name '7. Bootstrap 无 UI 引用' -Pass $true `
        -Reason 'Bootstrap 的 ProjectReference 未指向 ImageLyre.Presentation.*，PackageReference 无 Avalonia/AtomUI。'
}

try {
    $checks = @(
        (Test-ReferenceDirection),
        (Test-MechanismVocabulary),
        (Test-WildcardReferenceCount),
        (Test-ReadmePresence),
        (Test-PlatformSymbols),
        (Test-ShellContractPurity),
        (Test-BootstrapUiFree))
}
catch {
    Write-Host "FAIL 脚本执行异常：$($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$failed = 0
foreach ($check in $checks) {
    if ($check.Pass) {
        Write-Host "PASS $($check.Name)：$($check.Reason)" -ForegroundColor Green
    }
    else {
        $failed++
        Write-Host "FAIL $($check.Name)：$($check.Reason)" -ForegroundColor Red
        foreach ($detail in @($check.Details)) {
            Write-Host "     $detail" -ForegroundColor Red
        }
    }
}

if ($failed -gt 0) {
    Write-Host "结果：$failed 项失败。" -ForegroundColor Red
    exit 1
}

Write-Host "结果：$($checks.Count) 项全部 PASS。" -ForegroundColor Green
exit 0
