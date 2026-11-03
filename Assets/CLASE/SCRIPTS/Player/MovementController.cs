using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkCharacterController), typeof(GroundCheck))]
public class MovementController : NetworkBehaviour
{
    private NetworkCharacterController _ncc;

    [SerializeField] private Animator _animator;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float crouchSpeed = 3.9f;

    [Header("Health Settings")]
    [Min(1)] [SerializeField] private int maxHealth = 100;
    [Min(0f)] [SerializeField] private float regenerationDelay = 5f;
    [Min(1)] [SerializeField] private int regenerationAmount = 5;
    [Min(0.05f)] [SerializeField] private float regenerationInterval = 1f;
    [Min(0f)] [SerializeField] private float respawnDelay = 3f;

    [Networked] public int CurrentHealth { get; private set; }
    [Networked] public bool IsOutOfHealth { get; private set; }
    [Networked] public PlayerRef LastAttacker { get; private set; }
    [Networked] public int Team { get; private set; }
    [Networked] public PlayerRef InfectedBy { get; private set; }
    [Networked] public bool MatchEnded { get; private set; }
    [Networked] public PlayerRef Winner { get; private set; }
    [Networked] public PlayerRef InfectedPlayer { get; private set; }
    [Networked] private TickTimer RegenerationTimer { get; set; }
    [Networked] private TickTimer RespawnTimer { get; set; }

    public int MaxHealth => maxHealth;

    [Header("Camera & Rotation Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float maxAngleY = 80f;
    [SerializeField] private float minAngleY = -80f;

    [Header("Weapon References")]
    [SerializeField] private Handgun currentWeapon;

    [Header("Melee Settings")]
    [Min(0f)] [SerializeField] private float meleeRange = 2f;
    [Min(0f)] [SerializeField] private float meleeRadius = 0.5f;
    [Min(1)] [SerializeField] private int meleeDamage = 25;
    [SerializeField] private LayerMask meleeLayers = ~0;

    private Vector2 _camVelocity;
    private Renderer[] _renderers;
    private bool[] _rendererStates;
    private bool _deathVisualApplied;
    private Vector3 _spawnPosition;
    private Quaternion _spawnRotation;

    [Networked] private float NetworkedTargetPitch { get; set; }

    public override void Spawned()
    {
        _ncc = GetComponent<NetworkCharacterController>();
        _spawnPosition = transform.position;
        _spawnRotation = transform.rotation;

        if (Object.HasStateAuthority)
        {
            CurrentHealth = maxHealth;
            IsOutOfHealth = false;
            LastAttacker = PlayerRef.None;
            InfectedBy = PlayerRef.None;
            MatchEnded = false;
            Winner = PlayerRef.None;
            InfectedPlayer = PlayerRef.None;
        }

        if (Object.HasInputAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ApplyDamage(int damage, PlayerRef attacker)
    {
        if (!Object.HasStateAuthority || MatchEnded || damage <= 0 || IsOutOfHealth || attacker == Object.InputAuthority)
        {
            return;
        }

        NetworkObject attackerObject = Runner.GetPlayerObject(attacker);
        MovementController attackerController = attackerObject != null ? attackerObject.GetComponent<MovementController>() : null;

        if (attackerController == null)
        {
            foreach (MovementController player in FindObjectsByType<MovementController>(FindObjectsSortMode.None))
            {
                if (player.Object != null && player.Object.InputAuthority == attacker)
                {
                    attackerController = player;
                    break;
                }
            }
        }

        if (Team != 0 && attackerController != null && attackerController.Team == Team)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        if (CurrentHealth == 0)
        {
            Die(attacker);
            return;
        }

        RegenerationTimer = TickTimer.CreateFromSeconds(Runner, regenerationDelay);
    }

    public void SetTeam(int team)
    {
        if (Object.HasStateAuthority)
        {
            Team = team;
        }
    }

    public override void FixedUpdateNetwork()
    {
        Respawn();
        RegenerateHealth();

        if (MatchEnded)
        {
            _ncc.Move(Vector3.zero);
            return;
        }

        if (IsOutOfHealth)
        {
            return;
        }

        if (GetInput(out NetworkInfoData data))
        {
         
            Movement(data);

          
         
            if (data.isShooting && Object.HasStateAuthority)
            {
             
                if (currentWeapon == null)
                {
                    Debug.LogWarning("[MovementController] , el objeto de tu arma no está asignado en la casilla 'currentWeapon' del inspector.");
                }
                else
                {
                 
                    currentWeapon.PhysicShoot();
                }
            }

            if (data.isMeleeAttacking)
            {
                MeleeAttack();
            }

  
            if (Object.HasInputAuthority)
            {
                bool isMoving = data.movementAxis != Vector2.zero;

                _animator.SetBool("IsWalking", isMoving);
                _animator.SetBool("IsRunning", data.runInputPressed);
                _animator.SetFloat("WalkingZ", data.movementAxis.y);
                _animator.SetFloat("WalkingX", data.movementAxis.x);
            }
        }
        else
        {
          
            ApplyProxyWeaponRotation();
        }
    }

    public override void Render()
    {
        if (IsOutOfHealth && !_deathVisualApplied)
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _rendererStates = new bool[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
            {
                _rendererStates[i] = _renderers[i].enabled;
                _renderers[i].enabled = false;
            }

            _deathVisualApplied = true;
        }
        else if (!IsOutOfHealth && _deathVisualApplied)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = _rendererStates[i];
            }

            _deathVisualApplied = false;
        }
    }

    private void Die(PlayerRef attacker)
    {
        if (!Object.HasStateAuthority || IsOutOfHealth)
        {
            return;
        }

        LastAttacker = attacker;
        IsOutOfHealth = true;
        RegenerationTimer = TickTimer.None;
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, respawnDelay);
    }

    private void Respawn()
    {
        if (!Object.HasStateAuthority || !IsOutOfHealth || !RespawnTimer.Expired(Runner))
        {
            return;
        }

        _ncc.Teleport(_spawnPosition, _spawnRotation);

        NetworkObject attackerObject = Runner.GetPlayerObject(LastAttacker);
        MovementController attackerController = attackerObject != null ? attackerObject.GetComponent<MovementController>() : null;

        if (attackerController == null)
        {
            foreach (MovementController player in FindObjectsByType<MovementController>(FindObjectsSortMode.None))
            {
                if (player.Object != null && player.Object.InputAuthority == LastAttacker)
                {
                    attackerController = player;
                    break;
                }
            }
        }

        bool infectionCompleted = attackerController != null && attackerController.Team != 0;

        if (infectionCompleted)
        {
            Team = attackerController.Team;
            InfectedBy = LastAttacker;
        }

        CurrentHealth = maxHealth;
        IsOutOfHealth = false;
        RespawnTimer = TickTimer.None;
        RegenerationTimer = TickTimer.None;

        if (infectionCompleted && IsOnlyOneTeamRemaining())
        {
            FinishMatch(LastAttacker, Object.InputAuthority);
        }
    }

    private bool IsOnlyOneTeamRemaining()
    {
        int survivingTeam = 0;
        int playerCount = 0;

        foreach (MovementController player in FindObjectsByType<MovementController>(FindObjectsSortMode.None))
        {
            if (player.Object == null || !player.Object.HasStateAuthority || player.Team == 0)
            {
                continue;
            }

            playerCount++;
            if (survivingTeam == 0)
            {
                survivingTeam = player.Team;
            }
            else if (player.Team != survivingTeam)
            {
                return false;
            }
        }

        return playerCount >= 2;
    }

    private void FinishMatch(PlayerRef winner, PlayerRef infectedPlayer)
    {
        if (!Object.HasStateAuthority || MatchEnded)
        {
            return;
        }

        foreach (MovementController player in FindObjectsByType<MovementController>(FindObjectsSortMode.None))
        {
            if (player.Object == null || !player.Object.HasStateAuthority)
            {
                continue;
            }

            player.MatchEnded = true;
            player.Winner = winner;
            player.InfectedPlayer = infectedPlayer;

            if (player._animator != null)
            {
                player._animator.SetBool("IsWalking", false);
                player._animator.SetBool("IsRunning", false);
                player._animator.SetFloat("WalkingZ", 0f);
                player._animator.SetFloat("WalkingX", 0f);
            }
        }
    }

    private void RegenerateHealth()
    {
        if (!Object.HasStateAuthority || IsOutOfHealth || CurrentHealth >= maxHealth || !RegenerationTimer.Expired(Runner))
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + regenerationAmount);
        RegenerationTimer = CurrentHealth < maxHealth
            ? TickTimer.CreateFromSeconds(Runner, regenerationInterval)
            : TickTimer.None;
    }

    private void MeleeAttack()
    {
        if (!Object.HasStateAuthority || IsOutOfHealth)
        {
            return;
        }

        Vector3 origin = cameraTransform != null ? cameraTransform.position : transform.position + Vector3.up;
        Vector3 direction = cameraTransform != null ? cameraTransform.forward : transform.forward;
        RaycastHit[] hits = Physics.SphereCastAll(origin, meleeRadius, direction, meleeRange, meleeLayers, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

        foreach (RaycastHit hit in hits)
        {
            MovementController target = hit.collider.GetComponentInParent<MovementController>();

            if (target == this)
            {
                continue;
            }

            if (target != null)
            {
                target.ApplyDamage(meleeDamage, Object.InputAuthority);
            }

            break;
        }
    }

    #region Movimiento

    private void Movement(NetworkInfoData data)
    {
        data.movementAxis.Normalize();

        Quaternion yawRotation = Quaternion.Euler(0f, data.lookRotation.x, 0f);
        transform.rotation = yawRotation;

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.AngleAxis(data.lookRotation.y, Vector3.right);
        }

        NetworkedTargetPitch = data.lookRotation.y;

        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;
        Vector3 moveDirection = forward * data.movementAxis.y + right * data.movementAxis.x;

        _ncc.maxSpeed = Speed(data);
        _ncc.Move(moveDirection);
        transform.rotation = yawRotation;
    }

    private float Speed(NetworkInfoData data)
    {
        return data.movingBackwards || data.movingOnXAxis ? walkSpeed :
               data.runInputPressed ? runSpeed : walkSpeed;
    }

    #endregion

    #region Rotación de Red

    private void RotatePlayerAndCamera(NetworkInfoData data)
    {
        _camVelocity.x += data.mousePos.x * mouseSensitivity;
        _camVelocity.y += data.mousePos.y * mouseSensitivity;

        _camVelocity.y = Mathf.Clamp(_camVelocity.y, minAngleY, maxAngleY);

        transform.localRotation = Quaternion.AngleAxis(_camVelocity.x, Vector3.up);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.AngleAxis(-_camVelocity.y, Vector3.right);
        }

        if (weaponHolder != null)
        {
            weaponHolder.localRotation = Quaternion.AngleAxis(-_camVelocity.y, Vector3.right);
        }

        NetworkedTargetPitch = -_camVelocity.y;
    }

    private void ApplyProxyWeaponRotation()
    {
        if (weaponHolder != null)
        {
            weaponHolder.localRotation = Quaternion.AngleAxis(NetworkedTargetPitch, Vector3.right);
        }
    }

    #endregion
}
