using UnityEngine;
using TMPro;

/// <summary>
/// Component tự động cập nhật văn bản theo ngôn ngữ hiện tại.
/// Gắn vào bất kỳ GameObject nào có TextMeshProUGUI.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
[AddComponentMenu("Jelly Runner/Localized Text")]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("Khóa dịch thuật trong LocalizationManager (ví dụ: menu_play, btn_close)")]
    [SerializeField] private string localizationKey;

    private TextMeshProUGUI textComponent;

    public string LocalizationKey
    {
        get => localizationKey;
        set
        {
            localizationKey = value;
            UpdateText();
        }
    }

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateText();
        LocalizationManager.OnLanguageChanged += UpdateText;
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        if (textComponent != null && !string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = LocalizationManager.Get(localizationKey);
        }
    }
}
