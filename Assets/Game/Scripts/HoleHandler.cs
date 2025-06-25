using UnityEngine;

public class HoleHandler : MonoBehaviour
{
    [Tooltip("Шар для об'єктів, які нормально взаємодіють (не падають).")]
    public LayerMask NormalSphereLayer; 
   
    [Tooltip("Шар для об'єктів, які мають провалитися через землю.")]
    public LayerMask FallingSphereLayer; 

    private void OnTriggerEnter(Collider other)
    {
        // Перевіряємо, чи об'єкт, що увійшов, належить до 'NormalSphereLayer'.
        // ((1 << other.gameObject.layer) & NormalSphereLayer) != 0 перевіряє,
        // чи поточний шар об'єкта входить до маски NormalSphereLayer.
        if (((1 << other.gameObject.layer) & NormalSphereLayer) != 0) 
        {
            // Змінюємо шар об'єкта на FallingSphereLayer.
            // Mathf.Log(FallingSphereLayer.value, 2) перетворює LayerMask в індекс шару.
            other.gameObject.layer = (int)Mathf.Log(FallingSphereLayer.value, 2); 
            Debug.Log($"HoleHandler: Об'єкт '{other.name}' увійшов у дірку. Змінив шар на {LayerMask.LayerToName(other.gameObject.layer)}.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Перевіряємо, чи об'єкт, що вийшов, належить до 'FallingSphereLayer'.
        if (((1 << other.gameObject.layer) & FallingSphereLayer) != 0) 
        {
            // Повертаємо шар об'єкта на NormalSphereLayer,
            // якщо він вийшов з тригера дірки.
            other.gameObject.layer = (int)Mathf.Log(NormalSphereLayer.value, 2);
            Debug.Log($"HoleHandler: Об'єкт '{other.name}' вийшов з дірки. Повернув шар на {LayerMask.LayerToName(other.gameObject.layer)}.");
        }
    }
}