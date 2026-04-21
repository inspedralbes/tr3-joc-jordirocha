using UnityEngine;
using System;

/// <summary>
/// Gestiona la puntuació i el temps transcorregut durant la partida.
/// També s'encarrega d'avisar al servidor quan l'usuari local arriba a la meta.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Estat de Partida")]
    public float tempsTranscorregut = 0f;
    public int puntsTotals = 0;
    
    public bool haAcabatLocalment = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!haAcabatLocalment)
        {
            tempsTranscorregut += Time.deltaTime;
        }
    }

    /// <summary>
    /// S'utilitza quan recollim una moneda al joc.
    /// </summary>
    public void RecollirMoneda(int punts)
    {
        if (haAcabatLocalment) return; // Ja no compten monedes un cop arribes (opcional)
        puntsTotals += punts;
    }

    /// <summary>
    /// S'executa quan el nostre personatge toca el Trigger de Goal.
    /// </summary>
    public void OnPlayerReachedFinish()
    {
        if (haAcabatLocalment) return;
        haAcabatLocalment = true;

        Debug.Log($"[ScoreManager] Meta assollida! Temps: {tempsTranscorregut}s | Punts: {puntsTotals}");

        // Enviar Dades al Servidor
        if (LobbyNetworkManager.Instance != null && !string.IsNullOrEmpty(LobbyNetworkManager.Instance.CurrentRoomCode))
        {
            WsMessage msg = new WsMessage
            {
                type = "PLAYER_FINISHED",
                roomCode = LobbyNetworkManager.Instance.CurrentRoomCode,
                id = LobbyNetworkManager.localPlayerId,
                time = (float)Math.Round(tempsTranscorregut, 2),
                score = puntsTotals
            };

            LobbyNetworkManager.Instance.SendMessageToServer(msg);
        }

        // Activació de l'Espectador
        if (SpectatorManager.Instance != null)
        {
            SpectatorManager.Instance.ActivateSpectatorMode();
        }
    }
}
