using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f; 
    public float rotationSmoothTime = 0.1f; 

    [Header("Joystick Control")]
    public Joystick joystick; 

    [Header("Ground Constraints")] // <<< НОВИЙ РОЗДІЛ: Обмеження руху по землі
    [Tooltip("Коллайдер об'єкта землі для обмеження руху гравця.")]
    public Collider groundCollider; 
    
    // Посилання на GameProgressionManager для отримання поточного розміру гравця
    [Header("Game Progression Reference")]
    public GameProgressionManager gameProgressionManager; 

    private Vector3 moveDirection; 
    private float rotationVelocity; 

    private float groundMinX, groundMaxX, groundMinZ, groundMaxZ; // Межі землі
    private float fixedYPosition; // Фіксована висота гравця

    void Awake()
    {
        if (joystick == null)
        {
            Debug.LogWarning("PlayerMovement: Джойстик не призначений! Використовуються вхідні дані з клавіатури.");
        }
        if (groundCollider == null)
        {
            Debug.LogError("PlayerMovement: Ground Collider не призначений! Рух гравця не буде обмежений.");
            enabled = false;
            return;
        }
        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null)
            {
                Debug.LogError("PlayerMovement: GameProgressionManager не знайдено! Обмеження руху гравця не буде працювати коректно.");
                enabled = false; // Вимкнемо скрипт, якщо менеджер не знайдено
                return;
            }
        }

        // Обчислюємо межі землі з її коллайдера
        Bounds bounds = groundCollider.bounds;
        groundMinX = bounds.min.x;
        groundMaxX = bounds.max.x;
        groundMinZ = bounds.min.z;
        groundMaxZ = bounds.max.z;

        // Зберігаємо початкову Y-позицію гравця. Він буде рухатися на цій висоті.
        fixedYPosition = transform.position.y;

        Debug.Log($"PlayerMovement: Межі землі: X({groundMinX}, {groundMaxX}), Z({groundMinZ}, {groundMaxZ}). Фіксована Y: {fixedYPosition}");
    }

    void Update()
    {
        HandleMovement();
        HandleRotation(); 
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (joystick != null)
        {
            horizontalInput = joystick.Horizontal; 
            verticalInput = joystick.Vertical;     
        }
        else
        {
            horizontalInput = Input.GetAxis("Horizontal"); 
            verticalInput = Input.GetAxis("Vertical");
        }

        moveDirection = Vector3.right * horizontalInput + Vector3.forward * verticalInput;

        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        // Розраховуємо нову бажану позицію
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        // <<< НОВЕ: Застосовуємо обмеження руху >>>
        // Використовуємо PlayerCurrentSize з GameProgressionManager для радіуса гравця
        // Радіус дірки = половина її діаметра (PlayerCurrentSize - це діаметр)
        float currentHoleRadius = gameProgressionManager.PlayerCurrentSize / 2f; 

        // Обмежуємо рух по X та Z, враховуючи радіус дірки
        newPosition.x = Mathf.Clamp(newPosition.x, groundMinX + currentHoleRadius, groundMaxX - currentHoleRadius);
        newPosition.z = Mathf.Clamp(newPosition.z, groundMinZ + currentHoleRadius, groundMaxZ - currentHoleRadius);
        
        newPosition.y = fixedYPosition; // Фіксуємо Y-позицію гравця

        transform.position = newPosition;
        // ------------------------------------
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