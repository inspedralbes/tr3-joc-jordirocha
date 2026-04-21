using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generador de Nivells Avançat per a Generació Procedimental Pura.
/// Utilitza la Simulació de la Paràbola de Salt per garantir que cada
/// plataforma generada sigui matemàticament assolible pel jugador.
/// </summary>
public class AdvancedLevelGenerator : MonoBehaviour
{
    // =========================================================
    // SECCIÓ 1: VARIABLES DE FÍSICA DEL JUGADOR
    // Aquests valors han de coincidir exactament amb els de PlayerController.cs
    // =========================================================

    [Header("Física del Jugador (ha de coincidir amb PlayerController)")]

    [Tooltip("Força de salt del jugador. Valor: PlayerController.forcaSalt")]
    [SerializeField] private float forcaSalt = 12f;

    [Tooltip("Escala de gravetat del Rigidbody2D del jugador. Per defecte Unity = 1.")]
    [SerializeField] private float escalaGravetat = 1f;

    [Tooltip("Velocitat de moviment horitzontal del jugador. Valor: PlayerController.velocitat")]
    [SerializeField] private float velocitatMoviment = 8f;

    // =========================================================
    // SECCIÓ 2: PREFABS I CONFIGURACIÓ DE GENERACIÓ
    // =========================================================

    [Header("Prefabs Modulars de Plataforma")]

    [Tooltip("Prefab de l'extrem esquerre de la plataforma.")]
    [SerializeField] private GameObject prefabPlatEsquerra;

    [Tooltip("Prefab de les seccions centrals (repetides N vegades) de la plataforma.")]
    [SerializeField] private GameObject prefabPlatMig;

    [Tooltip("Prefab de l'extrem dret de la plataforma.")]
    [SerializeField] private GameObject prefabPlatDreta;

    [Tooltip("Prefab de les punxes (trampes) que s'instancien a SOBRE de les plataformes.")]
    [SerializeField] private GameObject prefabPunxes;

    [Header("Trampes a Sobre de les Plataformes")]

    [Tooltip("Probabilitat (0 = mai, 1 = sempre) que una plataforma tingui una trampa a sobre.")]
    [Range(0f, 1f)]
    [SerializeField] private float probabilitatTrampa = 0.3f;

    [Header("Elements Interactius")]
    [Tooltip("Prefab de la moneda per sumar punts.")]
    [SerializeField] private GameObject prefabMoneda;

    [Tooltip("Probabilitat que hi hagi una moneda (té prioritat sobre les trampes).")]
    [Range(0f, 1f)]
    [SerializeField] private float probabilitatMoneda = 0.4f;

    [Tooltip("Prefab del Checkpoint (es col·locarà a la meitat del nivell).")]
    [SerializeField] private GameObject prefabCheckpoint;

    [Tooltip("Prefab de la Meta (es col·locarà a l'última plataforma).")]
    [SerializeField] private GameObject prefabMeta;

    [Tooltip("Desplaçament en Y per col·locar els objectes (Meta, Checkpoint o Moneda) de manera que no quedin enterrats a terra.")]
    [SerializeField] private float offsetYObjectes = 1.5f;

    [Tooltip("Desplaçament en Y per col·locar la trampa just a la superfície de la plataforma. " +
             "Ajusta-ho perquè la base del prefab de punxes coincideixi amb la part superior de la plataforma.")]
    [SerializeField] private float offsetYTrampa = 0.5f;

    [Header("Configuració de Parts Centrals de Plataforma")]

    [Tooltip("Nombre mínim de parts centrals (prefabPlatMig) per plataforma.")]
    [SerializeField] private int partsMigMinim = 1;

    [Tooltip("Nombre màxim de parts centrals (prefabPlatMig) per plataforma.")]
    [SerializeField] private int partsMigMaxim = 4;

    [Tooltip("Amplada en unitats Unity de cadascun dels tres prefabs (esquerra, mig, dreta). " +
             "S'assumeix que tots tres fan la mateixa amplada.")]
    [SerializeField] private float ampladeaPeça = 1f;

    [Header("Paràmetres de Generació del Nivell")]

    [Tooltip("Nombre total de plataformes a generar.")]
    [SerializeField] private int nombreDePlataformes = 20;

    [Tooltip("Marge de seguretat (0.0 - 1.0) per fer els salts més còmodes. " +
             "0 = exactament al límit. 0.2 = 80% de la distància màxima.")]
    [Range(0f, 0.95f)]
    [SerializeField] private float margeDeSeguretat = 0.2f;

    [Tooltip("Diferència d'alçada màxima (en unitats Unity) entre plataformes.")]
    [SerializeField] private float deltaYMaxim = 3f;

    [Tooltip("Diferència d'alçada mínima (en unitats Unity) entre plataformes. " +
             "Pot ser negativa (baixar).")]
    [SerializeField] private float deltaYMinim = -2f;

    [Tooltip("Posició inicial de la primera plataforma.")]
    [SerializeField] private Vector2 posicioInicial = new Vector2(0f, 0f);

    [Header("Referència del Jugador")]
    [Tooltip("El Prefab del jugador que s'instanciarà a l'inici del nivell per a cada client.")]
    public GameObject prefabJugador;

    // =========================================================
    // SECCIÓ 2c: COMPTADOR PÚBLIC DE TRAMPES
    // Accessible des de scripts externs per mostrar estadístiques.
    // =========================================================

    /// <summary>
    /// Comptador del nombre total de trampes generades en el nivell actual.
    /// Es reinicia a cada crida de GenerarNivell().
    /// </summary>
    [HideInInspector] public int totalTrampesNivell = 0;
    // =========================================================
    // SECCIÓ 2b: ESTAT INTERN DE LA PLATAFORMA ACTUAL
    // ampladeaPlataformaActual es recalcula a cada instanciació
    // i s'usa en els càlculs de paràbola.
    // =========================================================

    // Amplada total de l'última plataforma instanciada.
    // = ampladeaPeça * (2 caps + nombrePartsMig)
    private float ampladeaPlataformaActual = 3f;

    // =========================================================
    // SECCIÓ 3: VARIABLES INTERNES (no serialitzades)
    // =========================================================

    // Física real de Unity: la gravetat és Physics2D.gravity.y * escalaGravetat
    private float gravetatReal;

    // Llista de totes les plataformes instanciades (per netejar o refrescar)
    private List<GameObject> plataformesInstanciades = new List<GameObject>();
    private List<GameObject> punxesInstanciades = new List<GameObject>();

    // =========================================================
    // SECCIÓ 4: MÈTODES DE UNITY
    // =========================================================

    void Start()
    {
        // Calculem la gravetat real que aplica Unity al Rigidbody2D del jugador.
        // Physics2D.gravity.y és negatiu per defecte (-9.81).
        gravetatReal = Physics2D.gravity.y * escalaGravetat;

        // Comprovem si venim del Lobby amb una seed designada pèl servidor
        if (LobbyNetworkManager.roomSeed != 0)
        {
            Debug.Log($"[AdvancedLevelGenerator] Inicialitzant el mapa sincronitzat amb la seed de la sala: {LobbyNetworkManager.roomSeed}");
            Random.InitState(LobbyNetworkManager.roomSeed);
        }
        else
        {
            Debug.Log("[AdvancedLevelGenerator] Cap seed detectada. Jugant mode offline amb seed aleatori.");
        }

        // Validació: els tres prefabs de plataforma són obligatoris.
        if (prefabPlatEsquerra == null || prefabPlatMig == null || prefabPlatDreta == null)
        {
            Debug.LogError("[AdvancedLevelGenerator] ERROR: Algun prefab modular de plataforma és buit! " +
                           "Assigna 'prefabPlatEsquerra', 'prefabPlatMig' i 'prefabPlatDreta' a l'Inspector.");
            return;
        }
        // Validació addicional del rang de parts centrals
        if (partsMigMinim < 0) partsMigMinim = 0;
        if (partsMigMaxim < partsMigMinim) partsMigMaxim = partsMigMinim;

        if (prefabPunxes == null)
        {
            Debug.LogWarning("[AdvancedLevelGenerator] AVÍS: El camp 'prefabPunxes' és buit. " +
                             "No es generaran trampes a sobre de les plataformes.");
        }

        GenerarNivell();
    }

    // =========================================================
    // SECCIÓ 5: LÒGICA PRINCIPAL DE GENERACIÓ
    // =========================================================

    /// <summary>
    /// Bucle principal que genera totes les plataformes del nivell,
    /// validant matemàticament la trajectòria de cada salt.
    /// </summary>
    private void GenerarNivell()
    {
        // Reiniciem el comptador de trampes per a aquesta nova generació
        totalTrampesNivell = 0;

        // Netegem qualsevol generació anterior
        NetejarNivell();

        // Posició actual (centre) de la darrera plataforma instanciada
        Vector2 posicioActual = posicioInicial;

        // Generem la primera plataforma a la posició d'inici.
        // InstanciarPlataforma actualitza ampladeaPlataformaActual internament.
        // La primera plataforma no té trampa (zona segura d'inici per al jugador).
        InstanciarPlataforma(posicioActual);

        for (int i = 1; i < nombreDePlataformes; i++)
        {
            // --- PAS 1: CALCUL DE LA DISTÀNCIA MÀXIMA HORITZONTAL ---
            // Usem cinemàtica del moviment projectil per trobar la X màxima assolible.
            float yOrigen = posicioActual.y;

            // Escollim aleatòriament una diferència d'alçada per a la nova plataforma
            float deltaY = Random.Range(deltaYMinim, deltaYMaxim);
            float yDesti = yOrigen + deltaY;

            // --- PAS 2: SIMULACIÓ DE LA PARÀBOLA DE SALT ---
            // Equacions cinemàtiques:
            //   y(t) = yOrigen + forcaSalt * t + (0.5 * gravetatReal * t²)
            //   x(t) = velocitatMoviment * t
            //
            // Resolem la quadràtica per a 't': 0.5*g*t² + forcaSalt*t + (yOrigen - yDesti) = 0

            float distanciaXMaxima = CalcularDistanciaHoritzontalMaxima(yOrigen, yDesti);

            // Si la distància és zero (salt impossible per alçada excessiva), nivelar les Y.
            if (distanciaXMaxima <= 0f)
            {
                deltaY = 0f;
                yDesti = yOrigen;
                distanciaXMaxima = CalcularDistanciaHoritzontalMaxima(yOrigen, yDesti);
            }

            // --- PAS 3: APLICAR EL MARGE DE SEGURETAT ---
            // Usem un % de la distància màxima per garantir comoditat en el salt.
            float distanciaSegura = distanciaXMaxima * (1f - margeDeSeguretat);

            // La separació mínima és mig ample de la plataforma ACTUAL (la que acabem de posar).
            // Això garanteix que sempre hi ha un buit visible.
            float distanciaMinima = ampladeaPlataformaActual * 0.5f;
            float separacioHoritzontal = Random.Range(
                distanciaMinima,
                Mathf.Max(distanciaSegura, distanciaMinima + 0.1f)
            );

            // --- PAS 4: CALCULAR POSICIO NOVA ---
            // La nova X = centre actual + mig ample actual + separació + mig ample futura.
            // Com que no coneixem l'ample futur fins instanciar, precomputem quantes parts
            // tindrà la nova plataforma per saber el seu mig ample.
            int partsMigNova = Random.Range(partsMigMinim, partsMigMaxim + 1);
            float ampladeaNova = ampladeaPeça * (2 + partsMigNova); // esquerra + N·mig + dreta

            // La posició X de la nova plataforma és el seu CENTRE:
            //   centre_nou = extrem_dret_actual + separació + mig_ample_nou
            float extrem_dret_actual = posicioActual.x + (ampladeaPlataformaActual * 0.5f);
            float novaX = extrem_dret_actual + separacioHoritzontal + (ampladeaNova * 0.5f);
            float novaY = yDesti;

            Vector2 novaPosicio = new Vector2(novaX, novaY);

            // --- PAS 5: DEPURACIÓ EN EDITOR (paràbola al Scene View) ---
            // El salt parteix del centre de la plataforma actual a la vora esquerra de la nova.
            DibuixarParabola(posicioActual, novaPosicio);

            // --- PAS 6: INSTANCIAR LA NOVA PLATAFORMA MODULAR ---
            // Passem el nombre de parts ja escollit per no tornar-lo a aleatoritzar.
            InstanciarPlataforma(novaPosicio, partsMigNova);

            // --- PAS 7: ELEMENTS A SOBRE DE LA NOVA PLATAFORMA ---
            // Posició per a Meta, Checkpoint i Moneda usant el nou offsetYObjectes (compensa el pivot)
            Vector3 posicioObjectes = new Vector3(
                novaPosicio.x,
                novaPosicio.y + offsetYObjectes,
                0f
            );

            // Posició per a les trampes usant el seu propi offset al ras de terra
            Vector3 posicioTrampa = new Vector3(
                novaPosicio.x,
                novaPosicio.y + offsetYTrampa, 
                0f
            );

            if (i == nombreDePlataformes / 2)
            {
                // Instanciar Checkpoint exactament a la meitat
                if (prefabCheckpoint != null)
                {
                    GameObject chk = Instantiate(prefabCheckpoint, posicioObjectes, Quaternion.identity);
                    chk.transform.SetParent(this.transform);
                    chk.name = $"Checkpoint_{i}";
                }
            }
            else if (i == nombreDePlataformes - 1)
            {
                // Instanciar Meta a l'última plataforma
                if (prefabMeta != null)
                {
                    GameObject meta = Instantiate(prefabMeta, posicioObjectes, Quaternion.identity);
                    meta.transform.SetParent(this.transform);
                    meta.name = $"Meta_{i}";
                }
            }
            else
            {
                // A la resta de plataformes, decidim si posem una moneda
                bool hiHaMoneda = false;
                if (prefabMoneda != null && Random.value < probabilitatMoneda)
                {
                    GameObject moneda = Instantiate(prefabMoneda, posicioObjectes, Quaternion.identity);
                    moneda.transform.SetParent(this.transform);
                    moneda.name = $"Moneda_{i}";
                    hiHaMoneda = true;
                }

                // O una trampa, si no hi hem posat moneda (per no superposar-les)
                if (!hiHaMoneda && prefabPunxes != null && Random.value < probabilitatTrampa)
                {
                    GameObject novaTrampa = Instantiate(prefabPunxes, posicioTrampa, Quaternion.identity);
                    novaTrampa.transform.SetParent(this.transform);
                    novaTrampa.name = $"Trampa_Plataforma_{plataformesInstanciades.Count}";
                    punxesInstanciades.Add(novaTrampa);
                    
                    // Incrementem el comptador públic de trampes
                    totalTrampesNivell++;
                }
            }

            // Actualitzem la posició actual per al següent cicle del bucle
            posicioActual = novaPosicio;

            Debug.Log($"[AdvancedLevelGenerator] Plataforma {i + 1}/{nombreDePlataformes} | " +
                      $"Centre: {novaPosicio} | Ample: {ampladeaPlataformaActual:F2}u | " +
                      $"PartsMig: {partsMigNova} | " +
                      $"DistMax: {distanciaXMaxima:F2}u | Separació: {separacioHoritzontal:F2}u");
        }

        Debug.Log($"[AdvancedLevelGenerator] Nivell generat! " +
                  $"{nombreDePlataformes} plataformes | " +
                  $"{totalTrampesNivell} trampes instanciades sobre les plataformes.");
        // --- PAS 8: INSTANCIAR ELS JUGADORS ---
        Vector2 posJugadorInicial = new Vector2(posicioInicial.x, posicioInicial.y + 2f);
        
        // Actualitzem el lloc de reaparició per defecte a la zona de l'inici
        DeathZone.posicioReaparicioActiva = posJugadorInicial;

        if (prefabJugador != null)
        {
            if (LobbyNetworkManager.roomPlayers != null && LobbyNetworkManager.roomPlayers.Length > 0)
            {
                // Mode Multijugador: Instanciem un jugador per cada registre en la llista rebuda del servidor
                for (int p = 0; p < LobbyNetworkManager.roomPlayers.Length; p++)
                {
                    // Desplacem una mica cada jugador per a evitar superposicions
                    Vector2 posicioSpawn = new Vector2(posJugadorInicial.x + (p * 0.5f), posJugadorInicial.y);
                    GameObject nouJugador = Instantiate(prefabJugador, posicioSpawn, Quaternion.identity);
                    nouJugador.name = $"Jugador_{LobbyNetworkManager.roomPlayers[p].name}";

                    // [AFEGIT PELA SINCRONITZACIÓ DE POSICIO]
                    PlayerNetworkSync syncScript = nouJugador.GetComponent<PlayerNetworkSync>();
                    if (syncScript == null) syncScript = nouJugador.AddComponent<PlayerNetworkSync>();
                    
                    syncScript.networkId = LobbyNetworkManager.roomPlayers[p].id;
                    syncScript.isLocalPlayer = (syncScript.networkId == LobbyNetworkManager.localPlayerId);
                    
                    if (LobbyNetworkManager.Instance != null)
                    {
                        syncScript.roomCode = LobbyNetworkManager.Instance.CurrentRoomCode;
                        LobbyNetworkManager.Instance.RegisterPlayer(syncScript.networkId, syncScript);
                    }
                }
            }
            else
            {
                // Mode Offline / Editor (si premem 'Play' directament en l'escena)
                GameObject nouJugador = Instantiate(prefabJugador, posJugadorInicial, Quaternion.identity);
                nouJugador.name = "Jugador_Local_Offline";
            }
        }
        else
        {
            Debug.LogError("[AdvancedLevelGenerator] Manca el prefabJugador per instanciar els participants. Assigna'l a l'Inspector!");
        }
        
    }

    // =========================================================
    // SECCIÓ 6: CÀLCULS DE FÍSICA (CINEMÀTICA I TRIGONOMETRIA)
    // =========================================================

    /// <summary>
    /// Calcula la distància horitzontal màxima que el jugador pot recórrer
    /// en un salt des de 'yOrigen' fins aterrar a 'yDesti'.
    ///
    /// Resol la equació cinemàtica quadràtica per a 't':
    ///   0.5 * g * t² + Vy₀ * t + (yOrigen - yDesti) = 0
    ///
    /// On:
    ///   g  = gravetatReal (valor negatiu)
    ///   Vy₀ = forcaSalt (velocitat vertical inicial)
    ///   yOrigen - yDesti = desplaçament vertical necessari
    ///
    /// Un cop trobat 't' (el temps de vol), la distància horitzontal màxima és:
    ///   X_max = velocitatMoviment * t
    /// </summary>
    /// <param name="yOrigen">Alçada Y de la plataforma de partida.</param>
    /// <param name="yDesti">Alçada Y de la plataforma de destinació.</param>
    /// <returns>Distància horitzontal màxima en unitats Unity, o 0 si és impossible.</returns>
    private float CalcularDistanciaHoritzontalMaxima(float yOrigen, float yDesti)
    {
        // Coeficients de la quadràtica: a*t² + b*t + c = 0
        float a = 0.5f * gravetatReal;   // Sempre negatiu (gravetat cap avall)
        float b = forcaSalt;              // Velocitat vertical inicial (positiva = cap amunt)
        float c = yOrigen - yDesti;       // Si yDesti > yOrigen, c és negatiu

        // Calculem el discriminant: b² - 4*a*c
        float discriminant = (b * b) - (4f * a * c);

        // Si el discriminant és negatiu, no hi ha solució real:
        // el jugador no pot arribar a aquella alçada ni saltant.
        if (discriminant < 0f)
        {
            Debug.LogWarning($"[AdvancedLevelGenerator] Salt impossible! " +
                             $"Discriminant negatiu ({discriminant:F3}) per a deltaY = {yDesti - yOrigen:F2}u. " +
                             $"Reduïnt distància...");
            return 0f;
        }

        // Resolem les dues solucions de la quadràtica
        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2f * a);
        float t2 = (-b - sqrtDiscriminant) / (2f * a);

        // Escollim el temps de vol positiu i màxim (la paràbola completa cap amunt i cap avall)
        float tempsDeVol = Mathf.Max(t1, t2);

        // Si el temps és negatiu o zero, no és vàlid
        if (tempsDeVol <= 0f)
        {
            Debug.LogWarning($"[AdvancedLevelGenerator] Temps de vol invàlid ({tempsDeVol:F3}s). " +
                             $"Comproveu els valors de física.");
            return 0f;
        }

        // Distància horitzontal màxima: X = velocitat * temps
        float distanciaMaxima = velocitatMoviment * tempsDeVol;
        return distanciaMaxima;
    }

    /// <summary>
    /// Calcula l'alçada màxima (apex) de la paràbola de salt.
    /// Útil per a depuració i visualització.
    ///
    /// L'apex es produeix quan la velocitat vertical és 0:
    ///   t_apex = forcaSalt / |gravetatReal|
    ///   y_apex = yOrigen + forcaSalt * t_apex + 0.5 * gravetatReal * t_apex²
    /// </summary>
    /// <param name="yOrigen">Alçada Y de partida.</param>
    /// <returns>Alçada màxima del salt.</returns>
    private float CalcularAlçadaMaximaSalt(float yOrigen)
    {
        // Temps fins al punt més alt (velocitat vertical = 0)
        float tempsApex = forcaSalt / Mathf.Abs(gravetatReal);

        // Alçada màxima usant cinemàtica
        float yApex = yOrigen + (forcaSalt * tempsApex) + (0.5f * gravetatReal * tempsApex * tempsApex);
        return yApex;
    }

    // =========================================================
    // SECCIÓ 7: INSTANCIACIÓ D'OBJECTES
    // =========================================================

    /// <summary>
    /// Construeix una plataforma modular a la posició indicada.
    ///
    /// Crea un GameObject buit contenidor centrat a 'posicio' i,
    /// a dins, instancia:
    ///   1. prefabPlatEsquerra  → a l'extrem esquerre
    ///   2. N × prefabPlatMig   → seccions centrals, concatenades
    ///   3. prefabPlatDreta     → a l'extrem dret
    ///
    /// Totes les peces es col·loquen sense buits, partint de
    /// la X esquerra del contenidor (centre - mig_ample_total).
    ///
    /// Actualitza la variable interna 'ampladeaPlataformaActual'
    /// perquè els càlculs de paràbola del següent cicle siguin correctes.
    /// </summary>
    /// <param name="posicio">Posició central del conjunt de plataforma.</param>
    /// <param name="nombrePartsMig">Nombre de seccions centrals a instanciar.
    /// Si és -1 (per defecte), s'escull aleatòriament entre els límits configurats.</param>
    private void InstanciarPlataforma(Vector2 posicio, int nombrePartsMig = -1)
    {
        // Si no s'ha especificat, escollim aleatòriament les parts centrals
        if (nombrePartsMig < 0)
            nombrePartsMig = Random.Range(partsMigMinim, partsMigMaxim + 1);

        // Calculem l'amplada total: cap esquerre + N parts centrals + cap dret
        // Cada peça fa 'ampladeaPeça' unitats d'amplada.
        int totalPeces = 2 + nombrePartsMig; // 2 caps (E+D) + N migs
        float ampladeaTotal = ampladeaPeça * totalPeces;

        // Actualitzem la variable de classe perquè el bucle principal la pugui usar
        ampladeaPlataformaActual = ampladeaTotal;

        // --- CONTENIDOR: GameObject buit que agrupa totes les peces ---
        GameObject contenidor = new GameObject($"Plataforma_{plataformesInstanciades.Count + 1}");
        contenidor.transform.position = new Vector3(posicio.x, posicio.y, 0f);
        contenidor.transform.SetParent(this.transform);
        plataformesInstanciades.Add(contenidor);

        // X de la vora esquerra del conjunt (el cursor avança cap a la dreta)
        float xCursor = posicio.x - (ampladeaTotal * 0.5f);

        // --- PEÇA ESQUERRA ---
        // La posem centrada a la seva cel·la: xCursor + mig_ample_peça
        InstanciarPeça(
            prefabPlatEsquerra,
            new Vector2(xCursor + ampladeaPeça * 0.5f, posicio.y),
            contenidor.transform,
            "Cap_Esquerra"
        );
        xCursor += ampladeaPeça; // Avancem el cursor una peça cap a la dreta

        // --- PECES CENTRALS (N vegades) ---
        for (int m = 0; m < nombrePartsMig; m++)
        {
            InstanciarPeça(
                prefabPlatMig,
                new Vector2(xCursor + ampladeaPeça * 0.5f, posicio.y),
                contenidor.transform,
                $"Mig_{m + 1}"
            );
            xCursor += ampladeaPeça;
        }

        // --- PEÇA DRETA ---
        InstanciarPeça(
            prefabPlatDreta,
            new Vector2(xCursor + ampladeaPeça * 0.5f, posicio.y),
            contenidor.transform,
            "Cap_Dret"
        );

        Debug.Log($"[AdvancedLevelGenerator] '{contenidor.name}' creada: " +
                  $"{totalPeces} peces | Ample total: {ampladeaTotal:F2}u | Centre: {posicio}");
    }

    /// <summary>
    /// Mètode auxiliar: instancia un prefab de peça i el col·loca com a fill del contenidor.
    /// </summary>
    /// <param name="prefab">Prefab a instanciar.</param>
    /// <param name="posicioMon">Posició en coordenades de món.</param>
    /// <param name="pare">Transform del contenidor pare.</param>
    /// <param name="nomPeça">Nom de la peça per a la jerarquia.</param>
    private void InstanciarPeça(GameObject prefab, Vector2 posicioMon, Transform pare, string nomPeça)
    {
        GameObject peça = Instantiate(
            prefab,
            new Vector3(posicioMon.x, posicioMon.y, 0f),
            Quaternion.identity
        );
        peça.transform.SetParent(pare);
        peça.name = nomPeça;
    }



    // =========================================================
    // SECCIÓ 8: UTILITATS I DEPURACIÓ
    // =========================================================

    /// <summary>
    /// Dibuixa la paràbola de salt al Scene View d'Unity per facilitar la depuració visual.
    /// Només visible quan l'editor DE Unity és obert i s'executa en mode Play.
    /// </summary>
    /// <param name="posicioOrigen">Posició de partida del salt.</param>
    /// <param name="posicioDesti">Posició de destinació del salt.</param>
    private void DibuixarParabola(Vector2 posicioOrigen, Vector2 posicioDesti)
    {
        // Calculem el temps total de vol d'origen a destinació
        float a = 0.5f * gravetatReal;
        float b = forcaSalt;
        float c = posicioOrigen.y - posicioDesti.y;
        float discriminant = (b * b) - (4f * a * c);

        if (discriminant < 0f) return;

        float sqrtD = Mathf.Sqrt(discriminant);
        float tempsTotal = Mathf.Max((-b + sqrtD) / (2f * a), (-b - sqrtD) / (2f * a));

        if (tempsTotal <= 0f) return;

        // Dibuixem la corba com una sèrie de segments de línia
        int segmentsParabola = 30;
        Vector3 puntAnterior = new Vector3(posicioOrigen.x, posicioOrigen.y, 0f);

        for (int s = 1; s <= segmentsParabola; s++)
        {
            // Temps proporcional al segment
            float t = (s / (float)segmentsParabola) * tempsTotal;

            // Cinemàtica: X lineal, Y parabòlica
            float x = posicioOrigen.x + velocitatMoviment * t;
            float y = posicioOrigen.y + (forcaSalt * t) + (0.5f * gravetatReal * t * t);

            Vector3 puntActual = new Vector3(x, y, 0f);

            // Dibuixem el segment de la paràbola en verd
            Debug.DrawLine(puntAnterior, puntActual, Color.green, 5f);

            puntAnterior = puntActual;
        }

        // Marquem el punt de destinació amb una creu groga
        float marcador = 0.3f;
        Vector3 v3Desti = new Vector3(posicioDesti.x, posicioDesti.y, 0f);
        Debug.DrawLine(v3Desti + Vector3.left * marcador, v3Desti + Vector3.right * marcador, Color.yellow, 5f);
        Debug.DrawLine(v3Desti + Vector3.down * marcador, v3Desti + Vector3.up * marcador, Color.yellow, 5f);
    }

    /// <summary>
    /// Destrueix totes les plataformes i punxes generades anteriorment.
    /// Permet regenerar el nivell en temps d'execució.
    /// </summary>
    public void NetejarNivell()
    {
        foreach (GameObject obj in plataformesInstanciades)
        {
            if (obj != null) Destroy(obj);
        }
        foreach (GameObject obj in punxesInstanciades)
        {
            if (obj != null) Destroy(obj);
        }
        plataformesInstanciades.Clear();
        punxesInstanciades.Clear();
    }

    /// <summary>
    /// Permet regenerar el nivell des d'un script extern o des del'inspector (botó públic).
    /// </summary>
    [ContextMenu("Regenerar Nivell")]
    public void RegenerarNivell()
    {
        Debug.Log("[AdvancedLevelGenerator] Regenerant nivell...");
        gravetatReal = Physics2D.gravity.y * escalaGravetat;
        GenerarNivell();
    }

    // =========================================================
    // SECCIÓ 9: GIZMOS D'EDITOR (VISUALITZACIÓ A LA SCENE VIEW)
    // =========================================================

    void OnDrawGizmos()
    {
        // Visualitzem la posició inicial amb una esfera blava
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(posicioInicial.x, posicioInicial.y, 0f), 0.4f);

        // Visualitzem la línia de la superfície de trampes (posicioInicial.y + offsetYTrampa) en taronja
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawLine(
            new Vector3(posicioInicial.x - 5f, posicioInicial.y + offsetYTrampa, 0f),
            new Vector3(posicioInicial.x + 100f, posicioInicial.y + offsetYTrampa, 0f)
        );

        // Visualitzem l'ample mínim i màxim de plataforma possible a l'Inspector
        // (rang de colors: verd clar = mínima, verd fosc = màxima)
        float ampladeaMin = ampladeaPeça * (2 + partsMigMinim);
        float ampladeaMax = ampladeaPeça * (2 + partsMigMaxim);
        Gizmos.color = new Color(0.3f, 0.9f, 0.3f, 0.6f);
        Gizmos.DrawWireCube(
            new Vector3(posicioInicial.x, posicioInicial.y, 0f),
            new Vector3(ampladeaMin, 0.5f, 0f)
        );
        Gizmos.color = new Color(0.0f, 0.5f, 0.0f, 0.4f);
        Gizmos.DrawWireCube(
            new Vector3(posicioInicial.x, posicioInicial.y, 0f),
            new Vector3(ampladeaMax, 0.5f, 0f)
        );

        // Visualitzem l'apex del primer salt en magenta
        float gravetatEditor = Physics2D.gravity.y * escalaGravetat;
        if (Mathf.Abs(gravetatEditor) > 0.01f)
        {
            float tempsApex = forcaSalt / Mathf.Abs(gravetatEditor);
            float yApex = posicioInicial.y + (forcaSalt * tempsApex) + (0.5f * gravetatEditor * tempsApex * tempsApex);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(new Vector3(posicioInicial.x, yApex, 0f), 0.25f);
        }
    }
}
