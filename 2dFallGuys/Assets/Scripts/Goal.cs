using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Meta del nivell: detecta quan el jugador (tag "Player") arriba a la línia d'arribada.
/// Quan hi arriba, mostra un missatge de "Nivell Completat!" en pantalla.
/// Adjunta aquest script a l'objecte de la meta (Trigger 2D activat).
/// </summary>
public class Goal : MonoBehaviour
{
    [Header("Interfície d'Usuari")]
    [Tooltip("Text de la UI que mostrarà el missatge de victòria. Si és null, es mostra per Debug.Log.")]
    public Text textNivellCompletat;

    [Tooltip("Missatge que es mostrarà en completar el nivell.")]
    public string missatgeVictoria = "🏆 Nivell Completat!";

    // Indica si el nivell ja ha estat completat per evitar repeticions
    private bool nivellCompletat = false;

    void Start()
    {
        // Assegurem que el missatge de victòria estigui amagat a l'inici
        if (textNivellCompletat != null)
        {
            textNivellCompletat.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// S'executa quan un collider entra al trigger de la meta.
    /// </summary>
    /// <param name="altre">El collider que ha entrat a la zona de meta.</param>
    private void OnTriggerEnter2D(Collider2D altre)
    {
        // Comprovem que sigui el jugador i que el nivell no estigui ja completat
        if (altre.CompareTag("Player") && !nivellCompletat)
        {
            NivellCompletat();
        }
    }

    /// <summary>
    /// Executa la lògica de fi de nivell: mostra el missatge i atura el temps.
    /// </summary>
    private void NivellCompletat()
    {
        // Marquem el nivell com a completat per no executar-ho dues vegades
        nivellCompletat = true;

        Debug.Log("[Goal] " + missatgeVictoria);

        // Mostrem el text de la UI si existeix
        if (textNivellCompletat != null)
        {
            textNivellCompletat.gameObject.SetActive(true);
            textNivellCompletat.text = missatgeVictoria;
        }

        // Opcionalment, aturem el temps del joc per celebrar
        Time.timeScale = 0f;
    }
}
