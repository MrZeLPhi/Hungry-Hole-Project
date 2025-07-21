using UnityEngine;
using System.Collections.Generic; // Для HashSet

public class HoleHandler : MonoBehaviour
{
   [Tooltip("Шар для об'єктів, які нормально взаємодіють (не падають).")]
   public LayerMask NormalSphereLayer; 
   
   [Tooltip("Шар для об'єктів, які мають провалитися через землю.")]
   public LayerMask FallingSphereLayer; 

   [Header("Bounce Settings")]
   [Tooltip("Сила поштовху вгору, коли об'єкт входить у тригер дірки.")]
   public float initialBounceForce = 5.0f; 
   
   [Tooltip("Сила постійного поштовху вгору для об'єктів у FallingSphereLayer.")]
   public float periodicBounceForce = 2.0f; 

   [Tooltip("Інтервал (у секундах) між постійними поштовхами.")]
   public float periodicBounceInterval = 1.0f; 

   [Header("Game Progression Reference")]
   [Tooltip("Посилання на GameProgressionManager на сцені.")]
   public GameProgressionManager gameProgressionManager; 

   private float bounceTimer; 
   private HashSet<Rigidbody> bouncingObjects = new HashSet<Rigidbody>();

   void Awake() 
   {
       if (gameProgressionManager == null)
       {
           Debug.LogError("HoleHandler: GameProgressionManager не знайдено на сцені! Рангова перевірка не працюватиме.");
           enabled = false;
       }
   }

   void FixedUpdate()
   {
       bounceTimer += Time.fixedDeltaTime; 
       if (bounceTimer >= periodicBounceInterval)
       {
           ApplyPeriodicBounce();
           bounceTimer = 0f; 
       }
   }

   private void OnTriggerEnter(Collider other)
   {
      Debug.Log($"HoleHandler: Об'єкт '{other.name}' увійшов у ТРИГЕР ГОЛОВНОЇ ДІРКИ.");

      Collectable collectable = other.GetComponent<Collectable>();
      Renderer otherRenderer = other.GetComponent<Renderer>(); // Отримуємо рендерер

      if (collectable != null && gameProgressionManager != null && otherRenderer != null)
      {
          if (collectable.rank > gameProgressionManager.CurrentLevel)
          {
              Debug.Log($"HoleHandler: Об'єкт '{other.name}' (ранг {collectable.rank}) занадто високого рангу для гравця (рівень {gameProgressionManager.CurrentLevel}). Не поглинається.");
              return; 
          }

          if (((1 << other.gameObject.layer) & NormalSphereLayer) != 0) 
          {
             // <<< ПОЧАТОК ДІАГНОСТИКИ МАТЕРІАЛУ >>>
             Material mat = otherRenderer.material; // Отримуємо інстанс матеріалу
             Debug.Log($"HoleHandler (До зміни шару): Об'єкт '{other.name}'. Шар: {LayerMask.LayerToName(other.gameObject.layer)}. Матеріал: {mat.name}, Шейдер: {mat.shader.name}, Alpha: {mat.color.a}, RenderQueue: {mat.renderQueue}.");
             // <<< КІНЕЦЬ ДІАГНОСТИКИ МАТЕРІАЛУ >>>

             int newLayerIndex = (int)Mathf.Log(FallingSphereLayer.value, 2); 
             other.gameObject.layer = newLayerIndex; 
             Debug.Log($"HoleHandler: Об'єкт '{other.name}' шар змінено на {LayerMask.LayerToName(newLayerIndex)}.");

             // <<< ПОЧАТОК ДІАГНОСТИКИ МАТЕРІАЛУ ПІСЛЯ ЗМІНИ ШАРУ >>>
             // Перевіряємо стан матеріалу ОДРАЗУ після зміни шару
             Debug.Log($"HoleHandler (Після зміни шару): Об'єкт '{other.name}'. Шар: {LayerMask.LayerToName(other.gameObject.layer)}. Матеріал: {mat.name}, Шейдер: {mat.shader.name}, Alpha: {mat.color.a}, RenderQueue: {mat.renderQueue}.");
             // <<< КІНЕЦЬ ДІАГНОСТИКИ МАТЕРІАЛУ >>>

             Rigidbody otherRb = other.GetComponent<Rigidbody>();
             if (otherRb != null)
             {
                 otherRb.AddForce(Vector3.up * initialBounceForce, ForceMode.Impulse); 
                 Debug.Log($"HoleHandler: Об'єкту '{other.name}' застосовано ПОЧАТКОВИЙ поштовх вгору з силою {initialBounceForce}.");

                 if (!bouncingObjects.Contains(otherRb))
                 {
                     bouncingObjects.Add(otherRb);
                     Debug.Log($"HoleHandler: '{other.name}' додано до списку періодичних поштовхів.");
                 }
             }
             else
             {
                 Debug.LogWarning($"HoleHandler: Об'єкт '{other.name}' не має Rigidbody. Неможливо застосувати поштовх вгору.");
             }
          }
          else
          {
              Debug.Log($"HoleHandler: Об'єкт '{other.name}' не на NormalSphereLayer або не цікавий для початкового поштовху.");
          }
      }
      else if (collectable == null)
      {
          Debug.LogWarning($"HoleHandler: Об'єкт '{other.name}' увійшов у тригер, але не має компонента Collectable.");
      }
      else if (otherRenderer == null)
      {
          Debug.LogWarning($"HoleHandler: Об'єкт '{other.name}' увійшов у тригер, але не має компонента Renderer.");
      }
   }

   private void OnTriggerExit(Collider other)
   {
      Debug.Log($"HoleHandler: Об'єкт '{other.name}' вийшов з ТРИГЕРА ГОЛОВНОЇ ДІРКИ.");

      Renderer otherRenderer = other.GetComponent<Renderer>();

      if (otherRenderer != null) // Перевірка, що рендерер існує
      {
          // Логіка повернення шару (як і раніше)
          if (((1 << other.gameObject.layer) & FallingSphereLayer) != 0) 
          {
             int newLayerIndex = (int)Mathf.Log(NormalSphereLayer.value, 2);
             other.gameObject.layer = newLayerIndex;
             Debug.Log($"HoleHandler: Об'єкт '{other.name}' повернув шар на {LayerMask.LayerToName(newLayerIndex)}.");
          }
          // <<< ПОЧАТОК ДІАГНОСТИКИ МАТЕРІАЛУ ПІСЛЯ ВИХОДУ З ТРИГЕРА >>>
          Material mat = otherRenderer.material;
          Debug.Log($"HoleHandler (Після виходу з тригера): Об'єкт '{other.name}'. Шар: {LayerMask.LayerToName(other.gameObject.layer)}. Матеріал: {mat.name}, Шейдер: {mat.shader.name}, Alpha: {mat.color.a}, RenderQueue: {mat.renderQueue}.");
          // <<< КІНЕЦЬ ДІАГНОСТИКИ МАТЕРІАЛУ >>>
      }
      else
      {
          Debug.LogWarning($"HoleHandler: Об'єкт '{other.name}' вийшов з тригера, але не має компонента Renderer.");
      }

      // Видаляємо об'єкт зі списку для періодичних поштовхів
      Rigidbody rb = other.GetComponent<Rigidbody>();
      if (rb != null && bouncingObjects.Contains(rb))
      {
          bouncingObjects.Remove(rb);
          Debug.Log($"HoleHandler: '{other.name}' видалено зі списку періодичних поштовхів (вийшов з тригера).");
      }
   }

   private void ApplyPeriodicBounce()
   {
       List<Rigidbody> toRemove = new List<Rigidbody>(); 
       foreach (Rigidbody rb in bouncingObjects)
       {
           if (rb == null || !rb.gameObject.activeInHierarchy || ((1 << rb.gameObject.layer) & FallingSphereLayer.value) == 0) 
           {
               toRemove.Add(rb); 
               continue;
           }
           rb.AddForce(Vector3.up * periodicBounceForce, ForceMode.Impulse);
       }
       foreach (Rigidbody rbToRemove in toRemove)
       {
           bouncingObjects.Remove(rbToRemove);
       }
   }
}