using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;  


public class BaseDeDatos : MonoBehaviour
{
    //Variables para conectar con Supabase
    private const string URL = SupabaseConfig.URL;
    private const string API_KEY = SupabaseConfig.API_KEY;
    public static BaseDeDatos Instancia;

    private void Awake() 
    {
        //NO Destruir este objeto al cambiar de escena
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // ─────────────────────────────────────────────
    //  GET — Traer todos los registros de una tabla
    //  Uso: StartCoroutine(Get("nombre_tabla", (json) => Debug.Log(json)));
    // ─────────────────────────────────────────────
    public IEnumerator Get(string tabla, System.Action<string> callback)
    {
        string uri = URL + tabla + "?select=*";

        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            AgregarHeaders(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                callback(request.downloadHandler.text);
            else
                Debug.LogError($"[BaseDeDatos] GET '{tabla}' error: {request.error}");
        }
    }

    // ─────────────────────────────────────────────
    //  POST — Insertar un nuevo registro
    //  Uso: StartCoroutine(Post("nombre_tabla", "{\"nombre\":\"Juan\"}"));
    // ─────────────────────────────────────────────
    public IEnumerator Post(string tabla, string jsonBody, System.Action onSuccess = null)
    {
        string uri = URL + tabla;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(uri, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            AgregarHeaders(request);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[BaseDeDatos] POST '{tabla}' exitoso.");
                onSuccess?.Invoke();
            }
            else
                Debug.LogError($"[BaseDeDatos] POST '{tabla}' error: {request.error} - {request.downloadHandler.text}");
        }
    }

    // ─────────────────────────────────────────────
    //  PATCH — Actualizar un registro por ID
    //  Uso: StartCoroutine(Patch("nombre_tabla", 5, "{\"nombre\":\"Carlos\"}"));
    // ─────────────────────────────────────────────
    public IEnumerator Patch(string tabla, int id, string jsonBody, System.Action onSuccess = null)
    {
        string uri = URL + tabla + "?id=eq." + id;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(uri, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            AgregarHeaders(request);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[BaseDeDatos] PATCH '{tabla}' id={id} exitoso.");
                onSuccess?.Invoke();
            }
            else
                Debug.LogError($"[BaseDeDatos] PATCH '{tabla}' error: {request.error} - {request.downloadHandler.text}");
        }
    }

    // ─────────────────────────────────────────────
    //  DELETE — Borrar un registro por ID
    //  Uso: StartCoroutine(Delete("nombre_tabla", 5));
    // ─────────────────────────────────────────────
    public IEnumerator Delete(string tabla, int id, System.Action onSuccess = null)
    {
        string uri = URL + tabla + "?id=eq." + id;

        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            AgregarHeaders(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[BaseDeDatos] DELETE '{tabla}' id={id} exitoso.");
                onSuccess?.Invoke();
            }
            else
                Debug.LogError($"[BaseDeDatos] DELETE '{tabla}' error: {request.error}");
        }
    }

    // ─────────────────────────────────────────────
    //  Agrega los headers necesarios para Supabase
    // ─────────────────────────────────────────────
    private void AgregarHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("apikey", API_KEY);
        request.SetRequestHeader("Authorization", "Bearer " + API_KEY);
    }
}
