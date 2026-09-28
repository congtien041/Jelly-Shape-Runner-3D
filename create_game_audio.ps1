$audioDir = "d:\Jelly Shape Runner 3D\Assets\Audio"
if (!(Test-Path $audioDir)) {
    New-Item -ItemType Directory -Path $audioDir -Force | Out-Null
}

function Write-WavFile($path, [short[]]$samples, [int]$sampleRate) {
    $fs = [System.IO.File]::Open($path, [System.IO.FileMode]::Create)
    $bw = New-Object System.IO.BinaryWriter($fs)
    
    $byteRate = $sampleRate * 2
    $dataLength = $samples.Length * 2
    
    # RIFF header
    $bw.Write([char[]]@('R', 'I', 'F', 'F'))
    $bw.Write([int](36 + $dataLength))
    $bw.Write([char[]]@('W', 'A', 'V', 'E'))
    
    # fmt chunk
    $bw.Write([char[]]@('f', 'm', 't', ' '))
    $bw.Write([int]16)
    $bw.Write([short]1) # PCM
    $bw.Write([short]1) # Mono
    $bw.Write([int]$sampleRate)
    $bw.Write([int]$byteRate)
    $bw.Write([short]2) # Block align
    $bw.Write([short]16) # Bits per sample
    
    # data chunk
    $bw.Write([char[]]@('d', 'a', 't', 'a'))
    $bw.Write([int]$dataLength)
    
    for ($i = 0; $i -lt $samples.Length; $i++) {
        $bw.Write($samples[$i])
    }
    
    $bw.Close()
    $fs.Close()
}

$sampleRate = 44100

# 1. SFX_CoinCollect.wav (leng keng 987Hz -> 1318Hz)
$duration = 0.35
$sampleCount = [int]($sampleRate * $duration)
$samples = New-Object short[] $sampleCount
for ($i = 0; $i -lt $sampleCount; $i++) {
    $t = $i / $sampleRate
    $env = [Math]::Exp(-$t * 9.0)
    $freq = if ($t -lt 0.1) { 987.77 } else { 1318.51 }
    $wave = [Math]::Sin(2.0 * [Math]::PI * $freq * $t) * 0.7 + [Math]::Sin(2.0 * [Math]::PI * $freq * 2.0 * $t) * 0.3
    $val = [Math]::Max(-1.0, [Math]::Min(1.0, $wave * $env)) * 32000.0
    $samples[$i] = [short]$val
}
Write-WavFile "$audioDir\SFX_CoinCollect.wav" $samples $sampleRate

# 2. SFX_ShapeShift.wav (Bloop đàn hồi 220Hz -> 580Hz -> 380Hz)
$duration = 0.28
$sampleCount = [int]($sampleRate * $duration)
$samples = New-Object short[] $sampleCount
for ($i = 0; $i -lt $sampleCount; $i++) {
    $t = $i / $sampleRate
    $progress = $t / $duration
    $freq = 220.0 + [Math]::Sin($progress * [Math]::PI) * 360.0
    $env = [Math]::Sin($progress * [Math]::PI)
    $wave = [Math]::Sin(2.0 * [Math]::PI * $freq * $t) + 0.25 * [Math]::Sin(2.0 * [Math]::PI * $freq * 0.5 * $t)
    $val = [Math]::Max(-1.0, [Math]::Min(1.0, $wave * $env)) * 28000.0
    $samples[$i] = [short]$val
}
Write-WavFile "$audioDir\SFX_ShapeShift.wav" $samples $sampleRate

# 3. SFX_PassWall.wav (Hợp âm C-E-G thành công)
$duration = 0.45
$sampleCount = [int]($sampleRate * $duration)
$samples = New-Object short[] $sampleCount
for ($i = 0; $i -lt $sampleCount; $i++) {
    $t = $i / $sampleRate
    $env = [Math]::Exp(-$t * 6.0)
    $c = [Math]::Sin(2.0 * [Math]::PI * 523.25 * $t)
    $e = [Math]::Sin(2.0 * [Math]::PI * 659.25 * $t)
    $g = [Math]::Sin(2.0 * [Math]::PI * 783.99 * $t)
    $wave = ($c * 0.4 + $e * 0.35 + $g * 0.35)
    $val = [Math]::Max(-1.0, [Math]::Min(1.0, $wave * $env)) * 30000.0
    $samples[$i] = [short]$val
}
Write-WavFile "$audioDir\SFX_PassWall.wav" $samples $sampleRate

# 4. SFX_GameOver.wav (Pitch drop 380Hz -> 70Hz)
$duration = 0.8
$sampleCount = [int]($sampleRate * $duration)
$samples = New-Object short[] $sampleCount
$rnd = New-Object System.Random
for ($i = 0; $i -lt $sampleCount; $i++) {
    $t = $i / $sampleRate
    $progress = $t / $duration
    $freq = 380.0 + (70.0 - 380.0) * $progress
    $env = 1.0 - $progress
    $noise = ($rnd.NextDouble() * 2.0 - 1.0) * 0.15 * (1.0 - $progress)
    $wave = [Math]::Sin(2.0 * [Math]::PI * $freq * $t) + 0.3 * [Math]::Sin(2.0 * [Math]::PI * $freq * 0.5 * $t) + $noise
    $val = [Math]::Max(-1.0, [Math]::Min(1.0, $wave * $env)) * 31000.0
    $samples[$i] = [short]$val
}
Write-WavFile "$audioDir\SFX_GameOver.wav" $samples $sampleRate

# 5. SFX_ButtonClick.wav (Pop nảy)
$duration = 0.08
$sampleCount = [int]($sampleRate * $duration)
$samples = New-Object short[] $sampleCount
for ($i = 0; $i -lt $sampleCount; $i++) {
    $t = $i / $sampleRate
    $env = [Math]::Exp(-$t * 50.0)
    $freq = 800.0 - ($t * 4000.0)
    $wave = [Math]::Sin(2.0 * [Math]::PI * $freq * $t)
    $val = [Math]::Max(-1.0, [Math]::Min(1.0, $wave * $env)) * 26000.0
    $samples[$i] = [short]$val
}
Write-WavFile "$audioDir\SFX_ButtonClick.wav" $samples $sampleRate

# 6. BGM_RunnerLoop.wav (16 beats vui nhộn lặp vô tận)
$bpm = 128.0
$beatDuration = 60.0 / $bpm
$totalBeats = 16
$duration = $beatDuration * $totalBeats
$sampleCount = [int]($sampleRate * $duration)
$samples = New-Object short[] $sampleCount
$melodyNotes = @(
    523.25, 659.25, 783.99, 880.00, 783.99, 659.25, 523.25, 587.33,
    659.25, 783.99, 1046.50, 880.00, 783.99, 659.25, 587.33, 523.25
)
$bassNotes = @(
    130.81, 130.81, 164.81, 164.81, 174.61, 174.61, 196.00, 196.00,
    130.81, 130.81, 164.81, 164.81, 174.61, 174.61, 196.00, 196.00
)
for ($i = 0; $i -lt $sampleCount; $i++) {
    $t = $i / $sampleRate
    $beatIndex = [int]($t / $beatDuration) % $totalBeats
    $beatT = $t % $beatDuration
    
    $mFreq = $melodyNotes[$beatIndex]
    $mEnv = [Math]::Exp(-$beatT * 4.5)
    $mWave = [Math]::Sin(2.0 * [Math]::PI * $mFreq * $t) * 0.35 + [Math]::Sin(2.0 * [Math]::PI * $mFreq * 2.0 * $t) * 0.1
    
    $bFreq = $bassNotes[$beatIndex]
    $bEnv = [Math]::Exp(-$beatT * 3.0)
    $sq = if ([Math]::Sin(2.0 * [Math]::PI * $bFreq * $t) -gt 0) { 0.2 } else { -0.2 }
    $bWave = $sq * $bEnv
    
    $halfBeat = $beatDuration / 2.0
    $hatEnv = [Math]::Exp(-($beatT % $halfBeat) * 40.0)
    $hatNoise = ($rnd.NextDouble() * 2.0 - 1.0) * 0.08 * $hatEnv
    
    $mix = ($mWave * $mEnv) + $bWave + $hatNoise
    $val = [Math]::Max(-1.0, [Math]::Min(1.0, $mix)) * 26000.0
    $samples[$i] = [short]$val
}
Write-WavFile "$audioDir\BGM_RunnerLoop.wav" $samples $sampleRate

Write-Output "Successfully generated all 6 WAV files in Assets/Audio"
