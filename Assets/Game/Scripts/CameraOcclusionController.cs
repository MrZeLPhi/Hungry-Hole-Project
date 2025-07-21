using UnityEngine;
using System.Collections; 
using System.Collections.Generic;

public class CameraOcclusionController : MonoBehaviour
{
    [Header("Occlusion Settings")] 
    [Tooltip("Об'єкт, за яким камера стежить для прозорості (ваш гравець).")]
    public Transform playerTarget; 
    [Tooltip("Шар, на якому знаходяться об'єкти, що мають ставати прозорими (наприклад, NormalObjects).")]
    public LayerMask normalObjectLayer; 
    
    [Tooltip("Цільова прозорість (альфа), коли об'єкт перекриває гравця (від 0.0 до 1.0).")]
    [Range(0f, 1f)]
    public float targetAlpha = 0.4f; 
    
    [Tooltip("Матеріал, який використовуватиметься для прозорості об'єктів-перешкод.")]
    public Material transparentOccluderMaterial; 

    [Header("References")]
    public GameProgressionManager gameProgressionManager; 
    public HoleHandler holeHandler; 

    // Внутрішня структура для зберігання оригінальних даних матеріалу
    private struct OriginalMaterialInfo
    {
        public Material originalMaterialInstance; // Зберігаємо саме оригінальний інстанс матеріалу
    }

    // Словник для відстеження об'єктів, які зараз прозорі/змінені
    private Dictionary<Renderer, OriginalMaterialInfo> occludingRenderersInfo = new Dictionary<Renderer, OriginalMaterialInfo>();

    private List<Renderer> renderersToMakeTransparentThisFrame = new List<Renderer>();

    void Awake()
    {
        if (playerTarget == null)
        {
            Debug.LogError("CameraOcclusionController: Player Target не призначений! Прозорість перешкод не працюватиме.");
            enabled = false; 
            return;
        }
        if (gameProgressionManager == null)
        {
            Debug.LogError("CameraOcclusionController: GameProgressionManager не знайдено! Логіка рангу не працюватиме.");
            enabled = false;
            return;
        }
        if (holeHandler == null)
        {
            Debug.LogError("CameraOcclusionController: HoleHandler не знайдено! Перевірка, чи об'єкт в дірці, не працюватиме.");
            enabled = false;
            return;
        }
        if (transparentOccluderMaterial == null)
        {
            Debug.LogError("CameraOcclusionController: Transparent Occluder Material не призначений! Прозорість перешкод не працюватиме.");
            enabled = false;
            return;
        }
    }
    
    void OnDisable()
    {
        foreach (var entry in occludingRenderersInfo)
        {
            Renderer renderer = entry.Key;
            OriginalMaterialInfo originalInfo = entry.Value;
            
            if (renderer != null && originalInfo.originalMaterialInstance != null)
            {
                // Повертаємо оригінальний матеріал-інстанс
                renderer.material = originalInfo.originalMaterialInstance; 
            }
        }
        occludingRenderersInfo.Clear();
    }

    void LateUpdate()
    {
        renderersToMakeTransparentThisFrame.Clear();

        if (playerTarget == null || gameProgressionManager == null || holeHandler == null || !enabled || transparentOccluderMaterial == null) 
        {
            return;
        }

        RaycastHit hit;
        Vector3 rayOrigin = transform.position; 
        Vector3 rayDirection = (playerTarget.position - rayOrigin).normalized;
        float rayDistance = Vector3.Distance(rayOrigin, playerTarget.position);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, normalObjectLayer)) 
        {
            if (hit.collider.transform != playerTarget) 
            {
                Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                Collectable hitCollectable = hit.collider.GetComponent<Collectable>(); 

                if (hitRenderer != null && hitCollectable != null)
                {
                    if (hitCollectable.rank > gameProgressionManager.CurrentLevel)
                    {
                        renderersToMakeTransparentThisFrame.Add(hitRenderer); 
                    }
                }
            }
        }

        foreach (Renderer renderer in renderersToMakeTransparentThisFrame)
        {
            if (renderer != null && renderer.material != null)
            {
                if (!occludingRenderersInfo.ContainsKey(renderer)) 
                {
                    // Зберігаємо оригінальний матеріал об'єкта (його інстанс)
                    occludingRenderersInfo.Add(renderer, 
                        new OriginalMaterialInfo { originalMaterialInstance = renderer.material }); 
                }
                // Призначаємо наш прозорий матеріал та встановлюємо альфа-канал
                renderer.material = transparentOccluderMaterial; 
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, targetAlpha);
            }
        }

        List<Renderer> renderersToMakeOpaque = new List<Renderer>();
        foreach (Renderer renderer in occludingRenderersInfo.Keys) 
        {
            if (renderer == null || !renderer.gameObject.activeInHierarchy || renderer.material == null) 
            {
                renderersToMakeOpaque.Add(renderer); 
                continue;
            }

            // <<< ВИПРАВЛЕНО: ВИДАЛЕНО УМОВУ isInHoleCollider! >>>
            // Об'єкт стає непрозорим ТІЛЬКИ, якщо він більше НЕ блокує огляд.
            bool shouldBeOpaqueBecauseNotBlocking = !renderersToMakeTransparentThisFrame.Contains(renderer);
            // bool isInHoleCollider = (holeHandler != null && holeHandler.gameObject.GetComponent<Collider>() != null && renderer.bounds.Intersects(holeHandler.gameObject.GetComponent<Collider>().bounds));
            
            if (shouldBeOpaqueBecauseNotBlocking) // Умова тепер залежить тільки від Raycast
            {
                renderer.material = occludingRenderersInfo[renderer].originalMaterialInstance; 
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 1.0f);

                Debug.Log($"CameraOcclusionController: Об'єкт '{renderer.name}' повертається до непрозорого (більше не блокує)."); 
                renderersToMakeOpaque.Add(renderer); 
            }
        }
        foreach (Renderer renderer in renderersToMakeOpaque)
        {
            occludingRenderersInfo.Remove(renderer); 
        }
    }

    private void UpdateCameraOffset(float newPlayerSize) 
    {
        // Цей метод належить CameraMovement.cs. Тут його не має бути.
    }

    // Ці методи SetMaterialTransparentInstant та SetMaterialOpaqueInstant були замінені в логіці LateUpdate
    // і не потрібні як окремі функції, якщо ви їх більше не викликаєте.
    // Якщо вони викликаються десь ще, їх потрібно буде відновити з попереднього коду.
}