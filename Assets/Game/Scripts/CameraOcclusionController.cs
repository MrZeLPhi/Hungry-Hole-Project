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
    
    [Header("References")]
    public GameProgressionManager gameProgressionManager; 
    public HoleHandler holeHandler; 

    // Внутрішня структура для зберігання оригінального кольору матеріалу
    private struct OriginalMaterialInfo
    {
        public Color originalColor;       
        public int originalRenderQueue; 
        public string originalRenderType; 
    }

    // Словник для відстеження об'єктів, які зараз прозорі/змінені
    private Dictionary<Renderer, OriginalMaterialInfo> occludingRenderersInfo = new Dictionary<Renderer, OriginalMaterialInfo>();

    // Змінна для відстеження об'єктів, які мають бути прозорими в поточному кадрі
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
    }
    
    void OnDisable()
    {
        foreach (var entry in occludingRenderersInfo)
        {
            Renderer renderer = entry.Key;
            OriginalMaterialInfo originalInfo = entry.Value;
            
            if (renderer != null && renderer.material != null)
            {
                SetMaterialOpaqueInstant(renderer.material, originalInfo); 
            }
        }
        occludingRenderersInfo.Clear();
    }

    void LateUpdate()
    {
        renderersToMakeTransparentThisFrame.Clear();

        if (playerTarget == null || gameProgressionManager == null || holeHandler == null || !enabled) 
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
                    Material originalMatInstance = renderer.material; 
                    occludingRenderersInfo.Add(renderer, 
                        new OriginalMaterialInfo { originalColor = originalMatInstance.color, originalRenderQueue = originalMatInstance.renderQueue, originalRenderType = originalMatInstance.GetTag("RenderType", false) });
                }
                SetMaterialTransparentInstant(renderer.material, targetAlpha); 
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

            // <<< ВИПРАВЛЕНО: ВИДАЛЕНО УМОВУ isInHoleCollider >>>
            // Об'єкт стає непрозорим ТІЛЬКИ, якщо він більше НЕ блокує огляд.
            bool shouldBeOpaqueBecauseNotBlocking = !renderersToMakeTransparentThisFrame.Contains(renderer);
            // bool isInHoleCollider = (holeHandler != null && holeHandler.gameObject.GetComponent<Collider>() != null && renderer.bounds.Intersects(holeHandler.gameObject.GetComponent<Collider>().bounds));
            
            if (shouldBeOpaqueBecauseNotBlocking /* || isInHoleCollider */) // isInHoleCollider видалено з умови
            {
                SetMaterialOpaqueInstant(renderer.material, occludingRenderersInfo[renderer]);
                Debug.Log($"CameraOcclusionController: Об'єкт '{renderer.name}' повертається до непрозорого (більше не блокує)."); // Змінено лог
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

    void SetMaterialTransparentInstant(Material material, float alpha)
    {
        if (material == null) return;
        Color color = material.color;
        color.a = alpha;
        material.color = color;

        material.SetInt("_Mode", 2); 
        material.SetOverrideTag("RenderType", "Fade");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0); 
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    }

    void SetMaterialOpaqueInstant(Material material, OriginalMaterialInfo originalInfo)
    {
        if (material == null) return;
        material.color = originalInfo.originalColor; 
        material.SetInt("_Mode", 0); 
        material.SetOverrideTag("RenderType", originalInfo.originalRenderType); 
        material.renderQueue = originalInfo.originalRenderQueue; 
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1); 
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.EnableKeyword("_ALPHATEST_ON"); 
    }
}