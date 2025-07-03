using UnityEngine;
using System.Linq; 
using System.Collections; 
using System.Collections.Generic; 

public class CollectablesManager : MonoBehaviour
{
    [Header("Collectable Settings")]
    public float collectableSpawnRadius = 50f; 
    public int maxCollectablesOnScreen = 20; 

    [Tooltip("Час до остаточного знищення об'єкта після його поглинання (в секундах).")]
    public float destroyDelay = 4.0f; 

    [Header("References")]
    public GameProgressionManager gameProgressionManager;
    public UIManager uiManager; 

    // Зберігаємо посилання на НАЙВИЩИЙ за рангом об'єкт
    // Тепер він публічний, щоб HoleHandler міг до нього звертатися
    public Collectable HighestRankCollectableTarget { get; private set; } 

    [Header("Physics Settings")] 
    [Tooltip("Коллайдер землі, з яким буде скасовано ігнорування колізій перед знищенням об'єкта.")]
    public Collider groundCollider; 


    void Awake()
    {
        if (gameProgressionManager == null)
        {
            gameProgressionManager = FindObjectOfType<GameProgressionManager>();
            if (gameProgressionManager == null)
            {
                Debug.LogError("CollectablesManager: GameProgressionManager не знайдено на сцені!");
                enabled = false;
            }
        }
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("CollectablesManager: UIManager не знайдено на сцені!");
                enabled = false;
            }
        }
        if (groundCollider == null) 
        {
            Debug.LogError("CollectablesManager: Ground Collider не призначений! Колізії з землею можуть не бути скинуті перед знищенням об'єкта.");
        }
    }

    void Start()
    {
        FindHighestRankCollectableTarget(); 
    }

    void FindHighestRankCollectableTarget() 
    {
        Collectable[] allCollectables = FindObjectsOfType<Collectable>();

        if (allCollectables.Length > 0)
        {
            HighestRankCollectableTarget = allCollectables.OrderByDescending(c => c.rank).FirstOrDefault(); 

            if (HighestRankCollectableTarget != null)
            {
                Debug.Log($"CollectablesManager: Глобальна ціль: з'їсти об'єкт '{HighestRankCollectableTarget.name}' з НАЙВИЩИМ рангом {HighestRankCollectableTarget.rank}");
            }
            else
            {
                Debug.LogWarning("CollectablesManager: Не знайдено Collectable об'єктів для встановлення глобальної цілі.");
            }
        }
        else
        {
            Debug.LogWarning("CollectablesManager: На сцені немає Collectable об'єктів.");
        }
    }

    // Цей OnTriggerEnter тепер обробляє тільки НЕ-цільові об'єкти.
    // Цільові об'єкти обробляються в HoleHandler.
    void OnTriggerEnter(Collider other)
    {
        Collectable collectable = other.GetComponent<Collectable>();

        // Перевіряємо, чи це не цільовий об'єкт.
        if (collectable != null && collectable != HighestRankCollectableTarget && other.gameObject != null && gameProgressionManager != null) 
        {
            Debug.Log($"CollectablesManager: Об'єкт '{other.name}' (не цільовий) увійшов у ТРИГЕР ЗНИЩЕННЯ.");

            if (other.transform.localScale.x > gameProgressionManager.PlayerCurrentSize + 0.1f) 
            {
                Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' (розмір X: {other.transform.localScale.x}) досяг знищувача, але візуально завеликий для поточного розміру дірки ({gameProgressionManager.PlayerCurrentSize:F2}).");
            }
            
            Debug.Log($"CollectablesManager: Об'єкт '{other.name}' ПОГЛИНУТО (фінальний етап).");
            gameProgressionManager.AddPoints(collectable.scoreValue); 
            StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay)); 
        }
    }

    // Корутина для знищення об'єктів (викликається як для звичайних, так і для цільових)
    public IEnumerator DestroyAfterDelay(GameObject objToDestroy, float delay) // Зроблено public
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

    // Фіналізація перемоги (зупинка гри та показ панелі)
    public void EndGameWin() // Зроблено public
    {
        Time.timeScale = 0f;
        Debug.Log("CollectablesManager: Гра зупинена (Time.timeScale = 0).");

        if (uiManager != null)
        {
            uiManager.ShowWinPanel();
            Debug.Log("CollectablesManager: Панель виграшу показано.");
        }
        else
        {
            Debug.LogError("CollectablesManager: UIManager не призначено, неможливо показати панель виграшу.");
        }
    }
}