using UnityEngine;

public class FurnitureObject : MonoBehaviour
{
    private GameObject _furnitureObject;
    
    public void SetObject(GameObject newObject)
    {
        Destroy(_furnitureObject);
        
        _furnitureObject = Instantiate(newObject, this.transform);
    }
}
