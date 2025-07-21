using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Об'єкт гравця, відносно якого буде обертатися стрілка.")]
    public Transform playerTransform;
    [Tooltip("Об'єкт, на який вказуватиме стрілка (ціль з найвищим рангом).")]
    public Transform targetObjectTransform; 
    [Tooltip("Посилання на Game Progression Manager.")]
    public GameProgressionManager gameProgressionManager;

    [Header("Visuals")]
    [Tooltip("Дочірній об'єкт, який є візуальною частиною стрілки. Його Active/Inactive буде контролюватися.")]
    public GameObject arrowVisualObject; 

    [Header("Indicator Settings")]
    [Tooltip("Мінімальна базова відстань стрілки від центру гравця.")]
    public float minDistanceFromPlayer = 3.0f; 
    
    // <<< НОВЕ: Відстань залежить від відсотка розміру гравця >>>
    [Tooltip("Відсоток від розміру гравця, на який стрілка додатково віддаляється. 1.0 = 100% від розміру гравця.")]
    [Range(0f, 2f)] // Дозволяє від 0% до 200% розміру гравця
    public float percentageDistanceFromPlayerSize = 1.0f; 
    // -------------------------------------------------------------
    
    [Tooltip("Відстань до цілі, при якій стрілка зникає.")]
    public float disappearanceDistance = 7.0f; 

    private Collectable targetCollectableComponent; 
    private float currentCalculatedDistance; 

    void Awake()
    {
        if (arrowVisualObject == null)
        {
            Debug.LogError("TargetIndicator: Візуальний об'єкт стрілки (Arrow Visual Object) не призначений! Стрілка не буде відображатися.");
            enabled = false; 
            return;
        }
        arrowVisualObject.SetActive(false); 
        
        if (playerTransform == null)
        {
            Debug.LogError("TargetIndicator: Player Transform не призначений!");
            enabled = false;
        }
        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null) Debug.LogError("TargetIndicator: GameProgressionManager не знайдено.");
        }
    }

    void OnEnable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnLevelChanged += UpdateIndicatorState; 
            GameProgressionManager.OnPlayerSizeChanged += UpdateIndicatorStateWithPlayerSize; 
        }
    }

    void OnDisable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnLevelChanged -= UpdateIndicatorState;
            GameProgressionManager.OnPlayerSizeChanged -= UpdateIndicatorStateWithPlayerSize; 
        }
        if (arrowVisualObject != null) arrowVisualObject.SetActive(false); 
    }

    public void SetTarget(Collectable targetCollectable)
    {
        if (targetCollectable != null)
        {
            targetObjectTransform = targetCollectable.transform;
            targetCollectableComponent = targetCollectable;
            Debug.Log($"TargetIndicator: Ціль встановлено на '{targetCollectable.name}' з рангом {targetCollectable.rank}.");
            if (gameProgressionManager != null) 
            {
                UpdateIndicatorState(gameProgressionManager.CurrentLevel); 
            }
        }
        else
        {
            targetObjectTransform = null;
            targetCollectableComponent = null;
            if (arrowVisualObject != null) arrowVisualObject.SetActive(false); 
            Debug.LogWarning("TargetIndicator: Ціль скинуто (null).");
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null || targetObjectTransform == null || !enabled || gameProgressionManager == null || targetCollectableComponent == null || arrowVisualObject == null)
        {
            if (arrowVisualObject != null) arrowVisualObject.SetActive(false); 
            return;
        }

        UpdateIndicatorState(gameProgressionManager.CurrentLevel); 
    }

    void UpdateIndicatorState(int currentLevel) 
    {
        if (playerTransform == null || targetObjectTransform == null || !enabled || gameProgressionManager == null || targetCollectableComponent == null || arrowVisualObject == null)
        {
            if (arrowVisualObject != null) arrowVisualObject.SetActive(false); 
            return;
        }

        // 1. Умови видимості стрілки
        bool playerLevelMet = (currentLevel >= targetCollectableComponent.rank); 
        float distanceToTarget = Vector3.Distance(playerTransform.position, targetObjectTransform.position);
        bool playerIsClose = (distanceToTarget <= disappearanceDistance);

        // 2. Встановлюємо видимість візуального об'єкта стрілки
        if (playerLevelMet && !playerIsClose)
        {
            arrowVisualObject.SetActive(true); 
            // Debug.Log($"TargetIndicator: Стрілка ВКЛЮЧЕНА. Рівень гравця: {currentLevel}, Ранг цілі: {targetCollectableComponent.rank}, Дистанція: {distanceToTarget:F2}");
        }
        else
        {
            arrowVisualObject.SetActive(false); 
            // Debug.Log($"TargetIndicator: Стрілка ВИМКНЕНА. Рівень гравця: {currentLevel}, Ранг цілі: {targetCollectableComponent.rank}, Дистанція: {distanceToTarget:F2}");
        }

        // 3. Якщо стрілка активна, обчислюємо позицію та орієнтацію
        if (arrowVisualObject.activeSelf) 
        {
            // <<< ВИПРАВЛЕНО: Оновлюємо динамічну відстань від гравця за ВІДСОТКОМ від його розміру >>>
            currentCalculatedDistance = minDistanceFromPlayer + (gameProgressionManager.PlayerCurrentSize * percentageDistanceFromPlayerSize);
            // -----------------------------------------------------------------------------------------
            currentCalculatedDistance = Mathf.Min(currentCalculatedDistance, distanceToTarget - 1.0f); 
            currentCalculatedDistance = Mathf.Max(currentCalculatedDistance, 0.1f); 

            // Позиція стрілки
            Vector3 directionToTarget = (targetObjectTransform.position - playerTransform.position).normalized;
            transform.position = playerTransform.position + directionToTarget * currentCalculatedDistance;

            // Орієнтація стрілки
            transform.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
            // Залежно від вашої картинки стрілки, можливо, знадобиться додаткове обертання:
            // transform.rotation *= Quaternion.Euler(0, 90, 0); 
        }
    }

    void UpdateIndicatorStateWithPlayerSize(float playerCurrentSize)
    {
        UpdateIndicatorState(gameProgressionManager.CurrentLevel);
    }

    void CheckIndicatorVisibility(int currentLevel) 
    {
        UpdateIndicatorState(currentLevel); 
    }
}