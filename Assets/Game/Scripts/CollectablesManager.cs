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

    [Header("Game Progression Reference")]
    [Tooltip("Посилання на GameProgressionManager на сцені.")]
    public GameProgressionManager gameProgressionManager; 

    [Header("Collectable Settings")]
    [Tooltip("Час до остаточного знищення об'єкта після його поглинання (в секундах).")]
    public float destroyDelay = 4.0f; 

    // <<< ВИДАЛЕНО: sizeComparisonTolerance, оскільки перевірка розміру більше не потрібна >>>
    // [Tooltip("Допустима похибка при порівнянні розмірів об'єкта з розміром отвору.")]
    // public float sizeComparisonTolerance = 0.1f; 

    // Очки та поточний розмір тепер керовані GameProgressionManager
    // [Header("Score Settings")]
    // private int _totalScore = 0; 
    // public int totalScore { get { return _totalScore; } private set { ... } }

    void Awake() 
    {
        if (playerHoleTransform == null)
        {
            Debug.LogError("CollectablesManager: playerHoleTransform не призначений! Неможливо змінити розмір гравця.");
            enabled = false;
            return;
        }
        
        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null)
            {
                Debug.LogError("CollectablesManager: GameProgressionManager не знайдено на сцені! Система прогресу не працюватиме.");
                enabled = false;
                return;
            }
        }

        Debug.Log("CollectablesManager: Ініціалізація завершена.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollectablesManager: Об'єкт '{other.name}' увійшов у ТРИГЕР ЗНИЩЕННЯ (HoleDestroyer).");

        Collectable collectable = other.GetComponent<Collectable>();

        if (collectable != null && other.gameObject != null && gameProgressionManager != null) 
        {
            // <<< ВИДАЛЕНО: Перевірка розміру об'єкта. Тепер будь-який Collectable, який доходить до DestroyZone, поглинається. >>>
            // if (other.transform.localScale.x < gameProgressionManager.PlayerCurrentSize - sizeComparisonTolerance) 
            
            Debug.Log($"CollectablesManager: Об'єкт '{other.name}' ПОГЛИНУТО (фінальний етап).");

            gameProgressionManager.AddPoints(collectable.scoreValue); 

            // Об'єкт залишається видимим, поки не закінчиться destroyDelay.
            // Приховування відбувається в корутині DestroyAfterDelay, перед самим знищенням.
            
            StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay));
            // <<< КІНЕЦЬ ВИДАЛЕННЯ: else-блоку від перевірки розміру теж немає >>>
            // }
            // else
            // {
            //     Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' завеликий для фінального поглинання (за розміром HoleDestroyer).");
            // }
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' увійшов у Коллайдер Знищення, але не є Collectable або вже знищений, або немає GameProgressionManager.");
        }
    }

    IEnumerator DestroyAfterDelay(GameObject objToDestroy, float delay)
    {
        Debug.Log($"CollectablesManager: Корутина 'DestroyAfterDelay' для об'єкта '{objToDestroy.name}' розпочалася. Затримка: {delay} с.");

        yield return new WaitForSeconds(delay); 
        
        if (objToDestroy != null)
        {
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