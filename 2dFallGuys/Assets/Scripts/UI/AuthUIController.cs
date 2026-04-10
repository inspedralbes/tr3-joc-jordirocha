using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador encarregat de gestionar els formularis de la interfície UI Toolkit
/// i connectar de forma asíncrona (via REST) amb el microservei natiu.
/// </summary>
public class AuthUIController : MonoBehaviour
{
    private UIDocument uiDocument;

    // Panells
    private VisualElement loginPanel;
    private VisualElement registerPanel;

    // Camps de Login
    private TextField loginUsernameField;
    private TextField loginPasswordField;
    private Label loginErrorLabel;

    // Camps de Registre
    private TextField regUsernameField;
    private TextField regEmailField;
    private TextField regPasswordField;
    private Label registerErrorLabel;

    // Adreça del Microservei Backend actiu via Docker Compose
    private readonly string baseUrl = "http://localhost:3000/api/users";

    [Header("Configuració")]
    [Tooltip("El nom de l'escena a la que vols que vagi un cop es logegi/registri. Ex: 'SampleScene'")]
    public string targetGameScene = "SampleScene";

    [System.Serializable]
    private class AuthRequest
    {
        public string username;
        public string password;
        public string email;
    }

    [System.Serializable]
    private class AuthResponse
    {
        public bool success;
        public string message;
        // Es podria incloure el format Token JWT per autoritzacions de sessions subsegüents...
    }

    private void Start()
    {
        Debug.Log("🚀 [AuthUIController] INICIANT... L'script s'ha processat correctament a Unity!");
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) {
            Debug.LogError("❌ FATAL: Aquest GameObject NO TÉ un UIDocument adjunt. Estàs segur que està al lloc correcte?");
            return;
        }

        var rootElement = uiDocument.rootVisualElement;

        // Recuperar instàncies dels panells per referències de noms
        loginPanel = rootElement.Q<VisualElement>("LoginPanel");
        registerPanel = rootElement.Q<VisualElement>("RegisterPanel");

        // Elements de Login
        loginUsernameField = rootElement.Q<TextField>("LoginUsername");
        loginPasswordField = rootElement.Q<TextField>("LoginPassword");
        loginErrorLabel = rootElement.Q<Label>("LoginError");

        rootElement.Q<Button>("BtnLogin").clicked += OnLoginClicked;
        rootElement.Q<Button>("BtnGoRegister").clicked += ShowRegisterPanel;

        // Elements de Registre
        regUsernameField = rootElement.Q<TextField>("RegUsername");
        regEmailField = rootElement.Q<TextField>("RegEmail");
        regPasswordField = rootElement.Q<TextField>("RegPassword");
        registerErrorLabel = rootElement.Q<Label>("RegisterError");

        rootElement.Q<Button>("BtnRegister").clicked += OnRegisterClicked;
        rootElement.Q<Button>("BtnGoLogin").clicked += ShowLoginPanel;
    }

    // Gestiona l'alteració d'estats de la classe ".hidden" de CSS
    private void ShowLoginPanel()
    {
        loginPanel.RemoveFromClassList("hidden");
        registerPanel.AddToClassList("hidden");
        loginErrorLabel.style.display = DisplayStyle.None;
    }

    private void ShowRegisterPanel()
    {
        registerPanel.RemoveFromClassList("hidden");
        loginPanel.AddToClassList("hidden");
        registerErrorLabel.style.display = DisplayStyle.None;
    }

    private void OnLoginClicked()
    {
        Debug.Log("🔘 BOTO CLICAT: Has enviat intent d'accés (Login)");
        string user = loginUsernameField.value;
        string pass = loginPasswordField.value;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowError(loginErrorLabel, "Siusplau, omple tots els camps.");
            return;
        }

        // Serialització utilitzant la classe JsonUtility pròpia de Unity
        string jsonPayload = JsonUtility.ToJson(new AuthRequest { username = user, password = pass });
        StartCoroutine(SendAuthRequest(baseUrl + "/login", jsonPayload, loginErrorLabel));
    }

    private void OnRegisterClicked()
    {
        Debug.Log("🔘 BOTO CLICAT: Has enviat intent de Registre!");
        string user = regUsernameField.value;
        string email = regEmailField.value;
        string pass = regPasswordField.value;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            ShowError(registerErrorLabel, "Siusplau, tots els camps són obligatoris.");
            return;
        }

        string jsonPayload = JsonUtility.ToJson(new AuthRequest { username = user, email = email, password = pass });
        StartCoroutine(SendAuthRequest(baseUrl + "/register", jsonPayload, registerErrorLabel));
    }

    private void ShowError(Label errorLabel, string msg)
    {
        errorLabel.text = msg;
        errorLabel.style.display = DisplayStyle.Flex;
    }

    private IEnumerator SendAuthRequest(string url, string jsonPayload, Label errorLabel)
    {
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                ShowError(errorLabel, "No s'ha pogut connectar amb el servidor. Està encès Docker?");
            }
            else
            {
                try
                {
                    AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                    if (www.responseCode == 200 || www.responseCode == 201)
                    {
                        Debug.Log("✔ Èxit: " + authResponse.message);
                        ShowError(errorLabel, "Accés autoritzat! (veure consola)");
                        errorLabel.style.color = new StyleColor(Color.green);
                        
                        // Carreguem l'escena principal programada per la variable targetGameScene
                        SceneManager.LoadScene(targetGameScene);
                    }
                    else
                    {
                        ShowError(errorLabel, authResponse.message ?? "Error desconegut retornat per l'API");
                        errorLabel.style.color = new StyleColor(new Color(1f, 0.4f, 0.4f));
                    }
                }
                catch
                {
                     ShowError(errorLabel, "Resposta malformada de la base de dades.");
                }
            }
        }
    }
}
