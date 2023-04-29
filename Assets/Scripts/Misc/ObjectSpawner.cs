using UnityEngine;

public class ObjectSpawner : MonoBehaviour 
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform anchor;    
    [SerializeField] private Vector3 offset;

    public void SpawnObject()
    {
        Instantiate(objectToSpawn, anchor.position + offset, objectToSpawn.transform.rotation);
    }
}