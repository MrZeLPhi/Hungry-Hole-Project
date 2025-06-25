using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Tooltip("Кількість очок, яку дає цей об'єкт при поглинанні.")]
    public int scoreValue = 1; // Скільки очок дає цей об'єкт
    
    [Tooltip("Розмір, на який збільшиться отвір при поглинанні цього об'єкта.")]
    public float sizeIncreaseAmount = 0.1f; // На скільки збільшиться розмір отвору
}