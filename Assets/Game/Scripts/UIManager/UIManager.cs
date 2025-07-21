using TMPro; 
using UnityEngine;
using UnityEngine.UI; 
using System; // Для Math.FloorToInt
using UnityEngine.SceneManagement; // Для SceneManager

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // "Рівень: X"
    public TextMeshProUGUI currentPointsDisplay; // ТЕПЕР: Для відображення ЗАГАЛЬНИХ очок гравця
    public TextMeshProUGUI scoreQuotaText;      // "Y/Z" (поточні очки рівня / квота рівня)
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
    public Button winLoadSceneButton; // НОВА КНОПКА: Для переходу на іншу сцену
    public string winLoadSceneName; // НОВА ЗМІННА: Назва сцени для переходу з панелі виграшу

    [Header("Lose Panel")] 
    [Tooltip("Панель, яка з'явиться при програші. Перетягніть сюди ваш GameObject LosePanel.")]
    public GameObject losePanel; 
    public TextMeshProUGUI loseTimeTakenText; 
    public TextMeshProUGUI loseBestTimeText;  
    public Button loseRestartButton;         
    public Button loseLoadSceneButton; // НОВА КНОПКА: Для переходу на іншу сцену
    public string loseLoadSceneName; // НОВА ЗМІННА: Назва сцени для переходу з панелі програшу

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

        // НОВА ЛОГІКА: Призначення обробників для кнопок переходу на сцену
        if (winLoadSceneButton != null)
        {
            winLoadSceneButton.onClick.AddListener(() => LoadSceneByName(winLoadSceneName));
        }
        if (loseLoadSceneButton != null)
        {
            loseLoadSceneButton.onClick.AddListener(() => LoadSceneByName(loseLoadSceneName));
        }
    }

    void OnEnable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnLevelChanged += UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated += UpdateScoreAndQuotaDisplays; 
            GameProgressionManager.OnTimerUpdated += UpdateTimerDisplay; 
            GameProgressionManager.OnGameLost += ShowLosePanel; 
            GameProgressionManager.OnTotalScoreChanged += UpdateTotalScoreDisplay; 
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
            GameProgressionManager.OnTimerUpdated -= UpdateTimerDisplay; 
            GameProgressionManager.OnGameLost -= ShowLosePanel; 
            GameProgressionManager.OnTotalScoreChanged -= UpdateTotalScoreDisplay; 
            GameProgressionManager.OnGameFinishedWithTime -= HandleGameFinishedWithTime; 
            Debug.Log("UIManager: Відписано від подій GameProgressionManager.");
        }
        if (winRestartButton != null) winRestartButton.onClick.RemoveListener(gameProgressionManager.RestartGame);
        if (loseRestartButton != null) loseRestartButton.onClick.RemoveListener(gameProgressionManager.RestartGame);
        
        // НОВА ЛОГІКА: Відписка від обробників для кнопок переходу на сцену
        if (winLoadSceneButton != null)
        {
            winLoadSceneButton.onClick.RemoveAllListeners(); // Видаляємо всі, щоб уникнути помилок з лямбда-виразами
        }
        if (loseLoadSceneButton != null)
        {
            loseLoadSceneButton.onClick.RemoveAllListeners();
        }
    }

    void Start()
    {
        if (gameProgressionManager != null)
        {
            UpdateLevelDisplay(gameProgressionManager.CurrentLevel);
            UpdateScoreAndQuotaDisplays(gameProgressionManager.CurrentLevelPoints, gameProgressionManager.GetPointsForCurrentLevelQuota());
            UpdateTimerDisplay(gameProgressionManager.gameDurationInSeconds); 
            UpdateTotalScoreDisplay(gameProgressionManager.TotalGameScore); 
        }
        else 
        {
            Debug.LogError("UIManager: GameProgressionManager не призначено в Start! Переконайтеся, що він призначений в Інспекторі.");
            if (levelText != null) levelText.text = "ERR";
            if (currentPointsDisplay != null) currentPointsDisplay.text = "ERR"; 
            if (scoreQuotaText != null) scoreQuotaText.text = "ERR/ERR"; 
            if (levelProgressBar != null) levelProgressBar.value = 0; 
            if (timerText != null) timerText.text = "00:00"; 
        }
        
        if (levelProgressBar != null) levelProgressBar.minValue = 0; 
    }

    private void UpdateLevelDisplay(int newLevel) 
    {
        if (levelText != null)
        {
            levelText.text = newLevel.ToString();
            Debug.Log($"UIManager: Оновлено рівень: {newLevel}");
        }
    }

    private void UpdateScoreAndQuotaDisplays(int currentPoints, int quota) 
    {
        if (scoreQuotaText != null) 
        {
            scoreQuotaText.text = currentPoints.ToString() + "/" + quota.ToString(); 
        }
        
        if (levelProgressBar != null) 
        {
            levelProgressBar.maxValue = quota;
            levelProgressBar.value = currentPoints;
            Debug.Log($"UIManager: Оновлено прогрес рівня: {currentPoints}/{quota}");
        }
    }

    private void UpdateTotalScoreDisplay(int totalScore) 
    {
        if (currentPointsDisplay != null)
        {
            currentPointsDisplay.text = totalScore.ToString();
            Debug.Log($"UIManager: Оновлено загальний рахунок: {totalScore}");
        }
    }

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
        if (winTimeTakenText != null) winTimeTakenText.text = "Time Taken: " + formattedTime;
        // Оновлюємо час для панелі програшу
        if (loseTimeTakenText != null) loseTimeTakenText.text = "Time Taken: " + formattedTime;

        // Обробка найкращого часу
        // PlayerPrefs.GetFloat повертає 0.0f за замовчуванням, якщо ключ не знайдено.
        // float.MaxValue використовується як початкове значення, щоб будь-який перший час був кращим.
        float bestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY, float.MaxValue); 
        if (timeTaken < bestTime) 
        {
            bestTime = timeTaken;
            PlayerPrefs.SetFloat(BEST_TIME_KEY, bestTime); 
            Debug.Log($"UIManager: Новий найкращий час: {formattedTime}");
        }
        // Якщо bestTime все ще float.MaxValue, це означає, що гра ще жодного разу не була завершена.
        string formattedBestTime = (bestTime == float.MaxValue) ? "N/A" : FormatTime(bestTime);

        // Оновлюємо найкращий час для панелі виграшу
        if (winBestTimeText != null) winBestTimeText.text = "Best Time: " + formattedBestTime;
        // Оновлюємо найкращий час для панелі програшу
        if (loseBestTimeText != null) loseBestTimeText.text = "Best Time: " + formattedBestTime;
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

    // НОВИЙ МЕТОД: Для завантаження сцени за назвою
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("UIManager: Назва сцени для завантаження порожня. Будь ласка, вкажіть назву сцени в Інспекторі.");
            return;
        }
        Time.timeScale = 1.0f; // Переконайтеся, що час не зупинений перед завантаженням сцени
        SceneManager.LoadScene(sceneName);
        Debug.Log($"UIManager: Завантажую сцену: {sceneName}");
    }
}