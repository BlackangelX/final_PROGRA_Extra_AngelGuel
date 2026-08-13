using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;

    [Networked] public int Damage { get; set; }

    [SerializeField] private GameObject impactEffectPrefab;

    private TickTimer _destroyTimer;
    private Rigidbody _rb;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.linearVelocity = transform.forward * speed;
        }
        _destroyTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (_destroyTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

   
    private void OnCollisionEnter(Collision collision)
    {
       
        if (Object.HasStateAuthority)
        {
            MovementController target = collision.collider.GetComponentInParent<MovementController>();
            if (target != null && target.Object.InputAuthority != Object.InputAuthority)
            {
                target.ApplyDamage(Damage, Object.InputAuthority);
            }

            if (impactEffectPrefab != null)
            {
               
                ContactPoint contact = collision.contacts[0];

               
                Runner.Spawn(impactEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
            }

            
            Runner.Despawn(Object);
        }
    }
}
