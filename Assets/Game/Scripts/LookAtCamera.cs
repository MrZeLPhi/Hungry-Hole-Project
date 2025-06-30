using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("LookAtCamera: Основна камера не знайдена. Будь ласка, переконайтеся, що вона має тег 'MainCamera'.");
            enabled = false; 
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward, mainCameraTransform.rotation * Vector3.up);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0); 
    }
}