$files = Get-ChildItem -Path "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts" -Filter "*.asset"
foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    if ($content -match "8f586378b4e144a9851e7b34d9b748ee") {
        Write-Host "Cleaning: $($f.Name)"
        $content = [regex]::Replace($content, "m_FallbackFontAssetTable:\r?\n\s*-\s*\{fileID: 11400000, guid: 8f586378b4e144a9851e7b34d9b748ee, type: 2\}", "m_FallbackFontAssetTable: []")
        [System.IO.File]::WriteAllText($f.FullName, $content)
    }
}
Write-Host "DONE CLEANING FALLBACK FONTS!"
