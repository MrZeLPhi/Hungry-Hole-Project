using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f; // Швидкість пересування

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Отримуємо вхідні дані з клавіатури (WASD або стрілки)
        float horizontalInput = Input.GetAxis("Horizontal"); // 'A'/'D' або ліва/права стрілка
        float verticalInput = Input.GetAxis("Vertical");   // 'W'/'S' або верхня/нижня стрілка

        // Створюємо вектор напрямку руху відносно гравця
        // transform.right - це вектор "вправо" від гравця
        // transform.forward - це вектор "вперед" від гравця
        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;

        // Нормалізуємо вектор, щоб швидкість не була вищою при русі по діагоналі
        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        // Переміщаємо гравця, використовуючи transform.Translate.
        // Time.deltaTime забезпечує плавність руху незалежно від частоти кадрів.
        // Space.World означає, що рух відбувається по глобальних осях світу.
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}