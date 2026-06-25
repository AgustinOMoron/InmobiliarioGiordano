using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAOInmueble : BaseDAO
{
    [Serializable]
    private class InmuebleData
    {
        public long id_Propiedad;
        public string Direccion;
        public int Numero_Direccion;
        public long Tipo;
        public long id_Duenos;
        public string Barrio;
    }

    // ═══ 1. REGISTRAR INMUEBLE (POST) ═══
    // RF-01.02: Registrar cada propiedad administrada

    public void RegistrarInmueble(string direccion, int numeroDireccion, string barrio, long tipo, long idDueno, Action<bool, long, string> callback)
    {
        StartCoroutine(RegistrarInmuebleCoroutine(direccion, numeroDireccion, barrio, tipo, idDueno, callback));
    }

    private IEnumerator RegistrarInmuebleCoroutine(string direccion, int numeroDireccion, string barrio, long tipo, long idDueno, Action<bool, long, string> callback)
    {
        if (string.IsNullOrEmpty(direccion))
        { callback?.Invoke(false, -1, "La dirección no puede estar vacía."); yield break; }
        if (numeroDireccion <= 0)
        { callback?.Invoke(false, -1, "El número de dirección no es válido."); yield break; }
        if (idDueno <= 0)
        { callback?.Invoke(false, -1, "Debe seleccionar un propietario válido."); yield break; }

        string barrioJson = string.IsNullOrEmpty(barrio) ? "null" : $"\"{barrio.Trim()}\"";
        string jsonBody = "{" +
            $"\"Direccion\":\"{direccion.Trim()}\"," +
            $"\"Numero_Direccion\":{numeroDireccion}," +
            $"\"Barrio\":{barrioJson}," +
            $"\"Tipo\":{tipo}," +
            $"\"id_Duenos\":{idDueno}" +
            "}";

        yield return PostRequest("Inmueble", jsonBody, (exito, sr) =>
        {
            if (exito)
            {
                InmuebleData data = ParsearPrimerItem<InmuebleData>(sr);
                callback?.Invoke(true, data != null ? data.id_Propiedad : -1, null);
            }
            else if (sr.Contains("foreign key"))
                callback?.Invoke(false, -1, "Error: el propietario seleccionado no existe.");
            else callback?.Invoke(false, -1, "Error al registrar el inmueble. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODOS (GET all) ═══
    // RF-01.02 / RF-01.04

    public void ObtenerTodosLosInmuebles(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Inmueble?select=*&order=Direccion.asc", callback));
    }

    // ═══ 3. OBTENER POR ID (GET by id) ═══
    // RF-01.02 / RF-03.01

    public void ObtenerInmueblePorId(long idPropiedad, Action<bool, string, string> callback)
    {
        StartCoroutine(ObtenerInmueblePorIdCoroutine(idPropiedad, callback));
    }

    private IEnumerator ObtenerInmueblePorIdCoroutine(long idPropiedad, Action<bool, string, string> callback)
    {
        yield return GetRequest("Inmueble?id_Propiedad=eq." + idPropiedad + "&select=*", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, null, error); return; }
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            { callback?.Invoke(false, null, "Inmueble no encontrado."); return; }
            callback?.Invoke(true, json, null);
        });
    }

    // ═══ 4. ACTUALIZAR (PATCH) ═══
    // RF-01.02

    public void ActualizarInmueble(long idPropiedad, string direccion, int numeroDireccion, string barrio, long tipo, long idDueno, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarInmuebleCoroutine(idPropiedad, direccion, numeroDireccion, barrio, tipo, idDueno, callback));
    }

    private IEnumerator ActualizarInmuebleCoroutine(long idPropiedad, string direccion, int numeroDireccion, string barrio, long tipo, long idDueno, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(direccion))
        { callback?.Invoke(false, "La dirección no puede estar vacía."); yield break; }
        if (numeroDireccion <= 0)
        { callback?.Invoke(false, "El número de dirección no es válido."); yield break; }

        string barrioJson = string.IsNullOrEmpty(barrio) ? "null" : $"\"{barrio.Trim()}\"";
        string jsonBody = "{" +
            $"\"Direccion\":\"{direccion.Trim()}\"," +
            $"\"Numero_Direccion\":{numeroDireccion}," +
            $"\"Barrio\":{barrioJson}," +
            $"\"Tipo\":{tipo}," +
            $"\"id_Duenos\":{idDueno}" +
            "}";

        yield return PatchRequest("Inmueble?id_Propiedad=eq." + idPropiedad, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el propietario seleccionado no existe.");
            else callback?.Invoke(false, "Error al actualizar el inmueble.");
        });
    }

    // ═══ 5. ELIMINAR (DELETE) ═══
    // RF-01.02

    public void EliminarInmueble(long idPropiedad, Action<bool, string> callback)
    {
        StartCoroutine(EliminarInmuebleCoroutine(idPropiedad, callback));
    }

    private IEnumerator EliminarInmuebleCoroutine(long idPropiedad, Action<bool, string> callback)
    {
        yield return DeleteRequest("Inmueble?id_Propiedad=eq." + idPropiedad, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "No se puede eliminar: el inmueble tiene contratos asociados.");
            else callback?.Invoke(false, "Error al eliminar el inmueble.");
        });
    }

    // ═══ 6. BUSCAR POR DIRECCIÓN (GET filtro) ═══
    // RF-01.04

    public void BuscarInmueblesPorDireccion(string termino, Action<bool, string, string> callback)
    {
        StartCoroutine(BuscarInmueblesPorDireccionCoroutine(termino, callback));
    }

    private IEnumerator BuscarInmueblesPorDireccionCoroutine(string termino, Action<bool, string, string> callback)
    {
        if (string.IsNullOrEmpty(termino))
        { callback?.Invoke(false, null, "El término de búsqueda no puede estar vacío."); yield break; }

        string t = UnityWebRequest.EscapeURL(termino.Trim());
        yield return GetRequest("Inmueble?select=*&Direccion=ilike.*" + t + "*&order=Direccion.asc", callback);
    }

    // ═══ 7. OBTENER POR DUEÑO (GET filtrado) ═══
    // RF-01.02 / RF-03.03

    public void ObtenerInmueblesPorDueno(long idDueno, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Inmueble?id_Duenos=eq." + idDueno + "&select=*&order=Direccion.asc", callback));
    }
}
