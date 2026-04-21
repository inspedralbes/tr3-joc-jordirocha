using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// </summary>
public class SpectatorManager : MonoBehaviour
{
    public static SpectatorManager Instance { get; private set; }

    private bool isSpectatorMode = false;
    private CameraController mainCamera;
    
    // Variables per a la navegació de l'Espectador
    private int currentTargetIndex = 0;
    private List<string> finishedPlayers = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        mainCamera = FindFirstObjectByType<CameraController>();
        LobbyNetworkManager.OnPlayerFinishedReceived += HandlePlayerFinished;
    }

    private void OnDestroy()
    {
        LobbyNetworkManager.OnPlayerFinishedReceived -= HandlePlayerFinished;
    }

    private void HandlePlayerFinished(string playerId, float time, int score)
    {
        if (!finishedPlayers.Contains(playerId))
        {
            finishedPlayers.Add(playerId);
            ValidateCurrentTarget();
        }
    }

    public void ActivateSpectatorMode()
    {
        if (isSpectatorMode) return;
        isSpectatorMode = true;

        // Aturar el moviment del jugador local
        PlayerController localPlayer = FindFirstObjectByType<PlayerController>();
        if (localPlayer != null)
        {
            localPlayer.enabled = false;
            Rigidbody2D rb = localPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }

        // Avisar a la Interfície
        if (HUDController.Instance != null)
        {
            HUDController.Instance.SetSpectatorHUD();
        }

        // Establir el target immediatament en activar el mode
        ValidateCurrentTarget();
    }

    private List<PlayerNetworkSync> GetActivePlayers()
    {
        if (LobbyNetworkManager.Instance == null) return new List<PlayerNetworkSync>();
        
        return LobbyNetworkManager.Instance.networkedPlayers
            .Where(kvp => !finishedPlayers.Contains(kvp.Key) && kvp.Value != null)
            .Select(kvp => kvp.Value)
            .OrderByDescending(p => p.transform.position.x)
            .ToList();
    }

    private void ValidateCurrentTarget()
    {
        if (!isSpectatorMode) return;
        var activePlayers = GetActivePlayers();
        
        if (activePlayers.Count == 0) return;
        
        if (currentTargetIndex >= activePlayers.Count)
        {
            currentTargetIndex = 0;
        }
        
        UpdateCameraTarget(activePlayers);
    }

    public void NextTarget()
    {
        var activePlayers = GetActivePlayers();
        if (activePlayers.Count == 0) return;

        currentTargetIndex++;
        if (currentTargetIndex >= activePlayers.Count) currentTargetIndex = 0;
        
        UpdateCameraTarget(activePlayers);
    }

    public void PrevTarget()
    {
        var activePlayers = GetActivePlayers();
        if (activePlayers.Count == 0) return;

        currentTargetIndex--;
        if (currentTargetIndex < 0) currentTargetIndex = activePlayers.Count - 1;
        
        UpdateCameraTarget(activePlayers);
    }

    private void UpdateCameraTarget(List<PlayerNetworkSync> activePlayers)
    {
        if (mainCamera != null && activePlayers.Count > 0 && currentTargetIndex < activePlayers.Count)
        {
            mainCamera.objectiuJugador = activePlayers[currentTargetIndex].transform;
        }
    }
}
