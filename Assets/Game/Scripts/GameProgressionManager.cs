using UnityEngine;
using System; // Для System.Action
using System.Collections.Generic; // Для List

public class GameProgressionManager : MonoBehaviour
{
    // ----- EVENTS -----
    public static event Action<int> OnLevelChanged; // Передає новий рівень гравця
    public static event Action<int, int> OnLevelProgressUpdated; // Передає поточні очки рівня та квоту
    public static event Action<float> OnPlayerSizeChanged; // Передає новий розмір гравця

    // <<< НОВИЙ ВНУТРІШНІЙ КЛАС ДАНИХ ДЛЯ РІВНЯ >>>
    [System.Serializable] // Дозволяє Unity серіалізувати цей клас і відображати його в Інспекторі
    public class LevelData
    {
        [Tooltip("Очки, необхідні для переходу з цього рівня на наступний.")]
        public int pointsRequiredForNextLevel;

        [Tooltip("На скільки збільшиться розмір гравця при досягненні цього рівня.")]
        public float sizeIncreaseOnLevelUp;
    }
    // ----------------------------------------------------

    [Header("Player Reference")]
    [Tooltip("Посилання на Transform основного об'єкта гравця (дірки).")]
    public Transform playerHoleTransform;

    [Header("Level Progression Settings")]
    [Tooltip("Початковий рівень гравця.")]
    public int initialLevel = 1;
    
    [Tooltip("Дані для кожного рівня: очки та збільшення розміру. " +
             "Індекс 0 = Дані для Рівня 1, Індекс 1 = Дані для Рівня 2, і т.д.")]
    public List<LevelData> levelProgressionData; // Налаштовуйте цей список в Інспекторі!

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
    public int CurrentLevelPoints // Очки, набрані для поточного рівня
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

    // Очки, необхідні для поточного рівня, щоб підвищити рівень
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
                // Застосовуємо новий розмір до Transform гравця (тільки X та Z)
                playerHoleTransform.localScale = new Vector3(_playerCurrentSize, playerHoleTransform.localScale.y, _playerCurrentSize);
                OnPlayerSizeChanged?.Invoke(_playerCurrentSize);
                Debug.Log($"GameProgressionManager: Розмір гравця оновлено до: {_playerCurrentSize:F2}.");
            }
        }
    }

    void Awake()
    {
        // Встановлюємо цільовий FPS на 60 (для оптимізації)
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

        // Ініціалізація початкових значень
        CurrentLevel = initialLevel;
        if (CurrentLevel < 1) CurrentLevel = 1; // Забезпечуємо, що рівень починається щонайменше з 1
        
        CurrentLevelPoints = 0; 
        PlayerCurrentSize = playerHoleTransform.localScale.x; 

        Debug.Log("GameProgressionManager: Ініціалізація системи прогресу завершена.");
    }

    public void AddPoints(int pointsToAdd)
    {
        CurrentLevelPoints += pointsToAdd;
        Debug.Log($"GameProgressionManager: Додано {pointsToAdd} очок. Поточний прогрес: {CurrentLevelPoints}/{GetPointsForCurrentLevelQuota()}");

        // Перевіряємо, чи досягнуто квоту для підвищення рівня, і чи є ще визначені рівні
        if (CurrentLevel < levelProgressionData.Count && CurrentLevelPoints >= GetPointsForCurrentLevelQuota())
        {
            LevelUp();
        }
        else if (CurrentLevel >= levelProgressionData.Count) // Якщо досягнуто максимальний визначений рівень
        {
            Debug.Log($"GameProgressionManager: Максимальний рівень ({CurrentLevel}) досягнуто!");
            CurrentLevelPoints = GetPointsForCurrentLevelQuota(); // Залишаємо очки на максимумі для відображення
        }
    }

    private void LevelUp()
    {
        // Індекс поточного рівня в списку (0-базовий)
        int currentLevelIndex = CurrentLevel - 1; 

        if (currentLevelIndex >= 0 && currentLevelIndex < levelProgressionData.Count)
        {
            // Застосовуємо збільшення розміру, визначене для щойно завершеного рівня
            PlayerCurrentSize += levelProgressionData[currentLevelIndex].sizeIncreaseOnLevelUp;
        }
        else
        {
            Debug.LogWarning($"GameProgressionManager: Дані для рівня {CurrentLevel} не знайдено! Розмір не збільшено.");
        }

        CurrentLevel++; // Збільшуємо рівень
        CurrentLevelPoints = 0; // Обнуляємо очки для нового рівня

        Debug.Log($"GameProgressionManager: *** РІВЕНЬ ПІДВИЩЕНО! Новий рівень: {CurrentLevel}, Нова квота: {GetPointsForCurrentLevelQuota()} ***");
    }

    // Допоміжна функція для отримання квоти очок для поточного рівня
    public int GetPointsForCurrentLevelQuota() // Зроблено public для доступу з UIManager
    {
        int listIndex = CurrentLevel - 1; 

        if (listIndex >= 0 && listIndex < levelProgressionData.Count)
        {
            return levelProgressionData[listIndex].pointsRequiredForNextLevel;
        }
        else if (levelProgressionData.Count > 0)
        {
            // Якщо поточний рівень перевищує кількість визначених рівнів, використовуємо квоту останнього визначеного рівня
            return levelProgressionData[levelProgressionData.Count - 1].pointsRequiredForNextLevel;
        }
        return 999999; // Запасний варіант: дуже високе значення, якщо квоти не визначені
    }
}