using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Hệ thống Đa Ngôn Ngữ (Localization Manager) cho game Unity:
/// Mặc định: Tiếng Việt (Vietnamese). Hỗ trợ: Tiếng Anh (English).
/// Quản lý từ khóa (Keys), dịch động theo thời gian thực (Realtime),
/// phát sự kiện OnLanguageChanged để tự động cập nhật toàn bộ UI trên màn hình mà không cần reload scene.
///
/// LƯU Ý: Không dùng emoji Unicode vì font SDF không hỗ trợ → hiển thị ô vuông.
/// Dùng text Unicode chuẩn (có dấu tiếng Việt) — font fallback SegoeUI-Bold SDF (Dynamic) sẽ tự render.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Localization Manager")]
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    public enum Language
    {
        Vietnamese,
        English
    }

    [Header("--- Ngôn Ngữ Hiện Tại ---")]
    [SerializeField] private Language currentLanguage = Language.Vietnamese;

    public Language CurrentLanguage => currentLanguage;

    public static event Action OnLanguageChanged;

    private const string PREF_LANGUAGE_KEY = "SelectedLanguageCode";

    // Bảng từ điển dịch thuật Key -> (Vietnamese, English)
    // Dùng tiếng Việt Unicode có dấu — font fallback SegoeUI-Bold SDF (Dynamic) sẽ tự render
    // KHÔNG dùng emoji — chỉ dùng ký tự Unicode chuẩn
    private static readonly Dictionary<string, (string vi, string en)> LocalizationDictionary = new Dictionary<string, (string, string)>()
    {
        // === MAIN MENU ===
        { "menu_title", ("JELLY SHAPE\nRUNNER 3D", "JELLY SHAPE\nRUNNER 3D") },
        { "menu_best_score", ("KỶ LỤC: {0} ĐIỂM", "BEST: {0} PTS") },
        { "menu_play", ("CHƠI NGAY", "PLAY NOW") },
        { "menu_shop", ("CỬA HÀNG", "SHOP") },
        { "menu_leaderboard", ("BẢNG XẾP HẠNG", "LEADERBOARD") },
        { "menu_settings", ("CÀI ĐẶT", "SETTINGS") },
        { "menu_quit", ("THOÁT", "QUIT") },
        { "menu_coin_format", ("{0} XU", "{0} COINS") },
        { "profile_age_format", ("{0} Tuổi", "Age: {0}") },

        // === ONBOARDING / PROFILE ===
        { "onb_title", ("HỒ SƠ NGƯỜI CHƠI", "PLAYER PROFILE") },
        { "onb_desc", ("Tùy chỉnh tên, độ tuổi và ảnh đại diện", "Customize your name, age and avatar") },
        { "onb_name_label", ("TÊN NGƯỜI CHƠI", "PLAYER NAME") },
        { "onb_name_placeholder", ("Nhập tên của bạn...", "Enter your name...") },
        { "onb_age_label", ("ĐỘ TUỔI", "AGE") },
        { "onb_age_val", ("{0} Tuổi", "{0} Years Old") },
        { "onb_lang_label", ("NGÔN NGỮ", "LANGUAGE") },
        { "onb_avatar_label", ("CHỌN ẢNH ĐẠI DIỆN", "CHOOSE AVATAR") },
        { "onb_done_btn", ("HOÀN TẤT", "CONFIRM") },
        { "lang_vietnamese", ("Tiếng Việt", "Tiếng Việt") },
        { "lang_english", ("English", "English") },

        // === SETTINGS ===
        { "settings_title", ("CÀI ĐẶT", "SETTINGS") },
        { "settings_volume", ("ÂM LƯỢNG", "VOLUME") },
        { "settings_sensitivity", ("ĐỘ NHẠY VUỐT", "SWIPE SENSITIVITY") },
        { "settings_bloom", ("ĐỘ PHÁT SÁNG (BLOOM)", "GLOW INTENSITY (BLOOM)") },
        { "settings_graphics_on", ("ĐỒ HỌA NEON: BẬT", "NEON GRAPHICS: ON") },
        { "settings_graphics_off", ("ĐỒ HỌA NEON: Tắt", "NEON GRAPHICS: OFF") },
        { "settings_sound_on", ("ÂM THANH: BẬT", "SOUND: ON") },
        { "settings_sound_off", ("ÂM THANH: Tắt", "SOUND: OFF") },
        { "settings_reset_best", ("XÓA KỶ LỤC", "RESET BEST SCORE") },
        { "settings_language", ("NGÔN NGỮ: {0}", "LANGUAGE: {0}") },
        { "btn_close", ("ĐÓNG", "CLOSE") },

        // === LEADERBOARD ===
        { "lb_title", ("BẢNG XẾP HẠNG TOP 5", "TOP 5 LEADERBOARD") },
        { "lb_empty", ("Chưa có kỷ lục nào!\nHãy chơi để ghi danh!", "No records yet!\nPlay now to make history!") },

        // === SHOP ===
        { "shop_title", ("CỬA HÀNG VẬT PHẨM", "ITEM SHOP") },
        { "shop_tab_player", ("SKIN JELLY", "JELLY SKINS") },
        { "shop_tab_wall", ("SKIN TƯỜNG", "WALL SKINS") },
        { "shop_tab_effect", ("KỸ NĂNG / HIỆU ỨNG", "SKILLS / EFFECTS") },
        { "shop_btn_buy", ("MUA", "BUY") },
        { "shop_btn_equip", ("TRANG BỊ", "EQUIP") },
        { "shop_btn_equipped", ("ĐANG DÙNG", "EQUIPPED") },
        { "shop_status_equipped", ("* ĐANG TRANG BỊ", "* EQUIPPED") },
        { "shop_status_owned", ("ĐÃ SỞ HỮU", "OWNED") },
        { "shop_price_format", ("{0} XU", "{0} COINS") },
        { "shop_back_menu", ("QUAY LẠI MENU", "BACK TO MENU") },

        // === IN-GAME HUD & GAME OVER ===
        { "hud_score", ("{0}", "{0}") },
        { "hud_distance", ("{0}m", "{0}m") },
        { "gameover_title", ("THUA CUỘC!", "GAME OVER!") },
        { "gameover_score", ("ĐIỂM CỦA BẠN: {0}", "FINAL SCORE: {0}") },
        { "gameover_new_record", ("KỶ LỤC MỚI!", "NEW RECORD!") },
        { "gameover_best", ("KỶ LỤC: {0}", "BEST: {0}") },
        { "btn_restart", ("CHƠI LẠI", "PLAY AGAIN") },
        { "btn_menu", ("VỀ MENU", "MAIN MENU") },

        // === PAUSE PANEL ===
        { "pause_title", ("TẠM DỪNG", "PAUSED") },
        { "btn_resume", ("TIẾP TỤC", "RESUME") }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedLanguage();
    }

    /// <summary>
    /// Lấy ngôn ngữ đang chọn, luôn đọc chính xác từ PlayerPrefs kể cả khi Instance chưa khởi tạo
    /// </summary>
    public static Language GetCurrentLanguage()
    {
        if (Instance != null) return Instance.currentLanguage;

        string saved = PlayerPrefs.GetString(PREF_LANGUAGE_KEY, "");
        if (Enum.TryParse<Language>(saved, out var lang)) return lang;

        string legacy = PlayerPrefs.GetString("PlayerLanguage", "Tieng Viet");
        return legacy.Contains("Viet") ? Language.Vietnamese : Language.English;
    }

    /// <summary>
    /// Chuyển đổi ngôn ngữ hiện tại và lưu vào PlayerPrefs
    /// </summary>
    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        PlayerPrefs.SetString(PREF_LANGUAGE_KEY, lang.ToString());
        PlayerPrefs.SetString("PlayerLanguage", lang == Language.Vietnamese ? "Tieng Viet" : "English");
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
        Debug.Log($"<color=#00E5FF>[LocalizationManager] Ngon ngu da doi sang: {lang}</color>");
    }

    /// <summary>
    /// Đổi luân phiên giữa Tiếng Việt và English
    /// </summary>
    public void ToggleLanguage()
    {
        SetLanguage(currentLanguage == Language.Vietnamese ? Language.English : Language.Vietnamese);
    }

    /// <summary>
    /// Lấy chuỗi bản dịch theo khóa (Key)
    /// </summary>
    public static string Get(string key)
    {
        Language current = GetCurrentLanguage();

        if (LocalizationDictionary.TryGetValue(key, out var pair))
        {
            return current == Language.Vietnamese ? pair.vi : pair.en;
        }

        return key; // Fallback trả về key nếu không tìm thấy
    }

    /// <summary>
    /// Lấy chuỗi bản dịch có kèm tham số định dạng
    /// </summary>
    public static string Get(string key, params object[] args)
    {
        string raw = Get(key);
        try
        {
            return string.Format(raw, args);
        }
        catch
        {
            return raw;
        }
    }

    private void LoadSavedLanguage()
    {
        currentLanguage = GetCurrentLanguage();
    }
}
