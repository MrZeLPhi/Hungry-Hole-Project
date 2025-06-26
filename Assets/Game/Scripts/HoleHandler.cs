using UnityEngine;

public class HoleHandler : MonoBehaviour
{
    [Tooltip("Шар для об'єктів, які нормально взаємодіють (не падають).")]
    public LayerMask NormalSphereLayer; 
   
    [Tooltip("Шар для об'єктів, які мають провалитися через землю.")]
    public LayerMask FallingSphereLayer; 

    [Header("Bounce Settings")]
    [Tooltip("Сила поштовху вгору, коли об'єкт входить у тригер дірки.")]
    public float bounceForce = 5.0f; 

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"HoleHandler: Об'єкт '{other.name}' увійшов у тригер.");

        if (((1 << other.gameObject.layer) & NormalSphereLayer) != 0) 
        {
            int newLayerIndex = (int)Mathf.Log(FallingSphereLayer.value, 2); 
            other.gameObject.layer = newLayerIndex; 
            Debug.Log($"HoleHandler: Об'єкт '{other.name}' шар змінено на {LayerMask.LayerToName(newLayerIndex)}.");

            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            if (otherRb != null)
            {
                otherRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse); 
                Debug.Log($"HoleHandler: Об'єкту '{other.name}' застосовано поштовх вгору з силою {bounceForce}.");
            }
            else
            {
                Debug.LogWarning($"HoleHandler: Об'єкт '{other.name}' не має Rigidbody. Неможливо застосувати поштовх вгору.");
            }
        }
        else
        {
            Debug.LogWarning($"HoleHandler: Об'єкт '{other.name}' не на NormalSphereLayer. Поточний шар: {LayerMask.LayerToName(other.gameObject.layer)}.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"HoleHandler: Об'єкт '{other.name}' вийшов з тригера.");

        if (((1 << other.gameObject.layer) & FallingSphereLayer) != 0) 
        {
            int newLayerIndex = (int)Mathf.Log(NormalSphereLayer.value, 2);
            other.gameObject.layer = newLayerIndex;
            Debug.Log($"HoleHandler: Об'єкт '{other.name}' повернув шар на {LayerMask.LayerToName(newLayerIndex)}.");
        }
    }
}