using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAODepartamento : BaseDAO
{
    [Serializable]
    private class DepartamentoData
    {
        public long id_Inmueble;
        public long Piso;
        public string Unidad;
        public long id_Propiedad;
    }

    // ═══ 1. REGISTRAR DEPARTAMENTO (POST) ═══

    public void RegistrarDepartamento(long piso, string unidad, long idPropiedad, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarDepartamentoCoroutine(piso, unidad, idPropiedad, callback));
    }

    private IEnumerator RegistrarDepartamentoCoroutine(long piso, string unidad, long idPropiedad, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(unidad))
        { callback?.Invoke(false, "La unidad no puede estar vacía."); yield break; }
        if (piso < 0)
        { callback?.Invoke(false, "El piso no es válido."); yield break; }
        if (idPropiedad <= 0)
        { callback?.Invoke(false, "Debe seleccionar un inmueble válido."); yield break; }

        string jsonBody = "{" +
            $"\"Piso\":{piso}," +
            $"\"Unidad\":\"{unidad.Trim()}\"," +
            $"\"id_Propiedad\":{idPropiedad}" +
            "}";

        yield return PostRequest("Departamento", jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr.Contains("foreign key") || sr.Contains("Departamento_id_Propiedad_fkey"))
                callback?.Invoke(false, "Error: el inmueble seleccionado no existe.");
            else if (sr.Contains("duplicate key"))
                callback?.Invoke(false, "Ya existe un departamento con esos datos.");
            else callback?.Invoke(false, "Error al registrar el departamento. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODOS (GET all) ═══

    public void ObtenerTodosLosDepartamentos(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Departamento?select=*&order=Piso.asc,Unidad.asc", callback));
    }

    // ═══ 3. OBTENER POR ID (GET by id_Inmueble) ═══

    public void ObtenerDepartamentoPorId(long idInmueble, Action<bool, string, string> callback)
    {
        StartCoroutine(ObtenerDepartamentoPorIdCoroutine(idInmueble, callback));
    }

    private IEnumerator ObtenerDepartamentoPorIdCoroutine(long idInmueble, Action<bool, string, string> callback)
    {
        yield return GetRequest("Departamento?id_Inmueble=eq." + idInmueble + "&select=*", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, null, error); return; }
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            { callback?.Invoke(false, null, "Departamento no encontrado."); return; }
            callback?.Invoke(true, json, null);
        });
    }

    // ═══ 4. OBTENER POR PROPIEDAD (GET filtrado) ═══

    public void ObtenerDepartamentosPorPropiedad(long idPropiedad, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Departamento?id_Propiedad=eq." + idPropiedad + "&select=*&order=Piso.asc,Unidad.asc", callback));
    }

    // ═══ 5. ACTUALIZAR (PATCH) ═══

    public void ActualizarDepartamento(long idInmueble, long piso, string unidad, long idPropiedad, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarDepartamentoCoroutine(idInmueble, piso, unidad, idPropiedad, callback));
    }

    private IEnumerator ActualizarDepartamentoCoroutine(long idInmueble, long piso, string unidad, long idPropiedad, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(unidad))
        { callback?.Invoke(false, "La unidad no puede estar vacía."); yield break; }
        if (piso < 0)
        { callback?.Invoke(false, "El piso no es válido."); yield break; }

        string jsonBody = "{" +
            $"\"Piso\":{piso}," +
            $"\"Unidad\":\"{unidad.Trim()}\"," +
            $"\"id_Propiedad\":{idPropiedad}" +
            "}";

        yield return PatchRequest("Departamento?id_Inmueble=eq." + idInmueble, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && (sr.Contains("foreign key") || sr.Contains("Departamento_id_Propiedad_fkey")))
                callback?.Invoke(false, "Error: el inmueble seleccionado no existe.");
            else callback?.Invoke(false, "Error al actualizar el departamento.");
        });
    }

    // ═══ 6. ELIMINAR (DELETE) ═══

    public void EliminarDepartamento(long idInmueble, Action<bool, string> callback)
    {
        StartCoroutine(EliminarDepartamentoCoroutine(idInmueble, callback));
    }

    private IEnumerator EliminarDepartamentoCoroutine(long idInmueble, Action<bool, string> callback)
    {
        yield return DeleteRequest("Departamento?id_Inmueble=eq." + idInmueble, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "No se puede eliminar: el departamento tiene contratos u otros registros asociados.");
            else callback?.Invoke(false, "Error al eliminar el departamento.");
        });
    }

    // ═══ 7. BUSCAR POR UNIDAD (GET filtro) ═══

    public void BuscarDepartamentosPorUnidad(string termino, Action<bool, string, string> callback)
    {
        StartCoroutine(BuscarDepartamentosPorUnidadCoroutine(termino, callback));
    }

    private IEnumerator BuscarDepartamentosPorUnidadCoroutine(string termino, Action<bool, string, string> callback)
    {
        if (string.IsNullOrEmpty(termino))
        { callback?.Invoke(false, null, "El término de búsqueda no puede estar vacío."); yield break; }

        string t = UnityWebRequest.EscapeURL(termino.Trim());
        yield return GetRequest("Departamento?select=*&Unidad=ilike.*" + t + "*&order=Piso.asc,Unidad.asc", callback);
    }
}
