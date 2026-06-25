using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField mail;
    public TMP_InputField contrasena;
    public TMP_InputField nombre;
    public TMP_InputField mailInicioSesion;
    public TMP_InputField contrasenaInicioSesion;

    [Header("Feedback")]
    public TMP_Text mensajeFeedback;
    public TMP_Text mensajeFeedbackLogin;

    [Header("Botones")]
    public Button btnCrearCuenta;
    public Button btnIniciarSesion;
    public Button btnToggleContrasena;       // Ojito — campo contraseña (Crear Cuenta)
    public Button btnToggleContrasenaLogin;  // Ojito — campo contraseña (Login)

    [Header("Sprites Ojito")]
    public Sprite spriteOjoAbierto;   // Imagen cuando la contraseña es visible
    public Sprite spriteOjoCerrado;   // Imagen cuando la contraseña está oculta

    [Header("Referencia al DAO")]
    public DAOAdmin daoAdmin;

    // Estado interno de visibilidad
    private bool contrasenaVisible = false;
    private bool contrasenaLoginVisible = false;

    // ─────────────────────────────────────────────
    //  INICIALIZACIÓN
    // ─────────────────────────────────────────────
    private void Awake()
    {
        if (btnToggleContrasena != null)
            btnToggleContrasena.onClick.AddListener(ToggleContrasenaCrearCuenta);

        if (btnToggleContrasenaLogin != null)
            btnToggleContrasenaLogin.onClick.AddListener(ToggleContrasenaLogin);

        // Inicializar ambos campos como Password (****) con el ojo cerrado por defecto
        InicializarCampoContrasena(contrasena, btnToggleContrasena);
        InicializarCampoContrasena(contrasenaInicioSesion, btnToggleContrasenaLogin);
    }

    private void InicializarCampoContrasena(TMP_InputField campo, Button btnOjo)
    {
        if (campo != null)
        {
            campo.contentType = TMP_InputField.ContentType.Password;
            campo.ForceLabelUpdate();
        }
        ActualizarIconoOjo(btnOjo, false); // ojo cerrado = contraseña oculta
    }

    // ─────────────────────────────────────────────
    //  TOGGLE CONTRASEÑA — Crear Cuenta
    // ─────────────────────────────────────────────
    public void ToggleContrasenaCrearCuenta()
    {
        contrasenaVisible = !contrasenaVisible;
        contrasena.contentType = contrasenaVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        contrasena.ForceLabelUpdate();
        ActualizarIconoOjo(btnToggleContrasena, contrasenaVisible);
    }

    // ─────────────────────────────────────────────
    //  TOGGLE CONTRASEÑA — Login
    // ─────────────────────────────────────────────
    public void ToggleContrasenaLogin()
    {
        contrasenaLoginVisible = !contrasenaLoginVisible;
        contrasenaInicioSesion.contentType = contrasenaLoginVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        contrasenaInicioSesion.ForceLabelUpdate();
        ActualizarIconoOjo(btnToggleContrasenaLogin, contrasenaLoginVisible);
    }

    // Cambia el sprite del botón según el estado
    private void ActualizarIconoOjo(Button btn, bool visible)
    {
        if (btn == null) return;
        Image img = btn.GetComponentInChildren<Image>();
        if (img == null) return;
        img.sprite = visible ? spriteOjoAbierto : spriteOjoCerrado;
    }

    // ─────────────────────────────────────────────
    //  CREAR CUENTA — Usa DAOAdmin.RegistrarAdmin
    //  Asignar este método al botón de "Crear Cuenta" en Unity
    // ─────────────────────────────────────────────
    public void CrearCuenta()
    {
        if (string.IsNullOrWhiteSpace(mail.text))
        {
            MostrarMensaje("Por favor ingresá un email.", Color.red);
            return;
        }

        if (string.IsNullOrWhiteSpace(nombre.text))
        {
            MostrarMensaje("Por favor ingresá un nombre.", Color.red);
            return;
        }

        if (contrasena.text.Length < 6)
        {
            MostrarMensaje("La contraseña debe tener al menos 6 caracteres.", Color.red);
            return;
        }

        MostrarMensaje("Creando cuenta...", Color.yellow);
        SetBotonesInteractable(false);

        daoAdmin.RegistrarAdmin(mail.text.Trim(), contrasena.text, nombre.text.Trim(), (exito, error) =>
        {
            SetBotonesInteractable(true);

            if (exito)
            {
                MostrarMensaje($"¡Cuenta creada exitosamente, porfavor revise su mail!\nBienvenido, {nombre.text.Trim()}.", Color.green);
                LimpiarCampos();
            }
            else
            {
                MostrarMensaje("Error: " + error, Color.red);
            }
        });
    }

    // ─────────────────────────────────────────────
    //  INICIAR SESIÓN — Usa DAOAdmin.IniciarSesion
    //  Asignar este método al botón de "Iniciar Sesión" en Unity
    // ─────────────────────────────────────────────
    public void IniciarSesion()
    {
        if (string.IsNullOrWhiteSpace(mailInicioSesion.text))
        {
            MostrarMensajeLogin("Por favor ingresá un email.", Color.red);
            return;
        }

        if (string.IsNullOrWhiteSpace(contrasenaInicioSesion.text))
        {
            MostrarMensajeLogin("Por favor ingresá la contraseña.", Color.red);
            return;
        }

        MostrarMensajeLogin("Iniciando sesión...", Color.yellow);
        SetBotonesInteractable(false);

        daoAdmin.IniciarSesion(mailInicioSesion.text.Trim(), contrasenaInicioSesion.text, (exito, nombreAdmin, error) =>
        {
            SetBotonesInteractable(true);

            if (exito)
            {
                MostrarMensajeLogin($"¡Bienvenido, {nombreAdmin}!", Color.green);
                Debug.Log("[AdminUI] Sesión iniciada como: " + nombreAdmin);

                if (CambioScenas.Instance != null)
                {
                    CambioScenas.Instance.MenuPrincipal();
                }
                else
                {
                    Debug.LogError("No se encontró una instancia de CambioScenas en la escena.");
                }
            }
            else
            {
                MostrarMensajeLogin("Error: " + error, Color.red);
            }
        });
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────
    private void MostrarMensaje(string mensaje, Color color)
    {
        if (mensajeFeedback == null) return;
        mensajeFeedback.text = mensaje;
        mensajeFeedback.color = color;
    }

    private void MostrarMensajeLogin(string mensaje, Color color)
    {
        if (mensajeFeedbackLogin == null) return;
        mensajeFeedbackLogin.text = mensaje;
        mensajeFeedbackLogin.color = color;
    }

    private void SetBotonesInteractable(bool interactable)
    {
        if (btnCrearCuenta != null) btnCrearCuenta.interactable = interactable;
        if (btnIniciarSesion != null) btnIniciarSesion.interactable = interactable;
    }

    private void LimpiarCampos()
    {
        mail.text = "";
        contrasena.text = "";
        nombre.text = "";

        // Resetear visibilidad al limpiar
        contrasenaVisible = false;
        contrasena.contentType = TMP_InputField.ContentType.Password;
        contrasena.ForceLabelUpdate();
        ActualizarIconoOjo(btnToggleContrasena, false);
    }
}