using TMPro; 
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           
    public TextMeshProUGUI currentLevelDisplay; 
    public TextMeshProUGUI currentPointsDisplay; 
    public TextMeshProUGUI scoreQuotaText;      
    public TextMeshProUGUI sizeText;            
    public Slider levelProgressBar;         

    [Header("Game Timer UI")] // <<< НОВИЙ РОЗДІЛ
    public TextMeshProUGUI timerText; // Текст для відображення таймера

    [Header("Game Progression Reference")]
    [Tooltip("Посилання на GameProgressionManager на сцені. ПОВИННО БУТИ ПРИЗНАЧЕНО В ІНСПЕКТОРІ!")]
    public GameProgressionManager gameProgressionManager; 

    [Header("Win Panel")]
    [Tooltip("Панель, яка з'явиться при виграші. Перетягніть сюди ваш GameObject WinPanel.")]
    public GameObject winPanel; 
    
    [Header("Lose Panel")] // <<< НОВИЙ РОЗДІЛ
    [Tooltip("Панель, яка з'явиться при програші. Перетягніть сюди ваш GameObject LosePanel.")]
    public GameObject losePanel; // Панель програшу

    void Awake()
    {
        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null)
            {
                Debug.LogError("UIManager: GameProgressionManager не знайдено на сцені! UI не буде оновлюватися.");
                enabled = false;
            }
        }
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        // Переконайтеся, що панель програшу вимкнена на старті
        if (losePanel != null) // <<< НОВЕ
        {
            losePanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnLevelChanged += UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated += UpdateScoreAndQuotaDisplays; 
            GameProgressionManager.OnPlayerSizeChanged += UpdateSizeDisplay; 
            GameProgressionManager.OnTimerUpdated += UpdateTimerDisplay; // <<< НОВЕ: Підписуємося на оновлення таймера
            GameProgressionManager.OnGameLost += ShowLosePanel; // <<< НОВЕ: Підписуємося на подію програшу
            Debug.Log("UIManager: Підписано на події GameProgressionManager.");
        }
    }

    void OnDisable()
    {
        if (gameProgressionManager != null) 
        {
            GameProgressionManager.OnLevelChanged -= UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated -= UpdateScoreAndQuotaDisplays;
            GameProgressionManager.OnPlayerSizeChanged -= UpdateSizeDisplay;
            GameProgressionManager.OnTimerUpdated -= UpdateTimerDisplay; // <<< НОВЕ
            GameProgressionManager.OnGameLost -= ShowLosePanel; // <<< НОВЕ
            Debug.Log("UIManager: Відписано від подій GameProgressionManager.");
        }
    }

    void Start()
    {
        if (gameProgressionManager != null)
        {
            UpdateLevelDisplay(gameProgressionManager.CurrentLevel);
            UpdateScoreAndQuotaDisplays(gameProgressionManager.CurrentLevelPoints, gameProgressionManager.GetPointsForCurrentLevelQuota());
            UpdateSizeDisplay(gameProgressionManager.PlayerCurrentSize);
            UpdateTimerDisplay(gameProgressionManager.gameDurationInSeconds); // <<< НОВЕ: Ініціалізуємо таймер
        }
        else 
        {
            Debug.LogError("UIManager: GameProgressionManager не призначено в Start! Переконайтеся, що він призначений в Інспекторі.");
            if (levelText != null) levelText.text = "Рівень: ERR";
            if (currentLevelDisplay != null) currentLevelDisplay.text = "Поточний Рівень: ERR"; 
            if (currentPointsDisplay != null) currentPointsDisplay.text = "Очки: ERR"; 
            if (scoreQuotaText != null) scoreQuotaText.text = "ERR/ERR"; 
            if (sizeText != null) sizeText.text = "Діаметр: ERR"; 
            if (levelProgressBar != null) levelProgressBar.value = 0;
            if (timerText != null) timerText.text = "00:00"; // <<< НОВЕ
        }
        
        if (levelProgressBar != null) levelProgressBar.minValue = 0;
    }

    private void UpdateLevelDisplay(int newLevel) 
    {
        if (levelText != null)
        {
            levelText.text = "Рівень: " + newLevel.ToString();
            Debug.Log($"UIManager: Оновлено рівень: {newLevel}");
        }
        if (currentLevelDisplay != null) 
        {
            currentLevelDisplay.text = "Поточний Рівень: " + newLevel.ToString();
        }
    }

    private void UpdateScoreAndQuotaDisplays(int currentPoints, int quota) 
    {
        if (scoreQuotaText != null) 
        {
            scoreQuotaText.text = currentPoints.ToString() + "/" + quota.ToString(); 
        }
        
        if (currentPointsDisplay != null) 
        {
            currentPointsDisplay.text = "Очки: " + currentPoints.ToString(); 
        }

        if (levelProgressBar != null)
        {
            levelProgressBar.maxValue = quota;
            levelProgressBar.value = currentPoints;
            Debug.Log($"UIManager: Оновлено прогрес рівня: {currentPoints}/{quota}");
        }
    }

    private void UpdateSizeDisplay(float newSize) 
    {
        if (sizeText != null)
        {
            int displaySize = Mathf.RoundToInt(newSize); 
            sizeText.text = "Діаметр: " + displaySize.ToString();
            Debug.Log($"UIManager: Оновлено діаметр: {displaySize}");
        }
    }

    // <<< НОВИЙ МЕТОД ДЛЯ ОНОВЛЕННЯ ТАЙМЕРА >>>
    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Debug.Log("UIManager: Панель виграшу активована.");
        }
        else
        {
            Debug.LogError("UIManager: Win Panel не призначено в Інспекторі! Неможливо показати.");
        }
    }

    // <<< НОВИЙ МЕТОД: Показати панель програшу >>>
    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Debug.Log("UIManager: Панель програшу активована.");
        }
        else
        {
            Debug.LogError("UIManager: Lose Panel не призначено в Інспекторі! Неможливо показати.");
        }
    }
}