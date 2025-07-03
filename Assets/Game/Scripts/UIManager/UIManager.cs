using TMPro; 
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // "Рівень: X"
    public TextMeshProUGUI currentLevelDisplay; // Для відображення поточного рівня гравця окремо
    public TextMeshProUGUI currentPointsDisplay; // Для відображення поточних очок гравця окремо
    public TextMeshProUGUI scoreQuotaText;      // "Y/Z" (поточні очки / квота)
    public TextMeshProUGUI sizeText;            // Діаметр (якщо ви його ще використовуєте)
    public Slider levelProgressBar;         

    [Header("Game Progression Reference")]
    [Tooltip("Посилання на GameProgressionManager на сцені. ПОВИННО БУТИ ПРИЗНАЧЕНО В ІНСПЕКТОРІ!")]
    public GameProgressionManager gameProgressionManager; 

    [Header("Win Panel")]
    [Tooltip("Панель, яка з'явиться при виграші. Перетягніть сюди ваш GameObject WinPanel.")]
    public GameObject winPanel; // Панель виграшу

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
        // Переконайтеся, що панель виграшу вимкнена на старті
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnLevelChanged += UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated += UpdateScoreAndQuotaDisplays; 
            GameProgressionManager.OnPlayerSizeChanged += UpdateSizeDisplay; 
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
}