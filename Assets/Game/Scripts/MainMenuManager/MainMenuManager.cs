using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems; // <<< НОВЕ: Для обробки подій UI (перетягування)

// Додаємо інтерфейси для обробки перетягування
public class MainMenuManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
{
    [Header("Main Menu Buttons")]
    public Button settingsButton;
    public Button shopButton;

    [Header("Panels")]
    public GameObject settingsPanel;
    public Button settingsCloseButton; 

    public GameObject shopPanel;
    public Button shopCloseButton; 

    [Header("Campaign Panel - UI Elements")]
    public GameObject campaignPanel; 
    public TextMeshProUGUI levelNameText; 
    public Button levelPlayButton; 
    
    // <<< ЗМІНА: Кнопки стрілок видалені, додано контейнер для свайпу >>>
    [Tooltip("RectTransform, який є батьківським для всіх карток рівнів.")]
    public RectTransform levelsContainer; // Контейнер, який буде переміщатися
    [Tooltip("Ширина однієї картки рівня. Важливо, щоб вона була однаковою для всіх карток!")]
    public float levelCardWidth = 1000f; // Приклад: встановіть ширину вашої картки
    [Tooltip("Швидкість прив'язки картки після свайпу.")]
    public float snapSpeed = 5f;
    [Tooltip("Мінімальна відстань перетягування для спрацьовування свайпу.")]
    public float swipeThreshold = 50f; // Скільки пікселів потрібно перетягнути для зміни рівня
    // ----------------------------------------------------------------------

    [Tooltip("Список ІНДЕКСІВ сцен (карток) у Build Settings в порядку проходження кампанії.")]
    public List<int> levelSceneBuildIndices; 
    private int currentLevelIndex = 0; 

    // Змінні для логіки перетягування
    private Vector2 dragStartMousePosition;
    private Vector2 dragCurrentContainerPosition;
    private Vector2 targetContainerPosition;
    private bool isDragging = false;

    void Awake()
    {
        Time.timeScale = 1.0f;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (campaignPanel != null) campaignPanel.SetActive(false); 

        // Призначення обробників для кнопок основного меню
        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettingsPanel);
        if (shopButton != null) shopButton.onClick.AddListener(ShowShopPanel);

        // Призначення обробників для кнопок закриття панелей
        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(HideAllPanels);
        if (shopCloseButton != null) shopCloseButton.onClick.AddListener(HideAllPanels);

        // Призначення обробника для кнопки Play
        if (levelPlayButton != null) levelPlayButton.onClick.AddListener(PlaySelectedLevel);

        // Ініціалізуємо цільову позицію контейнера
        if (levelsContainer != null)
        {
            targetContainerPosition = levelsContainer.anchoredPosition;
        }
    }

    void Start()
    {
        ShowCampaignPanel();
    }

    void Update()
    {
        // Плавно переміщуємо контейнер до цільової позиції, якщо не відбувається перетягування
        if (!isDragging && levelsContainer != null)
        {
            levelsContainer.anchoredPosition = Vector2.Lerp(levelsContainer.anchoredPosition, targetContainerPosition, Time.deltaTime * snapSpeed);
        }
    }

    // --- Методи для UI панелей ---
    public void ShowSettingsPanel()
    {
        HideAllPanels(); 
        if (settingsPanel != null) settingsPanel.SetActive(true);
        Debug.Log("MainMenuManager: Показано панель налаштувань.");
    }

    public void ShowShopPanel()
    {
        HideAllPanels(); 
        if (shopPanel != null) shopPanel.SetActive(true);
        Debug.Log("MainMenuManager: Показано панель магазину.");
    }

    public void ShowCampaignPanel()
    {
        HideAllPanels(); 
        if (campaignPanel != null) campaignPanel.SetActive(true);
        Debug.Log("MainMenuManager: Показано панель кампанії.");
        UpdateLevelDisplay(); 
        // При стартовій ініціалізації встановлюємо контейнер на поточний рівень
        if (levelsContainer != null && levelSceneBuildIndices != null && levelSceneBuildIndices.Count > 0)
        {
            targetContainerPosition = new Vector2(-currentLevelIndex * levelCardWidth, levelsContainer.anchoredPosition.y);
            levelsContainer.anchoredPosition = targetContainerPosition; // Одразу перемістити
        }
    }

    public void HideAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (campaignPanel != null) campaignPanel.SetActive(false); 
        Debug.Log("MainMenuManager: Приховано всі додаткові панелі.");
    }

    // --- Методи для завантаження сцен ---
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"MainMenuManager: Індекс сцени '{sceneIndex}' недійсний або не доданий до Build Settings!");
            return;
        }
        SceneManager.LoadScene(sceneIndex);
        Debug.Log($"MainMenuManager: Завантажую сцену за індексом: {sceneIndex}");
    }

    // --- Методи для логіки кампанії ---
    private void UpdateLevelDisplay()
    {
        if (levelNameText == null || levelSceneBuildIndices == null || levelSceneBuildIndices.Count == 0)
        {
            if (levelNameText != null) levelNameText.text = "Немає карт";
            if (levelPlayButton != null) levelPlayButton.interactable = false; 
            return;
        }

        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelSceneBuildIndices.Count - 1);

        string scenePath = SceneUtility.GetScenePathByBuildIndex(levelSceneBuildIndices[currentLevelIndex]);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath); 

        levelNameText.text = $"Рівень {currentLevelIndex + 1}: {sceneName}"; 
        
        bool isLevelPlayable = true; 
        
        if (levelPlayButton != null) levelPlayButton.interactable = isLevelPlayable;
    }

    public void PlaySelectedLevel()
    {
        if (levelSceneBuildIndices != null && currentLevelIndex >= 0 && currentLevelIndex < levelSceneBuildIndices.Count)
        {
            LoadSceneByIndex(levelSceneBuildIndices[currentLevelIndex]);
        }
        else
        {
            Debug.LogWarning("MainMenuManager: Немає обраного рівня для запуску.");
        }
    }

    // --- Реалізація інтерфейсів перетягування ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartMousePosition = eventData.position;
        dragCurrentContainerPosition = levelsContainer.anchoredPosition; // Зберігаємо поточну позицію контейнера
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (levelsContainer == null) return;

        float deltaX = eventData.position.x - dragStartMousePosition.x;
        Vector2 newPos = dragCurrentContainerPosition + new Vector2(deltaX, 0);

        // Обмежуємо перетягування, щоб не виходити за межі країв
        float minX = -(levelSceneBuildIndices.Count - 1) * levelCardWidth;
        float maxX = 0f; // Початкова позиція першої картки

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);

        levelsContainer.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        float dragDistance = eventData.position.x - dragStartMousePosition.x;

        // Визначаємо, чи потрібно змінити рівень
        if (Mathf.Abs(dragDistance) > swipeThreshold)
        {
            if (dragDistance < 0) // Свайп вліво (переходимо до наступного рівня)
            {
                currentLevelIndex++;
            }
            else // Свайп вправо (переходимо до попереднього рівня)
            {
                currentLevelIndex--;
            }
        }

        // Обмежуємо індекс рівня
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelSceneBuildIndices.Count - 1);

        // Встановлюємо цільову позицію для прив'язки
        targetContainerPosition = new Vector2(-currentLevelIndex * levelCardWidth, levelsContainer.anchoredPosition.y);
        
        UpdateLevelDisplay(); // Оновлюємо текст назви рівня
    }
}