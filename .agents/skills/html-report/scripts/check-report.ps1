#requires -Version 7.0
<#
.SYNOPSIS
    检查 html-report 产出的单文件 HTML：编码、标签配对、锚点、相对链接、CDN、条目数。

.DESCRIPTION
    只读检查，不修改文件。默认把 ERROR 视为失败；WARN 需人工确认，加 -Strict 时 WARN 也失败。

.PARAMETER Html
    待检查的 HTML 文件路径。

.PARAMETER Markdown
    对应的 Markdown 源文件；提供时比对台账编号与 P1/P2 数量。

.PARAMETER Strict
    把 WARN 也视为失败。

.EXAMPLE
    pwsh .agents/skills/html-report/scripts/check-report.ps1 `
      -Html docs/analyses/lukan/2026-11-16_x.html `
      -Markdown docs/analyses/lukan/2026-11-16_x.md
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Html,
    [string]$Markdown,
    [switch]$Strict
)

$ErrorActionPreference = 'Stop'
$errors = [System.Collections.Generic.List[string]]::new()
$warns  = [System.Collections.Generic.List[string]]::new()

function Add-Error([string]$message) { $errors.Add($message) }
function Add-Warn([string]$message)  { $warns.Add($message) }

if (-not (Test-Path -LiteralPath $Html)) {
    Write-Host "ERROR 找不到 HTML：$Html" -ForegroundColor Red
    exit 1
}

$htmlPath = (Resolve-Path -LiteralPath $Html).Path
$htmlDir  = Split-Path -Parent $htmlPath
$bytes    = [System.IO.File]::ReadAllBytes($htmlPath)

# 1. 编码
$utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
try {
    $text = $utf8Strict.GetString($bytes)
} catch {
    Add-Error '文件不是合法 UTF-8，请转码后重试'
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)
}
if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
    Add-Warn '文件带 UTF-8 BOM，建议去掉'
}
# 结构检查忽略注释里的示例代码，避免模板自带的示例被误判
$struct = [regex]::Replace($text, '(?s)<!--.*?-->', '')

# 2. 标签配对（只查有明确闭合语义的容器标签）
$pairTags = @(
    'div','section','table','thead','tbody','tr','td','th','pre','main','aside','nav',
    'span','code','button','a','h2','h3','h4','h5','ul','ol','li','p','footer','header','kbd'
)
foreach ($tag in $pairTags) {
    $open  = ([regex]::Matches($struct, "<$tag(\s|>)")).Count
    $close = ([regex]::Matches($struct, "</$tag>")).Count
    if ($open -ne $close) {
        Add-Error "标签 <$tag> 开合不一致：$open / $close"
    }
}

# 3. 锚点：href 必须有 id 或 data-id 承接（台账行由脚本补 id）
$idSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($m in [regex]::Matches($struct, '\bid="([^"]+)"'))  { [void]$idSet.Add($m.Groups[1].Value) }
foreach ($m in [regex]::Matches($struct, '\bdata-id="([^"]+)"')) { [void]$idSet.Add($m.Groups[1].Value) }

foreach ($m in [regex]::Matches($struct, '\bhref="#([^"]+)"')) {
    $target = $m.Groups[1].Value
    if ($target -notlike '*{{*' -and -not $idSet.Contains($target)) {
        Add-Error "锚点 #$target 没有对应的 id/data-id"
    }
}

# 4. 相对链接：除锚点、http(s)、mailto、占位符外都要求目标存在
foreach ($m in [regex]::Matches($struct, '\bhref="([^"#][^"]*)"')) {
    $href = $m.Groups[1].Value
    if ($href -match '^(https?:|mailto:|javascript:|data:|/|\{\{)') { continue }
    $decoded = [System.Uri]::UnescapeDataString($href)
    $targetPath = Join-Path $htmlDir $decoded
    if (-not (Test-Path -LiteralPath $targetPath)) {
        Add-Error "相对链接指向不存在的文件：$href"
    }
}

# 5. 外部资源：脚本只允许常见 CDN；禁止远程图片与内嵌字体
$allowedHosts = @('cdn.jsdelivr.net','unpkg.com','cdnjs.cloudflare.com','cdn.bootcdn.net')
foreach ($m in [regex]::Matches($struct, '<script\b[^>]*\bsrc="([^"]+)"')) {
    $src = $m.Groups[1].Value
    if ($src -like '*{{*') { continue }
    try {
        $hostName = ([System.Uri]$src).Host
    } catch {
        Add-Error "无法解析脚本地址：$src"
        continue
    }
    if ($allowedHosts -notcontains $hostName) {
        Add-Warn "脚本来自非白名单 CDN：$hostName"
    }
}
if ($text -match '(?i)data:font') {
    Add-Warn '检测到内嵌字体（data:font），单文件体积会明显变大'
}
if ($text -match '(?i)<img[^>]+src="https?://') {
    Add-Warn '检测到远程图片，离线打开会缺图'
}
if ($text -match '(?i)<link[^>]+href="https?://') {
    Add-Warn '检测到外部样式表，离线打开会丢样式'
}

# 6. 未替换的占位符
if ($text -match '\{\{[A-Z0-9_]+\}\}') {
    if ($Markdown) {
        Add-Error '仍存在未替换的 {{占位符}}'
    } else {
        Add-Warn '存在 {{占位符}}（检查模板本身时可忽略）'
    }
}

# 7. 与 Markdown 对账（台账编号与 P1/P2 数量）
$htmlIds = @([regex]::Matches($struct, '\bdata-id="([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
if ($Markdown) {
    if (-not (Test-Path -LiteralPath $Markdown)) {
        Add-Error "找不到 Markdown：$Markdown"
    } else {
        $mdText = Get-Content -LiteralPath $Markdown -Raw -Encoding utf8
        $mdIds = @([regex]::Matches($mdText, '(?m)^\|\s*(P[12]-\d+)\s*\|') |
                   ForEach-Object { $_.Groups[1].Value.ToLowerInvariant() })
        if ($mdIds.Count -gt 0) {
            $mdSet   = [System.Collections.Generic.HashSet[string]]::new([string[]]$mdIds)
            $htmlSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$htmlIds)
            foreach ($id in $mdSet) {
                if (-not $htmlSet.Contains($id)) { Add-Error "Markdown 有 $id，HTML 台账缺失" }
            }
            foreach ($id in $htmlSet) {
                if ($id -notlike 'p*' -or $id -notmatch '^p[12]-\d+$') { continue }
                if (-not $mdSet.Contains($id)) { Add-Error "HTML 有 $id，Markdown 台账缺失" }
            }
            $mdP1 = @($mdIds | Where-Object { $_ -like 'p1-*' }).Count
            $mdP2 = @($mdIds | Where-Object { $_ -like 'p2-*' }).Count
            $htmlP1 = @($htmlIds | Where-Object { $_ -like 'p1-*' }).Count
            $htmlP2 = @($htmlIds | Where-Object { $_ -like 'p2-*' }).Count
            if ($mdP1 -ne $htmlP1) { Add-Error "P1 数量不一致：Markdown $mdP1 / HTML $htmlP1" }
            if ($mdP2 -ne $htmlP2) { Add-Error "P2 数量不一致：Markdown $mdP2 / HTML $htmlP2" }
            Write-Host "台账：Markdown $($mdIds.Count) 条（P1 $mdP1 / P2 $mdP2），HTML $($htmlIds.Count) 条（P1 $htmlP1 / P2 $htmlP2）"
        }
    }
}

# 8. 内联脚本语法（有 Node 才检查）
$node = Get-Command node -ErrorAction SilentlyContinue
foreach ($m in [regex]::Matches($text, '(?s)<script(?<attrs>[^>]*)>(?<body>.*?)</script>')) {
    if ($m.Groups['attrs'].Value -match '\bsrc=') { continue }
    $js = $m.Groups['body'].Value
    if ([string]::IsNullOrWhiteSpace($js)) { continue }
    if (-not $node) {
        Add-Warn '未找到 node，跳过内联脚本语法检查'
        continue
    }
    $nodeCode = 'const s=require("fs").readFileSync(0,"utf8");try{new Function(s)}catch(e){console.error(e.message);process.exit(1)}'
    try {
        $output = $js | & $node.Source -e $nodeCode 2>&1
        if ($LASTEXITCODE -ne 0) {
            Add-Error "内联脚本语法错误：$output"
        }
    } catch {
        Add-Warn '无法运行 node，跳过内联脚本语法检查'
    }
}

# 9. 汇总
if ($warns.Count -gt 0) {
    foreach ($w in $warns) { Write-Host "WARN  $w" -ForegroundColor Yellow }
}
if ($errors.Count -gt 0) {
    foreach ($e in $errors) { Write-Host "ERROR $e" -ForegroundColor Red }
    Write-Host "检查失败：$($errors.Count) 个错误，$($warns.Count) 个警告" -ForegroundColor Red
    exit 1
}
if ($Strict -and $warns.Count -gt 0) {
    Write-Host "检查失败（-Strict）：$($warns.Count) 个警告" -ForegroundColor Red
    exit 1
}
Write-Host "检查通过：$($warns.Count) 个警告" -ForegroundColor Green
exit 0
