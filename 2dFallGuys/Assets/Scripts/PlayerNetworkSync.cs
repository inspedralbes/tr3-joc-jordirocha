using UnityEngine;
using System.Collections;

public class PlayerNetworkSync : MonoBehaviour
{
    [Header("Sincronització Multi-Jugador")]
    public string networkId;
    public bool isLocalPlayer = false;
    public string roomCode = "";
    
    // Configura quanta estona triga abans entre enviament i enviament de Transform.
    // Això permet jugar amb 20 Hz de freqüència llevant d'estrèss els microserveis.
    [SerializeField] private float taxaEnviamentPosicio = 0.05f;

    // Estat per predir la posició remota via interpolació.
    private Vector2 targetPosition;
    private float targetScaleX;

    // Estat local per reconstruir l'animació sense el PlayerController
    private Animator animator;
    private Vector2 posicioUltimFrame;

    void Start()
    {
        if (isLocalPlayer)
        {
            // És el personatge que nosaltres controlem: transmet el seu estat repetitivament
            StartCoroutine(EnviarPosicioConstantment());
            
            // Assignar la càmera al nostre personatge perquè ens segueixi
            CameraController camController = FindFirstObjectByType<CameraController>();
            if (camController != null)
            {
                camController.objectiuJugador = transform;
            }
        }
        else
        {
            // És un personatge remot: inicialitzem les variables d'arribada perquè d'entrada 
            // no hi hagi cap descarrilament per defecte.
            targetPosition = transform.position;
            targetScaleX = transform.localScale.x;
            posicioUltimFrame = transform.position;
            animator = GetComponent<Animator>();
            
            // Per evitar que el teclat accioni l'altre jugador o que les físiques intercedeixin amb els salts Lerp
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;
            
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.isKinematic = true;
            
            // Apaga les col·lisions d'aquest fantasma respecte el teu personatge 
            // Ell interactuarà amb l'entorn al seu propi PC, no necessitem que toqui les teves monedes!
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    void Update()
    {
        // Només volem aplicar el "Lerp" als rivals/companys. El nostre ja s'encarrega 
        // del seu propi control físic al document PlayerController.cs
        if (!isLocalPlayer)
        {
            // === ANIMACIONS ESTIMADES ===
            // Calculem quina és la velocitat de moviment matemàtica entre dos frames
            Vector2 velocitatMecanica = ((Vector2)transform.position - posicioUltimFrame) / Time.deltaTime;
            posicioUltimFrame = transform.position;

            if (animator != null)
            {
                animator.SetFloat("velocitatX", Mathf.Abs(velocitatMecanica.x));
                animator.SetFloat("velocitatY", velocitatMecanica.y);
                // Si la variació d'alçada és molt baixa, donem per fet que estem aguantats al terra
                animator.SetBool("tocaTerra", Mathf.Abs(velocitatMecanica.y) < 0.2f);
            }

            // === MOVIMENT (LERP) ===
            // Moviment suau progressiu utilitzant Interpolació a 15 de factor
            // per garantir que la corba esdevingui sedosa sense importar quin ping rebi el client
            transform.position = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);

            // Refrescar el desdoblament visual en funció de l'última adreça indicada pel client original
            Vector3 escalaAtual = transform.localScale;
            escalaAtual.x = targetScaleX;
            transform.localScale = escalaAtual;
        }
    }

    /// <summary>
    /// Coroutines que es triga repetitivament per interrogar les freqüències i reportar les posicions.
    /// Només s'executa si l'script està actiu (no heu mort) i és local.
    /// </summary>
    private IEnumerator EnviarPosicioConstantment()
    {
        while (true)
        {
            // Esbrinem si la xarxa està en disposició
            if (LobbyNetworkManager.Instance != null && !string.IsNullOrEmpty(roomCode))
            {
                WsMessage transformMsg = new WsMessage
                {
                    type = "PLAYER_TRANSFORM",
                    roomCode = this.roomCode,
                    id = this.networkId,
                    x = transform.position.x,
                    y = transform.position.y,
                    scaleX = transform.localScale.x
                };

                // Execució oberta pública
                LobbyNetworkManager.Instance.SendMessageToServer(transformMsg);
            }
            
            // Pausa cronològica controlada (20hz = 0.05s)
            yield return new WaitForSeconds(taxaEnviamentPosicio);
        }
    }

    /// <summary>
    /// Mètode que el Gestor crida explícitament en rebre dades des del WS per part d'algun altre peer de la sala.
    /// </summary>
    public void UpdateTargetPosition(float newX, float newY, float newScaleX)
    {
        targetPosition = new Vector2(newX, newY);
        targetScaleX = newScaleX;
    }
}
