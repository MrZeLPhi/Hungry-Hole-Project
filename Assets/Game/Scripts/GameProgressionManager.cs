using UnityEngine;
using System; // Для System.Action
using System.Collections.Generic; // Для List
using UnityEngine.SceneManagement; // Для SceneManager

public class GameProgressionManager : MonoBehaviour
{
    // ----- EVENTS -----
    public static event Action<int> OnLevelChanged; 
    public static event Action<int, int> OnLevelProgressUpdated; // current points, quota
    public static event Action<float> OnPlayerSizeChanged; // new size
    public static event Action<float> OnTimerUpdated; // Подія для оновлення таймера
    public static event Action OnGameLost; // Подія для програшу
    public static event Action<int> OnTotalScoreChanged; // Подія для загального рахунку
    public static event Action<float> OnGameFinishedWithTime; // Подія для часу завершення гри

    // Це клас, який буде містити дані для кожного рівня
    [System.Serializable] 
    public class LevelData
    {
        [Tooltip("Очки, необхідні для переходу з цього рівня на наступний.")]
        public int pointsRequiredForNextLevel;

        [Tooltip("На скільки збільшиться розмір гравця при досягненні цього рівня.")]
        public float sizeIncreaseOnLevelUp;
    }

    [Header("Player Reference")]
    public Transform playerHoleTransform; // Основний об'єкт гравця (для XZ масштабування)
    
    [Tooltip("Дочірній об'єкт (візуальна модель дірки), масштабування Y якого буде змінюватися окремо.")]
    public Transform holeVisualChildTransform; 

    [Header("Level Progression Settings")]
    public int initialLevel = 1;
    
    [Tooltip("Дані для кожного рівня: очки та збільшення розміру. " +
             "Індекс 0 = Дані для Рівня 1, Індекс 1 = Дані для Рівня 2, і т.д.")]
    public List<LevelData> levelProgressionData; 

    // <<< ВИДАЛЕНО: public float holeDepthIncreasePerLevel; >>>
    // -----------------------------------------------------------

    [Header("Game Timer Settings")] 
    [Tooltip("Тривалість гри в секундах.")]
    public float gameDurationInSeconds = 180f; 
    private float currentTimer; 

    private float totalGameTimeElapsed; 

    [Header("Game Over Settings")]
    [Tooltip("Список об'єктів GameObject, які потрібно вимкнути при закінченні гри (програші/виграші).")]
    public List<GameObject> objectsToDisableOnGameOver;

    // ----- Поточний стан гравця (використовуємо властивості для виклику подій) -----
    private int _currentLevel;
    public int CurrentLevel
    {
        get { return _currentLevel; }
        private set
        {
            if (_currentLevel != value)
            {
                _currentLevel = value;
                OnLevelChanged?.Invoke(_currentLevel);
                Debug.Log($"GameProgressionManager: Рівень гравця оновлено до: {_currentLevel}.");
            }
        }
    }

    private int _currentLevelPoints;
    public int CurrentLevelPoints 
    {
        get { return _currentLevelPoints; }
        private set
        {
            if (_currentLevelPoints != value)
            {
                _currentLevelPoints = value;
                OnLevelProgressUpdated?.Invoke(_currentLevelPoints, GetPointsForCurrentLevelQuota());
            }
        }
    }

    public int PointsForCurrentLevelQuota
    {
        get { return GetPointsForCurrentLevelQuota(); }
    }

    private float _playerCurrentSize;
    public float PlayerCurrentSize
    {
        get { return _playerCurrentSize; }
        private set
        {
            if (_playerCurrentSize != value)
            {
                _playerCurrentSize = value;
                playerHoleTransform.localScale = new Vector3(_playerCurrentSize, playerHoleTransform.localScale.y, _playerCurrentSize);
                OnPlayerSizeChanged?.Invoke(_playerCurrentSize);
                Debug.Log($"GameProgressionManager: Розмір гравця оновлено до: {_playerCurrentSize:F2}.");
            }
        }
    }

    private int _totalGameScore;
    public int TotalGameScore
    {
        get { return _totalGameScore; }
        private set
        {
            if (_totalGameScore != value)
            {
                _totalGameScore = value;
                OnTotalScoreChanged?.Invoke(_totalGameScore); 
                Debug.Log($"GameProgressionManager: Загальний рахунок оновлено до: {_totalGameScore}.");
            }
        }
    }

    void Awake()
    {
        Application.targetFrameRate = 60;
        Debug.Log("GameProgressionManager: Цільовий FPS встановлено на 60.");

        Time.timeScale = 1.0f; 

        if (playerHoleTransform == null)
        {
            Debug.LogError("GameProgressionManager: playerHoleTransform не призначений! Розмір гравця не буде змінюватися.");
            enabled = false;
            return;
        }
        if (holeVisualChildTransform == null)
        {
            Debug.LogWarning("GameProgressionManager: Hole Visual Child Transform не призначений! Глибина дірки не буде збільшуватися.");
        }

        if (levelProgressionData == null || levelProgressionData.Count == 0)
        {
            Debug.LogError("GameProgressionManager: Список 'Level Progression Data' порожній або не призначений! Будь ласка, налаштуйте його в Інспекторі.");
            enabled = false;
            return;
        }

        CurrentLevel = initialLevel;
        if (CurrentLevel < 1) CurrentLevel = 1; 
        
        CurrentLevelPoints = 0; 
        PlayerCurrentSize = playerHoleTransform.localScale.x; 
        TotalGameScore = 0; 

        currentTimer = gameDurationInSeconds;
        OnTimerUpdated?.Invoke(currentTimer); 

        totalGameTimeElapsed = 0f; 

        Debug.Log("GameProgressionManager: Ініціалізація системи прогресу завершена.");
    }

    void Update() 
    {
        if (Time.timeScale > 0)
        {
            totalGameTimeElapsed += Time.deltaTime; 

            if (currentTimer > 0)
            {
                currentTimer -= Time.deltaTime;
                OnTimerUpdated?.Invoke(currentTimer); 
                if (currentTimer <= 0)
                {
                    currentTimer = 0; 
                    EndGameLose(); 
                }
            }
        }
    }

    public void AddPoints(int pointsToAdd)
    {
        CurrentLevelPoints += pointsToAdd;
        TotalGameScore += pointsToAdd; 
        Debug.Log($"GameProgressionManager: Додано {pointsToAdd} очок. Поточний прогрес: {CurrentLevelPoints}/{GetPointsForCurrentLevelQuota()}");

        if (CurrentLevel < levelProgressionData.Count && CurrentLevelPoints >= GetPointsForCurrentLevelQuota())
        {
            LevelUp();
        }
        else if (CurrentLevel >= levelProgressionData.Count) 
        {
            Debug.Log($"GameProgressionManager: Максимальний рівень ({CurrentLevel}) досягнуто!");
            CurrentLevelPoints = GetPointsForCurrentLevelQuota(); 
        }
    }

    private void LevelUp()
    {
        int currentLevelIndex = CurrentLevel - 1; 

        if (currentLevelIndex >= 0 && currentLevelIndex < levelProgressionData.Count)
        {
            float sizeIncrease = levelProgressionData[currentLevelIndex].sizeIncreaseOnLevelUp; // Отримуємо збільшення розміру
            PlayerCurrentSize += sizeIncrease; // Збільшуємо XZ
            
            // <<< НОВЕ: Збільшуємо масштаб Y дочірнього об'єкта симетрично >>>
            if (holeVisualChildTransform != null)
            {
                holeVisualChildTransform.localScale = new Vector3(
                    holeVisualChildTransform.localScale.x,
                    holeVisualChildTransform.localScale.y + sizeIncrease, // ВИКОРИСТОВУЄМО ТЕ Ж ЗНАЧЕННЯ
                    holeVisualChildTransform.localScale.z
                );
                Debug.Log($"GameProgressionManager: Глибина дірки збільшена симетрично до: {holeVisualChildTransform.localScale.y:F2}.");
            }
            // ------------------------------------------------------------------
        }
        else
        {
            Debug.LogWarning($"GameProgressionManager: Дані для рівня {CurrentLevel} не знайдено! Розмір не збільшено.");
        }

        CurrentLevel++; 
        CurrentLevelPoints = 0; 

        Debug.Log($"GameProgressionManager: *** РІВЕНЬ ПІДВИЩЕНО! Новий рівень: {CurrentLevel}, Нова квота: {GetPointsForCurrentLevelQuota()} ***");
    }

    public int GetPointsForCurrentLevelQuota() 
    {
        int listIndex = CurrentLevel - 1; 

        if (listIndex >= 0 && listIndex < levelProgressionData.Count)
        {
            return levelProgressionData[listIndex].pointsRequiredForNextLevel;
        }
        else if (levelProgressionData.Count > 0)
        {
            return levelProgressionData[levelProgressionData.Count - 1].pointsRequiredForNextLevel;
        }
        return 999999; 
    }

    void EndGameLose()
    {
        Debug.Log("GameProgressionManager: Час вийшов! Ви програли.");
        
        DisableObjectsOnGameOver();
        OnGameLost?.Invoke(); 
        OnGameFinishedWithTime?.Invoke(totalGameTimeElapsed); 

        Time.timeScale = 0f; 
    }

    public void DisableObjectsOnGameOver() 
    {
        if (objectsToDisableOnGameOver != null && objectsToDisableOnGameOver.Count > 0)
        {
            foreach (GameObject obj in objectsToDisableOnGameOver)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"GameProgressionManager: Об'єкт '{obj.name}' вимкнено при завершенні гри.");
                }
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1.0f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
        Debug.Log("GameProgressionManager: Сцена перезавантажена.");
    }
}