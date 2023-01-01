using UnityEngine;

public class ObjectDestoryer : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToDestroy;

    public void SetDestroyed()
    {
        Destroy(objectToDestroy);
    }
}