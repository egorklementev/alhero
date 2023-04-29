using UnityEngine;

public class ParticleSystemCallbacker : MonoBehaviour 
{
    [SerializeField] private GameObject objectToDestroy;

    private void OnParticleSystemStopped() 
    {
        // Destroy(gameObject);
        Destroy(objectToDestroy);
    }
    
}