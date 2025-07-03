using TMPro; 
using UnityEngine;
using UnityEngine.UI; 
using System; // Для Math.FloorToInt

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // "Рівень: X"
    public TextMeshProUGUI currentPointsDisplay; // ТЕПЕР: Для відображення ЗАГАЛЬНИХ очок гравця
    public TextMeshProUGUI scoreQuotaText;      // "Y/Z" (поточні очки рівня / квота рівня)
    // public TextMeshProUGUI sizeText;            // ВИДАЛЕНО: Діаметр
    // public TextMeshProUGUI currentLevelDisplay; // ВИДАЛЕНО: Поточний рівень гравця окремо
    public Slider levelProgressBar;         

    [Header("Game Timer UI")] 
    public TextMeshProUGUI timerText; // Текст для відображення таймера

    [Header("Game Progression Reference")]
    [Tooltip("Посилання на GameProgressionManager на сцені. ПОВИННО БУТИ ПРИЗНАЧЕНО В ІНСПЕКТОРІ!")]
    public GameProgressionManager gameProgressionManager; 

    [Header("Win Panel")]
    [Tooltip("Панель, яка з'явиться при виграші. Перетягніть сюди ваш GameObject WinPanel.")]
    public GameObject winPanel; 
    public TextMeshProUGUI winTimeTakenText; 
    public TextMeshProUGUI winBestTimeText;  
    public Button winRestartButton;         

    [Header("Lose Panel")] 
    [Tooltip("Панель, яка з'явиться при програші. Перетягніть сюди ваш GameObject LosePanel.")]
    public GameObject losePanel; 
    public TextMeshProUGUI loseTimeTakenText; 
    public TextMeshProUGUI loseBestTimeText;  
    public Button loseRestartButton;         

    private const string BEST_TIME_KEY = "BestGameTime"; 

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
        if (losePanel != null) 
        {
            losePanel.SetActive(false);
        }

        if (winRestartButton != null)
        {
            winRestartButton.onClick.AddListener(gameProgressionManager.RestartGame);
        }
        if (loseRestartButton != null)
        {
            loseRestartButton.onClick.AddListener(gameProgressionManager.RestartGame);
        }
    }

    void OnEnable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnLevelChanged += UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated += UpdateScoreAndQuotaDisplays; 
            // GameProgressionManager.OnPlayerSizeChanged += UpdateSizeDisplay; // ВИДАЛЕНО: Більше не оновлюємо розмір
            GameProgressionManager.OnTimerUpdated += UpdateTimerDisplay; 
            GameProgressionManager.OnGameLost += ShowLosePanel; 
            GameProgressionManager.OnTotalScoreChanged += UpdateTotalScoreDisplay; // ТЕПЕР: Підписуємося на загальний рахунок
            GameProgressionManager.OnGameFinishedWithTime += HandleGameFinishedWithTime; 
            Debug.Log("UIManager: Підписано на події GameProgressionManager.");
        }
    }

    void OnDisable()
    {
        if (gameProgressionManager != null) 
        {
            GameProgressionManager.OnLevelChanged -= UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated -= UpdateScoreAndQuotaDisplays;
            // GameProgressionManager.OnPlayerSizeChanged -= UpdateSizeDisplay; // ВИДАЛЕНО: Більше не відписуємося від розміру
            GameProgressionManager.OnTimerUpdated -= UpdateTimerDisplay; 
            GameProgressionManager.OnGameLost -= ShowLosePanel; 
            GameProgressionManager.OnTotalScoreChanged -= UpdateTotalScoreDisplay; // ТЕПЕР: Відписуємося від загального рахунку
            GameProgressionManager.OnGameFinishedWithTime -= HandleGameFinishedWithTime; 
            Debug.Log("UIManager: Відписано від подій GameProgressionManager.");
        }
        if (winRestartButton != null) winRestartButton.onClick.RemoveListener(gameProgressionManager.RestartGame);
        if (loseRestartButton != null) loseRestartButton.onClick.RemoveListener(gameProgressionManager.RestartGame);
    }

    void Start()
    {
        if (gameProgressionManager != null)
        {
            UpdateLevelDisplay(gameProgressionManager.CurrentLevel);
            UpdateScoreAndQuotaDisplays(gameProgressionManager.CurrentLevelPoints, gameProgressionManager.GetPointsForCurrentLevelQuota());
            // UpdateSizeDisplay(gameProgressionManager.PlayerCurrentSize); // ВИДАЛЕНО
            UpdateTimerDisplay(gameProgressionManager.gameDurationInSeconds); 
            UpdateTotalScoreDisplay(gameProgressionManager.TotalGameScore); // ТЕПЕР: Оновлюємо загальний рахунок при старті
        }
        else 
        {
            Debug.LogError("UIManager: GameProgressionManager не призначено в Start! Переконайтеся, що він призначений в Інспекторі.");
            if (levelText != null) levelText.text = "Рівень: ERR";
            // if (currentLevelDisplay != null) currentLevelDisplay.text = "Поточний Рівень: ERR"; // ВИДАЛЕНО
            if (currentPointsDisplay != null) currentPointsDisplay.text = "Очки: ERR"; 
            if (scoreQuotaText != null) scoreQuotaText.text = "ERR/ERR"; 
            // if (sizeText != null) sizeText.text = "Діаметр: ERR"; // ВИДАЛЕНО
            if (levelProgressBar != null) levelProgressBar.value = 0; 
            if (timerText != null) timerText.text = "00:00"; 
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
        // if (currentLevelDisplay != null) // ВИДАЛЕНО: Більше не оновлюємо окремий показник рівня
        // {
        //     currentLevelDisplay.text = "Поточний Рівень: " + newLevel.ToString();
        // }
    }

    private void UpdateScoreAndQuotaDisplays(int currentPoints, int quota) 
    {
        if (scoreQuotaText != null) 
        {
            scoreQuotaText.text = currentPoints.ToString() + "/" + quota.ToString(); 
        }
        
        // currentPointsDisplay тепер оновлюється через UpdateTotalScoreDisplay
        // if (currentPointsDisplay != null) 
        // {
        //     currentPointsDisplay.text = "Очки: " + currentPoints.ToString(); 
        // }

        if (levelProgressBar != null) 
        {
            levelProgressBar.maxValue = quota;
            levelProgressBar.value = currentPoints;
            Debug.Log($"UIManager: Оновлено прогрес рівня: {currentPoints}/{quota}");
        }
    }

    private void UpdateTotalScoreDisplay(int totalScore) // ТЕПЕР: Цей метод оновлює currentPointsDisplay
    {
        if (currentPointsDisplay != null)
        {
            currentPointsDisplay.text = "Очки: " + totalScore.ToString();
            Debug.Log($"UIManager: Оновлено загальний рахунок: {totalScore}");
        }
    }

    // private void UpdateSizeDisplay(float newSize) // ВИДАЛЕНО: Більше не потрібен
    // {
    //     if (sizeText != null)
    //     {
    //         int displaySize = Mathf.RoundToInt(newSize); 
    //         sizeText.text = "Діаметр: " + displaySize.ToString();
    //         Debug.Log($"UIManager: Оновлено діаметр: {displaySize}");
    //     }
    // }

    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void HandleGameFinishedWithTime(float timeTaken) 
    {
        // Форматуємо час
        string formattedTime = FormatTime(timeTaken);
        
        // Оновлюємо час для панелі виграшу
        if (winTimeTakenText != null) winTimeTakenText.text = "Час гри: " + formattedTime;
        // Оновлюємо час для панелі програшу
        if (loseTimeTakenText != null) loseTimeTakenText.text = "Час гри: " + formattedTime;

        // Обробка найкращого часу
        float bestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY, float.MaxValue); 
        if (timeTaken < bestTime) 
        {
            bestTime = timeTaken;
            PlayerPrefs.SetFloat(BEST_TIME_KEY, bestTime); 
            Debug.Log($"UIManager: Новий найкращий час: {formattedTime}");
        }
        string formattedBestTime = (bestTime == float.MaxValue) ? "N/A" : FormatTime(bestTime);

        // Оновлюємо найкращий час для панелі виграшу
        if (winBestTimeText != null) winBestTimeText.text = "Кращий час: " + formattedBestTime;
        // Оновлюємо найкращий час для панелі програшу
        if (loseBestTimeText != null) loseTimeTakenText.text = "Кращий час: " + formattedBestTime;
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
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
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

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
        if (timerText != null) timerText.gameObject.SetActive(false);
    }
}