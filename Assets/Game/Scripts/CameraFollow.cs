using UnityEngine;

public class CameraFollow : MonoBehaviour
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

    // <<< НОВЕ ПОЛЕ: Посилання на GameProgressionManager >>>
    [Header("Game Progression Reference")]
    [Tooltip("Посилання на GameProgressionManager на сцені.")]
    public GameProgressionManager gameProgressionManager; 

    private Vector3 currentDesiredOffset; 

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: Ціль (target) не призначена! Будь ласка, перетягніть об'єкт гравця в Inspector.");
            enabled = false; 
            return;
        }

        if (initialOffset == Vector3.zero)
        {
            initialOffset = transform.position - target.position;
        }

        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null)
            {
                Debug.LogError("CameraFollow: GameProgressionManager не знайдено на сцені! Динамічна відстань камери не працюватиме.");
                // Тут можна вирішити, що робити, якщо менеджер не знайдено. Камера може залишитися на фіксованій відстані.
            }
        }

        // <<< ВИПРАВЛЕНО: Використовуємо target.localScale.x для отримання початкового розміру гравця >>>
        float initialPlayerSize = (gameProgressionManager != null) ? gameProgressionManager.PlayerCurrentSize : target.localScale.x;
        
        float initialOffsetCalcMultiplier = baseOffsetMultiplier + (initialPlayerSize * sizeToOffsetMultiplier);
        currentDesiredOffset = initialOffset.normalized * initialOffset.magnitude * initialOffsetCalcMultiplier;
    }

    void OnEnable()
    {
        // <<< ПРАВИЛЬНА ПІДПИСКА: Тепер на подію GameProgressionManager.OnPlayerSizeChanged >>>
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnPlayerSizeChanged += UpdateCameraOffset;
            Debug.Log("CameraFollow: Підписано на подію GameProgressionManager.OnPlayerSizeChanged.");
        }
    }

    void OnDisable()
    {
        // <<< ПРАВИЛЬНА ВІДПИСКА: З GameProgressionManager.OnPlayerSizeChanged >>>
        if (gameProgressionManager != null)
        {
            GameProgressionManager.OnPlayerSizeChanged -= UpdateCameraOffset;
            Debug.Log("CameraFollow: Відписано від події GameProgressionManager.OnPlayerSizeChanged.");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + currentDesiredOffset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime * 10f); 
        transform.position = smoothedPosition;
    }

    // Метод, який викликається при зміні розміру гравця (отримано від GameProgressionManager)
    private void UpdateCameraOffset(float newPlayerSize) // Змінено ім'я параметра для кращої читабельності
    {
        // Розраховуємо новий множник для offset на основі нового розміру гравця
        float currentOffsetMultiplier = baseOffsetMultiplier + (newPlayerSize * sizeToOffsetMultiplier);
        
        // Масштабуємо початкове зміщення на новий множник
        currentDesiredOffset = initialOffset.normalized * initialOffset.magnitude * currentOffsetMultiplier;

        Debug.Log($"CameraFollow: Розмір гравця змінився до {newPlayerSize:F2}. Нове бажане зміщення камери: {currentDesiredOffset}.");
    }
}