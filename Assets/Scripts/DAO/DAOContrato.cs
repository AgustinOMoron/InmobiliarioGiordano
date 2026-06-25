using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class DAOContrato : BaseDAO
{
    [Serializable]
    private class ContratoData
    {
        public long id_Contrato;
        public string FechaInicio;
        public string FechaFIn;
        public long MontoAlquiler;
        public long HonorarioPorcentaje;
        public long MesesActualizacion;
        public string TipoIndice;
        public long Estado;
        public long id_Duenos;
        public long id_Inquilino;
        public long id_Inmueble;
    }

    // ═══ 1. REGISTRAR CONTRATO (POST) ═══
    // RF-01.03: Administración de Contratos de Alquiler

    public void RegistrarContrato(string fechaInicio, string fechaFin, long montoAlquiler, long honorarioPorcentaje, long mesesActualizacion, string tipoIndice, long estado, long idDueno, long idInquilino, long idInmueble, Action<bool, string> callback)
    {
        StartCoroutine(RegistrarContratoCoroutine(fechaInicio, fechaFin, montoAlquiler, honorarioPorcentaje, mesesActualizacion, tipoIndice, estado, idDueno, idInquilino, idInmueble, callback));
    }

    private IEnumerator RegistrarContratoCoroutine(string fechaInicio, string fechaFin, long montoAlquiler, long honorarioPorcentaje, long mesesActualizacion, string tipoIndice, long estado, long idDueno, long idInquilino, long idInmueble, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(fechaInicio))
        { callback?.Invoke(false, "La fecha de inicio no puede estar vacía."); yield break; }
        if (montoAlquiler <= 0)
        { callback?.Invoke(false, "El monto de alquiler debe ser mayor a cero."); yield break; }
        if (idDueno <= 0 || idInquilino <= 0 || idInmueble <= 0)
        { callback?.Invoke(false, "Debe seleccionar un propietario, inquilino e inmueble válidos."); yield break; }

        string fechaFinJson = string.IsNullOrEmpty(fechaFin) ? "null" : $"\"{fechaFin.Trim()}\"";

        string jsonBody = "{" +
            $"\"FechaInicio\":\"{fechaInicio.Trim()}\"," +
            $"\"FechaFIn\":{fechaFinJson}," +
            $"\"MontoAlquiler\":{montoAlquiler}," +
            $"\"HonorarioPorcentaje\":{honorarioPorcentaje}," +
            $"\"MesesActualizacion\":{mesesActualizacion}," +
            $"\"TipoIndice\":\"{tipoIndice}\"," +
            $"\"Estado\":{estado}," +
            $"\"id_Duenos\":{idDueno}," +
            $"\"id_Inquilino\":{idInquilino}," +
            $"\"id_Inmueble\":{idInmueble}" +
            "}";

        yield return PostRequest("Contrato", jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el propietario, inquilino o inmueble seleccionado no existe.");
            else callback?.Invoke(false, "Error al registrar el contrato. Intentá nuevamente.");
        });
    }

    // ═══ 2. OBTENER TODOS (GET all) ═══
    // RF-01.03 / RF-01.04

    public void ObtenerTodosLosContratos(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Contrato?select=*&order=FechaInicio.desc", callback));
    }

    // ═══ 3. OBTENER POR ID (GET by id) ═══
    // RF-01.03 / RF-03.01/03.03

    public void ObtenerContratoPorId(long idContrato, Action<bool, string, string> callback)
    {
        StartCoroutine(ObtenerContratoPorIdCoroutine(idContrato, callback));
    }

    private IEnumerator ObtenerContratoPorIdCoroutine(long idContrato, Action<bool, string, string> callback)
    {
        yield return GetRequest("Contrato?id_contrato=eq." + idContrato + "&select=*", (exito, json, error) =>
        {
            if (!exito) { callback?.Invoke(false, null, error); return; }
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
            { callback?.Invoke(false, null, "Contrato no encontrado."); return; }
            callback?.Invoke(true, json, null);
        });
    }

    // ═══ 4. ACTUALIZAR (PATCH) ═══
    // RF-01.03

    public void ActualizarContrato(long idContrato, string fechaInicio, string fechaFin, long montoAlquiler, long honorarioPorcentaje, long mesesActualizacion, string tipoIndice, long estado, long idDueno, long idInquilino, long idInmueble, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarContratoCoroutine(idContrato, fechaInicio, fechaFin, montoAlquiler, honorarioPorcentaje, mesesActualizacion, tipoIndice, estado, idDueno, idInquilino, idInmueble, callback));
    }

    private IEnumerator ActualizarContratoCoroutine(long idContrato, string fechaInicio, string fechaFin, long montoAlquiler, long honorarioPorcentaje, long mesesActualizacion, string tipoIndice, long estado, long idDueno, long idInquilino, long idInmueble, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(fechaInicio))
        { callback?.Invoke(false, "La fecha de inicio no puede estar vacía."); yield break; }
        if (montoAlquiler <= 0)
        { callback?.Invoke(false, "El monto de alquiler debe ser mayor a cero."); yield break; }

        string fechaFinJson = string.IsNullOrEmpty(fechaFin) ? "null" : $"\"{fechaFin.Trim()}\"";

        string jsonBody = "{" +
            $"\"FechaInicio\":\"{fechaInicio.Trim()}\"," +
            $"\"FechaFIn\":{fechaFinJson}," +
            $"\"MontoAlquiler\":{montoAlquiler}," +
            $"\"HonorarioPorcentaje\":{honorarioPorcentaje}," +
            $"\"MesesActualizacion\":{mesesActualizacion}," +
            $"\"TipoIndice\":\"{tipoIndice}\"," +
            $"\"Estado\":{estado}," +
            $"\"id_Duenos\":{idDueno}," +
            $"\"id_Inquilino\":{idInquilino}," +
            $"\"id_Inmueble\":{idInmueble}" +
            "}";

        yield return PatchRequest("Contrato?id_contrato=eq." + idContrato, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "Error: el propietario, inquilino o inmueble seleccionado no existe.");
            else callback?.Invoke(false, "Error al actualizar el contrato.");
        });
    }

    // ═══ 5. ELIMINAR (DELETE) ═══
    // RF-01.03

    public void EliminarContrato(long idContrato, Action<bool, string> callback)
    {
        StartCoroutine(EliminarContratoCoroutine(idContrato, callback));
    }

    private IEnumerator EliminarContratoCoroutine(long idContrato, Action<bool, string> callback)
    {
        yield return DeleteRequest("Contrato?id_contrato=eq." + idContrato, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else if (sr != null && sr.Contains("foreign key"))
                callback?.Invoke(false, "No se puede eliminar: el contrato tiene recibos o liquidaciones asociadas.");
            else callback?.Invoke(false, "Error al eliminar el contrato.");
        });
    }

    // ═══ 6. BUSCAR POR ID (GET filtro) ═══
    // RF-01.04

    public void BuscarContratoPorId(long idContrato, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Contrato?id_contrato=eq." + idContrato + "&select=*", callback));
    }

    // ═══ 7. OBTENER POR DUEÑO (GET filtrado) ═══
    // RF-01.03 / RF-03.03

    public void ObtenerContratosPorDueno(long idDueno, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Contrato?id_Duenos=eq." + idDueno + "&select=*&order=FechaInicio.desc", callback));
    }

    // ═══ 8. OBTENER POR INQUILINO (GET filtrado) ═══
    // RF-01.03 / RF-03.01

    public void ObtenerContratosPorInquilino(long idInquilino, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Contrato?id_Inquilino=eq." + idInquilino + "&select=*&order=FechaInicio.desc", callback));
    }

    // ═══ 9. OBTENER POR INMUEBLE (GET filtrado) ═══
    // RF-01.03

    public void ObtenerContratosPorInmueble(long idInmueble, Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Contrato?id_Inmueble=eq." + idInmueble + "&select=*&order=FechaInicio.desc", callback));
    }

    // ═══ 10. OBTENER ACTIVOS (GET filtrado) ═══
    // RF-04.01 / RF-04.02

    public void ObtenerContratosActivos(Action<bool, string, string> callback)
    {
        StartCoroutine(GetRequest("Contrato?Estado=eq.1&select=*&order=FechaFIn.asc", callback));
    }

    // ═══ 11. ACTUALIZAR MONTO ALQUILER (PATCH parcial) ═══
    // RF-02.01

    public void ActualizarMontoAlquiler(long idContrato, long nuevoMonto, Action<bool, string> callback)
    {
        StartCoroutine(ActualizarMontoAlquilerCoroutine(idContrato, nuevoMonto, callback));
    }

    private IEnumerator ActualizarMontoAlquilerCoroutine(long idContrato, long nuevoMonto, Action<bool, string> callback)
    {
        if (nuevoMonto <= 0)
        { callback?.Invoke(false, "El nuevo monto debe ser mayor a cero."); yield break; }

        string jsonBody = $"{{\"MontoAlquiler\":{nuevoMonto}}}";
        yield return PatchRequest("Contrato?id_contrato=eq." + idContrato, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al actualizar el monto del alquiler.");
        });
    }

    // ═══ 12. ACTUALIZAR ESTADO (PATCH parcial) ═══
    // RF-01.03

    public void ActualizarEstadoContrato(long idContrato, long nuevoEstado, Action<bool, string> callback)
    {
        string jsonBody = $"{{\"Estado\":{nuevoEstado}}}";
        StartCoroutine(PatchRequest("Contrato?id_contrato=eq." + idContrato, jsonBody, (exito, sr) =>
        {
            if (exito) callback?.Invoke(true, null);
            else callback?.Invoke(false, "Error al actualizar el estado del contrato.");
        }));
    }
}
