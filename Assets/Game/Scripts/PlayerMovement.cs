using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f; // Швидкість пересування
    [Tooltip("Швидкість, з якою гравець обертається в напрямку руху.")]
    public float rotationSmoothTime = 0.1f; 

    // <<< НОВЕ ПОЛЕ: ПОСИЛАННЯ НА ВАШ ДЖОЙСТИК >>>
    [Header("Joystick Control")]
    [Tooltip("Перетягніть сюди ваш Joystick UI елемент з Canvas.")]
    public Joystick joystick; // Посилання на компонент Joystick з пакета

    private Vector3 moveDirection; 
    private float rotationVelocity; 

    void Update()
    {
        HandleMovement();
        HandleRotation(); 
    }

    void HandleMovement()
    {
        // <<< ЗМІНА ТУТ: Отримуємо вхідні дані з джойстика >>>
        // Перевіряємо, чи призначено джойстик, щоб уникнути помилок NullReferenceException
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal; // Отримуємо горизонтальний ввід з джойстика
            verticalInput = joystick.Vertical;     // Отримуємо вертикальний ввід з джойстика
        }
        else
        {
            // Якщо джойстик не призначений, можна використовувати клавіатуру як запасний варіант
            horizontalInput = Input.GetAxis("Horizontal"); 
            verticalInput = Input.GetAxis("Vertical");
            // Debug.LogWarning("PlayerMovement: Джойстик не призначений! Використовуються вхідні дані з клавіатури.");
        }
        // <<< КІНЕЦЬ ЗМІНИ ВВОДУ >>>


        moveDirection = Vector3.right * horizontalInput + Vector3.forward * verticalInput;

        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    void HandleRotation()
    {
        if (moveDirection.magnitude >= 0.1f) 
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}