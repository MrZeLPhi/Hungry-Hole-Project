using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // Знаходимо основну камеру на сцені.
        // Переконайтеся, що ваша камера має тег "MainCamera" в Інспекторі.
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("LookAtCamera: Основна камера не знайдена. Будь ласка, переконайтеся, що вона має тег 'MainCamera'.");
            enabled = false; // Вимкнути скрипт, якщо камеру не знайдено
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // Це найпростіший та найефективніший спосіб змусити 3D-об'єкт
        // (такий як ваш слайдер, якщо він у World Space Canvas, або просто 3D-об'єкт)
        // завжди дивитися обличчям до камери.
        // transform.LookAt() робить так, щоб вісь Z (синя) об'єкта вказувала на ціль.
        // Ми хочемо, щоб об'єкт дивився НА камеру, тому ціллю буде position + forward від камери.
        // Але для UI-білбордів краще, щоб вони були плоскими до камери.
        // transform.rotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
        // Або, щоб зберегти "вгору" для об'єкта, що відповідає "вгору" для світу:
        transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward);
        // Забезпечуємо, щоб вісь Y об'єкта залишалася "вгору" відносно світової осі Y,
        // це запобігає перевертанню UI.
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0); // Занулюємо обертання по Z, якщо UI має бути плоским.
    }
}