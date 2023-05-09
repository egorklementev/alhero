using UnityEngine;

public class EntityDestroyTimer : MonoBehaviour 
{
    public float destructionDelay = 3f;

    private void OnEnable() 
    {
        Destroy(gameObject, destructionDelay);
    }
}