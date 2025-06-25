
using TMPro; // Потрібно для TextMeshProUGUI
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText; // Посилання на текстовий елемент для очок
    public TextMeshProUGUI sizeText;  // Посилання на текстовий елемент для розміру/діаметра

    void OnEnable()
    {
        CollectablesManager.OnScoreChanged += UpdateScoreDisplay;
        CollectablesManager.OnSizeChanged += UpdateSizeDisplay;
        Debug.Log("UIManager: Підписано на події CollectablesManager.");
    }

    void OnDisable()
    {
        CollectablesManager.OnScoreChanged -= UpdateScoreDisplay;
        CollectablesManager.OnSizeChanged -= UpdateSizeDisplay;
        Debug.Log("UIManager: Відписано від подій CollectablesManager.");
    }

    void Start()
    {
        // Встановлюємо початкові значення, щоб на екрані не було порожньо
        if (scoreText != null)
        {
            scoreText.text = "P: 0";
        }
        if (sizeText != null)
        {
            sizeText.text = "R: 0"; // Тепер початкове значення теж ціле
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "P: " + newScore;
            Debug.Log($"UIManager: Оновлено очки: {newScore}");
        }
    }

    private void UpdateSizeDisplay(float newSize)
    {
        if (sizeText != null)
        {
            // <<< ЗМІНА ТУТ: Math.Round або (int) для цілого числа >>>
            // Math.Round округлює до найближчого цілого.
            // (int) відкидає дробову частину (округляє до нуля).
            int displaySize = Mathf.RoundToInt(newSize); // Рекомендовано для округлення до найближчого цілого
            // Або: int displaySize = (int)newSize; // Якщо хочеш просто відкинути дробову частину

            sizeText.text = "R: " + displaySize.ToString();
            Debug.Log($"UIManager: Оновлено діаметр: {displaySize}"); // Логуємо ціле число
        }
    }
}