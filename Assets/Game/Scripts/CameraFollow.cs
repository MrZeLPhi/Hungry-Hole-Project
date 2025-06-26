using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Об'єкт, за яким буде слідувати камера (ваш гравець/отвір).")]
    public Transform target; // Посилання на об'єкт гравця (отвору)

    [Header("Follow Settings")]
    [Tooltip("Швидкість, з якою камера плавно рухається до цілі.")]
    public float smoothSpeed = 0.125f; 
    [Tooltip("Початкове зміщення камери від цілі.")]
    public Vector3 initialOffset; // Початкове зміщення камери від центру гравця

    [Header("Dynamic Offset Settings")] // Змінив назву розділу
    [Tooltip("Базовий множник для зміщення камери, коли розмір гравця = 1.")]
    public float baseOffsetMultiplier = 1.0f; // Скільки початкового офсету зберігається
    [Tooltip("Множник, який визначає, наскільки сильно розмір гравця впливає на ВІДДАЛЕННЯ камери.")]
    public float sizeToOffsetMultiplier = 1.0f; // Чим більше, тим сильніше віддаляється за розміром

    private Vector3 currentDesiredOffset; // Поточне бажане зміщення камери від гравця

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: Ціль (target) не призначена! Будь ласка, перетягніть об'єкт гравця в Inspector.");
            enabled = false;
            return;
        }

        // Якщо initialOffset не встановлено в Inspector (тобто Vector3.zero),
        // ми розраховуємо його з поточної позиції камери відносно цілі.
        if (initialOffset == Vector3.zero)
        {
            initialOffset = transform.position - target.position;
        }

        // Встановлюємо початкове бажане зміщення
        currentDesiredOffset = initialOffset * baseOffsetMultiplier;
    }

    void OnEnable()
    {
        CollectablesManager.OnSizeChanged += UpdateCameraOffset;
        Debug.Log("CameraFollow: Підписано на подію CollectablesManager.OnSizeChanged.");
    }

    void OnDisable()
    {
        CollectablesManager.OnSizeChanged -= UpdateCameraOffset;
        Debug.Log("CameraFollow: Відписано від події CollectablesManager.OnSizeChanged.");
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Бажана позиція камери = позиція гравця + поточне бажане зміщення
        Vector3 desiredPosition = target.position + currentDesiredOffset;

        // Плавно переміщуємо камеру до бажаної позиції
        // smoothSpeed * Time.deltaTime * 10f - множник 10f для того, щоб Lerp був більш помітним за 0.125f.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime * 10f); 
        transform.position = smoothedPosition;

        // <<< transform.LookAt(target); ВИДАЛЕНО >>>
        // Камера не змінює свою орієнтацію, тільки позицію.
    }

    // Метод, який викликається при зміні розміру отвору
    private void UpdateCameraOffset(float newHoleSize)
    {
        // Розраховуємо новий множник для offset на основі нового розміру отвору
        // Чим більший newHoleSize, тим більше буде множник, і тим сильніше камера віддаляється
        float currentOffsetMultiplier = baseOffsetMultiplier + (newHoleSize * sizeToOffsetMultiplier);
        
        // Масштабуємо початкове зміщення на новий множник
        currentDesiredOffset = initialOffset.normalized * initialOffset.magnitude * currentOffsetMultiplier;

        Debug.Log($"CameraFollow: Розмір отвору змінився до {newHoleSize:F2}. Нове бажане зміщення камери: {currentDesiredOffset}.");
    }
}