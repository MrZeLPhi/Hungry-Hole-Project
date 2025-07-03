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
    public PlayerMovement playerMovement; // Для зупинки гравця при перемозі
    public UIManager uiManager; 

    // Зберігаємо посилання на НАЙВИЩИЙ за рангом об'єкт
    private Collectable highestRankCollectableTarget; 

    // <<< ВИДАЛЕНО: Physics Settings та groundCollider >>>


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
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("CollectablesManager: PlayerMovement не знайдено на сцені!");
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
        // <<< ВИДАЛЕНО: Перевірку groundCollider >>>
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
            highestRankCollectableTarget = allCollectables.OrderByDescending(c => c.rank).FirstOrDefault(); 

            if (highestRankCollectableTarget != null)
            {
                Debug.Log($"CollectablesManager: Глобальна ціль: з'їсти об'єкт '{highestRankCollectableTarget.name}' з НАЙВИЩИМ рангом {highestRankCollectableTarget.rank}");
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


    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollectablesManager: Об'єкт '{other.name}' увійшов у ТРИГЕР ЗНИЩЕННЯ (HoleDestroyer).");

        Collectable collectable = other.GetComponent<Collectable>();

        if (collectable != null && other.gameObject != null && gameProgressionManager != null) 
        {
            if (other.transform.localScale.x > gameProgressionManager.PlayerCurrentSize + 0.1f) 
            {
                Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' (розмір X: {other.transform.localScale.x}) досяг знищувача, але візуально завеликий для поточного розміру дірки ({gameProgressionManager.PlayerCurrentSize:F2}).");
            }
            
            Debug.Log($"CollectablesManager: Об'єкт '{other.name}' ПОГЛИНУТО (фінальний етап).");

            gameProgressionManager.AddPoints(collectable.scoreValue); 

            if (collectable == highestRankCollectableTarget) 
            {
                Debug.Log("CollectablesManager: Глобальну ціль досягнуто! Об'єкт з'їдений. Готуємо перемогу.");
                // Запускаємо корутину для цільового об'єкта
                StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay, true)); 
            } else {
                // Для нецільових об'єктів - звичайне знищення
                StartCoroutine(DestroyAfterDelay(other.gameObject, destroyDelay, false)); 
            }
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Об'єкт '{other.name}' увійшов у Коллайдер Знищення, але не є Collectable або вже знищений, або немає GameProgressionManager.");
        }
    }

    // Корутина для знищення об'єктів
    IEnumerator DestroyAfterDelay(GameObject objToDestroy, float delay, bool isWinTarget)
    {
        Debug.Log($"CollectablesManager: Корутина 'DestroyAfterDelay' для об'єкта '{objToDestroy.name}' розпочалася. Затримка: {delay} с. Ціль перемоги: {isWinTarget}");

        // Якщо це цільовий об'єкт (перемога), вимикаємо рух гравця та робимо його некінематичним для падіння
        if (isWinTarget)
        {
            if (playerMovement != null && playerMovement.enabled) // Перевірка, чи рух гравця ще увімкнений
            {
                playerMovement.enabled = false; // Вимкнення джойстика
                Debug.Log("CollectablesManager: Рух гравця (джойстик) вимкнено для фінального падіння цільового об'єкта.");
            }

            Rigidbody collectedRb = objToDestroy.GetComponent<Rigidbody>();
            if (collectedRb != null)
            {
                collectedRb.isKinematic = false; 
                collectedRb.useGravity = true;   
                collectedRb.linearVelocity = Vector3.zero; 
                collectedRb.angularVelocity = Vector3.zero; 
                Debug.Log($"CollectablesManager: Забезпечено падіння об'єкта '{objToDestroy.name}' після перемоги.");
            }
            else
            {
                Debug.LogWarning($"CollectablesManager: Зібраний об'єкт '{objToDestroy.name}' не має Rigidbody, не може падати. Перемога буде оброблена без падіння.");
                EndGameWin(); // Якщо не може падати, фіналізуємо одразу
                yield break; 
            }
        }

        // Чекаємо затримку (Time.timeScale ще не дорівнює 0)
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
            
            // Фіналізуємо перемогу, якщо це цільовий об'єкт
            if (isWinTarget)
            {
                EndGameWin(); 
            }
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Спроба знищити об'єкт, який вже дорівнює null.");
            if (isWinTarget)
            {
                EndGameWin(); 
            }
        }
    }

    void EndGameWin()
    {
        // Зупиняємо гру повністю
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