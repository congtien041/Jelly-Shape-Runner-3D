$lines = Get-Content 'Assets/Scenes/MainMenu.unity'
$gos = @{}
$trToGo = @{}
$trChildren = @{}

for ($i = 0; $i -lt $lines.Length; $i++) {
    $line = $lines[$i]
    if ($line -match '^--- !u!1 &(\d+)') {
        $id = $Matches[1]
        for ($j = $i + 1; $j -lt [Math]::Min($lines.Length, $i + 15); $j++) {
            if ($lines[$j] -match '^  m_Name: (.+)') {
                $gos[$id] = $Matches[1]
                break
            }
        }
    }
    elseif ($line -match '^--- !u!224 &(\d+)') {
        $trId = $Matches[1]
        $goId = $null
        $cList = @()
        $inChildren = $false
        for ($j = $i + 1; $j -lt [Math]::Min($lines.Length, $i + 40); $j++) {
            if ($lines[$j] -match '^  m_GameObject: \{fileID: (\d+)\}') {
                $goId = $Matches[1]
            }
            if ($lines[$j] -match '^  m_Children:') {
                $inChildren = $true
                continue
            }
            if ($inChildren) {
                if ($lines[$j] -match '^  - \{fileID: (\d+)\}') {
                    $cList += $Matches[1]
                } else {
                    $inChildren = $false
                }
            }
            if ($lines[$j] -match '^  m_Father:') {
                break
            }
        }
        if ($goId) { $trToGo[$trId] = $goId }
        $trChildren[$trId] = $cList
    }
}

function Print-Tree($trId, $indent = "") {
    $goId = $trToGo[$trId]
    $name = if ($goId -and $gos.ContainsKey($goId)) { $gos[$goId] } else { "Unknown ($trId)" }
    Write-Host "$indent- $name"
    if ($trChildren.ContainsKey($trId)) {
        foreach ($cId in $trChildren[$trId]) {
            Print-Tree $cId ($indent + "  ")
        }
    }
}

foreach ($trId in $trToGo.Keys) {
    $goId = $trToGo[$trId]
    if ($gos[$goId] -eq 'ShopModalPanel') {
        Write-Host "FOUND ShopModalPanel:"
        Print-Tree $trId ""
    }
}
