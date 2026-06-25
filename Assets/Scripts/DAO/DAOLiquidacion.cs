using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAOLiquidacion : BaseDAO
{
    [Serializable]
    private class LiquidacionData
    {
        public long id_Liquidacion;
        public string Fecha;
        public long Monto_Alquiler;
        public long Honorarios;
        public long DescuentoAdicional;
        public string DescuentoDescripcion;
        public long NetoPropietario;
        public long Estado;
        public long id_Contratos;
    }

    // ═══ 1. REGISTRAR LIQUIDACIÓN (POST) ═══
    // RF-03.03

    public void RegistrarLiquidacion(string fecha, long montoAlquiler, long honorarios, long descuentoAdicional, string descuentoDescripcion, long netoPropietario, long estado, long idContrato, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarLiquidacionCoroutine(fecha, montoAlquiler, honorarios, descuentoAdicional, descuentoDescripcion, netoPropietario, estado, idContrato, callback));
    }

    private IEnumerator RegistrarLiquidacionCoroutine(string fecha, long montoAlquiler, long honorarios, long descuentoAdicional, string descuentoDescripcion, long netoPropietario, long estado, long idContrato, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(fecha))
        { callback?.Invoke(false, "La fecha no puede estar vacía."); yield break; }
        if (idContrato <= 0)
        { callback?.Invoke(false, "Debe seleccionar un contrato válido."); yield break; }

        string descDescJson = string.IsNullOrEmpty(descuentoDescripcion) ? "null" : $"\"{descuentoDescripcion.Trim()}\"";

        string jsonBody = "{" +
            $"\"Fecha\":\"{fecha.Trim()}\"," +
            $"\"Monto_Alquiler\":{montoAlquiler}," +
            $"\"Honorarios\":{honorarios}," +
            $"\"DescuentoAdicional\":{descuentoAdicional}," +
            $"\"DescuentoDescripcion\":{descDescJson}," +
            $"\"NetoPropietario\":{netoPropietario}," +
            $"\"Estado\":{estado}," +
            $"\"id_Contratos\":{idContrato}" +
            "}";

        yield return PostRequest("Liquidacion", jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el contrato seleccionado no existe.");
            else callback?.Invoke(false, "Error al registrar la liquidación. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODAS (GET all) ═══
    // RF-03.04

    public void ObtenerTodasLasLiquidaciones(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Liquidacion?select=*&order=Fecha.desc", callback));
    }

    // ═══ 3. OBTENER POR ID (GET by id) ═══
    // RF-03.03

    public void ObtenerLiquidacionPorId(long idLiquidacion, Action<bool, string, string> callback)
    {
        StartCoroutine(ObtenerLiquidacionPorIdCoroutine(idLiquidacion, callback));
    }

    private IEnumerator ObtenerLiquidacionPorIdCoroutine(long idLiquidacion, Action<bool, string, string> callback)
    {
        yield return GetRequest("Liquidacion?id_Liquidacion=eq." + idLiquidacion + "&select=*", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, null, error); return; }
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            { callback?.Invoke(false, null, "Liquidación no encontrada."); return; }
            callback?.Invoke(true, json, null);
        });
    }

    // ═══ 4. OBTENER POR CONTRATO (GET filtrado) ═══
    // RF-03.03 / RF-03.04

    public void ObtenerLiquidacionesPorContrato(long idContrato, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Liquidacion?id_Contratos=eq." + idContrato + "&select=*&order=Fecha.desc", callback));
    }

    // ═══ 5. FILTRAR POR PERÍODO (GET rango fechas) ═══
    // RF-03.04

    public void FiltrarLiquidacionesPorPeriodo(string fechaDesde, string fechaHasta, Action<bool, string, string> callback)
    {
        StartCoroutine(FiltrarLiquidacionesPorPeriodoCoroutine(fechaDesde, fechaHasta, callback));
    }

    private IEnumerator FiltrarLiquidacionesPorPeriodoCoroutine(string fechaDesde, string fechaHasta, Action<bool, string, string> callback)
    {
        if (string.IsNullOrEmpty(fechaDesde) || string.IsNullOrEmpty(fechaHasta))
        { callback?.Invoke(false, null, "Las fechas de inicio y fin son obligatorias."); yield break; }

        yield return GetRequest(
            "Liquidacion?Fecha=gte." + fechaDesde.Trim() + "&Fecha=lte." + fechaHasta.Trim() + "&select=*&order=Fecha.desc",
            callback);
    }

    // ═══ 6. ACTUALIZAR (PATCH) ═══
    // RF-03.03

    public void ActualizarLiquidacion(long idLiquidacion, string fecha, long montoAlquiler, long honorarios, long descuentoAdicional, string descuentoDescripcion, long netoPropietario, long estado, long idContrato, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarLiquidacionCoroutine(idLiquidacion, fecha, montoAlquiler, honorarios, descuentoAdicional, descuentoDescripcion, netoPropietario, estado, idContrato, callback));
    }

    private IEnumerator ActualizarLiquidacionCoroutine(long idLiquidacion, string fecha, long montoAlquiler, long honorarios, long descuentoAdicional, string descuentoDescripcion, long netoPropietario, long estado, long idContrato, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(fecha))
        { callback?.Invoke(false, "La fecha no puede estar vacía."); yield break; }

        string descDescJson = string.IsNullOrEmpty(descuentoDescripcion) ? "null" : $"\"{descuentoDescripcion.Trim()}\"";

        string jsonBody = "{" +
            $"\"Fecha\":\"{fecha.Trim()}\"," +
            $"\"Monto_Alquiler\":{montoAlquiler}," +
            $"\"Honorarios\":{honorarios}," +
            $"\"DescuentoAdicional\":{descuentoAdicional}," +
            $"\"DescuentoDescripcion\":{descDescJson}," +
            $"\"NetoPropietario\":{netoPropietario}," +
            $"\"Estado\":{estado}," +
            $"\"id_Contratos\":{idContrato}" +
            "}";

        yield return PatchRequest("Liquidacion?id_Liquidacion=eq." + idLiquidacion, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el contrato seleccionado no existe.");
            else callback?.Invoke(false, "Error al actualizar la liquidación.");
        });
    }

    // ═══ 7. ELIMINAR (DELETE) ═══
    // RF-03.03

    public void EliminarLiquidacion(long idLiquidacion, Action<bool, string> callback)
    {
        StartCoroutine(DeleteRequest("Liquidacion?id_Liquidacion=eq." + idLiquidacion, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al eliminar la liquidación.");
        }));
    }

    // ═══ 8. ACTUALIZAR ESTADO (PATCH parcial) ═══
    // RF-03.03

    public void ActualizarEstadoLiquidacion(long idLiquidacion, long nuevoEstado, Action<bool, string> callback)
    {
        string jsonBody = $"{{\"Estado\":{nuevoEstado}}}";
        StartCoroutine(PatchRequest("Liquidacion?id_Liquidacion=eq." + idLiquidacion, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al actualizar el estado de la liquidación.");
        }));
    }

    // ═══ 9. OBTENER POR CONTRATO Y ESTADO (Historial filtrado) ═══
    // RF-03.04 - Histórico de Liquidaciones

    public void ObtenerLiquidacionesPorContratoYEstado(long idContrato, long estado, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest(
            "Liquidacion?id_Contratos=eq." + idContrato + "&Estado=eq." + estado + "&select=*&order=Fecha.desc",
            callback));
    }

    // ═══ 10. OBTENER HISTORIAL POR CONTRATO Y PERÍODO (Rango de fechas) ═══
    // RF-03.04 - Histórico de Liquidaciones

    public void ObtenerHistorialPorContratoYPeriodo(long idContrato, string fechaDesde, string fechaHasta, Action<bool, string, string> callback)
    {
        StartCoroutine(ObtenerHistorialPorContratoYPeriodoCoroutine(idContrato, fechaDesde, fechaHasta, callback));
    }

    private IEnumerator ObtenerHistorialPorContratoYPeriodoCoroutine(long idContrato, string fechaDesde, string fechaHasta, Action<bool, string, string> callback)
    {
        if (idContrato <= 0)
        { callback?.Invoke(false, null, "El ID del contrato es inválido."); yield break; }
        if (string.IsNullOrEmpty(fechaDesde) || string.IsNullOrEmpty(fechaHasta))
        { callback?.Invoke(false, null, "Las fechas de inicio y fin son obligatorias."); yield break; }

        yield return GetRequest(
            "Liquidacion?id_Contratos=eq." + idContrato + "&Fecha=gte." + fechaDesde.Trim() + "&Fecha=lte." + fechaHasta.Trim() + "&select=*&order=Fecha.desc",
            callback);
    }

    // ═══ 11. OBTENER ESTADÍSTICAS DE LIQUIDACIONES POR CONTRATO ═══
    // RF-03.04 - Histórico de Liquidaciones
    // Retorna: totalPagado, totalPendiente, cantidadLiquidaciones

    public void ObtenerEstadisticasLiquidacionesPorContrato(long idContrato, Action<bool, long, long, long> callback)
    {
        StartCoroutine(ObtenerEstadisticasCoroutine(idContrato, callback));
    }

    private IEnumerator ObtenerEstadisticasCoroutine(long idContrato, Action<bool, long, long, long> callback)
    {
        if (idContrato <= 0)
        { callback?.Invoke(false, 0, 0, 0); yield break; }

        yield return GetRequest("Liquidacion?id_Contratos=eq." + idContrato + "&select=NetoPropietario,Estado", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, 0, 0, 0); return; }
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            { callback?.Invoke(true, 0, 0, 0); return; }

            long totalPagado = 0;
            long totalPendiente = 0;
            long cantidadLiquidaciones = 0;

            try
            {
                json = json.Trim();
                if (json.StartsWith("[") && json.EndsWith("]"))
                {
                    json = json.Substring(1, json.Length - 2);
                    string[] items = json.Split(new string[] { "},{" }, System.StringSplitOptions.None);

                    foreach (string item in items)
                    {
                        cantidadLiquidaciones++;
                        long neto = ExtractLongValue(item, "NetoPropietario");
                        long estado = ExtractLongValue(item, "Estado");

                        if (estado == 1) // Pagada
                            totalPagado += neto;
                        else // Pendiente
                            totalPendiente += neto;
                    }
                }

                callback?.Invoke(true, totalPagado, totalPendiente, cantidadLiquidaciones);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error al parsear estadísticas: " + ex.Message);
                callback?.Invoke(false, 0, 0, 0);
            }
        });
    }

    // ═══ AUXILIAR: Extraer valores long del JSON ═══

    private long ExtractLongValue(string json, string fieldName)
    {
        try
        {
            string pattern = "\"" + fieldName + "\":";
            int startIndex = json.IndexOf(pattern);
            if (startIndex == -1) return 0;

            startIndex += pattern.Length;
            int endIndex = json.IndexOf(",", startIndex);
            if (endIndex == -1) endIndex = json.IndexOf("}", startIndex);
            if (endIndex == -1) endIndex = json.Length;

            string valueStr = json.Substring(startIndex, endIndex - startIndex).Trim();
            if (long.TryParse(valueStr, out long result))
                return result;

            return 0;
        }
        catch
        {
            return 0;
        }
    }
}
