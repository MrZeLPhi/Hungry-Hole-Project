using UnityEngine;
using System.Collections;
// using UnityEngine.UI; // <<< Цей рядок більше не потрібен тут!

public class CollectablesManager : MonoBehaviour
{
    // ----- EVENTS -----
    // Статичні події, на які можуть підписуватися інші класи.
    // 'static' означає, що до них можна звертатися напряму через CollectablesManager.OnScoreChanged.
    // 'Action<int>' означає, що подія передає одне ціле число (нові очки).
    // 'Action<float>' означає, що подія передає одне число з плаваючою комою (новий розмір).
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<float> OnSizeChanged;
    // ------------------

    [Header("Hole Growth Settings")]
    public float initialHoleSize = 1.0f; 
    private float _currentHoleSize; // Змінено на private
    public float currentHoleSize // Публічний геттер, який викликає подію при зміні
    {
        get { return _currentHoleSize; }
        private set
        {
            if (_currentHoleSize != value)
            {
                _currentHoleSize = value;
                // Викликаємо подію OnSizeChanged, передаючи новий розмір
                OnSizeChanged?.Invoke(_currentHoleSize);
            }
        }
    }
    public float growthMultiplier = 0.01f;

    [Header("Collectable Settings")]
    public float destroyDelay = 4.0f;
    public float sizeComparisonTolerance = 0.1f;

    [Header("Score Settings")]
    private int _totalScore = 0; // Змінено на private
    public int totalScore // Публічний геттер, який викликає подію при зміні
    {
        get { return _totalScore; }
        private set
        {
            if (_totalScore != value)
            {
                _totalScore = value;
                // Викликаємо подію OnScoreChanged, передаючи нові очки
                OnScoreChanged?.Invoke(_totalScore);
            }
        }
    }

    // <<< Видаляємо посилання на UI Text, бо тепер це відповідальність UI Manager'а >>>
    // public Text scoreText;
    // public Text sizeText;

    void Awake() // Використовуємо Awake, щоб переконатися, що ініціалізація відбувається раніше Start
    {
        // Встановлюємо початковий розмір отвору, зберігаючи Y-розмір незмінним
        transform.localScale = new Vector3(initialHoleSize, transform.localScale.y, initialHoleSize);
        currentHoleSize = initialHoleSize; // Це викличе OnSizeChanged

        Debug.Log("CollectablesManager: Ініціалізація завершена.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollectablesManager: Об'єкт '{other.name}' увійшов у тригер.");

        Collectable collectable = other.GetComponent<Collectable>();

        if (collectable != null && other.gameObject != this.gameObject)
        {
            if (other.transform.localScale.x < currentHoleSize - sizeComparisonTolerance)
            {
                Debug.Log($"CollectablesManager: Об'єкт '{other.name}' поглинається.");

                totalScore += collectable.scoreValue; // Це викличе OnScoreChanged

                currentHoleSize += (collectable.scoreValue * growthMultiplier); // Це викличе OnSizeChanged
                transform.localScale = new Vector3(currentHoleSize, transform.localScale.y, currentHoleSize);

                // UI оновлюється автоматично через події, тому цей виклик більше не потрібен
                // UpdateUI(); 

                other.enabled = false;
                MeshRenderer objRenderer = other.GetComponent<MeshRenderer>();
                if (objRenderer != null)
                {
                    objRenderer.enabled = true;
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

    IEnumerator DestroyAfterDelay(GameObject objToDestroy, float delay)
    {
        Debug.Log($"CollectablesManager: Корутина 'DestroyAfterDelay' для об'єкта '{objToDestroy.name}' розпочалася. Затримка: {delay} с.");
        yield return new WaitForSeconds(delay); 
        if (objToDestroy != null)
        {
            Destroy(objToDestroy);
            Debug.Log($"CollectablesManager: Об'єкт '{objToDestroy.name}' успішно знищено.");
        }
        else
        {
            Debug.LogWarning($"CollectablesManager: Спроба знищити об'єкт, який вже дорівнює null.");
        }
    }

    // <<< Цей метод UpdateUI більше не потрібен тут! >>>
    // void UpdateUI() { ... }
}