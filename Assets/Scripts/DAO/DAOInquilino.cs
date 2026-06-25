using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAOInquilino : BaseDAO
{
    [Serializable]
    private class InquilinoData
    {
        public long id_Inquilino;
        public string Nombre_Inquilinos;
        public string Apellido_Inquilinos;
        public long Num_Telefono;
    }

    // ═══ 1. REGISTRAR INQUILINO (POST) ═══
    // RF-01.01: Registrar nuevos perfiles de inquilinos

    public void RegistrarInquilino(string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarInquilinoCoroutine(nombre, apellido, telefono, callback));
    }

    private IEnumerator RegistrarInquilinoCoroutine(string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        { callback?.Invoke(false, "El nombre y apellido no pueden estar vacíos."); yield break; }
        if (telefono <= 0)
        { callback?.Invoke(false, "El número de teléfono no es válido."); yield break; }

        string jsonBody = $"{{\"Nombre_Inquilinos\":\"{nombre.Trim()}\",\"Apellido_Inquilinos\":\"{apellido.Trim()}\",\"Num_Telefono\":{telefono}}}";

        yield return PostRequest("Inquilino", jsonBody, (exito, serverResponse) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (serverResponse.Contains("Inquilino_Num_Telefono_key"))
                callback?.Invoke(false, "Ya existe un inquilino con ese número de teléfono.");
            else callback?.Invoke(false, "Error al registrar el inquilino. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODOS (GET all) ═══
    // RF-01.01 / RF-01.03

    public void ObtenerTodosLosInquilinos(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Inquilino?select=*&order=Apellido_Inquilinos.asc", callback));
    }

    // ═══ 3. OBTENER POR ID (GET by id) ═══
    // RF-01.01 / RF-03.01 / RF-04.03/RF-04.04

    public void ObtenerInquilinoPorId(long idInquilino, Action<bool, string, string, long, string> callback)
    {
        StartCoroutine(ObtenerInquilinoPorIdCoroutine(idInquilino, callback));
    }

    private IEnumerator ObtenerInquilinoPorIdCoroutine(long idInquilino, Action<bool, string, string, long, string> callback)
    {
        yield return GetRequest("Inquilino?id_Inquilino=eq." + idInquilino + "&select=*", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, null, null, 0, error); return; }
            InquilinoData inq = ParsearPrimerItem<InquilinoData>(json);
            if (inq != null) callback?.Invoke(true, inq.Nombre_Inquilinos, inq.Apellido_Inquilinos, inq.Num_Telefono, null);
            else callback?.Invoke(false, null, null, 0, "Inquilino no encontrado.");
        });
    }

    // ═══ 4. ACTUALIZAR (PATCH) ═══
    // RF-01.01

    public void ActualizarInquilino(long idInquilino, string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarInquilinoCoroutine(idInquilino, nombre, apellido, telefono, callback));
    }

    private IEnumerator ActualizarInquilinoCoroutine(long idInquilino, string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        { callback?.Invoke(false, "El nombre y apellido no pueden estar vacíos."); yield break; }

        string jsonBody = $"{{\"Nombre_Inquilinos\":\"{nombre.Trim()}\",\"Apellido_Inquilinos\":\"{apellido.Trim()}\",\"Num_Telefono\":{telefono}}}";

        yield return PatchRequest("Inquilino?id_Inquilino=eq." + idInquilino, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("Inquilino_Num_Telefono_key"))
                callback?.Invoke(false, "Ya existe otro inquilino con ese número de teléfono.");
            else callback?.Invoke(false, "Error al actualizar el inquilino.");
        });
    }

    // ═══ 5. ELIMINAR (DELETE) ═══
    // RF-01.01

    public void EliminarInquilino(long idInquilino, Action<bool, string> callback)
    {
        StartCoroutine(EliminarInquilinoCoroutine(idInquilino, callback));
    }

    private IEnumerator EliminarInquilinoCoroutine(long idInquilino, Action<bool, string> callback)
    {
        yield return DeleteRequest("Inquilino?id_Inquilino=eq." + idInquilino, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "No se puede eliminar: el inquilino tiene contratos asociados.");
            else callback?.Invoke(false, "Error al eliminar el inquilino.");
        });
    }

    // ═══ 6. BUSCAR POR NOMBRE/APELLIDO (GET filtro) ═══
    // RF-01.04

    public void BuscarInquilinos(string termino, Action<bool, string, string> callback)
    {
        StartCoroutine(BuscarInquilinosCoroutine(termino, callback));
    }

    private IEnumerator BuscarInquilinosCoroutine(string termino, Action<bool, string, string> callback)
    {
        if (string.IsNullOrEmpty(termino))
        { callback?.Invoke(false, null, "El término de búsqueda no puede estar vacío."); yield break; }

        string t = UnityWebRequest.EscapeURL(termino.Trim());
        yield return GetRequest(
            "Inquilino?select=*&or=(Nombre_Inquilinos.ilike.*" + t + "*,Apellido_Inquilinos.ilike.*" + t + "*)&order=Apellido_Inquilinos.asc",
            callback);
    }
}
