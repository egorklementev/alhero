using UnityEngine;

public class Bomb : MonoBehaviour 
{
    public ParticleSystem[] pss;
    public GameObject blastParticles;
    public float blastRadius = 5f;
    public float blastTimer = 3f;
    public Trap trapScript;

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
        if (TryGetComponent<ItemWorld>(out var item)) 
        {
            item.logic.KillInRange(transform.position, blastRadius);
            item.logic.PlaySound("explosion", .2f, transform.position);
        }
        else if (trapScript != null)
        {
            trapScript.logic.KillInRange(transform.position, blastRadius);
            trapScript.logic.PlaySound("explosion", .2f, transform.position);
        }

        Instantiate(blastParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}