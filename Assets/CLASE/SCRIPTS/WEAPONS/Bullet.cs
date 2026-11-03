using UnityEngine;
using Fusion;
using System;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool isImpactEffect;

    [Networked] public int Damage { get; set; }

    [SerializeField] private GameObject impactEffectPrefab;

    private TickTimer _destroyTimer;
    private Rigidbody _rb;
    private SphereCollider _collider;
    private bool _hasImpacted;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _destroyTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (_destroyTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        if (!isImpactEffect && _rb != null)
        {
            Vector3 direction = transform.forward;
            float distance = speed * Runner.DeltaTime;
            float radius = _collider != null
                ? _collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z)
                : 0.05f;
            RaycastHit[] hits = Physics.SphereCastAll(_rb.position, radius, direction, distance, ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == _collider || IsShooterCollider(hit.collider))
                {
                    continue;
                }

                ApplyImpact(hit.collider, hit.point, hit.normal);
                return;
            }

            _rb.MovePosition(_rb.position + direction * distance);
        }
    }

   
    private void OnCollisionEnter(Collision collision)
    {
        if (!isImpactEffect && Object.HasStateAuthority && !_hasImpacted && !IsShooterCollider(collision.collider))
        {
            ContactPoint contact = collision.contacts[0];
            ApplyImpact(collision.collider, contact.point, contact.normal);
        }
    }

    private bool IsShooterCollider(Collider hitCollider)
    {
        MovementController target = hitCollider.GetComponentInParent<MovementController>();
        return target != null && target.Object.InputAuthority == Object.InputAuthority;
    }

    private void ApplyImpact(Collider hitCollider, Vector3 point, Vector3 normal)
    {
        if (_hasImpacted)
        {
            return;
        }

        _hasImpacted = true;
        MovementController target = hitCollider.GetComponentInParent<MovementController>();

        if (target != null)
        {
            target.ApplyDamage(Damage, Object.InputAuthority);
        }

        if (impactEffectPrefab != null)
        {
            Runner.Spawn(impactEffectPrefab, point + normal * 0.02f, Quaternion.LookRotation(normal));
        }

        Runner.Despawn(Object);
    }
}
