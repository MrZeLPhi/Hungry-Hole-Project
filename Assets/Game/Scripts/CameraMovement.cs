using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Об'єкт, за яким буде слідувати камера (ваш гравець/отвір).")]
    public Transform target; 

    [Header("Follow Settings")]
    [Tooltip("Швидкість, з якою камера плавно рухається до цілі.")]
    public float smoothSpeed = 0.125f; 
    [Tooltip("Початкове зміщення камери від цілі.")]
    public Vector3 initialOffset; 

    [Header("Dynamic Offset Settings")] 
    [Tooltip("Базовий множник для зміщення камери, коли розмір гравця = 1.")]
    public float baseOffsetMultiplier = 1.0f; 
    [Tooltip("Множник, який визначає, наскільки сильно розмір гравця впливає на ВІДДАЛЕННЯ камери.")]
    public float sizeToOffsetMultiplier = 1.0f; 

    [Header("Game Progression Reference")]
    public GameProgressionManager gameProgressionManager; 
    
    private Vector3 currentDesiredOffset; 
    private Transform mainCameraTransform; 

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError("CameraMovement: Ціль (target) не призначена! Будь ласка, перетягніть об'єкт гравця в Inspector.");
            enabled = false; 
            return;
        }

        if (initialOffset == Vector3.zero)
        {
            initialOffset = transform.position - target.position;
            Debug.Log($"CameraMovement: InitialOffset розраховано: {initialOffset}");
        }

        if (Camera.main != null) 
        {
            mainCameraTransform = Camera.main.transform;
            Debug.Log($"CameraMovement: Основна камера знайдена: {mainCameraTransform.name}");
        }
        else
        {
            Debug.LogError("CameraMovement: Основна камера не знайдена. Переконайтеся, що вона має тег 'MainCamera'.");
            enabled = false;
        }

        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null)
            {
                Debug.LogError("CameraMovement: GameProgressionManager не знайдено на сцені! Динамічна відстань камери не працюватиме.");
            }
            else
            {
                Debug.Log("CameraMovement: GameProgressionManager знайдено.");
            }
        }
        
        // Обчислюємо початкове бажане зміщення камери
        float initialPlayerSize = (gameProgressionManager != null) ? gameProgressionManager.PlayerCurrentSize : target.localScale.x;
        float initialOffsetCalcMultiplier = baseOffsetMultiplier + (initialPlayerSize * sizeToOffsetMultiplier);
        currentDesiredOffset = initialOffset.normalized * initialOffset.magnitude * initialOffsetCalcMultiplier;
        Debug.Log($"CameraMovement: Початкове currentDesiredOffset розраховано: {currentDesiredOffset}");
    }

    void OnEnable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnPlayerSizeChanged += UpdateCameraOffset;
            Debug.Log("CameraMovement: Підписано на подію GameProgressionManager.OnPlayerSizeChanged.");
        }
    }

    void OnDisable()
    {
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnPlayerSizeChanged -= UpdateCameraOffset;
            Debug.Log("CameraMovement: Відписано від подій GameProgressionManager.OnPlayerSizeChanged.");
        }
    }

    void LateUpdate()
    {
        if (target == null || mainCameraTransform == null || gameProgressionManager == null || !enabled) 
        {
            return;
        }

        // --- ЛОГІКА РУХУ КАМЕРИ ---
        Vector3 desiredPosition = target.position + currentDesiredOffset;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime * 5f); 

        transform.LookAt(target); 
        
        // Debug.Log($"CameraMovement: LateUpdate. PlayerPos: {target.position}, DesiredPos: {desiredPosition}, CurrentCamPos: {transform.position}");
    }

    private void UpdateCameraOffset(float newPlayerSize) 
    {
        float currentOffsetMultiplier = baseOffsetMultiplier + (newPlayerSize * sizeToOffsetMultiplier);
        currentDesiredOffset = initialOffset.normalized * initialOffset.magnitude * currentOffsetMultiplier;
        Debug.Log($"CameraMovement: Розмір гравця змінився до {newPlayerSize:F2}. Нове бажане зміщення камери: {currentDesiredOffset}.");
    }
}