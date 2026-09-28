using UnityEngine;
using UnityEditor;
using System;
using System.IO;

/// <summary>
/// Công cụ tạo toàn bộ Hiệu Ứng Âm Thanh (SFX) và Nhạc Nền (BGM) chuẩn cho game Jelly Shape Runner 3D.
/// Tổng hợp dạng sóng âm thanh chuẩn PCM 16-bit 44.1kHz và lưu thành các file .wav vào Assets/Audio/.
/// Tự động gán vào AudioManager trên Scene.
/// </summary>
public static class AudioGenerator
{
    private const string AUDIO_FOLDER = "Assets/Audio";

    [InitializeOnLoadMethod]
    private static void AutoAssignAudioOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            AssignAudioToSceneManagersSilent();
        };
    }

    [MenuItem("Tools/🎵 Tạo & Cài Đặt Toàn Bộ Âm Thanh Game (BGM & SFX)", false, 20)]
    public static void GenerateAllGameAudio()
    {
        if (!AssetDatabase.IsValidFolder(AUDIO_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets", "Audio");
        }

        Debug.Log("<color=#00E5FF><b>[AudioGenerator] Bắt đầu tạo bộ âm thanh chuẩn cho Jelly Runner 3D...</b></color>");

        // 1. Tiếng ăn đồng xu (Coin Collect) - Trong trẻo, leng keng kép 987Hz -> 1318Hz
        CreateCoinSound($"{AUDIO_FOLDER}/SFX_CoinCollect.wav");

        // 2. Tiếng Jelly biến hình (Shape Shift) - Đàn hồi, dẻo dai "Bloop-Woosh"
        CreateShapeShiftSound($"{AUDIO_FOLDER}/SFX_ShapeShift.wav");

        // 3. Tiếng vượt tường thành công (Pass Wall) - Tiếng chuông chiến thắng êm dịu
        CreatePassWallSound($"{AUDIO_FOLDER}/SFX_PassWall.wav");

        // 4. Tiếng thua cuộc (Game Over) - Âm trầm bass tụt dần
        CreateGameOverSound($"{AUDIO_FOLDER}/SFX_GameOver.wav");

        // 5. Tiếng click nút bấm UI (Button Click) - Tiếng "Pop" nảy của Layer Lab
        CreateButtonClickSound($"{AUDIO_FOLDER}/SFX_ButtonClick.wav");

        // 6. Nhạc nền (BGM) - Giai điệu điện tử tươi vui, sôi động, lặp vô tận (Loop)
        CreateBackgroundMusic($"{AUDIO_FOLDER}/BGM_RunnerLoop.wav");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=#00FF66><b>[AudioGenerator] Đã tạo thành công toàn bộ 6 file âm thanh (.wav) vào Assets/Audio!</b></color>");

        // Gán tự động vào AudioManager trên Scene
        EditorApplication.delayCall += AssignAudioToSceneManagers;
    }

    /// <summary>
    /// Tự động tìm và gán các file AudioClip vừa tạo vào AudioManager trên các Scene
    /// </summary>
    private static void AssignAudioToSceneManagers()
    {
        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/BGM_RunnerLoop.wav");
        AudioClip sfxCoin = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_CoinCollect.wav");
        AudioClip sfxShift = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_ShapeShift.wav");
        AudioClip sfxPass = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_PassWall.wav");
        AudioClip sfxOver = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_GameOver.wav");
        AudioClip sfxBtn = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_ButtonClick.wav");

        // Quét AudioManager trên scene hiện tại
        AudioManager audioMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (audioMgr != null)
        {
            SerializedObject so = new SerializedObject(audioMgr);
            so.FindProperty("backgroundMusic").objectReferenceValue = bgm;
            so.FindProperty("coinCollectSound").objectReferenceValue = sfxCoin;
            so.FindProperty("shapeShiftSound").objectReferenceValue = sfxShift;
            so.FindProperty("passWallSound").objectReferenceValue = sfxPass;
            so.FindProperty("gameOverSound").objectReferenceValue = sfxOver;
            var btnProp = so.FindProperty("buttonClickSound");
            if (btnProp != null) btnProp.objectReferenceValue = sfxBtn;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(audioMgr);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(audioMgr.gameObject.scene);
            Debug.Log("<color=#00FF66><b>[AudioGenerator] Đã gán đầy đủ BGM và SFX vào AudioManager trên Scene!</b></color>");
        }

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Hoàn tất Âm Thanh", 
                "Đã tạo và cài đặt thành công toàn bộ bộ âm thanh game:\n" +
                "• 🎵 Nhạc nền sôi động (BGM Runner Loop)\n" +
                "• 🪙 Tiếng ăn xu leng keng (Coin Collect)\n" +
                "• 💧 Tiếng thạch Jelly biến hình (Shape Shift)\n" +
                "• 🧱 Tiếng chui qua tường thành công (Pass Wall)\n" +
                "• 💥 Tiếng thua cuộc (Game Over)\n" +
                "• 🔘 Tiếng click nút UI (Button Click)", "Tuyệt vời!");
        }
    }

    private static void AssignAudioToSceneManagersSilent()
    {
        AudioClip bgm = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/BGM_RunnerLoop.wav");
        AudioClip sfxCoin = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_CoinCollect.wav");
        AudioClip sfxShift = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_ShapeShift.wav");
        AudioClip sfxPass = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_PassWall.wav");
        AudioClip sfxOver = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_GameOver.wav");
        AudioClip sfxBtn = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AUDIO_FOLDER}/SFX_ButtonClick.wav");

        if (bgm == null) return; // Chưa import xong

        AudioManager audioMgr = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (audioMgr != null)
        {
            SerializedObject so = new SerializedObject(audioMgr);
            bool needSave = false;

            var bgmProp = so.FindProperty("backgroundMusic");
            if (bgmProp != null && bgmProp.objectReferenceValue == null) { bgmProp.objectReferenceValue = bgm; needSave = true; }

            var coinProp = so.FindProperty("coinCollectSound");
            if (coinProp != null && coinProp.objectReferenceValue == null) { coinProp.objectReferenceValue = sfxCoin; needSave = true; }

            var shiftProp = so.FindProperty("shapeShiftSound");
            if (shiftProp != null && shiftProp.objectReferenceValue == null) { shiftProp.objectReferenceValue = sfxShift; needSave = true; }

            var passProp = so.FindProperty("passWallSound");
            if (passProp != null && passProp.objectReferenceValue == null) { passProp.objectReferenceValue = sfxPass; needSave = true; }

            var overProp = so.FindProperty("gameOverSound");
            if (overProp != null && overProp.objectReferenceValue == null) { overProp.objectReferenceValue = sfxOver; needSave = true; }

            var btnProp = so.FindProperty("buttonClickSound");
            if (btnProp != null && btnProp.objectReferenceValue == null) { btnProp.objectReferenceValue = sfxBtn; needSave = true; }

            if (needSave)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(audioMgr);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(audioMgr.gameObject.scene);
                Debug.Log("<color=#00FF66><b>[AudioGenerator] Đã tự động gán BGM & SFX vào AudioManager trên Scene!</b></color>");
            }
        }
    }

    // =========================================================================
    // CÁC HÀM TỔNG HỢP SÓNG ÂM THANH (WAV GENERATION)
    // =========================================================================

    private static void CreateCoinSound(string path)
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 9f); // Envelope tắt dần

            // Tần số nốt Si (B5 - 987Hz) và Mi (E6 - 1318Hz)
            float freq = t < 0.1f ? 987.77f : 1318.51f;
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                       + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f; // Họa âm sáng

            samples[i] = (short)(Mathf.Clamp(wave * env, -1f, 1f) * 32000f);
        }

        WriteWavFile(path, samples, sampleRate);
    }

    private static void CreateShapeShiftSound(string path)
    {
        int sampleRate = 44100;
        float duration = 0.28f;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            // Pitch uốn lượn dạng Jelly co giãn: 220Hz -> 580Hz -> 380Hz
            float freq = 220f + Mathf.Sin(progress * Mathf.PI) * 360f;
            float env = Mathf.Sin(progress * Mathf.PI); // Envelope êm ái

            float wave = Mathf.Sin(2f * Mathf.PI * freq * t)
                       + 0.25f * Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t);

            samples[i] = (short)(Mathf.Clamp(wave * env, -1f, 1f) * 28000f);
        }

        WriteWavFile(path, samples, sampleRate);
    }

    private static void CreatePassWallSound(string path)
    {
        int sampleRate = 44100;
        float duration = 0.45f;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 6f);

            // Hợp âm Đô trưởng (C-E-G) tạo cảm giác vượt ải thành công
            float c = Mathf.Sin(2f * Mathf.PI * 523.25f * t);
            float e = Mathf.Sin(2f * Mathf.PI * 659.25f * t);
            float g = Mathf.Sin(2f * Mathf.PI * 783.99f * t);

            float wave = (c * 0.4f + e * 0.35f + g * 0.35f);
            samples[i] = (short)(Mathf.Clamp(wave * env, -1f, 1f) * 30000f);
        }

        WriteWavFile(path, samples, sampleRate);
    }

    private static void CreateGameOverSound(string path)
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            // Tần số giảm dần (pitch drop) từ 400Hz xuống 70Hz
            float freq = Mathf.Lerp(380f, 70f, progress);
            float env = (1f - progress);

            // Tiếng rung và méo tiếng tạo cảm giác va chạm
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t)
                       + 0.3f * Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * t)
                       + (UnityEngine.Random.value * 2f - 1f) * 0.15f * (1f - progress); // tiếng noise va đập nhẹ

            samples[i] = (short)(Mathf.Clamp(wave * env, -1f, 1f) * 31000f);
        }

        WriteWavFile(path, samples, sampleRate);
    }

    private static void CreateButtonClickSound(string path)
    {
        int sampleRate = 44100;
        float duration = 0.08f;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 50f);
            float freq = 800f - (t * 4000f);

            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            samples[i] = (short)(Mathf.Clamp(wave * env, -1f, 1f) * 26000f);
        }

        WriteWavFile(path, samples, sampleRate);
    }

    private static void CreateBackgroundMusic(string path)
    {
        int sampleRate = 44100;
        float bpm = 128f;
        float beatDuration = 60f / bpm;
        int totalBeats = 16; // Đoạn nhạc lặp 16 nhịp (~7.5 giây)
        float duration = beatDuration * totalBeats;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        // Giai điệu vui tươi (Melody notes: Pentatonic C D E G A)
        float[] melodyNotes = new float[]
        {
            523.25f, 659.25f, 783.99f, 880.00f, 783.99f, 659.25f, 523.25f, 587.33f,
            659.25f, 783.99f, 1046.50f, 880.00f, 783.99f, 659.25f, 587.33f, 523.25f
        };

        // Bassline đi theo nhịp
        float[] bassNotes = new float[]
        {
            130.81f, 130.81f, 164.81f, 164.81f, 174.61f, 174.61f, 196.00f, 196.00f,
            130.81f, 130.81f, 164.81f, 164.81f, 174.61f, 174.61f, 196.00f, 196.00f
        };

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int beatIndex = (int)(t / beatDuration) % totalBeats;
            float beatT = t % beatDuration;

            // 1. Melody (Giai điệu)
            float mFreq = melodyNotes[beatIndex];
            float mEnv = Mathf.Exp(-beatT * 4.5f);
            float mWave = Mathf.Sin(2f * Mathf.PI * mFreq * t) * 0.35f
                        + Mathf.Sin(2f * Mathf.PI * mFreq * 2f * t) * 0.1f;

            // 2. Bass (Âm trầm nhịp điệu)
            float bFreq = bassNotes[beatIndex];
            float bEnv = Mathf.Exp(-beatT * 3f);
            float bWave = (Mathf.Sin(2f * Mathf.PI * bFreq * t) > 0f ? 0.2f : -0.2f) * bEnv; // Square bass ấm

            // 3. Hi-hat / Shaker nhịp phách
            float hatEnv = Mathf.Exp(-(beatT % (beatDuration / 2f)) * 40f);
            float hatNoise = (UnityEngine.Random.value * 2f - 1f) * 0.08f * hatEnv;

            float mix = (mWave * mEnv) + bWave + hatNoise;
            samples[i] = (short)(Mathf.Clamp(mix, -1f, 1f) * 26000f);
        }

        WriteWavFile(path, samples, sampleRate);
    }

    /// <summary>
    /// Ghi mảng samples 16-bit PCM thành file WAV chuẩn RIFF
    /// </summary>
    private static void WriteWavFile(string filePath, short[] samples, int sampleRate)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            int byteRate = sampleRate * 2; // 1 channel * 16-bit
            int dataLength = samples.Length * 2;

            // RIFF header
            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + dataLength);
            bw.Write(new char[4] { 'W', 'A', 'V', 'E' });

            // fmt chunk
            bw.Write(new char[4] { 'f', 'm', 't', ' ' });
            bw.Write(16); // Chunk size
            bw.Write((short)1); // PCM
            bw.Write((short)1); // 1 Channel (Mono)
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write((short)2); // Block align
            bw.Write((short)16); // Bits per sample

            // data chunk
            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(dataLength);

            // Audio data
            for (int i = 0; i < samples.Length; i++)
            {
                bw.Write(samples[i]);
            }
        }
    }
}
