using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Callback: Un metodo que se pasa a otra como argumento. Existe una funcion receptora que se ejecuta mas tarde. 
/// Su funcion principal es indicar que hacer justo despues de que concluya un proceso.
/// 
/// NetworkBehaviour lo que hace es exactamente lo mismo que el monobehaviour, pero, tomando en cuenta
/// que realizará funciones de red
/// </summary>
public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    /// <summary>
    /// El Runner es el componente mas importante de photon, porque es el que manda a llamar la API. Es el que nos conecta con el servidor o host
    /// </summary>
    private NetworkRunner runner;

    [SerializeField] private UnityEvent onPlayerJoined;

    [SerializeField] private NetworkPrefabRef playerPrefab; // El prefab del player es un objeto importante. Lo que guarda es el ID
    [SerializeField] private Text matchMessage;
    [Min(0f)] [SerializeField] private float killFeedDuration = 2.5f;

    [SerializeField] private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>(); // Este diccionario me va a servir para tener todos los jugadores que hay en la partida guardados
    private readonly HashSet<PlayerRef> displayedDeaths = new HashSet<PlayerRef>();
    private float messageHideTime;
    private bool finalMessageShown;
    private CameraController localCameraController;

    private void Awake() // es lo primero que sucede al cargar la escena
    {
        Debug.Log("Awake");
        runner = GetComponent<NetworkRunner>();
    }

    private void Start() // Cuando inicia el script
    {
        Debug.Log("Start");
        runner.AddCallbacks(this); // Este runner va a tener vinculadas las callbacks de este script
    }

    private void Update()
    {
        if (runner == null || !runner.IsRunning || matchMessage == null)
        {
            return;
        }

        foreach (MovementController player in FindObjectsByType<MovementController>(FindObjectsSortMode.None))
        {
            if (player.MatchEnded && !finalMessageShown)
            {
                ShowMessage($"Jugador {player.Winner.PlayerId} infectó a Jugador {player.InfectedPlayer.PlayerId}", false);
                finalMessageShown = true;
                return;
            }

            if (!finalMessageShown && player.IsOutOfHealth && !player.LastAttacker.IsNone && displayedDeaths.Add(player.Object.InputAuthority))
            {
                ShowMessage($"Jugador {player.LastAttacker.PlayerId} mató a Jugador {player.Object.InputAuthority.PlayerId}", true);
            }
        }

        if (!finalMessageShown && matchMessage.gameObject.activeSelf && Time.time >= messageHideTime)
        {
            matchMessage.gameObject.SetActive(false);
        }
    }

    private void ShowMessage(string message, bool hideAutomatically)
    {
        matchMessage.text = message;
        matchMessage.gameObject.SetActive(true);
        messageHideTime = hideAutomatically ? Time.time + killFeedDuration : float.PositiveInfinity;
    }

    // Tarea asincrona que nos inicia la partida
    private async Task StartGame(GameMode mode)
    {
        // argumentos son como los parametros de un metodo, sin embargo, argumentos se refiere a los datos que pide una llamada a la api
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "#0001", // esto me sirve para que solo haya una partida en todo el juego, no puede haber más de una sesion con el mismo nombre
            PlayerCount = 2,
            CustomLobbyName = "Looby de pruebas para clase"
        });
    }

    public void StartGameAsHost()
    {
        Task startGame = StartGame(GameMode.Host);
    }

    public void StartGameAsClient()
    {
        Task startGame = StartGame(GameMode.Client);
    }

    #region Metodos de Photon

    // Cuando un jugador entra a la partida
    // PlayerRef es un id que nos dice a cual jugador se refiere
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) // OnPlayerJoined recibe como parametro el jugador exacto que entró a la partida
    {
        // El InputAuthority es el jugador/cliente en su propia computadora. Es para el jugador local
        if (runner.IsServer) // El StateAuthority es el host/server
        {
            int team = 1;

            foreach (NetworkObject existingPlayer in players.Values)
            {
                MovementController existingController = existingPlayer.GetComponent<MovementController>();
                if (existingController != null && existingController.Team == team)
                {
                    team = 2;
                    break;
                }
            }

            Vector3 spawnPosition = new Vector3(team == 1 ? -2f : 2f, 0f, 0f);
            NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
            players.Add(player, playerObject); // Añado a mi jugador a la lista de jugadores
            runner.SetPlayerObject(player, playerObject);
            playerObject.GetComponent<MovementController>().SetTeam(team);
        }

        onPlayerJoined?.Invoke(); // Me apago el canvas
    }

    // Cuando un jugador se sale
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (players.ContainsKey(player))
        {
            runner.Despawn(players[player]);
            players.Remove(player);
        }
    }

    // 🔽 Ocultamos las advertencias falsas UNT0006 analizadas por Visual Studio
#pragma warning disable UNT0006

    // Firma clásica para tu entorno
    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    // Cuando te intentas conectar pero no lo logra
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log(reason);
    }

    // Cuando empieza a intentar conectarte al servidor
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    // Firma corregida con NetDisconnectReason para solucionar el error CS0535 definitivo
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

#pragma warning restore UNT0006 // 🔼 Reactivamos el análisis normal del compilador

    // Cuando alguien se sale de la partida y tienes activada la migracion de host
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    /// <summary>
    /// Photon debe de evaluar todos los inputs que realiza el usuario
    /// 
    /// Estos inputs photon los recibe como un struct, esto nosotros lo podemos saber
    /// gracias a que el callback que recibe OnInput, recibe de parametro un struct de tipo
    /// NetworkInput
    /// </summary>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (localCameraController == null)
        {
            NetworkObject localPlayer = runner.GetPlayerObject(runner.LocalPlayer);
            localCameraController = localPlayer != null
                ? localPlayer.GetComponentInChildren<CameraController>()
                : null;
        }

        NetworkInfoData data = new NetworkInfoData()
        {
            movementAxis = InputManager.Instance.GetMoveInput(),
            runInputPressed = InputManager.Instance.WasRunInputPressed(),
            movingBackwards = InputManager.Instance.IsMovingBackwards(),
            movingOnXAxis = InputManager.Instance.IsMovingOnXAxis(),
            moveInputPressed = InputManager.Instance.IsMoveInputPressed(),
            mousePos = InputManager.Instance.GetMouseDelta(),
            lookRotation = localCameraController != null ? localCameraController.LookRotation : Vector2.zero,
            isShooting = InputManager.Instance.IsShooting(),
            isMeleeAttacking = InputManager.Instance.IsMeleeAttacking()
        };

        input.Set(data);
    }

    // Cuando se intenta mandar un input pero se pierde paquete
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    // Cuando carga una escena
    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    // Cuando empieza a cargar una escena
    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    // Cuando se crea una nueva partida y cuando se termina o elimina una partida, funciona tanto para publicas como privadas
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    // Cuando el host o el server le damos en apagar
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    #endregion

    #region Callbacks de Red Obligatorias (Limpias de Excepciones)
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
    #endregion
}
