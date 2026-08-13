using System;
using UnityEngine;
using Fusion; 

public class Handgun : Weapon 
{
    [Header("Handgun Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask layers;

    // Propiedades para que el MovementController acceda si lo necesita
    public GameObject ProjectilePrefab => projectile;
    public Transform ShootPoint => shootPoint;

    // El método base que manda a llamar el MovementController
    public override void PhysicShoot()
    {
        if (!HasInputAuthority) return;

        // Ejecutamos el RPC enviando la información
        RPC_PhysicShoot();
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] // El cliente da la orden, el Servidor/Host la ejecuta
    public void RPC_PhysicShoot(RpcInfo info = default)
    {
        if (projectile != null && shootPoint != null)
        {
            // El servidor spawnea de forma segura el proyectil en red en la posición del cañón
            NetworkObject spawnedProjectile = Runner.Spawn(projectile, shootPoint.position, shootPoint.rotation, info.Source, (runner, networkObject) =>
            {
                Bullet bullet = networkObject.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.Damage = damage;
                }
            });
        }
    }

    public override void RaycastShoot()
    {
        if (!HasInputAuthority) return;
        RPC_RaycastShoot();
    }

    /// <summary>
    /// Remote Procedure Call
    /// 
    /// Un RPC sirve para mandar a llamar un método desde un cliente, y que este se ejecute en ya sea
    /// el resto de clientes o donde el servidor decida
    /// 
    /// Para hacer funcionar un RPC hay que cumplir con 2 cosas.
    /// 
    /// 1.- El método que ejecuta el RPC debe tener el prefijo o sufijo RPC en el nombre del método
    /// 2.- Debe tener un identificador de RPC arriba del método. Este identificador requiere
    /// de 2 datos. El primero es, quien está dando la orden, y el segundo es, quien la ejecuta
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_RaycastShoot(RpcInfo info = default)
    {
        Debug.Log("Disparo raycast");
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, maxDistance, layers))
        {
            Debug.Log(hit.collider.name);
        }
    }

    public override void Reload()
    {
        
    }
}
