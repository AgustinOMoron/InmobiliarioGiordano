using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAORecibo : BaseDAO
{
    [Serializable]
    private class ReciboData
    {
        public long id_Recibo;
        public string Fecha;
        public long Monto;
        public long Total_Abonar;
        public long Tipo;
        public long Tipo_Pago;
        public long id_Contrato;
    }

    // ═══ 1. REGISTRAR RECIBO (POST) ═══
    // RF-03.01 / RF-03.02

    /// <summary>Callback: (bool exito, long idReciboCreado, string error)</summary>
    public void RegistrarRecibo(string fecha, long monto, long totalAbonar, long tipo, long tipoPago, long idContrato, Action<bool, long, string> callback)
    {
        StartCoroutine(RegistrarReciboCoroutine(fecha, monto, totalAbonar, tipo, tipoPago, idContrato, callback));
    }

    [System.Serializable] private class ReciboCreado { public long id_Recibo; }
    [System.Serializable] private class RecibosCreadosArray { public ReciboCreado[] items; }

    private IEnumerator RegistrarReciboCoroutine(string fecha, long monto, long totalAbonar, long tipo, long tipoPago, long idContrato, Action<bool, long, string> callback)
    {
        if (string.IsNullOrEmpty(fecha))
        { callback?.Invoke(false, 0, "La fecha no puede estar vacía."); yield break; }
        if (idContrato <= 0)
        { callback?.Invoke(false, 0, "Debe seleccionar un contrato válido."); yield break; }

        string jsonBody = "{" +
            $"\"Fecha\":\"{fecha.Trim()}\"," +
            $"\"Monto\":{monto}," +
            $"\"Total_Abonar\":{totalAbonar}," +
            $"\"Tipo\":{tipo}," +
            $"\"Tipo_Pago\":{tipoPago}," +
            $"\"id_Contrato\":{idContrato}" +
            "}";

        yield return PostRequest("Recibo", jsonBody, (exito, sr) =>
        {
            if (exito)
            {
                // BaseDAO usa Prefer: return=representation → sr contiene el objeto creado
                long idCreado = 0;
                try
                {
                    var arr = JsonUtility.FromJson<RecibosCreadosArray>("{\"items\":" + sr + "}");
                    if (arr?.items != null && arr.items.Length > 0) idCreado = arr.items[0].id_Recibo;
                }
                catch { Debug.LogWarning("[DAORecibo] No se pudo extraer id_Recibo del POST response."); }
                callback?.Invoke(true, idCreado, null);
            }
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, 0, "Error: el contrato seleccionado no existe.");
            else callback?.Invoke(false, 0, "Error al registrar el recibo. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODOS (GET all) ═══
    // RF-03.01

    public void ObtenerTodosLosRecibos(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Recibo?select=*&order=Fecha.desc", callback));
    }

    // ═══ 3. OBTENER POR ID (GET by id) ═══
    // RF-03.01 / RF-03.02

    public void ObtenerReciboPorId(long idRecibo, Action<bool, string, string> callback)
    {
        StartCoroutine(ObtenerReciboPorIdCoroutine(idRecibo, callback));
    }

    private IEnumerator ObtenerReciboPorIdCoroutine(long idRecibo, Action<bool, string, string> callback)
    {
        yield return GetRequest("Recibo?id_Recibo=eq." + idRecibo + "&select=*", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, null, error); return; }
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            { callback?.Invoke(false, null, "Recibo no encontrado."); return; }
            callback?.Invoke(true, json, null);
        });
    }

    // ═══ 4. OBTENER POR CONTRATO (GET filtrado) ═══
    // RF-03.01

    public void ObtenerRecibosPorContrato(long idContrato, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Recibo?id_Contrato=eq." + idContrato + "&select=*&order=Fecha.desc", callback));
    }

    // ═══ 5. FILTRAR POR PERÍODO (GET rango fechas) ═══
    // RF-03.01

    public void FiltrarRecibosPorPeriodo(string fechaDesde, string fechaHasta, Action<bool, string, string> callback)
    {
        StartCoroutine(FiltrarRecibosPorPeriodoCoroutine(fechaDesde, fechaHasta, callback));
    }

    private IEnumerator FiltrarRecibosPorPeriodoCoroutine(string fechaDesde, string fechaHasta, Action<bool, string, string> callback)
    {
        if (string.IsNullOrEmpty(fechaDesde) || string.IsNullOrEmpty(fechaHasta))
        { callback?.Invoke(false, null, "Las fechas de inicio y fin son obligatorias."); yield break; }

        yield return GetRequest(
            "Recibo?Fecha=gte." + fechaDesde.Trim() + "&Fecha=lte." + fechaHasta.Trim() + "&select=*&order=Fecha.desc",
            callback);
    }

    // ═══ 6. ACTUALIZAR (PATCH) ═══
    // RF-03.01

    public void ActualizarRecibo(long idRecibo, string fecha, long monto, long totalAbonar, long tipo, long tipoPago, long idContrato, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarReciboCoroutine(idRecibo, fecha, monto, totalAbonar, tipo, tipoPago, idContrato, callback));
    }

    private IEnumerator ActualizarReciboCoroutine(long idRecibo, string fecha, long monto, long totalAbonar, long tipo, long tipoPago, long idContrato, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(fecha))
        { callback?.Invoke(false, "La fecha no puede estar vacía."); yield break; }

        string jsonBody = "{" +
            $"\"Fecha\":\"{fecha.Trim()}\"," +
            $"\"Monto\":{monto}," +
            $"\"Total_Abonar\":{totalAbonar}," +
            $"\"Tipo\":{tipo}," +
            $"\"Tipo_Pago\":{tipoPago}," +
            $"\"id_Contrato\":{idContrato}" +
            "}";

        yield return PatchRequest("Recibo?id_Recibo=eq." + idRecibo, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el contrato seleccionado no existe.");
            else callback?.Invoke(false, "Error al actualizar el recibo.");
        });
    }

    // ═══ 7. ELIMINAR (DELETE) ═══
    // RF-03.01

    public void EliminarRecibo(long idRecibo, Action<bool, string> callback)
    {
        StartCoroutine(DeleteRequest("Recibo?id_Recibo=eq." + idRecibo, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al eliminar el recibo.");
        }));
    }
}
