using UnityEngine;
using System; 
using System.Collections.Generic; 

public class GameProgressionManager : MonoBehaviour
{
    // ----- EVENTS -----
    public static event Action<int> OnLevelChanged; 
    public static event Action<int, int> OnLevelProgressUpdated; 
    public static event Action<float> OnPlayerSizeChanged; 

    [System.Serializable] 
    public class LevelData
    {
        [Tooltip("Очки, необхідні для переходу з цього рівня на наступний.")]
        public int pointsRequiredForNextLevel;

        [Tooltip("На скільки збільшиться розмір гравця при досягненні цього рівня.")]
        public float sizeIncreaseOnLevelUp;
    }

    [Header("Player Reference")]
    public Transform playerHoleTransform;

    [Header("Level Progression Settings")]
    public int initialLevel = 1;
    public List<LevelData> levelProgressionData; 

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

    void Awake()
    {
        Application.targetFrameRate = 60;
        Debug.Log("GameProgressionManager: Цільовий FPS встановлено на 60.");

        if (playerHoleTransform == null)
        {
            Debug.LogError("GameProgressionManager: playerHoleTransform не призначений! Розмір гравця не буде змінюватися.");
            enabled = false;
            return;
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

        Debug.Log("GameProgressionManager: Ініціалізація системи прогресу завершена.");
    }

    public void AddPoints(int pointsToAdd)
    {
        CurrentLevelPoints += pointsToAdd;
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
            PlayerCurrentSize += levelProgressionData[currentLevelIndex].sizeIncreaseOnLevelUp;
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
}