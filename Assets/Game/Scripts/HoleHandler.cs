using UnityEngine;

public class HoleHandler : MonoBehaviour
{
    [Tooltip("Шар для об'єктів, які нормально взаємодіють (не падають).")]
    public LayerMask NormalSphereLayer; 
   
    [Tooltip("Шар для об'єктів, які мають провалитися через землю.")]
    public LayerMask FallingSphereLayer; 

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"HoleHandler: {other.name} увійшов у тригер. Поточний шар: {LayerMask.LayerToName(other.gameObject.layer)}");

        // Перевірка, чи об'єкт належить до NormalSphereLayer (замість прямого порівняння шарів)
        // (1 << other.gameObject.layer) створює бітову маску для поточного шару об'єкта.
        // & NormalSphereLayer виконує побітове "І" з LayerMask.
        // Якщо результат не 0, значить, шар об'єкта включений у NormalSphereLayer.
        if (((1 << other.gameObject.layer) & NormalSphereLayer) != 0) 
        {
            int newLayerIndex = (int)Mathf.Log(FallingSphereLayer.value, 2); // Отримуємо числовий індекс шару
            other.gameObject.layer = newLayerIndex; 
            Debug.Log($"HoleHandler: {other.name} шар змінено на {LayerMask.LayerToName(newLayerIndex)}.");
        }
        else
        {
            Debug.LogWarning($"HoleHandler: {other.name} не на NormalSphereLayer. Поточний шар: {LayerMask.LayerToName(other.gameObject.layer)}.");
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