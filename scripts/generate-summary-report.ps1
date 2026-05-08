#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot "reporting/summary-report.config.json"),
    [string]$OutputRoot,
    [switch]$SkipFullSuite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "../.specify/scripts/powershell/common.ps1")

function Quote-Argument {
    param([string]$Value)

    if ([string]::IsNullOrEmpty($Value)) {
        return '""'
    }

    if ($Value -notmatch '[\s"]') {
        return $Value
    }

    return '"' + ($Value -replace '"', '\"') + '"'
}

function Invoke-CommandCapture {
    param(
        [Parameter(Mandatory = $true)][string]$FileName,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [hashtable]$EnvironmentVariables = @{}
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false

    foreach ($argument in $Arguments) {
        $null = $startInfo.ArgumentList.Add([string]$argument)
    }

    foreach ($entry in $EnvironmentVariables.GetEnumerator()) {
        $startInfo.Environment[$entry.Key] = [string]$entry.Value
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $null = $process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    return [ordered]@{
        command = ((@($FileName) + ($Arguments | ForEach-Object { Quote-Argument $_ })) -join ' ')
        exitCode = $process.ExitCode
        stdout = $stdout.TrimEnd("`r", "`n")
        stderr = $stderr.TrimEnd("`r", "`n")
    }
}

function Test-ExpectedFragments {
    param(
        [string]$Text,
        [object[]]$ExpectedFragments
    )

    if ($null -eq $ExpectedFragments -or $ExpectedFragments.Count -eq 0) {
        return $true
    }

    foreach ($fragment in $ExpectedFragments) {
        if (-not $Text.Contains([string]$fragment, [System.StringComparison]::Ordinal)) {
            return $false
        }
    }

    return $true
}

  function Test-MapContainsKey {
    param(
      [AllowNull()][object]$Map,
      [Parameter(Mandatory = $true)][string]$Key
    )

    if ($null -eq $Map) {
      return $false
    }

    if ($Map -is [System.Collections.IDictionary]) {
      return $Map.Contains($Key)
    }

    return $null -ne $Map.PSObject.Properties[$Key]
  }

function Get-TextSummary {
    param([hashtable]$Result)

    $summary = @("Exit code: $($Result.exitCode)")
    if (-not [string]::IsNullOrWhiteSpace($Result.stdout)) {
        $summary += "stdout captured"
    }
    if (-not [string]::IsNullOrWhiteSpace($Result.stderr)) {
        $summary += "stderr captured"
    }

    return ($summary -join ', ')
}

function Invoke-EvidenceItem {
    param(
        [hashtable]$Evidence,
        [string]$RepoRoot
    )

    $kind = [string]$Evidence.kind
    $expected = if (Test-MapContainsKey -Map $Evidence -Key "expected") { $Evidence.expected } else { @{} }

    switch ($kind) {
        "file" {
            $absolutePath = Join-Path $RepoRoot ([string]$Evidence.path)
            $exists = Test-Path -LiteralPath $absolutePath -PathType Any
            $passed = if (Test-MapContainsKey -Map $expected -Key "exists") { $exists -eq [bool]$expected.exists } else { $exists }
            return [ordered]@{
                id = $Evidence.id
                title = $Evidence.title
                kind = $kind
                status = if ($passed) { "passed" } else { "failed" }
                expected = [ordered]@{ exists = if (Test-MapContainsKey -Map $expected -Key "exists") { [bool]$expected.exists } else { $true } }
                observed = [ordered]@{ path = $Evidence.path; exists = $exists }
                summary = if ($exists) { "Required file exists." } else { "Required file is missing." }
            }
        }
        "test" {
            $arguments = @("test", [string]$Evidence.project, "--filter", [string]$Evidence.filter, "--nologo")
            $result = Invoke-CommandCapture -FileName "dotnet" -Arguments $arguments -WorkingDirectory $RepoRoot
            $passed = $result.exitCode -eq 0
            return [ordered]@{
                id = $Evidence.id
                title = $Evidence.title
                kind = $kind
                status = if ($passed) { "passed" } else { "failed" }
                expected = [ordered]@{ exitCode = 0 }
                observed = $result
                summary = Get-TextSummary -Result $result
            }
        }
        "cli" {
            $arguments = @("run", "--project", "src/TimezoneMeetingCli/TimezoneMeetingCli.csproj", "--") + @($Evidence.args | ForEach-Object { [string]$_ })
            $environment = @{}
            if (Test-MapContainsKey -Map $Evidence -Key "fixture") {
                $environment["TIMEZONE_MEETING_CLI_GEOCODING_FIXTURE_JSON"] = (($Evidence.fixture | ConvertTo-Json -Depth 20 -Compress))
            }

            $result = Invoke-CommandCapture -FileName "dotnet" -Arguments $arguments -WorkingDirectory $RepoRoot -EnvironmentVariables $environment
            $passed = $true
            if ((Test-MapContainsKey -Map $expected -Key "exitCode") -and $result.exitCode -ne [int]$expected.exitCode) {
                $passed = $false
            }
            if ((Test-MapContainsKey -Map $expected -Key "stdoutContains") -and -not (Test-ExpectedFragments -Text $result.stdout -ExpectedFragments $expected.stdoutContains)) {
                $passed = $false
            }
            if ((Test-MapContainsKey -Map $expected -Key "stderrContains") -and -not (Test-ExpectedFragments -Text $result.stderr -ExpectedFragments $expected.stderrContains)) {
                $passed = $false
            }
            if ((Test-MapContainsKey -Map $expected -Key "stdoutEmpty") -and [bool]$expected.stdoutEmpty -and -not [string]::IsNullOrWhiteSpace($result.stdout)) {
                $passed = $false
            }
            if ((Test-MapContainsKey -Map $expected -Key "stderrEmpty") -and [bool]$expected.stderrEmpty -and -not [string]::IsNullOrWhiteSpace($result.stderr)) {
                $passed = $false
            }

            return [ordered]@{
                id = $Evidence.id
                title = $Evidence.title
                kind = $kind
                status = if ($passed) { "passed" } else { "failed" }
                expected = $expected
                observed = $result
                summary = Get-TextSummary -Result $result
            }
        }
        default {
            throw "Unsupported evidence kind '$kind'."
        }
    }
}

function New-Status {
    param([object[]]$Statuses)

    if ($Statuses.Count -eq 0) {
        return "not-run"
    }

    if ($Statuses -contains "failed") {
        return "failed"
    }

    if ($Statuses -contains "not-run") {
        return "not-run"
    }

    return "passed"
}

function Convert-StatusToCssClass {
    param([string]$Status)

    switch ($Status) {
        "passed" { return "status-pass" }
        "failed" { return "status-fail" }
        default { return "status-neutral" }
    }
}

function ConvertTo-HtmlEncoded {
    param([AllowNull()][object]$Value)
    return [System.Net.WebUtility]::HtmlEncode([string]$Value)
}

function Format-TextBlock {
    param([AllowNull()][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return "<em>Empty</em>"
    }

    return "<pre>$(ConvertTo-HtmlEncoded $Value)</pre>"
}

function Get-EvidenceLookup {
    param([object[]]$EvidenceResults)

    $lookup = @{}
    foreach ($result in $EvidenceResults) {
        $lookup[$result.id] = $result
    }
    return $lookup
}

function Convert-CollectionToHtml {
    param(
        [object[]]$Items,
        [hashtable]$EvidenceLookup,
        [string]$SectionTitle,
        [string]$BodyProperty
    )

    $rows = foreach ($item in $Items) {
        $evidenceResults = @($item.evidence | ForEach-Object { $EvidenceLookup[$_] } | Where-Object { $null -ne $_ })
        $status = New-Status -Statuses @($evidenceResults | ForEach-Object { $_.status })
        $evidenceList = if ($evidenceResults.Count -gt 0) {
            '<ul>' + (($evidenceResults | ForEach-Object {
                "<li><strong>$(ConvertTo-HtmlEncoded $_.title)</strong> <span class='$(Convert-StatusToCssClass $_.status)'>$(ConvertTo-HtmlEncoded $_.status)</span></li>"
            }) -join '') + '</ul>'
        }
        else {
            '<p>No evidence mapped.</p>'
        }

        @"
        <article class="card">
          <h3>$(ConvertTo-HtmlEncoded $item.title)</h3>
          <p class="meta">$(ConvertTo-HtmlEncoded $item.id) <span class="$(Convert-StatusToCssClass $status)">$(ConvertTo-HtmlEncoded $status)</span></p>
          <p>$(ConvertTo-HtmlEncoded $item.$BodyProperty)</p>
          <div class="evidence-list">$evidenceList</div>
        </article>
"@
    }

    return @"
    <section>
      <h2>$(ConvertTo-HtmlEncoded $SectionTitle)</h2>
      $($rows -join "`n")
    </section>
"@
}

$repoRoot = Get-RepoRoot
$resolvedConfigPath = Resolve-Path -LiteralPath $ConfigPath
$config = Get-Content -LiteralPath $resolvedConfigPath -Raw | ConvertFrom-Json -AsHashtable -Depth 100

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts/reports"
}

$latestDirectory = Join-Path $OutputRoot "latest"
$historyDirectory = Join-Path $OutputRoot "history"
New-Item -ItemType Directory -Path $latestDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $historyDirectory -Force | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$branchName = Get-CurrentBranch
$commitSha = "unknown"
if (Test-HasGit) {
    $commitSha = (git -C $repoRoot rev-parse --short HEAD).Trim()
}

$evidenceResults = @()
foreach ($evidence in $config.evidence) {
    $evidenceResults += (Invoke-EvidenceItem -Evidence $evidence -RepoRoot $repoRoot)
}

$fullSuiteResult = $null
if (-not $SkipFullSuite) {
    $fullSuiteResult = Invoke-CommandCapture -FileName "dotnet" -Arguments @("test", "--nologo") -WorkingDirectory $repoRoot
}

$evidenceLookup = Get-EvidenceLookup -EvidenceResults $evidenceResults

$goalStatuses = foreach ($goal in $config.goals) {
    $results = @($goal.evidence | ForEach-Object { $evidenceLookup[$_] } | Where-Object { $null -ne $_ })
    [ordered]@{
        id = $goal.id
        title = $goal.title
        description = $goal.description
        evidence = @($results | ForEach-Object { $_.id })
        status = New-Status -Statuses @($results | ForEach-Object { $_.status })
    }
}

$feedbackStatuses = foreach ($feedback in $config.reviewFeedback) {
    $results = @($feedback.evidence | ForEach-Object { $evidenceLookup[$_] } | Where-Object { $null -ne $_ })
    [ordered]@{
        id = $feedback.id
        title = $feedback.title
        description = $feedback.description
        evidence = @($results | ForEach-Object { $_.id })
        status = New-Status -Statuses @($results | ForEach-Object { $_.status })
    }
}

$requirementStatuses = foreach ($requirement in $config.requirements) {
    $results = @($requirement.evidence | ForEach-Object { $evidenceLookup[$_] } | Where-Object { $null -ne $_ })
    [ordered]@{
        id = $requirement.id
        title = $requirement.title
        description = $requirement.description
        evidence = @($results | ForEach-Object { $_.id })
        status = New-Status -Statuses @($results | ForEach-Object { $_.status })
    }
}

$manifest = [ordered]@{
    generatedAt = (Get-Date).ToString("o")
    branch = $branchName
    commit = $commitSha
    pr = $config.pr
    report = $config.report
    evidence = $evidenceResults
    goals = $goalStatuses
    reviewFeedback = $feedbackStatuses
    requirements = $requirementStatuses
    fullSuite = $fullSuiteResult
}

$manifestPath = Join-Path $latestDirectory "report_manifest.json"
$manifest | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $manifestPath

$passedEvidence = @($evidenceResults | Where-Object { $_.status -eq "passed" }).Count
$failedEvidence = @($evidenceResults | Where-Object { $_.status -eq "failed" }).Count
$overallStatus = if ($failedEvidence -gt 0 -or ($fullSuiteResult -and $fullSuiteResult.exitCode -ne 0)) { "failed" } else { "passed" }

$goalSection = Convert-CollectionToHtml -Items $manifest.goals -EvidenceLookup $evidenceLookup -SectionTitle "PR Goals" -BodyProperty "description"
$feedbackSection = Convert-CollectionToHtml -Items $manifest.reviewFeedback -EvidenceLookup $evidenceLookup -SectionTitle "PR Feedback Coverage" -BodyProperty "description"
$requirementsSection = Convert-CollectionToHtml -Items $manifest.requirements -EvidenceLookup $evidenceLookup -SectionTitle "Requirements Coverage" -BodyProperty "description"

$evidenceCards = foreach ($result in $evidenceResults) {
    $expectedJson = $result.expected | ConvertTo-Json -Depth 20
    $observedCommand = if (Test-MapContainsKey -Map $result.observed -Key "command") {
        "<p><strong>Command:</strong> $(ConvertTo-HtmlEncoded $result.observed.command)</p>"
    }
    else {
        ""
    }
    $observedDetails = if ((Test-MapContainsKey -Map $result.observed -Key "stdout") -or (Test-MapContainsKey -Map $result.observed -Key "stderr")) {
        @"
      <details>
        <summary>Observed stdout</summary>
        $(Format-TextBlock $(if (Test-MapContainsKey -Map $result.observed -Key "stdout") { $result.observed.stdout } else { "" }))
      </details>
      <details>
        <summary>Observed stderr</summary>
        $(Format-TextBlock $(if (Test-MapContainsKey -Map $result.observed -Key "stderr") { $result.observed.stderr } else { "" }))
      </details>
"@
    }
    else {
        "<pre>$(ConvertTo-HtmlEncoded ($result.observed | ConvertTo-Json -Depth 20))</pre>"
    }

    @"
    <article class="card">
      <h3>$(ConvertTo-HtmlEncoded $result.title)</h3>
      <p class="meta">$(ConvertTo-HtmlEncoded $result.id) <span class="$(Convert-StatusToCssClass $result.status)">$(ConvertTo-HtmlEncoded $result.status)</span></p>
      <p><strong>Kind:</strong> $(ConvertTo-HtmlEncoded $result.kind)</p>
      $observedCommand
      <p><strong>Summary:</strong> $(ConvertTo-HtmlEncoded $result.summary)</p>
      <details>
        <summary>Expected outcome</summary>
        <pre>$(ConvertTo-HtmlEncoded $expectedJson)</pre>
      </details>
      $observedDetails
    </article>
"@
}

$fullSuiteSection = if ($fullSuiteResult) {
    @"
    <section>
      <h2>Full Test Suite</h2>
      <article class="card">
        <p class="meta"><span class="$(Convert-StatusToCssClass $(if ($fullSuiteResult.exitCode -eq 0) { 'passed' } else { 'failed' }))">Exit code $(ConvertTo-HtmlEncoded $fullSuiteResult.exitCode)</span></p>
        <p><strong>Command:</strong> $(ConvertTo-HtmlEncoded $fullSuiteResult.command)</p>
        <details open>
          <summary>Observed output</summary>
          $(Format-TextBlock ($fullSuiteResult.stdout + "`n" + $fullSuiteResult.stderr).Trim())
        </details>
      </article>
    </section>
"@
}
else {
    @"
    <section>
      <h2>Full Test Suite</h2>
      <article class="card">
        <p class="meta"><span class="status-neutral">Skipped</span></p>
      </article>
    </section>
"@
}

$html = @"
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>$(ConvertTo-HtmlEncoded $config.report.title)</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #f7f4ed;
      --surface: #fffdf8;
      --text: #1f2521;
      --muted: #5f6b63;
      --accent: #0d5c63;
      --pass: #1f7a4c;
      --fail: #b42318;
      --neutral: #7a6f5c;
      --border: #d8d0c2;
    }
    body {
      margin: 0;
      font-family: Georgia, "Times New Roman", serif;
      background: linear-gradient(180deg, #efe7d8 0%, var(--bg) 240px);
      color: var(--text);
    }
    main {
      max-width: 1100px;
      margin: 0 auto;
      padding: 32px 20px 64px;
    }
    header {
      background: rgba(255, 253, 248, 0.92);
      border: 1px solid var(--border);
      border-radius: 18px;
      padding: 28px;
      box-shadow: 0 14px 40px rgba(44, 49, 45, 0.08);
    }
    h1, h2, h3 {
      margin-top: 0;
      font-family: Cambria, Georgia, serif;
    }
    section {
      margin-top: 28px;
    }
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
      gap: 16px;
      margin-top: 18px;
    }
    .card {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 16px;
      padding: 18px;
      margin-top: 16px;
      box-shadow: 0 10px 28px rgba(44, 49, 45, 0.05);
    }
    .meta {
      color: var(--muted);
      font-size: 0.95rem;
    }
    .status-pass,
    .status-fail,
    .status-neutral {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 999px;
      font-size: 0.85rem;
      font-weight: 700;
    }
    .status-pass {
      background: #e6f6ec;
      color: var(--pass);
    }
    .status-fail {
      background: #fde7e7;
      color: var(--fail);
    }
    .status-neutral {
      background: #f3ede1;
      color: var(--neutral);
    }
    pre {
      white-space: pre-wrap;
      word-break: break-word;
      background: #fbf8f2;
      border: 1px solid #e7ddcf;
      border-radius: 12px;
      padding: 14px;
      overflow-x: auto;
    }
    details {
      margin-top: 12px;
    }
    ul {
      padding-left: 20px;
    }
    a {
      color: var(--accent);
    }
  </style>
</head>
<body>
  <main>
    <header>
      <p class="meta">Generated $(ConvertTo-HtmlEncoded $manifest.generatedAt) on branch $(ConvertTo-HtmlEncoded $manifest.branch) at commit $(ConvertTo-HtmlEncoded $manifest.commit)</p>
      <h1>$(ConvertTo-HtmlEncoded $config.report.title)</h1>
      <p>$(ConvertTo-HtmlEncoded $config.report.subtitle)</p>
      <p><strong>PR:</strong> <a href="$(ConvertTo-HtmlEncoded $config.pr.url)">$(ConvertTo-HtmlEncoded $config.pr.title)</a></p>
      <div class="grid">
        <div class="card">
          <h2>Overall Status</h2>
          <p><span class="$(Convert-StatusToCssClass $overallStatus)">$(ConvertTo-HtmlEncoded $overallStatus)</span></p>
        </div>
        <div class="card">
          <h2>Evidence Checks</h2>
          <p>Passed: $(ConvertTo-HtmlEncoded $passedEvidence)</p>
          <p>Failed: $(ConvertTo-HtmlEncoded $failedEvidence)</p>
        </div>
        <div class="card">
          <h2>Full Suite</h2>
          <p><span class="$(Convert-StatusToCssClass $(if ($fullSuiteResult -and $fullSuiteResult.exitCode -eq 0) { 'passed' } elseif ($fullSuiteResult) { 'failed' } else { 'not-run' }))">$(ConvertTo-HtmlEncoded $(if ($fullSuiteResult) { "exit code $($fullSuiteResult.exitCode)" } else { 'skipped' }))</span></p>
        </div>
      </div>
      <section>
        <h2>PR Goals Claimed</h2>
        <ul>
          $((@($config.pr.goals) | ForEach-Object { "<li>$(ConvertTo-HtmlEncoded $_)</li>" }) -join "`n")
        </ul>
      </section>
    </header>
    $goalSection
    $feedbackSection
    $requirementsSection
    $fullSuiteSection
    <section>
      <h2>Observed Evidence</h2>
      $($evidenceCards -join "`n")
    </section>
  </main>
</body>
</html>
"@

$htmlPath = Join-Path $latestDirectory "summary_report.html"
$html | Set-Content -LiteralPath $htmlPath

$archiveSlug = "$timestamp-$branchName"
$archivePath = Join-Path $historyDirectory $archiveSlug
New-Item -ItemType Directory -Path $archivePath -Force | Out-Null
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $archivePath "report_manifest.json") -Force
Copy-Item -LiteralPath $htmlPath -Destination (Join-Path $archivePath "summary_report.html") -Force

Write-Host "Summary report written to $htmlPath"
Write-Host "Manifest written to $manifestPath"