using UnityEngine;

public class Bomb : MonoBehaviour 
{
    public ParticleSystem[] pss;
    public GameObject blastParticles;
    public float blastRadius = 5f;
    public float blastTimer = 3f;

    private bool _isActivated = false;
    private float _timer = 0f;
    
    private void FixedUpdate() 
    {
        if (_isActivated)
        {
            _timer -= Time.fixedDeltaTime;
            if (_timer < 0f)
            {
                Explode();
                _timer = blastTimer;
            }
        }     
    }
    public void Activate()
    {
        _isActivated = true;
        _timer = blastTimer;
        foreach (ParticleSystem ps in pss)
        {
            ps.Play();
        }
    }

    public void Explode()
    {
        GetComponent<ItemWorld>().logic.KillInRange(transform.position, blastRadius);
        Instantiate(blastParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}