using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;

    // Параметри розміру отвору тепер будуть в CollectablesManager
    // [Header("Hole Growth Settings")]
    // public float initialHoleSize = 1.0f; 
    // public float currentHoleSize; 
    // public float minSizeForGrowth = 0.1f; 
    // public float growthMultiplier = 0.01f; 

    // Параметри очок також будуть в CollectablesManager
    // [Header("Score Settings")]
    // public int totalScore = 0; 

    void Start()
    {
        // Ці рядки тепер будуть в CollectablesManager, якщо він керує розміром
        // transform.localScale = new Vector3(initialHoleSize, initialHoleSize, initialHoleSize);
        // currentHoleSize = initialHoleSize;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontalInput + transform.forward * verticalInput;

        if (moveDirection.magnitude > 1.0f)
        {
            moveDirection.Normalize();
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

}