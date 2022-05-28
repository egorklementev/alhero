using UnityEngine;

public class EntityKiller : MonoBehaviour
{
    public GameObject parent;

    public void SetDead()
    {
        Destroy(parent);        
    }    
}