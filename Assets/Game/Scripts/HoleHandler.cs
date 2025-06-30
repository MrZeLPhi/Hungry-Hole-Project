using UnityEngine;
using System.Collections; // Для Coroutines
using System.Collections.Generic; // Для Dictionary

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

   [Header("Transparency Settings (Hole Trigger)")] // <<< НОВИЙ РОЗДІЛ: Налаштування прозорості
   [Tooltip("Цільова прозорість (альфа), коли об'єкт знаходиться в дірці.")]
   [Range(0f, 1f)]
   public float targetAlphaInHole = 0.3f; 
   [Tooltip("Швидкість (в секундах), з якою об'єкт стає прозорим або назад.")]
   public float fadeDuration = 0.5f; 

   [Header("Game Progression Reference")]
   [Tooltip("Посилання на GameProgressionManager на сцені.")]
   public GameProgressionManager gameProgressionManager; 

   private float bounceTimer; 
   private HashSet<Rigidbody> bouncingObjects = new HashSet<Rigidbody>();
   // Словник для зберігання оригінальних матеріалів об'єктів, які ми робимо прозорими
   private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
   // Словник для відстеження активних корутин затухання, щоб їх можна було зупинити
   private Dictionary<Renderer, Coroutine> activeFadeCoroutines = new Dictionary<Renderer, Coroutine>();


   void Awake() 
   {
       if (gameProgressionManager == null)
       {
           gameProgressionManager = FindObjectOfType<GameProgressionManager>();
           if (gameProgressionManager == null)
           {
               Debug.LogError("HoleHandler: GameProgressionManager не знайдено на сцені! Рангова перевірка не працюватиме.");
               enabled = false;
           }
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
             int newLayerIndex = (int)Mathf.Log(FallingSphereLayer.value, 2); 
             other.gameObject.layer = newLayerIndex; 
             Debug.Log($"HoleHandler: Об'єкт '{other.name}' шар змінено на {LayerMask.LayerToName(newLayerIndex)}.");

             // <<< ЛОГІКА ПРОЗОРОСТІ: Робимо об'єкт прозорим >>>
             if (!originalMaterials.ContainsKey(otherRenderer)) // Якщо ми його ще не робили прозорим
             {
                 originalMaterials.Add(otherRenderer, otherRenderer.material); // Зберігаємо оригінальний матеріал
             }
             StopFadeCoroutine(otherRenderer); // Зупиняємо попередню корутину, якщо вона була
             Coroutine newFadeRoutine = StartCoroutine(FadeObjectAlpha(otherRenderer, targetAlphaInHole, fadeDuration));
             activeFadeCoroutines.Add(otherRenderer, newFadeRoutine); // Зберігаємо нову корутину
             // --------------------------------------------------

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

      // Якщо об'єкт вийшов з тригера дірки, і він був зроблений прозорим нами
      if (otherRenderer != null && originalMaterials.ContainsKey(otherRenderer))
      {
          StopFadeCoroutine(otherRenderer); // Зупиняємо поточну корутину
          // <<< ЛОГІКА ПРОЗОРОСТІ: Повертаємо об'єкт до непрозорого стану >>>
          Coroutine newFadeRoutine = StartCoroutine(FadeObjectAlpha(otherRenderer, 1.0f, fadeDuration)); // Робимо повністю непрозорим
          activeFadeCoroutines.Add(otherRenderer, newFadeRoutine); // Зберігаємо нову корутину
          // ------------------------------------------------------------------
          originalMaterials.Remove(otherRenderer); // Видаляємо з відстежуваних
          Debug.Log($"HoleHandler: Об'єкт '{other.name}' повернуто до непрозорого стану.");
      }

      // Логіка повернення шару (як і раніше)
      if (((1 << other.gameObject.layer) & FallingSphereLayer) != 0) 
      {
         int newLayerIndex = (int)Mathf.Log(NormalSphereLayer.value, 2);
         other.gameObject.layer = newLayerIndex;
         Debug.Log($"HoleHandler: Об'єкт '{other.name}' повернув шар на {LayerMask.LayerToName(newLayerIndex)}.");
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

   // <<< ДОПОМІЖНІ МЕТОДИ ДЛЯ ПРОЗОРОСТІ (ТАЙ ЖЕ КОД, ЩО БУВ У CAMERA_FOLLOW) >>>
   void StopFadeCoroutine(Renderer renderer)
   {
       if (activeFadeCoroutines.ContainsKey(renderer) && activeFadeCoroutines[renderer] != null)
       {
           StopCoroutine(activeFadeCoroutines[renderer]);
           activeFadeCoroutines.Remove(renderer);
       }
   }

   IEnumerator FadeObjectAlpha(Renderer renderer, float targetAlpha, float duration)
   {
       if (renderer == null || renderer.material == null) yield break; 

       Material mat = renderer.material;
       Color currentColor = mat.color;
       float startAlpha = currentColor.a;

       if (targetAlpha < 1.0f) // Якщо робимо прозорим
       {
           SetMaterialTransparent(mat); 
       }
       // Якщо робимо непрозорим (targetAlpha = 1.0f), режим Opaque встановлюємо в кінці

       float timer = 0f;
       while (timer < duration)
       {
           timer += Time.deltaTime;
           float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
           currentColor.a = newAlpha;
           mat.color = currentColor;
           yield return null; 
       }

       currentColor.a = targetAlpha;
       mat.color = currentColor;

       if (targetAlpha >= 1.0f) // Після повного затухання до непрозорого стану
       {
           SetMaterialOpaque(mat); 
       }
   }

   void SetMaterialTransparent(Material material)
   {
       material.SetOverrideTag("RenderType", "Fade");
       material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
       material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
       material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
       material.SetInt("_ZWrite", 0); 
       material.DisableKeyword("_ALPHATEST_ON");
       material.EnableKeyword("_ALPHABLEND_ON");
       material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
   }

   void SetMaterialOpaque(Material material)
   {
       material.SetOverrideTag("RenderType", ""); 
       material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry; 
       material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
       material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
       material.SetInt("_ZWrite", 1); 
       material.DisableKeyword("_ALPHABLEND_ON");
       material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
       material.EnableKeyword("_ALPHATEST_ON"); 
   }
   // <<< КІНЕЦЬ ДОПОМІЖНИХ МЕТОДІВ ДЛЯ ПРОЗОРОСТІ >>>
}