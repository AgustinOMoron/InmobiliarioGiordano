using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAOServicio : BaseDAO
{
    [Serializable]
    private class ServicioData
    {
        public long id;
        public string Nombre_servicio;
        public double MontoTotal;
        public string FechaServicio;
        public float PorcentajePagar;
        public long id_Propietario;
        public long id_Propiedad;   // FK → Inmueble (nueva columna en Supabase)
        public long id_Recibo;      // FK → Recibo  (ON DELETE CASCADE en Supabase)
    }

    // ═══ 1. REGISTRAR SERVICIO (POST) ═══
    public void RegistrarServicio(string nombreServicio, double montoTotal, string fechaServicio, long idPropietario, float porcentajePagar, long idPropiedad, Action<bool, string> callback, long idRecibo = 0)
    {
        StartCoroutine(RegistrarServicioCoroutine(nombreServicio, montoTotal, fechaServicio, idPropietario, porcentajePagar, idPropiedad, callback, idRecibo));
    }

    private IEnumerator RegistrarServicioCoroutine(string nombreServicio, double montoTotal, string fechaServicio, long idPropietario, float porcentajePagar, long idPropiedad, Action<bool, string> callback, long idRecibo = 0)
    {
        if (string.IsNullOrEmpty(nombreServicio))
        { callback?.Invoke(false, "El nombre del servicio no puede estar vacío."); yield break; }
        if (montoTotal < 0)
        { callback?.Invoke(false, "El monto del servicio no puede ser negativo."); yield break; }
        if (porcentajePagar < 0f || porcentajePagar > 100f)
        { callback?.Invoke(false, "El porcentaje debe estar entre 0 y 100."); yield break; }
        if (idPropietario <= 0)
        { callback?.Invoke(false, "Debe asociar el servicio a un propietario válido."); yield break; }
        if (string.IsNullOrEmpty(fechaServicio))
        { callback?.Invoke(false, "La fecha del servicio es obligatoria."); yield break; }

        // id_Propiedad es opcional (0 = sin propiedad específica)
        string propiedadJson = idPropiedad > 0 ? $",\"id_propiedad\":{idPropiedad}" : "";
        // id_Recibo es opcional (0 = servicio no asociado a recibo)
        string reciboJson = idRecibo > 0 ? $",\"id_Recibo\":{idRecibo}" : "";

        string jsonBody = "{" +
            $"\"Nombre_servicio\":\"{nombreServicio.Trim()}\"," +
            $"\"MontoTotal\":{montoTotal}," +
            $"\"FechaServicio\":\"{fechaServicio.Trim()}\"," +
            $"\"id_Propietario\":{idPropietario}," +
            $"\"PorcentajePagar\":{porcentajePagar}" +
            propiedadJson +
            reciboJson +
            "}";

        yield return PostRequest("Servicios", jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el propietario o la propiedad seleccionada no existe.");
            else callback?.Invoke(false, "Error al registrar el servicio. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODOS LOS SERVICIOS (GET all) ═══
    public void ObtenerTodosLosServicios(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Servicios?select=*&order=FechaServicio.asc", callback));
    }

    // ═══ 3. OBTENER SERVICIO POR ID (GET by id) ═══
    public void ObtenerServicioPorId(long idServicio, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Servicios?id=eq." + idServicio + "&select=*", callback));
    }

    // ═══ 4. OBTENER SERVICIOS POR PROPIETARIO (GET filtrado) ═══
    public void ObtenerServiciosPorPropietario(long idPropietario, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Servicios?id_Propietario=eq." + idPropietario + "&select=*&order=FechaServicio.asc", callback));
    }

    // ═══ 5. OBTENER SERVICIOS POR PROPIEDAD (GET filtrado) ═══
    // RF-05: Filtrar servicios de una propiedad específica
    public void ObtenerServiciosPorPropiedad(long idPropiedad, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Servicios?id_propiedad=eq." + idPropiedad + "&select=*&order=FechaServicio.asc", callback));
    }

    // ═══ 5b. OBTENER SERVICIOS POR RECIBO (GET filtrado) ═══
    public void ObtenerServiciosPorRecibo(long idRecibo, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Servicios?id_Recibo=eq." + idRecibo + "&select=*&order=FechaServicio.asc", callback));
    }

    // ═══ 5c. ELIMINAR SERVICIOS POR RECIBO (DELETE filtrado) ═══
    // Borra todos los servicios asociados a un recibo (backup por si CASCADE falla)
    public void EliminarServiciosPorRecibo(long idRecibo, Action<bool, string> callback)
    {
        StartCoroutine(DeleteRequest("Servicios?id_Recibo=eq." + idRecibo, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al eliminar los servicios del recibo.");
        }));
    }

    // ═══ 6. ACTUALIZAR SERVICIO (PATCH) ═══
    public void ActualizarServicio(long idServicio, string nombreServicio, double montoTotal, string fechaServicio, long idPropietario, float porcentajePagar, long idPropiedad, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarServicioCoroutine(idServicio, nombreServicio, montoTotal, fechaServicio, idPropietario, porcentajePagar, idPropiedad, callback));
    }

    private IEnumerator ActualizarServicioCoroutine(long idServicio, string nombreServicio, double montoTotal, string fechaServicio, long idPropietario, float porcentajePagar, long idPropiedad, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(nombreServicio))
        { callback?.Invoke(false, "El nombre del servicio no puede estar vacío."); yield break; }
        if (montoTotal < 0)
        { callback?.Invoke(false, "El monto del servicio no puede ser negativo."); yield break; }
        if (porcentajePagar < 0f || porcentajePagar > 100f)
        { callback?.Invoke(false, "El porcentaje debe estar entre 0 y 100."); yield break; }
        if (idPropietario <= 0)
        { callback?.Invoke(false, "Debe asociar el servicio a un propietario válido."); yield break; }
        if (string.IsNullOrEmpty(fechaServicio))
        { callback?.Invoke(false, "La fecha del servicio es obligatoria."); yield break; }

        string propiedadJson = idPropiedad > 0 ? $",\"id_propiedad\":{idPropiedad}" : ",\"id_propiedad\":null";

        string jsonBody = "{" +
            $"\"Nombre_servicio\":\"{nombreServicio.Trim()}\"," +
            $"\"MontoTotal\":{montoTotal}," +
            $"\"FechaServicio\":\"{fechaServicio.Trim()}\"," +
            $"\"id_Propietario\":{idPropietario}," +
            $"\"PorcentajePagar\":{porcentajePagar}" +
            propiedadJson +
            "}";

        yield return PatchRequest("Servicios?id=eq." + idServicio, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el propietario o la propiedad seleccionada no existe.");
            else callback?.Invoke(false, "Error al actualizar el servicio.");
        });
    }

    // ═══ 7. ELIMINAR SERVICIO (DELETE) ═══
    public void EliminarServicio(long idServicio, Action<bool, string> callback)
    {
        StartCoroutine(DeleteRequest("Servicios?id=eq." + idServicio, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al eliminar el servicio.");
        }));
    }
}
