using UnityEngine;
using System.Collections;
// using System.Collections.Generic; // HashSet більше не потрібен

public class CollectablesManager : MonoBehaviour
{
    // ----- EVENTS -----
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<float> OnSizeChanged;
    // ------------------

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

    [Tooltip("Допустима похибка при порівнянні розмірів об'єкта з розміром отвору. " +
             "Об'єкт вважається меншим, якщо його розмір менший за розмір отвору МІНУС цей допуск.")]
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

    // <<< ВИДАЛЯЄМО ВЕСЬ РОЗДІЛ "Forced Fall Settings" та related code >>>
    // [Header("Forced Fall Settings")]
    // public float forcedFallSpeed = 0.5f; 
    // public LayerMask fallingObjectsLayer; 
    // private HashSet<GameObject> fallingObjectsSet = new HashSet<GameObject>();

    void Awake() 
    {
        currentHoleSize = initialHoleSize; 
        transform.localScale = new Vector3(currentHoleSize, transform.localScale.y, currentHoleSize);
        totalScore = 0; 
        Debug.Log("CollectablesManager: Ініціалізація завершена.");
    }

    // <<< МЕТОД Update() тепер не потрібен, якщо немає ApplyForcedFall() >>>
    // void Update()
    // {
    //     ApplyForcedFall();
    // }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollectablesManager: Об'єкт '{other.name}' увійшов у тригер CollectablesManager.");

        Collectable collectable = other.GetComponent<Collectable>();

        if (collectable != null && other.gameObject != this.gameObject)
        {
            if (other.transform.localScale.x < currentHoleSize - sizeComparisonTolerance) 
            {
                Debug.Log($"CollectablesManager: Об'єкт '{other.name}' поглинається.");

                totalScore += collectable.scoreValue; 
                currentHoleSize += (collectable.scoreValue * growthMultiplier); 
                transform.localScale = new Vector3(currentHoleSize, transform.localScale.y, currentHoleSize);

                // Вимикаємо візуалізацію та коллайдер ОДРАЗУ, щоб він "провалювався"
                Collider objCollider = other.GetComponent<Collider>();
                if (objCollider != null)
                {
                    objCollider.enabled = false;
                }
                MeshRenderer objRenderer = other.GetComponent<MeshRenderer>();
                if (objRenderer != null)
                {
                    objRenderer.enabled = false;
                }
                
                StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay));
            }
            else
            {
                Debug.Log($"CollectablesManager: Об'єкт '{other.name}' завеликий для поглинання.");
            }
        }
        else if (collectable == null)
        {
            Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' увійшов у тригер, але не має компонента Collectable.");
        }
    }

    // <<< МЕТОД ApplyForcedFall() ПОВНІСТЮ ВИДАЛЯЄМО >>>
    // void ApplyForcedFall() { ... }

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
            Debug.LogWarning($"CollectablesManager: Спроба знищити об'єкт, який вже дорівнює null.");
        }
    }
}