using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour 
{
    public UnityEvent onProjectileCollision;
    public string onCollisionClipName;

    private List<string> _immuneEntities = new List<string>();

    private void OnCollisionEnter(Collision other) 
    {
        if (TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }

        onProjectileCollision.Invoke();

        if (other.gameObject.TryGetComponent<AIManager>(out AIManager ai))
        {
            foreach (string entName in _immuneEntities)
            {
                if (ai.gameObject.name.Contains(entName))
                {
                    return;
                }
            }

            ai.Transition("Death");
        }

    }

    public void InstantiateEffects(GameObject obj) 
    {
        Instantiate(obj, transform.position, obj.transform.rotation);
    }

    public void SetImmune(params string[] entities)
    {
        foreach (string ent in entities) {
            _immuneEntities.Add(ent);
        }
    }
}