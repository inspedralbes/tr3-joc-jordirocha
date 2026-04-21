using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controlador principal de la interfície durant la partida (GameHUD i ResultScreen).
/// Adaptat per usar CSS modern, posicions geomètriques i animacions orgàniques.
/// </summary>
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Documents UI")]
    public UIDocument gameHUDDocument;
    public UIDocument resultScreenDocument;

    // Elements de la GameHUD
    private VisualElement mainHUDContent;
    private VisualElement spectatorContainer;
    private Button btnSpectatorPrev;
    private Button btnSpectatorNext;
    private Label lblTemps;
    private Label lblMonedes;
    
    // Panell per leaderboard animat
    private VisualElement leaderboardDynamicContainer;

    // Elements de la ResultScreen
    private VisualElement resultContainer;
    private VisualElement resultBox;
    private ScrollView othersList;
    private Button btnTornarLobby;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeGameHUD();
        InitializeResultScreen();

        LobbyNetworkManager.OnMatchResultsReceived += ShowResultScreen;
        StartCoroutine(UpdateLeaderboardRutine());
    }

    private void OnDestroy()
    {
        LobbyNetworkManager.OnMatchResultsReceived -= ShowResultScreen;
    }

    private void Update()
    {
        // Actualitza temps i punts localment sense canviar fons
        if (ScoreManager.Instance != null && lblTemps != null && lblMonedes != null)
        {
            lblTemps.text = $"{ScoreManager.Instance.tempsTranscorregut:F1}"; // La 's' ara és un element a part
            lblMonedes.text = $"{ScoreManager.Instance.puntsTotals}";
        }
    }

    private void InitializeGameHUD()
    {
        if (gameHUDDocument == null) return;
        var root = gameHUDDocument.rootVisualElement;
        if (root == null) return;

        lblTemps = root.Q<Label>("LblTemps");
        lblMonedes = root.Q<Label>("LblMonedes");
        
        mainHUDContent = root.Q<VisualElement>("MainHUDContent");
        spectatorContainer = root.Q<VisualElement>("SpectatorContainer");
        btnSpectatorPrev = root.Q<Button>("BtnSpectatorPrev");
        btnSpectatorNext = root.Q<Button>("BtnSpectatorNext");

        if (btnSpectatorPrev != null)
            btnSpectatorPrev.clicked += () => { if (SpectatorManager.Instance != null) SpectatorManager.Instance.PrevTarget(); };
            
        if (btnSpectatorNext != null)
            btnSpectatorNext.clicked += () => { if (SpectatorManager.Instance != null) SpectatorManager.Instance.NextTarget(); };
        
        leaderboardDynamicContainer = root.Q<VisualElement>("LeaderboardDynamicContainer");
    }

    private void InitializeResultScreen()
    {
        if (resultScreenDocument == null) return;
        var root = resultScreenDocument.rootVisualElement;
        if (root == null) return;

        resultContainer = root.Q<VisualElement>("ResultContainer");
        resultBox = root.Q<VisualElement>("ResultBox");
        othersList = root.Q<ScrollView>("OthersList");
        btnTornarLobby = root.Q<Button>("BtnTornarLobby");

        if (btnTornarLobby != null)
        {
            btnTornarLobby.clicked += () => {
                if (LobbyNetworkManager.Instance != null) {
                    Destroy(LobbyNetworkManager.Instance.gameObject);
                }
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            };
        }
    }

    private IEnumerator UpdateLeaderboardRutine()
    {
        while (true)
        {
            if (leaderboardDynamicContainer != null)
            {
                UpdateLeaderboardUI();
            }
            // Reordenament cada 1 segon per donar peu a les transicions USS de veure el moviment
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateLeaderboardUI()
    {
        if (leaderboardDynamicContainer == null) return;
        
        // 1. OBLIGATORI: Buidar contenidor visual per evitar acumulació de "gins" morts com se sol·licitava
        leaderboardDynamicContainer.Clear();
        
        // 2. Diccionari ÚNIC per a jugadors actius usant l'ID real, evita auto-replicants
        var uniquePlayers = new Dictionary<string, (Transform t, bool isLocal)>();

        var localPlayer = GameObject.FindWithTag("Player");
        if (localPlayer != null) 
        {
            string idLocal = !string.IsNullOrEmpty(LobbyNetworkManager.localPlayerId) ? LobbyNetworkManager.localPlayerId : "LOCAL";
            uniquePlayers[idLocal] = (localPlayer.transform, true);
        }

        if (LobbyNetworkManager.Instance != null && LobbyNetworkManager.Instance.networkedPlayers != null)
        {
            foreach (var kvp in LobbyNetworkManager.Instance.networkedPlayers)
            {
                if (kvp.Value != null) 
                {
                    // Si el servidor ens retorna el nostre ID com a player xarxa (i no s'havia eliminat abans),
                    // al tractar-se d'un diccionari es sobreescriurà naturalment, evitant repetits. El valor "False" assumiria espectre remot..
                    uniquePlayers[kvp.Key] = (kvp.Value.transform, uniquePlayers.ContainsKey(kvp.Key) ? uniquePlayers[kvp.Key].isLocal : false);
                }
            }
        }

        // Ordenem de major a menor per posició X
        var sorted = uniquePlayers.OrderByDescending(p => p.Value.t.position.x).ToList();

        // 3. Crear elements nets un per un
        for (int i = 0; i < sorted.Count; i++)
        {
            var pData = sorted[i];
            
            VisualElement item = new VisualElement();
            item.AddToClassList("leaderboard-pill");
            if (pData.Value.isLocal) item.AddToClassList("local-player");
            
            Label posLbl = new Label();
            posLbl.name = "Pos";
            posLbl.AddToClassList("lb-pos");

            string playerName = "Desconegut";
            if (LobbyNetworkManager.roomPlayers != null)
            {
                playerName = LobbyNetworkManager.roomPlayers.FirstOrDefault(p => p.id == pData.Key)?.name;
                if (string.IsNullOrEmpty(playerName)) 
                {
                    playerName = pData.Value.isLocal ? "Tu" : "Rival";
                }
            }
            else
            {
                playerName = pData.Value.isLocal ? "Tu" : "Rival";
            }

            Label nameLbl = new Label(playerName);
            nameLbl.name = "Name";
            nameLbl.AddToClassList("lb-name");

            item.Add(posLbl);
            item.Add(nameLbl);

            leaderboardDynamicContainer.Add(item);

            item.Q<Label>("Pos").text = (i + 1).ToString();
            
            // Assignació vertical de layout CSS
            item.style.top = i * 65; 

            if (i == 0) item.AddToClassList("rank-1");
        }
    }

    public void SetSpectatorHUD()
    {
        if (mainHUDContent != null) mainHUDContent.style.display = DisplayStyle.None;
        if (spectatorContainer != null) spectatorContainer.style.display = DisplayStyle.Flex;
    }

    private void ShowResultScreen(MatchResultData[] results)
    {
        if (resultContainer != null)
        {
            // Amagar el mode espectador quan mostrem els resultats finals netament
            if (spectatorContainer != null) spectatorContainer.style.display = DisplayStyle.None;

            // Amaguem el contingut mitjançant la classe d'opacity/translate per preparar l'animació
            resultContainer.AddToClassList("result-hidden");
            if (resultBox != null) resultBox.AddToClassList("result-box-hidden");

            resultContainer.style.display = DisplayStyle.Flex;

            SetupGeometricsPodium(results);

            StartCoroutine(AnimateResultScreen());
            Time.timeScale = 0f;
        }
    }

    private void SetupGeometricsPodium(MatchResultData[] results)
    {
        var lbl1 = resultContainer.Q<Label>("Lbl1");
        var pos1 = resultContainer.Q<VisualElement>("Pos1");
        var lbl2 = resultContainer.Q<Label>("Lbl2");
        var pos2 = resultContainer.Q<VisualElement>("Pos2");
        var lbl3 = resultContainer.Q<Label>("Lbl3");
        var pos3 = resultContainer.Q<VisualElement>("Pos3");

        if (lbl1 != null) { lbl1.text = results.Length > 0 ? results[0].name : "-"; pos1.style.display = results.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None; }
        if (lbl2 != null) { lbl2.text = results.Length > 1 ? results[1].name : "-"; pos2.style.display = results.Length > 1 ? DisplayStyle.Flex : DisplayStyle.None; }
        if (lbl3 != null) { lbl3.text = results.Length > 2 ? results[2].name : "-"; pos3.style.display = results.Length > 2 ? DisplayStyle.Flex : DisplayStyle.None; }

        if (othersList != null)
        {
            othersList.Clear();
            for (int i = 3; i < results.Length; i++)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("other-item");

                Label lblNom = new Label($"{i + 1}. {results[i].name}");
                lblNom.AddToClassList("other-text");

                Label lblStats = new Label($"{results[i].time}s | {results[i].score} Pts");
                lblStats.AddToClassList("other-text");

                row.Add(lblNom);
                row.Add(lblStats);
                
                othersList.Add(row);
            }
        }
    }

    private IEnumerator AnimateResultScreen()
    {
        // Donar un cert delay de frames per permetre a UI Toolkit recalcular l'arbre d'elements visuals després del display: Flex
        yield return null; 
        yield return null;
        
        // Quitar les classes dispara la transició d'aparició definida en el CSS
        resultContainer.RemoveFromClassList("result-hidden");
        if (resultBox != null) resultBox.RemoveFromClassList("result-box-hidden");
    }
}
