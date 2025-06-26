using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f; // Швидкість пересування
    [Tooltip("Швидкість, з якою гравець обертається в напрямку руху.")]
    public float rotationSmoothTime = 0.1f; // Час для плавного обертання

    private Vector3 moveDirection; // Зберігаємо напрямок руху для обертання
    private float rotationVelocity; // Допоміжна змінна для плавного обертання (SmoothDampAngle)

    void Update()
    {
        HandleMovement();
        HandleRotation(); // <<< НОВИЙ ВИКЛИК: Обробка обертання
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); 
        float verticalInput = Input.GetAxis("Vertical");   

        // Важливо: обчислюємо напрямок руху на основі глобальних осей,
        // щоб обертання гравця не впливало на WASD-керування
        // Vector3.right - глобальна вісь X
        // Vector3.forward - глобальна вісь Z
        moveDirection = Vector3.right * horizontalInput + Vector3.forward * verticalInput;

        // Нормалізуємо вектор, щоб швидкість не була вищою при русі по діагоналі
        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        // Переміщаємо гравця
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandleRotation()
    {
        // Якщо гравець рухається (величина вектора руху більша за невеликий поріг)
        if (moveDirection.magnitude >= 0.1f) // Використовуємо 0.1f, щоб уникнути обертання при незначних рухах
        {
            // Розраховуємо кут, в який має дивитися об'єкт
            // Mathf.Atan2 повертає кут у радіанах, тому множимо на Mathf.Rad2Deg для градусів
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            // Плавно обертаємо об'єкт до цього кута навколо осі Y
            // transform.eulerAngles.y - поточний кут об'єкта по Y
            // rotationVelocity - допоміжна змінна, яка використовується SmoothDampAngle
            // rotationSmoothTime - час, за який відбувається згладжування обертання
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}