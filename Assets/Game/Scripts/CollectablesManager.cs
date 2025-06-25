using UnityEngine;
using System.Collections; // Обов'язково для використання Coroutines

public class CollectablesManager : MonoBehaviour
{
    [Header("Hole Growth Settings")]
    public float initialHoleSize = 1.0f; // Початковий розмір отвору (X та Z)
    private float currentHoleSize; // Поточний розмір отвору (X та Z)
    public float growthMultiplier = 0.01f; // Множник для перетворення очок на збільшення розміру

    [Header("Collectable Settings")]
    [Tooltip("Час до остаточного знищення об'єкта після його поглинання (в секундах).")]
    public float destroyDelay = 4.0f; // Час затримки до знищення

    [Tooltip("Допустима похибка при порівнянні розмірів об'єкта з розміром отвору. " +
             "Об'єкт вважається меншим, якщо його розмір менший за розмір отвору МІНУС цей допуск.")]
    public float sizeComparisonTolerance = 0.1f; 

    [Header("Score Settings")]
    public int totalScore = 0; // Загальна кількість набраних очок

    void Start()
    {
        // Встановлюємо початковий розмір отвору, зберігаючи Y-розмір незмінним
        transform.localScale = new Vector3(initialHoleSize, transform.localScale.y, initialHoleSize);
        currentHoleSize = initialHoleSize;

        Debug.Log("CollectablesManager: Ініціалізація завершена. Початковий розмір отвору: " + initialHoleSize);
    }

    // Цей метод спрацьовує, коли інший коллайдер входить у тригер цього об'єкта
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollectablesManager: Об'єкт '{other.name}' увійшов у тригер.");

        // Спробуємо отримати компонент Collectable з об'єкта, що зіткнувся
        Collectable collectable = other.GetComponent<Collectable>();

        // Перевіряємо, чи об'єкт має скрипт Collectable, і чи він не є самим гравцем
        if (collectable != null && other.gameObject != this.gameObject)
        {
            // Перевіряємо, чи об'єкт достатньо малий, щоб його "з'їсти"
            if (other.transform.localScale.x < currentHoleSize - sizeComparisonTolerance) 
            {
                Debug.Log($"CollectablesManager: Об'єкт '{other.name}' (розмір X: {other.transform.localScale.x}) поглинається. Поточний розмір отвору XZ: {currentHoleSize}.");

                // Додаємо очки
                totalScore += collectable.scoreValue;
                Debug.Log("Очки: " + totalScore);

                // Збільшуємо розмір отвору тільки по X та Z
                currentHoleSize += (collectable.scoreValue * growthMultiplier); 
                transform.localScale = new Vector3(currentHoleSize, transform.localScale.y, currentHoleSize);

                // *** ВАЖЛИВО: Вимикаємо коллайдер та рендерер негайно ***
                // Об'єкт стає невидимим і не взаємодіє одразу після "поглинання".
                other.enabled = false; // Вимкнути Collider
                MeshRenderer objRenderer = other.GetComponent<MeshRenderer>();
                if (objRenderer != null)
                {
                    objRenderer.enabled = true; // Вимкнути візуалізацію (зробити невидимим)
                }
                
                // *** ЗАПУСКАЄМО КОРУТИНУ ДЛЯ ЗНИЩЕННЯ ІЗ ЗАТРИМКОЮ ***
                StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay));
            }
            else
            {
                Debug.Log($"CollectablesManager: Об'єкт '{other.name}' (розмір X: {other.transform.localScale.x}) завеликий для поглинання (поточний розмір отвору XZ: {currentHoleSize}).");
            }
        }
        else if (collectable == null)
        {
            Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' увійшов у тригер, але не має компонента Collectable.");
        }
    }

    // Корутина для знищення об'єкта після заданої затримки
    IEnumerator DestroyAfterDelay(GameObject objToDestroy, float delay)
    {
        Debug.Log($"CollectablesManager: Корутина 'DestroyAfterDelay' для об'єкта '{objToDestroy.name}' розпочалася. Затримка: {delay} с.");

        yield return new WaitForSeconds(delay); 
        
        if (objToDestroy != null)
        {
            Destroy(objToDestroy);
            Debug.Log($"CollectablesManager: Об'єкт '{objToDestroy.name}' успішно знищено після затримки {delay} с.");
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Спроба знищити об'єкт, який вже дорівнює null (можливо, вже знищений).");
        }
    }
}