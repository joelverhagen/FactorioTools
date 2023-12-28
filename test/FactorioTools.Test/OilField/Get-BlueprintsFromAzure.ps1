[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]
    $StorageAccountName,

    [string]
    $ContainerName = "insights-logs-apptraces"
)

$ErrorActionPreference = "Stop"

$storageAccount = Get-AzStorageAccount | Where-Object { $_.StorageAccountName -eq $StorageAccountName }
if (!$storageAccount) {
    throw "Could not find storage account $StorageAccountName"
}

$container = $storageAccount | Get-AzStorageContainer | Where-Object { $_.Name -eq $ContainerName }
if (!$container) {
    throw "Could not find container $ContainerName"
}

$blobs = $container | Get-AzStorageBlob | Sort-Object -Property LastModified
$lastBlobDatePath = Join-Path $PSScriptRoot "last-blob-date.txt"
if (Test-Path $lastBlobDatePath) {
    $lastBlobDate = Get-Content $lastBlobDatePath
    $lastBlobDate = [DateTimeOffset]::Parse($lastBlobDate)
    Write-Host "Using blobs >= $($lastBlobDate.ToString("O"))"
    $blobs = $blobs | Where-Object { $_.LastModified -gt $lastBlobDate }
}

$uniqueBlueprints = New-Object System.Collections.Generic.HashSet[string]

foreach ($blob in $blobs) {
    $namePieces = $blob.Name.Split('/')
    Write-Host (($namePieces[-6..-1] -join '/') + " ") -NoNewline

    $jsonLines = $blob.BlobClient.DownloadContent().Value.Content.ToString().Split("`n")
    $blueprints = $jsonLines | `
        ForEach-Object { $_ | ConvertFrom-Json } | `
        Where-Object { $_.Properties.OriginalFormat -eq "Planning oil field for blueprint {Blueprint}" } | `
        ForEach-Object { $_.Properties.Blueprint }
    foreach ($blueprint in $blueprints) {
        if ($uniqueBlueprints.Add($blueprint)) {
            while ($true) {
                try {
                    Add-Content -Path (Join-Path $PSScriptRoot "big-list.txt") -Value $blueprint -Encoding UTF8
                    break
                } catch {
                    Write-Host "!" -NoNewline
                    Start-Sleep -Milliseconds 100
                }
            }
            Write-Host "O" -NoNewline
        } else {
            Write-Host "." -NoNewline
        }
    }

    Write-Host
    $blob.LastModified.ToString("O") | Out-File -FilePath (Join-Path $PSScriptRoot "last-blob-date.txt") -Encoding utf8
}

Write-Host "Normalizing blueprints..."
$sandboxProj = Join-Path $PSScriptRoot "../../../src/Sandbox"
dotnet run --project $sandboxProj -- normalize
Write-Host "Done."
