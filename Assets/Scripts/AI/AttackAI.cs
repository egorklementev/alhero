using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackAI : SomeAI 
{
    public float attackDuration;
    public float attackRange;
    public Vector2 projOffset;
    public float shootForce = 20f;
    public string[] immuneEntities;
    public List<Action> postAttackActions = new List<Action>();

    [Range(0f, 359f)]
    public float angleShift = 0f;

    private AIManager _target = null;
    private float _currentAttackDuration;
    private GameObject _projectile = null;

    public override void PrepareAction()
    {
        _currentAttackDuration = attackDuration;
    }

    public override void Act()
    {
        _currentAttackDuration -= Time.fixedDeltaTime;
        if (_currentAttackDuration < 0f)
        {
            _aiManager.Transition("Idle");

            if (_target == null) // The target is already dead
                return;

            if (_projectile != null)
            {
                GameObject proj = Instantiate(
                    _projectile, 
                    transform.position + new Vector3(
                        projOffset.x * Mathf.Sin((transform.rotation.eulerAngles.y + angleShift) / 180f * Mathf.PI), 
                        projOffset.y, 
                        projOffset.x * Mathf.Cos((transform.rotation.eulerAngles.y + angleShift) / 180f * Mathf.PI)), 
                    Quaternion.identity);

                if (proj.TryGetComponent<Rigidbody>(out Rigidbody body))
                {
                    Vector3 throwVector = Vector3.MoveTowards(transform.position, _target.transform.position, shootForce);
                    body.AddForce(throwVector - transform.position, ForceMode.Impulse);
                }

                if (proj.TryGetComponent<Projectile>(out Projectile projScript))
                {
                    projScript.SetImmune(immuneEntities);
                    projScript.onProjectileCollision.AddListener(
                        () => _aiManager.logic.PlaySound(projScript.onCollisionClipName, .5f, proj.transform.position));
                }
            }
            else
            {
                float distanceToTarget = Vector3.Distance(_target.transform.position, transform.position);
                if (distanceToTarget < attackRange)
                {
                    _target.Transition("Death");
                }
                else
                {
                    $"out of range: ({distanceToTarget}/{attackRange})".Log(this);
                }
            }

            foreach (Action act in postAttackActions)
            {
                act.Invoke();
            }
        }
        else
        {
            if (_target != null)
            {
                transform.LookAt(_target.transform);
                Vector3 euler = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0f, euler.y, euler.z);
            }
        }
    }

    public void SetTarget(AIManager target)
    {
        _target = target;
    }

    public void SetProjectile(GameObject projectile)
    {
        _projectile = projectile;
    }
}