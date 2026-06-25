using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAOAdmin : BaseDAO
{
    // URL de Supabase Auth (diferente al REST API)
    private const string SUPABASE_LOGIN_URL = SupabaseConfig.SUPABASE_LOGIN_URL;
    private const string SUPABASE_AUTH_URL = SupabaseConfig.AUTH_URL;

    // ─────────────────────────────────────────────
    //  Clase para deserializar JSON de Supabase
    // ─────────────────────────────────────────────

    [Serializable]
    private class AdminData
    {
        public int id_admin;
        public string email;
        public string nombre_admin;
    }

    // ═════════════════════════════════════════════
    //  INICIAR SESIÓN — Verificar credenciales del administrador
    //  Uso: daoAdmin.IniciarSesion("email@ejemplo.com", (exito, nombre, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Inicia sesión verificando credenciales en Supabase Auth y obteniendo datos de la tabla Admin.
    /// Callback: (bool exito, string nombreAdmin, string mensajeError)
    /// </summary>
    public void IniciarSesion(string email, string password, Action<bool, string, string> callback)
    {
        StartCoroutine(IniciarSesionCoroutine(email, password, callback));
    }

    private IEnumerator IniciarSesionCoroutine(string email, string password, Action<bool, string, string> callback)
    {
        // 1. Validaciones básicas
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            callback?.Invoke(false, null, "El email y la contraseña no pueden estar vacíos.");
            yield break;
        }

        email = email.Trim().ToLower();

        // 2. PASO 1: Autenticar con Supabase Auth
        string jsonBody = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest authRequest = new UnityWebRequest(SUPABASE_LOGIN_URL, "POST"))
        {
            authRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            authRequest.downloadHandler = new DownloadHandlerBuffer();
            authRequest.SetRequestHeader("apikey", API_KEY);
            authRequest.SetRequestHeader("Content-Type", "application/json");

            yield return authRequest.SendWebRequest();

            if (authRequest.result != UnityWebRequest.Result.Success)
            {
                string responseText = authRequest.downloadHandler?.text ?? "";
                string errorMsg = InterpretarErrorLogin(responseText);
                Debug.LogError("[DAOAdmin] Error en Auth Login: " + responseText);
                callback?.Invoke(false, null, errorMsg);
                yield break;
            }
        }

        // 3. PASO 2: Si Auth fue exitoso, obtener el nombre de la tabla "Admin"
        Debug.Log("[DAOAdmin] Auth exitoso. Obteniendo datos de tabla Admin...");

        yield return GetRequest("Admin?email=eq." + UnityWebRequest.EscapeURL(email) + "&select=*", (exito, json, error) =>
        {
            if (!exito)
            {
                // Si falla obtener los datos extra, igual lo dejamos pasar pero con nombre genérico
                Debug.LogWarning("[DAOAdmin] Login Auth OK, pero no se pudieron obtener datos extras: " + error);
                callback?.Invoke(true, "Administrador", null);
                return;
            }

            AdminData admin = ParsearPrimerItem<AdminData>(json);
            if (admin != null)
            {
                callback?.Invoke(true, admin.nombre_admin, null);
            }
            else
            {
                // Usuario autenticado en Auth pero no existe en la tabla Admin
                callback?.Invoke(true, "Administrador", null);
            }
        });
    }

    private string InterpretarErrorLogin(string responseJson)
    {
        if (responseJson.Contains("invalid_credentials") || responseJson.Contains("Invalid login credentials"))
            return "Email o contraseña incorrectos.";
        if (responseJson.Contains("Email not confirmed"))
            return "El email aún no ha sido confirmado.";
        return "Error al iniciar sesión. Revisá tus datos.";
    }

    // ═════════════════════════════════════════════
    //  OBTENER ADMIN POR ID
    //  Uso: daoAdmin.ObtenerAdminPorId(1, (exito, nombre, email, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Obtiene los datos de un administrador por su ID.
    /// Callback: (bool exito, string nombreAdmin, string email, string mensajeError)
    /// </summary>
    public void ObtenerAdminPorId(int idAdmin, Action<bool, string, string, string> callback)
    {
        StartCoroutine(ObtenerAdminPorIdCoroutine(idAdmin, callback));
    }

    private IEnumerator ObtenerAdminPorIdCoroutine(int idAdmin, Action<bool, string, string, string> callback)
    {
        yield return GetRequest("Admin?id_admin=eq." + idAdmin + "&select=*", (exito, json, error) =>
        {
            if (!exito)
            {
                callback?.Invoke(false, null, null, error);
                return;
            }

            AdminData admin = ParsearPrimerItem<AdminData>(json);
            if (admin != null)
            {
                callback?.Invoke(true, admin.nombre_admin, admin.email, null);
            }
            else
            {
                callback?.Invoke(false, null, null, "Administrador no encontrado.");
            }
        });
    }

    // ═════════════════════════════════════════════
    //  REGISTRAR USUARIO (POST)
    //  Inserta un nuevo administrador en la tabla Admin de Supabase
    //  Uso: daoAdmin.RegistrarUsuario("email@ejemplo.com", "Nombre Admin", (exito, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Registra un nuevo administrador en la tabla Admin de Supabase.
    /// Callback: (bool exito, string mensajeError)
    /// </summary>
    public void RegistrarUsuario(string email, string nombreAdmin, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarUsuarioCoroutine(email, nombreAdmin, callback));
    }

    private IEnumerator RegistrarUsuarioCoroutine(string email, string nombreAdmin, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(email))
        { callback?.Invoke(false, "El email no puede estar vacío."); yield break; }
        if (string.IsNullOrEmpty(nombreAdmin))
        { callback?.Invoke(false, "El nombre no puede estar vacío."); yield break; }

        email = email.Trim().ToLower();
        nombreAdmin = nombreAdmin.Trim();

        string jsonBody = "{\"email\":\"" + email + "\",\"nombre_admin\":\"" + nombreAdmin + "\"}";

        yield return PostRequest("Admin", jsonBody, (exito, serverResponse) =>
        {
            if (exito)
            {
                Debug.Log("[DAOAdmin] ✓ Administrador registrado exitosamente: " + nombreAdmin);
                callback?.Invoke(true, null);
            }
            else if (serverResponse.Contains("Admin_email_key") || serverResponse.Contains("duplicate key"))
            {
                callback?.Invoke(false, "Ya existe un administrador con ese email.");
            }
            else
            {
                callback?.Invoke(false, "Error al registrar el administrador. Intentá nuevamente.");
            }
        });
    }

    // ═════════════════════════════════════════════
    //  REGISTRAR ADMIN — Crear cuenta vía Supabase Auth
    //  Uso: daoAdmin.RegistrarAdmin("email", "password", "nombre", (exito, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Registra un nuevo administrador en Supabase Auth.
    /// Callback: (bool exito, string mensajeError)
    /// </summary>
    public void RegistrarAdmin(string email, string password, string nombre, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarAdminCoroutine(email, password, nombre, callback));
    }

    private IEnumerator RegistrarAdminCoroutine(string email, string password, string nombre, Action<bool, string> callback)
    {
        // 1. Validaciones básicas
        if (string.IsNullOrEmpty(email))
        { callback?.Invoke(false, "El email no puede estar vacío."); yield break; }
        if (string.IsNullOrEmpty(nombre))
        { callback?.Invoke(false, "El nombre no puede estar vacío."); yield break; }
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        { callback?.Invoke(false, "La contraseña debe tener al menos 6 caracteres."); yield break; }

        // 2. Construir el JSON para Supabase Auth signup
        string jsonBody = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"data\":{{\"nombre\":\"{nombre}\"}}}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(SUPABASE_AUTH_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", API_KEY);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[DAOAdmin] Cuenta creada en Auth. Procediendo a guardar en tabla Admin...");
                
                // SEGUNDO PASO: Guardar en la tabla "Admin"
                // Usamos la corrutina que ya existe para no repetir código
                yield return StartCoroutine(RegistrarUsuarioCoroutine(email, nombre, (exitoTabla, errorTabla) => {
                    if (exitoTabla)
                    {
                        callback?.Invoke(true, null);
                    }
                    else
                    {
                        // Si falla el guardado en la tabla, avisamos
                        callback?.Invoke(false, "Cuenta creada pero hubo un error en la base de datos: " + errorTabla);
                    }
                }));
            }
            else
            {
                string responseText = request.downloadHandler?.text ?? "";
                string errorMsg = InterpretarErrorRegistro(responseText);
                Debug.LogError("[DAOAdmin] Error al registrar en Auth: " + responseText);
                callback?.Invoke(false, errorMsg);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  Interpreta los errores comunes de registro en Supabase
    // ─────────────────────────────────────────────
    private string InterpretarErrorRegistro(string responseJson)
    {
        if (responseJson.Contains("already registered") || responseJson.Contains("User already registered"))
            return "Ese email ya está registrado.";
        if (responseJson.Contains("invalid_email") || responseJson.Contains("Invalid email"))
            return "El email no es válido.";
        if (responseJson.Contains("weak_password") || responseJson.Contains("Password should"))
            return "La contraseña es muy débil. Usá al menos 6 caracteres.";
        return "No se pudo crear la cuenta. Revisá los datos.";
    }
}