using System;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayFabLoginController : MonoBehaviour
{
    [Header("Configuración de PlayFab")]
    [Tooltip("ID del título creado en PlayFab Game Manager. También puede configurarse en PlayFabSharedSettings.")]
    [SerializeField] private string titleId;

    [Header("Paneles")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject createAccountPanel;

    [Header("Iniciar sesión")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text loginStatusText;

    [Header("Crear cuenta")]
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private TMP_Text registerStatusText;

    private bool requestInProgress;

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(titleId))
        {
            PlayFabSettings.TitleId = titleId.Trim();
        }

        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        createAccountPanel.SetActive(false);
        SetStatus(loginStatusText, string.Empty, false);
    }

    public void ShowCreateAccountPanel()
    {
        loginPanel.SetActive(false);
        createAccountPanel.SetActive(true);
        SetStatus(registerStatusText, string.Empty, false);
    }

    public async void Login()
    {
        if (requestInProgress)
        {
            return;
        }

        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (!ValidatePlayFabConfiguration(loginStatusText) ||
            !ValidateCredentials(email, password, loginStatusText))
        {
            return;
        }

        SetBusy(true);
        SetStatus(loginStatusText, "INICIANDO SESIÓN...", false);

        try
        {
            LoginResult result = await LoginWithEmailAsync(email, password);
            string playFabId = string.IsNullOrEmpty(result.PlayFabId) ? "sin ID" : result.PlayFabId;
            Debug.Log($"LOGIN EXITOSO. PlayFabId: {playFabId}");
            SetStatus(loginStatusText, "SESIÓN INICIADA CORRECTAMENTE", false);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(loginStatusText, $"ERROR: {exception.Message}", true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    public async void CreateAccount()
    {
        if (requestInProgress)
        {
            return;
        }

        string email = registerEmailInput.text.Trim();
        string username = registerUsernameInput.text.Trim();
        string password = registerPasswordInput.text;

        if (!ValidatePlayFabConfiguration(registerStatusText) ||
            !ValidateRegistration(email, username, password))
        {
            return;
        }

        SetBusy(true);
        SetStatus(registerStatusText, "CREANDO CUENTA...", false);

        try
        {
            RegisterPlayFabUserResult result = await RegisterUserAsync(email, username, password);
            string playFabId = string.IsNullOrEmpty(result.PlayFabId) ? "sin ID" : result.PlayFabId;
            Debug.Log($"CUENTA CREADA EXITOSAMENTE. PlayFabId: {playFabId}");
            SetStatus(registerStatusText, "CUENTA CREADA EXITOSAMENTE", false);

            loginEmailInput.text = email;
            loginPasswordInput.text = string.Empty;
            await Task.Delay(700);
            ShowLoginPanel();
            SetStatus(loginStatusText, "CUENTA CREADA. AHORA INICIA SESIÓN", false);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(registerStatusText, $"ERROR: {exception.Message}", true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private Task<LoginResult> LoginWithEmailAsync(string email, string password)
    {
        var completion = new TaskCompletionSource<LoginResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(
            request,
            result => completion.TrySetResult(result),
            error => completion.TrySetException(new Exception(error.GenerateErrorReport())));

        return completion.Task;
    }

    private Task<RegisterPlayFabUserResult> RegisterUserAsync(string email, string username, string password)
    {
        var completion = new TaskCompletionSource<RegisterPlayFabUserResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            result => completion.TrySetResult(result),
            error => completion.TrySetException(new Exception(error.GenerateErrorReport())));

        return completion.Task;
    }

    private bool ValidatePlayFabConfiguration(TMP_Text statusText)
    {
        if (!string.IsNullOrWhiteSpace(PlayFabSettings.TitleId))
        {
            return true;
        }

        const string message = "CONFIGURA EL TITLE ID DE PLAYFAB EN EL INSPECTOR";
        Debug.LogError(message);
        SetStatus(statusText, message, true);
        return false;
    }

    private bool ValidateCredentials(string email, string password, TMP_Text statusText)
    {
        if (!IsValidEmail(email))
        {
            SetStatus(statusText, "ESCRIBE UN CORREO VÁLIDO", true);
            return false;
        }

        if (password.Length < 6)
        {
            SetStatus(statusText, "LA CONTRASEÑA DEBE TENER AL MENOS 6 CARACTERES", true);
            return false;
        }

        return true;
    }

    private bool ValidateRegistration(string email, string username, string password)
    {
        if (!IsValidEmail(email))
        {
            SetStatus(registerStatusText, "ESCRIBE UN CORREO VÁLIDO", true);
            return false;
        }

        if (username.Length < 3 || username.Length > 20)
        {
            SetStatus(registerStatusText, "EL USUARIO DEBE TENER ENTRE 3 Y 20 CARACTERES", true);
            return false;
        }

        if (password.Length < 6)
        {
            SetStatus(registerStatusText, "LA CONTRASEÑA DEBE TENER AL MENOS 6 CARACTERES", true);
            return false;
        }

        return true;
    }

    private void SetBusy(bool busy)
    {
        requestInProgress = busy;
        loginButton.interactable = !busy;
        registerButton.interactable = !busy;
    }

    private static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".");
    }

    private static void SetStatus(TMP_Text label, string message, bool isError)
    {
        label.text = message;
        label.color = isError
            ? new Color(1f, 0.38f, 0.32f)
            : new Color(0.25f, 0.95f, 0.78f);
        label.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }
}
