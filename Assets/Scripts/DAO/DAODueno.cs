using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAODueno : BaseDAO
{
    // ─────────────────────────────────────────────
    //  Clase para deserializar JSON de Supabase
    // ─────────────────────────────────────────────

    [Serializable]
    private class DuenoData
    {
        public long id_Dueno;
        public string Nombre_Dueno;
        public string Apellido_Dueno;
        public long Num_Telefono;
    }

    // ═════════════════════════════════════════════
    //  1. REGISTRAR DUEÑO (POST)
    //  RF-01.01: Registrar nuevos perfiles de propietarios
    //  Uso: daoDueno.RegistrarDueno("Juan", "Pérez", 1155667788, (exito, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Registra un nuevo propietario en la base de datos.
    /// Callback: (bool exito, string mensajeError)
    /// </summary>
    public void RegistrarDueno(string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarDuenoCoroutine(nombre, apellido, telefono, callback));
    }

    private IEnumerator RegistrarDuenoCoroutine(string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        // Validaciones básicas
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        {
            callback?.Invoke(false, "El nombre y apellido no pueden estar vacíos.");
            yield break;
        }

        if (telefono <= 0)
        {
            callback?.Invoke(false, "El número de teléfono no es válido.");
            yield break;
        }

        // Construir el JSON del body
        string jsonBody = $"{{\"Nombre_Dueno\":\"{nombre.Trim()}\",\"Apellido_Dueno\":\"{apellido.Trim()}\",\"Num_Telefono\":{telefono}}}";

        yield return PostRequest("Dueno", jsonBody, (exito, serverResponse) =>
        {
            if (exito)
            {
                callback?.Invoke(true, null);
            }
            else if (serverResponse.Contains("Dueno_Num_Telefono_key"))
            {
                callback?.Invoke(false, "Ya existe un propietario con ese número de teléfono.");
            }
            else
            {
                callback?.Invoke(false, "Error al registrar el propietario. Intentá nuevamente.");
            }
        });
    }

    // ═════════════════════════════════════════════
    //  2. OBTENER TODOS LOS DUEÑOS (GET all)
    //  RF-01.01: Consultar datos de propietarios
    //  RF-01.02/03: Necesario para dropdowns al crear inmuebles/contratos
    //  Uso: daoDueno.ObtenerTodosLosDuenos((exito, lista, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Obtiene todos los propietarios registrados.
    /// Callback: (bool exito, string jsonResultados, string mensajeError)
    /// </summary>
    public void ObtenerTodosLosDuenos(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Dueno?select=*&order=Apellido_Dueno.asc", callback));
    }

    // ═════════════════════════════════════════════
    //  3. OBTENER DUEÑO POR ID (GET by id)
    //  RF-01.01: Consultar datos de un propietario específico
    //  RF-03.03: Obtener datos del propietario para liquidaciones
    //  Uso: daoDueno.ObtenerDuenoPorId(1, (exito, json, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Obtiene los datos de un propietario por su ID.
    /// Callback: (bool exito, string nombre, string apellido, long telefono, string mensajeError)
    /// </summary>
    public void ObtenerDuenoPorId(long idDueno, Action<bool, string, string, long, string> callback)
    {
        StartCoroutine(ObtenerDuenoPorIdCoroutine(idDueno, callback));
    }

    private IEnumerator ObtenerDuenoPorIdCoroutine(long idDueno, Action<bool, string, string, long, string> callback)
    {
        yield return GetRequest("Dueno?id_Dueno=eq." + idDueno + "&select=*", (exito, json, error) =>
        {
            if (!exito)
            {
                callback?.Invoke(false, null, null, 0, error);
                return;
            }

            DuenoData dueno = ParsearPrimerItem<DuenoData>(json);
            if (dueno != null)
            {
                callback?.Invoke(true, dueno.Nombre_Dueno, dueno.Apellido_Dueno, dueno.Num_Telefono, null);
            }
            else
            {
                callback?.Invoke(false, null, null, 0, "Propietario no encontrado.");
            }
        });
    }

    // ═════════════════════════════════════════════
    //  4. ACTUALIZAR DUEÑO (PATCH)
    //  RF-01.01: "actualizarlos" — Modificar datos de propietarios existentes
    //  Uso: daoDueno.ActualizarDueno(1, "Juan", "García", 1199887766, (exito, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Actualiza los datos de un propietario existente.
    /// Callback: (bool exito, string mensajeError)
    /// </summary>
    public void ActualizarDueno(long idDueno, string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarDuenoCoroutine(idDueno, nombre, apellido, telefono, callback));
    }

    private IEnumerator ActualizarDuenoCoroutine(long idDueno, string nombre, string apellido, long telefono, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        {
            callback?.Invoke(false, "El nombre y apellido no pueden estar vacíos.");
            yield break;
        }

        string jsonBody = $"{{\"Nombre_Dueno\":\"{nombre.Trim()}\",\"Apellido_Dueno\":\"{apellido.Trim()}\",\"Num_Telefono\":{telefono}}}";

        yield return PatchRequest("Dueno?id_Dueno=eq." + idDueno, jsonBody, (exito, serverResponse) =>
        {
            if (exito)
            {
                callback?.Invoke(true, null);
            }
            else if (serverResponse != null && serverResponse.Contains("Dueno_Num_Telefono_key"))
            {
                callback?.Invoke(false, "Ya existe otro propietario con ese número de teléfono.");
            }
            else
            {
                callback?.Invoke(false, "Error al actualizar el propietario.");
            }
        });
    }

    // ═════════════════════════════════════════════
    //  5. ELIMINAR DUEÑO (DELETE)
    //  RF-01.01: "mantenerlos organizados" — Dar de baja propietarios
    //  Uso: daoDueno.EliminarDueno(1, (exito, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Elimina un propietario por su ID.
    /// Callback: (bool exito, string mensajeError)
    /// </summary>
    public void EliminarDueno(long idDueno, Action<bool, string> callback)
    {
        StartCoroutine(EliminarDuenoCoroutine(idDueno, callback));
    }

    private IEnumerator EliminarDuenoCoroutine(long idDueno, Action<bool, string> callback)
    {
        yield return DeleteRequest("Dueno?id_Dueno=eq." + idDueno, (exito, serverResponse) =>
        {
            if (exito)
            {
                callback?.Invoke(true, null);
            }
            else if (serverResponse != null && serverResponse.Contains("foreign key"))
            {
                callback?.Invoke(false, "No se puede eliminar: el propietario tiene inmuebles o contratos asociados.");
            }
            else
            {
                callback?.Invoke(false, "Error al eliminar el propietario.");
            }
        });
    }

    // ═════════════════════════════════════════════
    //  6. BUSCAR DUEÑOS POR NOMBRE/APELLIDO (GET con filtro)
    //  RF-01.04: "búsqueda centralizada para localizar rápidamente propietarios por nombre"
    //  Uso: daoDueno.BuscarDuenos("García", (exito, json, error) => { ... });
    // ═════════════════════════════════════════════

    /// <summary>
    /// Busca propietarios cuyo nombre o apellido contenga el término de búsqueda.
    /// Callback: (bool exito, string jsonResultados, string mensajeError)
    /// </summary>
    public void BuscarDuenos(string termino, Action<bool, string, string> callback)
    {
        StartCoroutine(BuscarDuenosCoroutine(termino, callback));
    }

    private IEnumerator BuscarDuenosCoroutine(string termino, Action<bool, string, string> callback)
    {
        if (string.IsNullOrEmpty(termino))
        {
            callback?.Invoke(false, null, "El término de búsqueda no puede estar vacío.");
            yield break;
        }

        // Supabase: or=(col1.ilike.*term*,col2.ilike.*term*)
        // ilike = case-insensitive LIKE
        string terminoEncoded = UnityWebRequest.EscapeURL(termino.Trim());
        yield return GetRequest(
            "Dueno?select=*&or=(Nombre_Dueno.ilike.*" + terminoEncoded + "*,Apellido_Dueno.ilike.*" + terminoEncoded + "*)&order=Apellido_Dueno.asc",
            callback);
    }
}
