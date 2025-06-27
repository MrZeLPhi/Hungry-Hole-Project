using UnityEngine;
using System;
using System.Collections.Generic; // For List<int>
[System.Serializable] // Дозволяє Unity серіалізувати цей клас і відображати його в Інспекторі
public class LevelData
{
    [Tooltip("Очки, необхідні для переходу з цього рівня на наступний.")]
    public int pointsRequiredForNextLevel;

    [Tooltip("На скільки збільшиться розмір гравця при досягненні цього рівня.")]
    public float sizeIncreaseOnLevelUp;
}

public class GameProgressionManager : MonoBehaviour
{
    // ----- EVENTS -----
    public static event Action<int> OnLevelChanged;
    public static event Action<int, int> OnLevelProgressUpdated; // current points, quota
    public static event Action<float> OnPlayerSizeChanged; // new size

    // <<< НОВИЙ КЛАС LevelData (розмістіть його вище цього рядка у файлі) >>>
    // [System.Serializable]
    // public class LevelData { /* ... */ }

    [Header("Player Reference")]
    public Transform playerHoleTransform;

    [Header("Level Progression Settings")]
    [Tooltip("Початковий рівень гравця.")]
    public int initialLevel = 1;
    
    // <<< ЗМІНА: Тепер це список об'єктів LevelData >>>
    [Tooltip("Дані для кожного рівня: очки та збільшення розміру. " +
             "Індекс 0 = Дані для Рівня 1, Індекс 1 = Дані для Рівня 2, і т.д.")]
    public List<LevelData> levelProgressionData; // Налаштовуйте цей список в Інспекторі!
    // <<< ВИДАЛЕНО: basePointsForLevelUp, pointsQuotaGrowthFactor, fixedSizeIncreasePerLevel >>>

    // ----- Поточний стан гравця -----
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
    public int CurrentLevelPoints // Points accumulated for the current level
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

    // This property remains for convenience, but its value comes from the list
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

    void Awake()
    {
        if (playerHoleTransform == null)
        {
            Debug.LogError("GameProgressionManager: playerHoleTransform не призначений! Розмір гравця не буде змінюватися.");
            enabled = false;
            return;
        }

        // Validate the levelProgressionData list
        if (levelProgressionData == null || levelProgressionData.Count == 0)
        {
            Debug.LogError("GameProgressionManager: Список 'Level Progression Data' порожній або не призначений! Будь ласка, налаштуйте його в Інспекторі.");
            enabled = false;
            return;
        }

        // Initialize values
        CurrentLevel = initialLevel;
        if (CurrentLevel < 1) CurrentLevel = 1; 
        
        CurrentLevelPoints = 0; 
        PlayerCurrentSize = playerHoleTransform.localScale.x; 

        // Initial UI update is handled by UIManager.Start/Awake subscribing to events
        Debug.Log("GameProgressionManager: Ініціалізація системи прогресу завершена.");
    }

    public void AddPoints(int pointsToAdd)
    {
        CurrentLevelPoints += pointsToAdd;
        Debug.Log($"GameProgressionManager: Додано {pointsToAdd} очок. Поточний прогрес: {CurrentLevelPoints}/{GetPointsForCurrentLevelQuota()}");

        // Check if there are more levels defined and if quota is met
        if (CurrentLevel < levelProgressionData.Count && CurrentLevelPoints >= GetPointsForCurrentLevelQuota())
        {
            LevelUp();
        }
        else if (CurrentLevel >= levelProgressionData.Count) // Max level reached
        {
            Debug.Log($"GameProgressionManager: Максимальний рівень ({CurrentLevel}) досягнуто!");
            CurrentLevelPoints = GetPointsForCurrentLevelQuota(); // Cap points for display
        }
    }

    private void LevelUp()
    {
        // Get the size increase amount for the current level (before incrementing CurrentLevel)
        // Adjust index for 0-based list (Level 1 uses index 0, Level 2 uses index 1, etc.)
        int currentLevelIndex = CurrentLevel - 1; 

        if (currentLevelIndex >= 0 && currentLevelIndex < levelProgressionData.Count)
        {
            // Apply size increase from the data of the level just completed
            PlayerCurrentSize += levelProgressionData[currentLevelIndex].sizeIncreaseOnLevelUp;
        }
        else
        {
            Debug.LogWarning($"GameProgressionManager: Дані для рівня {CurrentLevel} не знайдено! Розмір не збільшено.");
        }

        CurrentLevel++; // Increase level
        CurrentLevelPoints = 0; // Reset points for the new level

        Debug.Log($"GameProgressionManager: *** РІВЕНЬ ПІДВИЩЕНО! Новий рівень: {CurrentLevel}, Нова квота: {GetPointsForCurrentLevelQuota()} ***");
    }

    // Helper to get points required for the current level to level up
    // This method is now PUBLIC, as UIManager needs to access it directly.
    public int GetPointsForCurrentLevelQuota() 
    {
        // Adjust index for 0-based list
        int listIndex = CurrentLevel - 1; 

        if (listIndex >= 0 && listIndex < levelProgressionData.Count)
        {
            return levelProgressionData[listIndex].pointsRequiredForNextLevel;
        }
        else if (levelProgressionData.Count > 0)
        {
            // If we're past the defined levels, use the quota of the last defined level
            return levelProgressionData[levelProgressionData.Count - 1].pointsRequiredForNextLevel;
        }
        return 999999; // Fallback to a very high number if no quotas are defined
    }
}