using UnityEngine;
using UnityEngine.UI;
using TMPro; // Потрібно для TMP_Dropdown
using UnityEngine.Audio; // Потрібно для AudioMixer

public class SettingsManager : MonoBehaviour
{
    // Створення Singleton інстансу: дозволяє звертатися до SettingsManager.Instance з будь-якого місця.
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Tooltip("UI Toggle для увімкнення/вимкнення всього звуку.")]
    public Toggle masterSoundToggle;
    [Tooltip("UI Slider для керування гучністю SFX.")]
    public Slider sfxSlider;
    [Tooltip("UI Slider для керування гучністю звуків меню.")]
    public Slider menuSoundSlider;
    [Tooltip("UI Slider для керування гучністю музики.")]
    public Slider musicSlider;

    [Tooltip("Головний Audio Mixer. Створіть 'Audio Mixer' через Assets -> Create -> Audio Mixer.")]
    public AudioMixer masterMixer; 

    [Tooltip("Назви параметрів гучності в Audio Mixer (повинні бути Exposed та перейменовані в Audio Mixer).")]
    // ЦІ ЗМІННІ ПОВИННІ МАТИ ТАКІ Ж НАЗВИ, ЯКІ ВИ ЗАДАЄТЕ В 'Exposed Parameters' В AUDIO MIXER
    // (Наприклад: MasterVolume, SFXVolume, MenuVolume, MusicVolume)
    public string masterVolumeParam = "MasterVolume";
    public string sfxVolumeParam = "SFXVolume";
    public string menuVolumeParam = "MenuVolume"; 
    public string musicVolumeParam = "MusicVolume";

    [Header("Other Settings")]
    [Tooltip("UI Toggle для увімкнення/вимкнення вібрації телефону.")]
    public Toggle vibrationToggle; 
    // Публічний метод для зручної перевірки стану вібрації з інших скриптів
    public bool IsVibrationEnabled() { return PlayerPrefs.GetInt(VIBRATION_TOGGLE_KEY, 1) == 1; }

    [Header("FPS Settings")]
    [Tooltip("TMP Dropdown для вибору частоти кадрів.")]
    public TMP_Dropdown fpsDropdown; 

    // Константи для збереження ключів у PlayerPrefs
    private const string MASTER_SOUND_TOGGLE_KEY = "MasterSoundToggle";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MENU_VOLUME_KEY = "MenuVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string VIBRATION_TOGGLE_KEY = "VibrationToggle";
    private const string FPS_KEY = "TargetFPS";

    // Масив значень FPS, що відповідають індексам опцій Dropdown
    // Переконайтеся, що ці значення відповідають порядку опцій у вашому Dropdown в Inspector!
    // Dropdown Index 0 -> 30 FPS
    // Dropdown Index 1 -> 60 FPS
    // Dropdown Index 2 -> 120 FPS
    private int[] fpsOptions = { 30, 60, 120 }; 

    void Awake()
    {
        // Логіка Singleton: перевіряємо, чи вже існує інстанс цього менеджера.
        // Це запобігає створенню дублікатів, якщо сцена завантажується повторно,
        // і гарантує, що існує лише один "SettingsManager" протягом життя гри.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Якщо інстанс вже є, знищуємо цей новий об'єкт
            return; // Виходимо з методу, щоб уникнути подальшої ініціалізації дубліката
        }
        Instance = this; // Встановлюємо цей об'єкт як єдиний інстанс
        DontDestroyOnLoad(gameObject); // Забезпечуємо, що він не знищується при зміні сцен

        // Прив'язуємо обробники подій до UI елементів
        if (masterSoundToggle != null) masterSoundToggle.onValueChanged.AddListener(SetMasterSound);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (menuSoundSlider != null) menuSoundSlider.onValueChanged.AddListener(SetMenuSoundVolume);
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (vibrationToggle != null) vibrationToggle.onValueChanged.AddListener(SetVibration);

        // Прив'язуємо обробник подій до Dropdown.onValueChanged
        if (fpsDropdown != null) fpsDropdown.onValueChanged.AddListener(SetTargetFPSFromDropdown);

        LoadSettings(); // Завантажуємо налаштування при старті гри (перший Awake)
    }

    // --- Методи для керування звуком ---

    public void SetMasterSound(bool isEnabled)
    {
        // AudioMixer працює з логарифмічними значеннями в dB.
        // -80f dB це майже повна тиша, 0f dB це повна гучність.
        float volume = isEnabled ? 0f : -80f; 
        if (masterMixer != null)
        {
            bool success = masterMixer.SetFloat(masterVolumeParam, volume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{masterVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetInt(MASTER_SOUND_TOGGLE_KEY, isEnabled ? 1 : 0);
            PlayerPrefs.Save(); // Зберігаємо негайно
            Debug.Log($"Master Sound: {isEnabled}. Volume set to {volume} dB.");
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dbVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20; 
            bool success = masterMixer.SetFloat(sfxVolumeParam, dbVolume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{sfxVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save(); // Зберігаємо негайно
        }
    }

    public void SetMenuSoundVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dbVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20;
            bool success = masterMixer.SetFloat(menuVolumeParam, dbVolume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{menuVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetFloat(MENU_VOLUME_KEY, volume);
            PlayerPrefs.Save(); // Зберігаємо негайно
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dbVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20;
            bool success = masterMixer.SetFloat(musicVolumeParam, dbVolume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{musicVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
            PlayerPrefs.Save(); // Зберігаємо негайно
        }
    }

    // --- Методи для інших налаштувань ---

    public void SetVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt(VIBRATION_TOGGLE_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save(); // Зберігаємо негайно
        Debug.Log($"Vibration: {isEnabled}");
        // Тут можна додати реальний код для вібрації, якщо це мобільна гра
        // (наприклад, Handheld.Vibrate(); якщо isEnabled == true і ви хочете короткочасну вібрацію при перемиканні)
    }

    // Метод для встановлення FPS з Dropdown
    public void SetTargetFPSFromDropdown(int dropdownIndex)
    {
        if (dropdownIndex >= 0 && dropdownIndex < fpsOptions.Length)
        {
            int selectedFPS = fpsOptions[dropdownIndex];
            Application.targetFrameRate = selectedFPS;
            PlayerPrefs.SetInt(FPS_KEY, selectedFPS);
            PlayerPrefs.Save(); // Зберігаємо негайно
            Debug.Log($"Target FPS set to: {selectedFPS} (from Dropdown index {dropdownIndex})");
        }
        else
        {
            Debug.LogWarning($"Invalid FPS Dropdown index: {dropdownIndex}. No FPS change.");
        }
    }

    // Цей метод можна використовувати, якщо потрібно встановити FPS програмно (не з Dropdown)
    public void SetTargetFPS(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(FPS_KEY, fps);
        PlayerPrefs.Save(); // Зберігаємо негайно
        Debug.Log($"Target FPS set to: {fps}");
    }

    // --- Завантаження налаштувань ---
    // Цей метод викликається в Awake(), а також з MainMenuManager, коли відкривається панель налаштувань.
    public void LoadSettings()
    {
        // Завантажуємо Master Sound Toggle
        bool masterSoundEnabled = PlayerPrefs.GetInt(MASTER_SOUND_TOGGLE_KEY, 1) == 1; 
        if (masterSoundToggle != null) masterSoundToggle.isOn = masterSoundEnabled;
        // Важливо: викликаємо SetMasterSound, щоб застосувати значення до AudioMixer та PlayerPrefs (навіть якщо вони вже встановлені, це забезпечує синхронізацію UI)
        SetMasterSound(masterSoundEnabled); 

        // Завантажуємо гучність SFX
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f); 
        if (sfxSlider != null) sfxSlider.value = sfxVolume;
        SetSFXVolume(sfxVolume);

        // Завантажуємо гучність Menu Sound
        float menuVolume = PlayerPrefs.GetFloat(MENU_VOLUME_KEY, 1f);
        if (menuSoundSlider != null) menuSoundSlider.value = menuVolume;
        SetMenuSoundVolume(menuVolume);

        // Завантажуємо гучність Music
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        if (musicSlider != null) musicSlider.value = musicVolume;
        SetMusicVolume(musicVolume);

        // Завантажуємо вібрацію
        bool vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_TOGGLE_KEY, 1) == 1;
        if (vibrationToggle != null) vibrationToggle.isOn = vibrationEnabled;
        SetVibration(vibrationEnabled);

        // Завантажуємо та встановлюємо FPS з Dropdown
        int loadedFPS = PlayerPrefs.GetInt(FPS_KEY, 60); // За замовчуванням 60 FPS

        // Знаходимо відповідний індекс в Dropdown для завантаженого FPS
        int dropdownIndexToSet = -1;
        for (int i = 0; i < fpsOptions.Length; i++)
        {
            if (fpsOptions[i] == loadedFPS)
            {
                dropdownIndexToSet = i;
                break;
            }
        }

        if (fpsDropdown != null)
        {
            if (dropdownIndexToSet != -1)
            {
                // Встановлюємо вибране значення в Dropdown UI
                fpsDropdown.value = dropdownIndexToSet; 
            }
            else
            {
                // Якщо завантажений FPS не знайдено в опціях, встановлюємо дефолтне значення (наприклад, 60 FPS)
                // Індекс 1 відповідає 60 FPS в нашому fpsOptions масиві
                fpsDropdown.value = 1; // Встановлюємо на UI Dropdown
                loadedFPS = 60; // Переконайтеся, що FPS також оновлюється
            }
            // Застосовуємо FPS до гри
            SetTargetFPS(loadedFPS); // Цей виклик також збереже значення в PlayerPrefs
        } else {
            // Якщо Dropdown не призначений, все одно встановлюємо FPS для гри
            SetTargetFPS(loadedFPS);
        }
        
        Debug.Log("Settings loaded.");
    }
}