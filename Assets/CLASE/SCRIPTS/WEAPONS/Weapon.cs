using Fusion;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    [Header("Configuración del Enum")]
    public ShootType shootType;

    [Header("Variables de Munición")]
    public int damage;
    public int actualAmmo;
    public int magazineSize;
    public int maxAmmoCapacity;

    [Header("Prefab de Proyectil")]
    public GameObject projectile;

    // Métodos abstractos de la rúbrica
    public abstract void RaycastShoot();
    public abstract void PhysicShoot();
    public abstract void Reload();
}

public enum ShootType
{
    Raycast,
    Physic
}