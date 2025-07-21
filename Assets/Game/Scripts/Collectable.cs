using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Tooltip("Кількість очок, яку дає цей об'єкт при поглинанні.")]
    public int scoreValue = 1;
    
    [Tooltip("Ранг об'єкта. Гравець повинен мати рівень, не менший за цей ранг, щоб поглинути об'єкт.")]
    public int rank = 1;
}