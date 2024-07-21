using UnityEngine;

public class AnimFixer : MonoBehaviour
{
    void Start()
    {
        // Получаем родительский объект
        Transform parentTransform = transform.parent;
        
        if (parentTransform != null)
        {
            // Получаем координаты родительского объекта
            Vector3 parentPosition = parentTransform.position;
            Quaternion parentRotation = parentTransform.rotation;
            
            // Устанавливаем координаты на дочерний объект
            transform.position = parentPosition;
            transform.rotation = parentRotation;
        }
        else
        {
            Debug.LogWarning("Данный объект не имеет родителя.");
        }
    }
}
