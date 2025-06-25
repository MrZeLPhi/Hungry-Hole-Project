using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Follow Settings")]
    public Transform target; // Об'єкт, за яким камера буде слідувати (гравець)
    public float smoothSpeed = 0.125f; // Швидкість плавного слідування (чим менше, тим плавніше)
    public Vector3 offset; // Зміщення камери відносно цілі (гравця)

    void Start()
    {
        // Захоплюємо та приховуємо курсор миші, якщо це необхідно для гри.
        // Зазвичай це робиться для FPS/TPS ігор, де курсор не потрібен на екрані.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // LateUpdate викликається один раз за кадр, після того, як усі Update() методи були викликані.
    // Це ідеально для камер, що слідують за об'єктами, які вже оновили свою позицію в Update().
    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target (гравець) не призначений! Будь ласка, перетягніть гравця в поле 'Target' у Інспекторі.");
            return;
        }

        // Обчислюємо бажану позицію камери
        Vector3 desiredPosition = target.position + offset;

        // Використовуємо Lerp для плавного переходу від поточної позиції до бажаної.
        // Це забезпечує плавне слідування, навіть якщо гравець рухається швидко.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Якщо потрібно, щоб камера завжди дивилася на гравця.
        // Розкоментуй цей рядок, якщо хочеш, щоб камера постійно "дивилася" на гравця.
        // transform.LookAt(target);
    }
}