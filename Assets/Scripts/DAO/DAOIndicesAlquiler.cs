using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Consulta la API oficial del BCRA para calcular actualizaciones de alquiler.
/// API: https://api.bcra.gob.ar/estadisticas/v4.0/datosvariable/{id}/{desde}/{hasta}
///   40 = ICL (Índice para Contratos de Locación) — diario
///   27 = IPC (Inflación mensual %)               — mensual
///   30 = CER (proxy Casa Propia)                 — diario
/// </summary>
public class DAOIndicesAlquiler : MonoBehaviour
{
    private const string BASE_URL  = "https://api.bcra.gob.ar/estadisticas/v4.0/monetarias";
    private const int    ID_ICL    = 40;   // Índice para Contratos de Locación (diario)
    private const int    ID_CER    = 30;   // CER = proxy de inflación (diario, derivado del IPC)
    // IPC mensual (variable 27) no está disponible en el endpoint de series del BCRA.
    // Se usa CER como proxy: mismo resultado ya que CER se actualiza diariamente según el IPC.

    // Cache: clave = "idVariable_desde_hasta"
    private readonly Dictionary<string, List<PuntoSerie>> _cache =
        new Dictionary<string, List<PuntoSerie>>();

    // ─────────────────────────────────────────────────────────────────────────
    //  MÉTODO PÚBLICO
    // ─────────────────────────────────────────────────────────────────────────

    public void CalcularNuevoImporte(
        string tipoIndice,
        long montoActual,
        DateTime fechaAjusteAnterior,
        DateTime fechaAjusteActual,
        Action<bool, long, string> callback)
    {
        string idx = (tipoIndice ?? "").Trim().ToUpperInvariant();

        if (idx == "ICL")
            StartCoroutine(CalcularConRatio(ID_ICL, montoActual,
                fechaAjusteAnterior, fechaAjusteActual, callback));
        else if (idx == "IPC")
            // IPC: usamos CER como proxy (derivado del IPC, disponible diariamente en BCRA)
            StartCoroutine(CalcularConRatio(ID_CER, montoActual,
                fechaAjusteAnterior, fechaAjusteActual, callback, etiqueta: "~CER"));
        else if (idx.Contains("CASA") || idx.Contains("PROPIA"))
            StartCoroutine(CalcularConRatio(ID_CER, montoActual,
                fechaAjusteAnterior, fechaAjusteActual, callback, esCasaPropia: true));
        else
            callback?.Invoke(false, 0, $"Índice '{tipoIndice}' no soportado");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ICL / CER  →  ratio de índice
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator CalcularConRatio(
        int idVar,
        long monto,
        DateTime fechaAnterior,
        DateTime fechaActual,
        Action<bool, long, string> callback,
        bool esCasaPropia = false,
        string etiqueta = null)
    {
        // Pedir desde 10 días antes del ajuste anterior.
        // Para índices diarios (ICL, CER): hasta = hoy.
        // Si el BCRA publica con retraso, usamos ayer como fallback en la lógica de búsqueda.
        DateTime desde = fechaAnterior.AddDays(-10);
        DateTime hasta = DateTime.Today;

        List<PuntoSerie> serie = null;
        yield return StartCoroutine(ObtenerSerie(idVar, desde, hasta,
            (ok, data, err) => { serie = ok ? data : null; }));

        if (serie == null || serie.Count < 2)
        {
            Debug.LogWarning($"[BCRA] Serie vacía para variable {idVar}. " +
                             $"Rango: {desde:yyyy-MM-dd} → {hasta:yyyy-MM-dd}");
            callback?.Invoke(false, 0, "Sin datos del BCRA");
            yield break;
        }

        double valAnterior = BuscarUltimo(serie, fechaAnterior);
        // Para fechaActual: si es futura, usamos el último valor disponible (más reciente en la serie)
        DateTime fechaBusquedaActual = fechaActual > DateTime.Today ? DateTime.Today : fechaActual;
        double valActual = BuscarUltimo(serie, fechaBusquedaActual);

        Debug.Log($"[BCRA] Variable {idVar}: val anterior ({fechaAnterior:dd/MM/yy})={valAnterior:F4}  " +
                  $"val actual ({fechaBusquedaActual:dd/MM/yy})={valActual:F4}");

        if (valAnterior <= 0 || valActual <= 0)
        {
            callback?.Invoke(false, 0, "Valores inválidos en la serie");
            yield break;
        }

        long nuevoMonto = (long)Math.Round(monto * (valActual / valAnterior));
        string tag = esCasaPropia ? "~CER" : etiqueta;
        callback?.Invoke(true, nuevoMonto, tag);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  IPC  →  acumulado de variaciones mensuales
    // ─────────────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────────────
    //  HTTP  →  BCRA API
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator ObtenerSerie(
        int idVariable,
        DateTime desde,
        DateTime hasta,
        Action<bool, List<PuntoSerie>, string> callback)
    {
        string cacheKey = $"{idVariable}_{desde:yyyyMMdd}_{hasta:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            callback?.Invoke(true, cached, null);
            yield break;
        }

        string url = $"{BASE_URL}/{idVariable}?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        Debug.Log($"[BCRA] GET {url}");

        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.certificateHandler = new BypassCertificate();  // Evita errores SSL en Unity
        req.timeout = 15;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            // ⚠ Guardar error ANTES de Dispose (req.error se vuelve null después)
            string errorMsg = req.error;
            long   errorCode = req.responseCode;
            req.Dispose();
            Debug.LogError($"[BCRA] Error HTTP para variable {idVariable}: {errorCode} — {errorMsg}");
            callback?.Invoke(false, null, errorMsg ?? "Error de red");
            yield break;
        }

        string json = req.downloadHandler.text;
        Debug.Log($"[BCRA] Respuesta var {idVariable} ({json.Length} chars): " +
                  $"{json.Substring(0, Mathf.Min(200, json.Length))}...");
        req.Dispose();

        List<PuntoSerie> puntos = ParsearBCRA(json);

        if (puntos.Count > 0)
        {
            _cache[cacheKey] = puntos;
            Debug.Log($"[BCRA] Variable {idVariable}: {puntos.Count} puntos cargados.");
            callback?.Invoke(true, puntos, null);
        }
        else
        {
            Debug.LogWarning($"[BCRA] JSON recibido pero sin puntos válidos. Raw: {json.Substring(0, Mathf.Min(500, json.Length))}");
            callback?.Invoke(false, null, "JSON vacío o no parseable");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PARSEO DE JSON  —  manual, sin dependencias externas
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// La API v4.0 devuelve: {"status":200,"results":[{"idVariable":30,"detalle":[{"fecha":"2026-01-02","valor":25.5},...]}]}
    /// Parseo manual para evitar limitaciones de JsonUtility con campos privados.
    /// </summary>
    private List<PuntoSerie> ParsearBCRA(string json)
    {
        var lista = new List<PuntoSerie>();
        if (string.IsNullOrEmpty(json)) return lista;

        try
        {
            // Extraer el array de "detalle" en la nueva estructura v4.0
            string arrStr = ExtraerArray(json, "detalle");
            if (string.IsNullOrEmpty(arrStr) || arrStr == "[]") return lista;

            // Parsear cada objeto { "fecha":"...", "valor":... }
            int pos = 0;
            while (pos < arrStr.Length)
            {
                // Buscar inicio de objeto
                int objStart = arrStr.IndexOf('{', pos);
                if (objStart < 0) break;
                int objEnd = arrStr.IndexOf('}', objStart);
                if (objEnd < 0) break;

                string obj = arrStr.Substring(objStart, objEnd - objStart + 1);
                pos = objEnd + 1;

                string fechaStr = ExtraerCadena(obj, "fecha");
                string valorStr = ExtraerCadena(obj, "valor");

                if (string.IsNullOrEmpty(fechaStr) || string.IsNullOrEmpty(valorStr))
                    continue;

                if (!DateTime.TryParseExact(fechaStr, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    continue;

                if (!double.TryParse(valorStr, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out double val))
                    continue;

                lista.Add(new PuntoSerie { fecha = dt, valor = val });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[BCRA] Excepción parseando JSON: " + ex.Message);
        }

        return lista;
    }

    /// <summary>Extrae el texto del array JSON con la clave dada.</summary>
    private string ExtraerArray(string json, string key)
    {
        string search = $"\"{key}\":";
        int idx = json.IndexOf(search, StringComparison.Ordinal);
        if (idx < 0) return string.Empty;
        int start = json.IndexOf('[', idx + search.Length);
        if (start < 0) return string.Empty;
        int depth = 0, end = start;
        for (int i = start; i < json.Length; i++)
        {
            if (json[i] == '[') depth++;
            else if (json[i] == ']') { depth--; if (depth == 0) { end = i; break; } }
        }
        return json.Substring(start, end - start + 1);
    }

    /// <summary>
    /// Extrae el valor de una clave dentro de un objeto JSON simple.
    /// Funciona para strings y números (devuelve siempre como string sin comillas).
    /// </summary>
    private string ExtraerCadena(string obj, string key)
    {
        string search = $"\"{key}\":";
        int idx = obj.IndexOf(search, StringComparison.Ordinal);
        if (idx < 0) return null;
        int valStart = idx + search.Length;
        while (valStart < obj.Length && obj[valStart] == ' ') valStart++;
        if (valStart >= obj.Length) return null;

        if (obj[valStart] == '"')
        {
            // Valor string: extraer entre comillas
            int end = obj.IndexOf('"', valStart + 1);
            return end < 0 ? null : obj.Substring(valStart + 1, end - valStart - 1);
        }
        else
        {
            // Valor numérico: leer hasta coma, } o fin
            int end = valStart;
            while (end < obj.Length && obj[end] != ',' && obj[end] != '}') end++;
            return obj.Substring(valStart, end - valStart).Trim();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private double BuscarUltimo(List<PuntoSerie> serie, DateTime fechaLimite)
    {
        PuntoSerie mejor = null;
        foreach (var p in serie)
        {
            if (p.fecha <= fechaLimite && (mejor == null || p.fecha > mejor.fecha))
                mejor = p;
        }
        return mejor?.valor ?? 0;
    }

    public void LimpiarCache() => _cache.Clear();

    // ─────────────────────────────────────────────────────────────────────────
    //  SSL BYPASS — necesario en Unity Editor con algunos certificados HTTPS
    // ─────────────────────────────────────────────────────────────────────────

    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    private class PuntoSerie
    {
        public DateTime fecha;
        public double   valor;
    }
}
