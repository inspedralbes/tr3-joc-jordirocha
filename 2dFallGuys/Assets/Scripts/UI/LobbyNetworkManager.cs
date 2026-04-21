using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

// Classes per a la serialització de JSON amb JsonUtility
[Serializable]
public class WsMessage
{
    public string type;
    public string username;
    public string roomCode;
    public string message;
    public bool isHost;
    public int seed;
    public string id;
    public string playerId;
    public float x;
    public float y;
    public float scaleX;
    public float time;
    public int score;
    public PlayerData[] players;
    public MatchResultData[] matchResults;
}

[Serializable]
public class PlayerData
{
    public string id;
    public string name;
}

[Serializable]
public class MatchResultData
{
    public string id;
    public string name;
    public float time;
    public int score;
}

public class LobbyNetworkManager : MonoBehaviour
{
    [Header("Configuració Xarxa")]
    [SerializeField] private string serverUrl = "ws://localhost:8080";
    [Tooltip("El nom de l'escena on es desenvoluparà la partida. Ex: 'SampleScene'")]
    [SerializeField] private string gameSceneName = "SampleScene";

    // Variables pel WebSocket de fil secundari
    private ClientWebSocket webSocket;
    private CancellationTokenSource cts;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    // Elements d'UI Toolkit
    private VisualElement joinScreen;
    private VisualElement roomScreen;
    
    private Button btnCreateRoom;
    private Button btnJoinRoom;
    private Button btnLeaveRoom;
    private Button btnStartGame;
    
    private TextField inputRoomCode;
    private Label lblStatusMessage;
    private Label lblCurrentRoomCode;
    private ScrollView playersList;

    // Estat local
    public string CurrentRoomCode { get { return currentRoomCode; } }
    private string currentRoomCode = "";
    private bool isHostUser = false;
    private string myUsername = "Jugador";

    // Variables compartides pel Joc Multijugador (persistiran al canviar d'escena)
    public static int roomSeed = 0;
    public static PlayerData[] roomPlayers = null;
    public static string localPlayerId = "";

    // Diccionari actiu pels jugadors sincronitzats
    public Dictionary<string, PlayerNetworkSync> networkedPlayers = new Dictionary<string, PlayerNetworkSync>();

    // Esdeveniments per a la UI In-Game (ResultScreen)
    public static event Action<MatchResultData[]> OnMatchResultsReceived;
    public static event Action<string, float, int> OnPlayerFinishedReceived;

    // Singleton per persistir en l'escena del joc
    public static LobbyNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Enllaçar nom dinàmic guardat en l'autenticació, o aleatori de fallback.
        if (!string.IsNullOrEmpty(AuthUIController.LoggedInUsername))
        {
            myUsername = AuthUIController.LoggedInUsername;
        }
        else
        {
            myUsername = "Jugador_" + UnityEngine.Random.Range(1000, 9999).ToString();
        }

        // Enllaçar UI
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        joinScreen = root.Q<VisualElement>("JoinScreen");
        roomScreen = root.Q<VisualElement>("RoomScreen");

        btnCreateRoom = root.Q<Button>("BtnCreateRoom");
        btnJoinRoom = root.Q<Button>("BtnJoinRoom");
        btnLeaveRoom = root.Q<Button>("BtnLeaveRoom");
        btnStartGame = root.Q<Button>("BtnStartGame");

        inputRoomCode = root.Q<TextField>("InputRoomCode");
        lblStatusMessage = root.Q<Label>("LblStatusMessage");
        lblCurrentRoomCode = root.Q<Label>("LblCurrentRoomCode");
        playersList = root.Q<ScrollView>("PlayersList");

        // Afegir Events UI
        btnCreateRoom.clicked += OnCreateRoomClicked;
        btnJoinRoom.clicked += OnJoinRoomClicked;
        btnLeaveRoom.clicked += OnLeaveRoomClicked;
        btnStartGame.clicked += OnStartGameClicked;

        // Iniciar connexió al servidor
        ConnectToServer();
    }

    private void OnDisable()
    {
        // Netejar la desconnexió en tancar o desactivar l'script
        if (webSocket != null)
        {
            cts?.Cancel();
            webSocket.Dispose();
        }
    }

    private void Update()
    {
        // Aquest mètode s'executa al Fil Principal (Main Thread) de Unity.
        // Processa els missatges rebuts per la cua concurrent i els tradueix en accions UI.
        while (messageQueue.TryDequeue(out string jsonMessage))
        {
            ProcessMessageOnMainThread(jsonMessage);
        }
    }

    #region Connexió i Fils Secundaris

    private async void ConnectToServer()
    {
        webSocket = new ClientWebSocket();
        cts = new CancellationTokenSource();

        try
        {
            UpdateStatus("Connectant al servidor...");
            await webSocket.ConnectAsync(new Uri(serverUrl), cts.Token);
            UpdateStatus("Connectat. Llest per jugar.");
            
            // Iniciar el procés de recepció de missatges en un fil asíncron al fons
            _ = ReceiveMessages(cts.Token);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error connectant: {ex.Message}");
            Debug.LogError($"WebSocket Error: {ex.Message}");
        }
    }

    private async Task ReceiveMessages(CancellationToken token)
    {
        byte[] buffer = new byte[8192];

        try
        {
            while (webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Servidor tancat", token);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // L'enviem a la cua perquè el Main Thread se n'ocupi
                    messageQueue.Enqueue(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Recepció de WebSocket cancel·lada.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al fil de recepció WS: {ex.Message}");
        }
    }

    public async void SendMessageToServer(WsMessage msg)
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
        }
        else
        {
            UpdateStatus("Error d'enviament: no estàs connectat.");
        }
    }

    #endregion

    #region Processament Lògic (Main Thread)

    private void ProcessMessageOnMainThread(string json)
    {
        try
        {
            WsMessage msg = JsonUtility.FromJson<WsMessage>(json);

            switch (msg.type)
            {
                case "ROOM_CREATED":
                    currentRoomCode = msg.roomCode;
                    isHostUser = msg.isHost;
                    localPlayerId = msg.playerId;
                    ShowRoomScreen();
                    break;

                case "JOINED_ROOM":
                    currentRoomCode = msg.roomCode;
                    isHostUser = msg.isHost;
                    localPlayerId = msg.playerId;
                    ShowRoomScreen();
                    break;

                case "ROOM_UPDATED":
                    UpdatePlayerList(msg.players);
                    break;

                case "BECAME_HOST":
                    isHostUser = true;
                    btnStartGame.style.display = DisplayStyle.Flex; // El botó d'inici ara és visible
                    break;

                case "START_GAME":
                case "MATCH_STARTED":
                    Debug.Log($"Carregant escena multijugador... Seed: {msg.seed}");
                    UpdateStatus("La partida està a punt de començar!");
                    
                    roomSeed = msg.seed;
                    roomPlayers = msg.players;
                    
                    // Amagar la Interfície del Lobby abans de saltar
                    var uiDocument = GetComponent<UIDocument>();
                    if (uiDocument != null) uiDocument.rootVisualElement.style.display = DisplayStyle.None;
                    
                    UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
                    break;

                case "PLAYER_TRANSFORM":
                    // Deleguem les noves coordenades al component del jugador corresponent per fer-ne Lerp en pantalla real.
                    if (networkedPlayers.TryGetValue(msg.id, out PlayerNetworkSync syncScript))
                    {
                        syncScript.UpdateTargetPosition(msg.x, msg.y, msg.scaleX);
                    }
                    break;

                case "PLAYER_FINISHED_BROADCAST":
                    Debug.Log($"Jugador {msg.id} ha arribat a la meta! Temps: {msg.time} segons.");
                    OnPlayerFinishedReceived?.Invoke(msg.id, msg.time, msg.score);
                    break;

                case "MATCH_RESULTS":
                    Debug.Log($"Rebuts resultats finals de la partida. Mostrant Podi...");
                    OnMatchResultsReceived?.Invoke(msg.matchResults);
                    break;

                case "ERROR":
                    UpdateStatus($"Error del servidor: {msg.message}");
                    break;

                default:
                    Debug.LogWarning($"Missatge no reconegut: {msg.type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"No s'ha pogut parsejar el JSON: {ex.Message}. Raw: {json}");
        }
    }

    #endregion

    #region Invocar Esdeveniments UI

    private void OnCreateRoomClicked()
    {
        WsMessage msg = new WsMessage
        {
            type = "CREATE_ROOM",
            username = myUsername
        };
        SendMessageToServer(msg);
    }

    private void OnJoinRoomClicked()
    {
        string code = inputRoomCode.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        {
            UpdateStatus("Si us plau, introdueix un codi de sala.");
            return;
        }

        WsMessage msg = new WsMessage
        {
            type = "JOIN_ROOM",
            roomCode = code,
            username = myUsername
        };
        SendMessageToServer(msg);
    }

    private void OnLeaveRoomClicked()
    {
        // Simplement desconnectem i reconnectem per restablir l'estat en el servidor
        UpdateStatus("Surtint de la sala...");
        
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            cts?.Cancel();
            webSocket.Dispose();
            ConnectToServer(); // Tornar-se a connectar pur
        }
        
        ShowJoinScreen();
    }

    private void OnStartGameClicked()
    {
        if (isHostUser)
        {
            WsMessage msg = new WsMessage
            {
                type = "START_GAME",
                roomCode = currentRoomCode
            };
            SendMessageToServer(msg);
        }
    }

    #endregion

    #region Ajuts de Pantalla

    private void ShowJoinScreen()
    {
        joinScreen.style.display = DisplayStyle.Flex;
        roomScreen.style.display = DisplayStyle.None;
        currentRoomCode = "";
        isHostUser = false;
        
        UpdateStatus(""); // Neteja l'estatus antic
    }

    private void ShowRoomScreen()
    {
        joinScreen.style.display = DisplayStyle.None;
        roomScreen.style.display = DisplayStyle.Flex;

        lblCurrentRoomCode.text = $"Codi: {currentRoomCode}";

        // Només mostrar el botó Iniciar si som el host
        btnStartGame.style.display = isHostUser ? DisplayStyle.Flex : DisplayStyle.None;
        
        playersList.Clear();
    }

    private void UpdatePlayerList(PlayerData[] players)
    {
        if (players == null) return;

        playersList.Clear();

        foreach (var p in players)
        {
            Label playerLabel = new Label(p.name);
            playerLabel.AddToClassList("player-item");
            playersList.Add(playerLabel);
        }
    }

    private void UpdateStatus(string text)
    {
        if (lblStatusMessage != null)
        {
            lblStatusMessage.text = text;
        }
    }

    #endregion
    
    #region Jugadors Multijugador Dinàmics
    
    public void RegisterPlayer(string rcvPlayerId, PlayerNetworkSync syncScript)
    {
        if (!networkedPlayers.ContainsKey(rcvPlayerId))
        {
            networkedPlayers.Add(rcvPlayerId, syncScript);
        }
    }

    #endregion
}
