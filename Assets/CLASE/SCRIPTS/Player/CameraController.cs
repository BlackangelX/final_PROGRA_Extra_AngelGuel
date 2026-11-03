using System;
using UnityEngine;
using Fusion; 

public class CameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform weaponHolder; 
    [SerializeField] private float mouseSensitivity = 1;
    [Min(0f)] [SerializeField] private float maxMouseDelta = 25f;

    [SerializeField] private float maxAngleY = 80;
    [SerializeField] private float minAngleY = -80;
    [SerializeField] private Renderer[] localHiddenRenderers;

    private Vector2 camVelocity;
    private MovementController movementController;
    private bool isLocalPlayer;
    private bool ignoreNextMouseDelta;

    public Vector2 LookRotation => new Vector2(camVelocity.x, -camVelocity.y);

    // Usamos Networked para que Photon sincronice este ángulo automáticamente con los demás jugadores
    [Networked] private float networkedTargetPitch { get; set; }

    public override void Spawned()
    {
        isLocalPlayer = Object.HasInputAuthority;
        movementController = GetComponentInParent<MovementController>();

        GetComponent<Camera>().enabled = isLocalPlayer;
        GetComponent<AudioListener>().enabled = isLocalPlayer;

        if (isLocalPlayer)
        {
            foreach (Renderer playerRenderer in localHiddenRenderers)
            {
                playerRenderer.enabled = false;
            }
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ignoreNextMouseDelta = true;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer || InputManager.Instance == null ||
            (movementController != null && (movementController.IsOutOfHealth || movementController.MatchEnded)))
        {
            return;
        }

        if (ignoreNextMouseDelta)
        {
            ignoreNextMouseDelta = false;
            return;
        }

        Vector2 rawFrameVelocity = InputManager.Instance.GetMouseDelta() * mouseSensitivity;
        rawFrameVelocity = Vector2.ClampMagnitude(rawFrameVelocity, maxMouseDelta);
        camVelocity += rawFrameVelocity;
        camVelocity.y = Mathf.Clamp(camVelocity.y, minAngleY, maxAngleY);
    }

    public override void FixedUpdateNetwork()
    {
        if (movementController != null && (movementController.IsOutOfHealth || movementController.MatchEnded))
        {
            return;
        }

        // 1. Verificar si este objeto tiene la autoridad del jugador local
        if (GetInput(out NetworkInfoData input))
        {
            // Guardamos el ángulo vertical en la variable sincronizada por red
            networkedTargetPitch = input.lookRotation.y;

            // El jugador local rota de inmediato para no tener lag
            ApplyRotations(networkedTargetPitch, input.lookRotation.x);
        }
        else
        {
            
            ApplyProxyRotations();
        }
    }

    private void ApplyRotations(float pitch, float yaw)
    {
        // Rota la cámara del jugador local arriba y abajo
        transform.localRotation = Quaternion.AngleAxis(pitch, Vector3.right);

        if (weaponHolder != null)
        {
            weaponHolder.localRotation = Quaternion.AngleAxis(pitch, Vector3.right);
        }

        // Rota el cuerpo hacia los lados (Yaw)
        player.localRotation = Quaternion.AngleAxis(yaw, Vector3.up);
    }

    private void ApplyProxyRotations()
    {
        transform.localRotation = Quaternion.AngleAxis(networkedTargetPitch, Vector3.right);

        // Para los demás jugadores en su pantalla: 
        // No tienen tu "camVelocity.x" porque Photon ya se encarga de sincronizar el giro del Player entero mediante el NetworkTransform.
        // Pero el giro del arma arriba/abajo (X) NO lo sincroniza por defecto el NetworkTransform del jugador.

        if (weaponHolder != null)
        {
            // Hacemos que los demás vean cómo tu arma sube o baja basándose en la variable [Networked]
            weaponHolder.localRotation = Quaternion.AngleAxis(networkedTargetPitch, Vector3.right);
        }
    }
}
