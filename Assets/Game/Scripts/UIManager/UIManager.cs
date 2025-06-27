using TMPro; 
using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;           // "Рівень: X"
    public TextMeshProUGUI currentLevelDisplay; // <<< НОВЕ: Для відображення поточного рівня гравця окремо
    public TextMeshProUGUI currentPointsDisplay; // <<< НОВЕ: Для відображення поточних очок гравця окремо
    public TextMeshProUGUI scoreQuotaText;      // "Y/Z" (поточні очки / квота)
    public TextMeshProUGUI sizeText;            // Діаметр (якщо ви його ще використовуєте)
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
            // !!! Тепер OnLevelProgressUpdated буде оновлювати кілька полів
            GameProgressionManager.OnLevelProgressUpdated += UpdateScoreAndQuotaDisplays; // <<< Змінено назву методу
            GameProgressionManager.OnPlayerSizeChanged += UpdateSizeDisplay; // Залишаємо, якщо sizeText використовується
            Debug.Log("UIManager: Підписано на події GameProgressionManager.");
        }
    }

    void OnDisable()
    {
        if (gameProgressionManager != null) 
        {
            GameProgressionManager.OnLevelChanged -= UpdateLevelDisplay;
            GameProgressionManager.OnLevelProgressUpdated -= UpdateScoreAndQuotaDisplays; // <<< Змінено назву методу
            GameProgressionManager.OnPlayerSizeChanged -= UpdateSizeDisplay; // Залишаємо, якщо sizeText використовується
            Debug.Log("UIManager: Відписано від подій GameProgressionManager.");
        }
    }

    void Start()
    {
        if (gameProgressionManager != null)
        {
            UpdateLevelDisplay(gameProgressionManager.CurrentLevel);
            // Викликаємо метод оновлення з поточними значеннями для ініціалізації UI
            UpdateScoreAndQuotaDisplays(gameProgressionManager.CurrentLevelPoints, gameProgressionManager.GetPointsForCurrentLevelQuota()); // <<< Змінено назву методу
            UpdateSizeDisplay(gameProgressionManager.PlayerCurrentSize); // Оновлення sizeText, якщо він є
        }
        else 
        {
            Debug.LogError("UIManager: GameProgressionManager не призначено в Start! Переконайтеся, що він призначений в Інспекторі.");
            if (levelText != null) levelText.text = "ERR";
            if (currentLevelDisplay != null) currentLevelDisplay.text = "ERR"; // <<< Оновлено
            if (currentPointsDisplay != null) currentPointsDisplay.text = "ERR"; // <<< Оновлено
            if (scoreQuotaText != null) scoreQuotaText.text = "ERR/ERR"; 
            if (sizeText != null) sizeText.text = "ERR"; 
            if (levelProgressBar != null) levelProgressBar.value = 0;
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
        // Оновлюємо також і нове поле для поточного рівня
        if (currentLevelDisplay != null) // <<< Оновлено
        {
            currentLevelDisplay.text =  newLevel.ToString();
        }
    }

    // <<< Змінено назву методу на UpdateScoreAndQuotaDisplays >>>
    // Цей метод оновлює всі текстові поля, пов'язані з очками та квотою, а також прогрес-бар
    private void UpdateScoreAndQuotaDisplays(int currentPoints, int quota) 
    {
        if (scoreQuotaText != null) // Для формату "поточні/квота"
        {
            scoreQuotaText.text = currentPoints.ToString() + "/" + quota.ToString(); 
        }
        
        if (currentPointsDisplay != null) // <<< Оновлено: Тільки поточні очки
        {
            currentPointsDisplay.text = currentPoints.ToString() + "/" + quota.ToString(); 
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
            sizeText.text = displaySize.ToString();
            Debug.Log($"UIManager: Оновлено діаметр: {displaySize}");
        }
    }
}