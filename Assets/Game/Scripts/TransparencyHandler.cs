using UnityEngine;
using System.Collections.Generic;

public class TransparencyHandler : MonoBehaviour
{
    public Transform playerTransform;
    public float raycastDistance = 100f;
    public LayerMask obstacleLayer; // Створіть шар (Layer) для об'єктів-перешкод

    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    public Material transparentMaterial;

    void Update()
    {
        if (playerTransform == null || transparentMaterial == null)
        {
            return;
        }

        RaycastHit hit;
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        // Використовуємо Raycast, щоб перевірити, чи є щось між камерою та гравцем
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, raycastDistance, obstacleLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Перевіряємо, чи об'єкт, у який ми влучили, ще не є прозорим
            if (!originalMaterials.ContainsKey(hitObject))
            {
                // Запам'ятовуємо оригінальний матеріал та змінюємо на прозорий
                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials.Add(hitObject, renderer.material);
                    renderer.material = transparentMaterial;
                }
            }
        }
        else
        {
            // Якщо між камерою та гравцем нічого немає, повертаємо матеріали до оригінальних
            List<GameObject> objectsToRemove = new List<GameObject>();
            foreach (var pair in originalMaterials)
            {
                Renderer renderer = pair.Key.GetComponent<Renderer>();
                if (renderer != null && renderer.material == transparentMaterial)
                {
                    renderer.material = pair.Value;
                    objectsToRemove.Add(pair.Key);
                }
            }
            foreach (var obj in objectsToRemove)
            {
                originalMaterials.Remove(obj);
            }
        }
    }
}