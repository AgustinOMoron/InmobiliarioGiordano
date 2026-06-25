using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

/// <summary>
/// Clase base abstracta para todos los DAOs de Supabase.
/// Centraliza las operaciones HTTP comunes (GET, POST, PATCH, DELETE),
/// los headers de autenticación y el parseo de respuestas JSON.
/// </summary>
public abstract class BaseDAO : MonoBehaviour
{
    protected const string URL = SupabaseConfig.URL;
    protected const string API_KEY = SupabaseConfig.API_KEY;

    // ─────────────────────────────────────────────
    //  Wrapper genérico para deserializar arrays JSON de Supabase
    //  Reemplaza las clases XxxDataArray individuales de cada DAO
    // ─────────────────────────────────────────────

    [Serializable]
    protected class DataArray<T>
    {
        public T[] items;
    }

    // ─────────────────────────────────────────────
    //  HEADERS — Una sola implementación, heredada por todos
    // ─────────────────────────────────────────────

    protected void AgregarHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("apikey", API_KEY);
        request.SetRequestHeader("Authorization", "Bearer " + API_KEY);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
        request.SetRequestHeader("Pragma", "no-cache");
        request.SetRequestHeader("Expires", "0");
    }

    // ─────────────────────────────────────────────
    //  GET genérico
    //  Ejecuta un GET a Supabase con la query dada.
    //  Callback: (bool exito, string jsonResponse, string error)
    // ─────────────────────────────────────────────

    protected IEnumerator GetRequest(string query, Action<bool, string, string> callback)
    {
        string queryUrl = URL + query;

        using (UnityWebRequest request = UnityWebRequest.Get(queryUrl))
        {
            AgregarHeaders(request);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string serverBody = request.downloadHandler?.text ?? "(sin respuesta)";
                Debug.LogError($"[{GetType().Name}] Error GET: {request.error}\n  URL: {queryUrl}\n  Respuesta: {serverBody}");
                callback?.Invoke(false, null, "Error de conexión.");
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"[{GetType().Name}] GET OK: {jsonResponse}");
            callback?.Invoke(true, jsonResponse, null);
        }
    }

    // ─────────────────────────────────────────────
    //  POST genérico
    //  Ejecuta un POST a Supabase en la tabla dada.
    //  Callback: (bool exito, string serverResponse)
    //  En caso de error, serverResponse contiene el texto del servidor
    //  para que el DAO hijo pueda interpretar errores específicos.
    // ─────────────────────────────────────────────

    protected IEnumerator PostRequest(string tabla, string jsonBody, Action<bool, string> callback)
    {
        string uri = URL + tabla;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(uri, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            AgregarHeaders(request);
            // Cambiamos a representation para obtener el objeto creado (IDs, etc)
            request.SetRequestHeader("Prefer", "return=representation");

            yield return request.SendWebRequest();

            string serverResponse = request.downloadHandler?.text ?? "";

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[{GetType().Name}] ✓ POST OK en {tabla}: {serverResponse}");
                callback?.Invoke(true, serverResponse);
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] Error POST: {request.error} - {serverResponse}");
                callback?.Invoke(false, serverResponse);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  PATCH genérico
    //  Ejecuta un PATCH a Supabase con la query dada (incluye filtro).
    //  Callback: (bool exito, string serverResponse)
    // ─────────────────────────────────────────────

    protected IEnumerator PatchRequest(string query, string jsonBody, Action<bool, string> callback)
    {
        string uri = URL + query;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(uri, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            AgregarHeaders(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[{GetType().Name}] ✓ PATCH OK");
                callback?.Invoke(true, null);
            }
            else
            {
                string serverResponse = request.downloadHandler?.text ?? "";
                Debug.LogError($"[{GetType().Name}] Error PATCH: {request.error} - {serverResponse}");
                callback?.Invoke(false, serverResponse);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  DELETE genérico
    //  Ejecuta un DELETE a Supabase con la query dada (incluye filtro).
    //  Callback: (bool exito, string serverResponse)
    // ─────────────────────────────────────────────

    protected IEnumerator DeleteRequest(string query, Action<bool, string> callback)
    {
        string uri = URL + query;

        using (UnityWebRequest request = UnityWebRequest.Delete(uri))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            AgregarHeaders(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[{GetType().Name}] ✓ DELETE OK");
                callback?.Invoke(true, null);
            }
            else
            {
                string serverResponse = request.downloadHandler?.text ?? "";
                Debug.LogError($"[{GetType().Name}] Error DELETE: {request.error} - {serverResponse}");
                callback?.Invoke(false, serverResponse);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  Helper: Parsear primer item de un array JSON de Supabase
    //  Retorna default(T) si el array está vacío o es inválido.
    // ─────────────────────────────────────────────

    protected T ParsearPrimerItem<T>(string jsonResponse)
    {
        if (string.IsNullOrEmpty(jsonResponse) || jsonResponse.Trim() == "[]")
            return default;

        try
        {
            string jsonWrapped = "{\"items\":" + jsonResponse + "}";
            DataArray<T> array = JsonUtility.FromJson<DataArray<T>>(jsonWrapped);

            if (array != null && array.items != null && array.items.Length > 0)
                return array.items[0];
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{GetType().Name}] Error al parsear JSON: {ex.Message}");
        }

        return default;
    }
}
