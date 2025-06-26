using UnityEngine;
using System.Collections;

public class CollectablesManager : MonoBehaviour
{
    // ----- EVENTS -----
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<float> OnSizeChanged;
    // ------------------

    [Header("Player Reference")]
    [Tooltip("Посилання на Transform основного об'єкта гравця (дірки), розмір якого буде змінюватися.")]
    public Transform playerHoleTransform; 

    [Header("Hole Growth Settings")]
    public float initialHoleSize = 1.0f; 
    
    private float _currentHoleSize; 
    public float currentHoleSize 
    {
        get { return _currentHoleSize; }
        private set
        {
            if (_currentHoleSize != value)
            {
                _currentHoleSize = value;
                OnSizeChanged?.Invoke(_currentHoleSize);
                Debug.Log($"CollectablesManager: Розмір отвору оновлено до: {_currentHoleSize:F2}. Подія OnSizeChanged викликана.");
            }
        }
    }
    public float growthMultiplier = 0.01f;

    [Header("Collectable Settings")]
    [Tooltip("Час до остаточного знищення об'єкта після його поглинання (в секундах).")]
    public float destroyDelay = 4.0f; 

    // sizeComparisonTolerance тепер має менше значення тут, бо об'єкти вже мають бути поглинуті
    [Tooltip("Допустима похибка при порівнянні розмірів об'єкта з розміром отвору. (Менш критично для цього тригера).")]
    public float sizeComparisonTolerance = 0.1f; 

    [Header("Score Settings")]
    private int _totalScore = 0; 
    public int totalScore 
    {
        get { return _totalScore; }
        private set
        {
            if (_totalScore != value)
            {
                _totalScore = value;
                OnScoreChanged?.Invoke(_totalScore);
                Debug.Log($"CollectablesManager: Очки оновлено до: {_totalScore}. Подія OnScoreChanged викликана.");
            }
        }
    }

    void Awake() 
    {
        if (playerHoleTransform == null)
        {
            Debug.LogError("CollectablesManager: playerHoleTransform не призначений! Неможливо змінити розмір гравця.");
            enabled = false;
            return;
        }

        playerHoleTransform.localScale = new Vector3(initialHoleSize, playerHoleTransform.localScale.y, initialHoleSize);
        currentHoleSize = initialHoleSize; 
        totalScore = 0; 

        Debug.Log("CollectablesManager: Ініціалізація завершена.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollectablesManager: Об'єкт '{other.name}' увійшов у ТРИГЕР ЗНИЩЕННЯ (HoleDestroyer).");

        Collectable collectable = other.GetComponent<Collectable>();

        if (collectable != null && other.gameObject != null) 
        {
            Debug.Log($"CollectablesManager: Об'єкт '{other.name}' ПОГЛИНУТО (фінальний етап).");

            totalScore += collectable.scoreValue; 
            currentHoleSize += (collectable.scoreValue * growthMultiplier); 
            playerHoleTransform.localScale = new Vector3(currentHoleSize, playerHoleTransform.localScale.y, currentHoleSize);

            // Об'єкт залишається видимим, поки не закінчиться destroyDelay.
            // Приховування відбувається в корутині DestroyAfterDelay, перед самим знищенням.
            
            StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay));
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' увійшов у Коллайдер Знищення, але не є Collectable або вже знищений.");
        }
    }

    IEnumerator DestroyAfterDelay(GameObject objToDestroy, float delay)
    {
        Debug.Log($"CollectablesManager: Корутина 'DestroyAfterDelay' для об'єкта '{objToDestroy.name}' розпочалася. Затримка: {delay} с.");

        yield return new WaitForSeconds(delay); 
        
        if (objToDestroy != null)
        {
            // Приховуємо об'єкт (вимкнення рендерера та коллайдера) безпосередньо перед знищенням
            Collider objCollider = objToDestroy.GetComponent<Collider>();
            if (objCollider != null) 
            {
                objCollider.enabled = false; 
                Debug.Log($"CollectablesManager: Вимкнено Collider для '{objToDestroy.name}' перед знищенням.");
            }
            MeshRenderer objRenderer = objToDestroy.GetComponent<MeshRenderer>();
            if (objRenderer != null) 
            {
                objRenderer.enabled = false; 
                Debug.Log($"CollectablesManager: Вимкнено MeshRenderer для '{objToDestroy.name}' перед знищенням.");
            }
            
            Destroy(objToDestroy);
            Debug.Log($"CollectablesManager: Об'єкт '{objToDestroy.name}' успішно знищено після затримки {delay} с.");
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Спроба знищити об'єкт, який вже дорівнює null.");
        }
    }
}