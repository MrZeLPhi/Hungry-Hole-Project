using TMPro; 
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Текстовий елемент для відображення поточного рівня гравця.")]
    public TextMeshProUGUI levelText;       
    
    [Tooltip("Текстовий елемент для відображення поточного рівня гравця (дублюється, якщо потрібно).")]
    public TextMeshProUGUI currentLevelDisplay; 
    
    [Tooltip("Текстовий елемент для відображення поточних очок гравця.")]
    public TextMeshProUGUI currentPointsDisplay; 
    
    [Tooltip("Текстовий елемент для відображення поточних очок гравця у форматі 'поточні/квота'.")]
    public TextMeshProUGUI scoreQuotaText;      
    
    [Tooltip("Текстовий елемент для відображення розміру/діаметра гравця (залишається, якщо використовується).")]
    public TextMeshProUGUI sizeText;        
    
    [Tooltip("Слайдер для відображення прогресу заповнення рівня.")]
    public Slider levelProgressBar;         

    [Header("Game Progression Reference")]
    [Tooltip("Посилання на GameProgressionManager на сцені. ПОВИННО БУТИ ПРИЗНАЧЕНО В ІНСПЕКТОРІ!")]
    public GameProgressionManager gameProgressionManager; 

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
}