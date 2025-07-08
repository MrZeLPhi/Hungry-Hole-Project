using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Важливо, що він є

// Тепер MainMenuManager не повинен реалізовувати ці інтерфейси напряму,
// якщо він не знаходиться на LevelsContainer і ти використовуєш EventTrigger.
// Якщо ти його переносиш на LevelsContainer, то залишай ці інтерфейси!
// public class MainMenuManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
public class MainMenuManager : MonoBehaviour
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
    
    [Tooltip("RectTransform, який є батьківським для всіх карток рівнів.")]
    public RectTransform levelsContainer; // Контейнер, який буде переміщатися
    [Tooltip("Ширина однієї картки рівня. Важливо, щоб вона була однаковою для всіх карток!")]
    public float levelCardWidth = 700f; // <--- ПЕРЕВІР, ЩО ЦЕ ЗНАЧЕННЯ ПРАВИЛЬНЕ
    [Tooltip("Швидкість прив'язки картки після свайпу.")]
    public float snapSpeed = 5f;
    [Tooltip("Мінімальна відстань перетягування для спрацьовування свайпу.")]
    public float swipeThreshold = 50f; 

    [Tooltip("Список ІНДЕКСІВ сцен (карток) у Build Settings в порядку проходження кампанії.")]
    public List<int> levelSceneBuildIndices; 
    private int currentLevelIndex = 0; 

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

        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettingsPanel);
        if (shopButton != null) shopButton.onClick.AddListener(ShowShopPanel);

        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(HideAllPanels);
        if (shopCloseButton != null) shopCloseButton.onClick.AddListener(HideAllPanels);

        if (levelPlayButton != null) levelPlayButton.onClick.AddListener(PlaySelectedLevel);

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
        if (levelsContainer != null && levelSceneBuildIndices != null && levelSceneBuildIndices.Count > 0)
        {
            targetContainerPosition = new Vector2(-currentLevelIndex * levelCardWidth, levelsContainer.anchoredPosition.y);
            levelsContainer.anchoredPosition = targetContainerPosition; 
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

    // --- ОБГОРТКОВІ МЕТОДИ ДЛЯ EventTrigger ---
    // Ці методи викликаються з EventTrigger і передають дані далі
    public void HandleBeginDrag(BaseEventData eventData)
    {
        OnBeginDrag((PointerEventData)eventData);
    }

    public void HandleDrag(BaseEventData eventData)
    {
        OnDrag((PointerEventData)eventData);
    }

    public void HandleEndDrag(BaseEventData eventData)
    {
        OnEndDrag((PointerEventData)eventData);
    }

    // --- Оригінальні методи обробки перетягування (тепер private або protected) ---
    // Їх сигнатура залишається такою, як була для інтерфейсів.
    // Змінюємо їх доступність на private або protected, оскільки вони не викликаються напряму з EventTrigger.
    private void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartMousePosition = eventData.position;
        dragCurrentContainerPosition = levelsContainer.anchoredPosition; 
    }

    private void OnDrag(PointerEventData eventData)
    {
        if (levelsContainer == null) return;

        float deltaX = eventData.position.x - dragStartMousePosition.x;
        Vector2 newPos = dragCurrentContainerPosition + new Vector2(deltaX, 0);

        // Обмежуємо перетягування, щоб не виходити за межі країв
        float minX = -(levelSceneBuildIndices.Count - 1) * levelCardWidth;
        float maxX = 0f; // Початкова позиція першої картки

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX); // <-- Ось тут відбувається обмеження!

        levelsContainer.anchoredPosition = newPos;
    }

    private void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        float dragDistance = eventData.position.x - dragStartMousePosition.x;

        if (Mathf.Abs(dragDistance) > swipeThreshold)
        {
            if (dragDistance < 0) 
            {
                currentLevelIndex++;
            }
            else 
            {
                currentLevelIndex--;
            }
        }

        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelSceneBuildIndices.Count - 1);

        targetContainerPosition = new Vector2(-currentLevelIndex * levelCardWidth, levelsContainer.anchoredPosition.y);
        
        UpdateLevelDisplay(); 
    }
}