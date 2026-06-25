using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class MenuPrincipalUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  DAOs 
    // ─────────────────────────────────────────────

    [Header("DAOs")]
    [SerializeField] private DAOContrato daoContrato;
    [SerializeField] private DAOInquilino daoInquilino;
    [SerializeField] private DAOInmueble daoInmueble;
    [SerializeField] private DAODueno daoDueno;
    [SerializeField] private DAOServicio daoServicio;
    [SerializeField] private DAOIndicesAlquiler daoIndices;

    // ─────────────────────────────────────────────
    //  Scroll View AumentosAplicar
    // ─────────────────────────────────────────────

    [Header("Aumentos por Aplicar")]
    [SerializeField] private Transform contenedorAumentos;
    [SerializeField] private GameObject itemAumentoPrefab;

    // ─────────────────────────────────────────────
    //  Scroll View ContratosVencer
    // ─────────────────────────────────────────────

    [Header("Próximos Contratos a Vencer")]
    [SerializeField] private Transform contenedorContratosVencer;
    [SerializeField] private GameObject itemContratoVencerPrefab;

    // ─────────────────────────────────────────────
    //  Scroll View InfoGenerica (tabla resumen)
    // ─────────────────────────────────────────────

    [Header("Tabla Resumen (InfoGenerica)")]
    [SerializeField] private Transform contenedorInfoGenerica;
    [SerializeField] private GameObject itemResumenPrefab;
    [SerializeField] private GameObject separadorAlfabeticoPrefab;
    [SerializeField] private TMP_InputField buscadorGenericoInput;

    // ─────────────────────────────────────────────
    //  Campana Notificaciones (Desplegable)
    // ─────────────────────────────────────────────

    [Header("Notificaciones (Campana)")]
    [SerializeField] private Button botonCampanaNotificaciones;
    [SerializeField] private GameObject badgeNotificaciones;
    [SerializeField] private TMP_Text textoBadgeNotificaciones;
    [SerializeField] private GameObject panelNotificacionesDesplegable;
    [SerializeField] private Transform contenedorListaNotificaciones;
    [SerializeField] private GameObject itemNotificacionElegantePrefab;
    [SerializeField] private Button botonBorrarTodasNotificaciones;

    // ─────────────────────────────────────────────
    //  Contadores del menú principal
    // ─────────────────────────────────────────────

    [Header("Contadores Menú Principal")]
    [Tooltip("TMP_Text que muestra la cantidad total de inquilinos activos")]
    [SerializeField] private TMP_Text totalInquilinosText;

    // ─────────────────────────────────────────────
    //  Datos internos
    // ─────────────────────────────────────────────

    private List<FilaResumen> datosCompletos = new List<FilaResumen>();
    private List<NotificacionItem> listaNotificaciones = new List<NotificacionItem>();
    private HashSet<string> notificacionesEliminadas = new HashSet<string>();

    public class NotificacionItem
    {
        public string Id;
        public DateTime FechaReferencia;
        public string Titulo;
        public string Subtitulo;
        public string TiempoUrgencia;
        public bool EsPeligro;
    }

    private static readonly string[] MESES = {
        "", "ENERO", "FEBRERO", "MARZO", "ABRIL", "MAYO", "JUNIO",
        "JULIO", "AGOSTO", "SEPTIEMBRE", "OCTUBRE", "NOVIEMBRE", "DICIEMBRE"
    };

    private class FilaResumen
    {
        public long idContrato;
        public string ApellidoInquilino;
        public string NombreInquilino;
        public long TelefonoInquilino;
        public string ApellidoDueno;
        public string NombreDueno;
        public long idDueno;
        public string DireccionInmueble;
        public long MontoAlquiler;
        public long HonorarioPorcentaje;
        public long Comisiones;
        public string FechaInicio;
        public string FechaFin;
        public long MesesActualizacion;
        public string TipoIndice;
        public string ServiciosImpuestos; // Nombre y monto de servicios del propietario
    }

    // ─────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────

    private void Awake()
    {
        CargarNotificacionesEliminadas();

        if (daoContrato == null) daoContrato = FindObjectOfType<DAOContrato>();
        if (daoInquilino == null) daoInquilino = FindObjectOfType<DAOInquilino>();
        if (daoInmueble == null) daoInmueble = FindObjectOfType<DAOInmueble>();
        if (daoDueno == null) daoDueno = FindObjectOfType<DAODueno>();

        // DAOIndicesAlquiler: si no está asignado en el Inspector ni en la escena, se crea como componente local
        if (daoIndices == null) daoIndices = FindObjectOfType<DAOIndicesAlquiler>();
        if (daoIndices == null) daoIndices = gameObject.AddComponent<DAOIndicesAlquiler>();

        if (buscadorGenericoInput != null)
            buscadorGenericoInput.onValueChanged.AddListener((v) => RenderizarTablaResumen());

        if (botonCampanaNotificaciones != null)
            botonCampanaNotificaciones.onClick.AddListener(TogglePanelNotificaciones);

        if (panelNotificacionesDesplegable != null)
            panelNotificacionesDesplegable.SetActive(false);

        // Suscribirse al evento de clicks del Canvas para cerrar la campana al hacer click fuera
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var graphicRaycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Buscar botón genérico para borrar todas si no está asignado en el Inspector
        if (botonBorrarTodasNotificaciones == null && panelNotificacionesDesplegable != null)
        {
            Button[] btns = panelNotificacionesDesplegable.GetComponentsInChildren<Button>(true);
            foreach (Button btn in btns)
            {
                string nameLower = btn.gameObject.name.ToLower();
                if (nameLower.Contains("borrartodas") || nameLower.Contains("clearall") || nameLower.Contains("borrar_todas") || (nameLower.Contains("borrar") && nameLower.Contains("todas")) || nameLower.Contains("limpiar") || nameLower.Contains("clear"))
                {
                    botonBorrarTodasNotificaciones = btn;
                    break;
                }
            }
        }

        if (botonBorrarTodasNotificaciones != null)
            botonBorrarTodasNotificaciones.onClick.AddListener(BorrarTodasLasNotificaciones);

        // Suscribirse al evento de cambios en Servicios para refrescar el dashboard
        ServicioUI.OnServiciosCambiados += CargarDashboard;
        GlobalDropdownRefreshManager.OnAnyDataChanged += CargarDashboard;
    }

    private void OnDestroy()
    {
        ServicioUI.OnServiciosCambiados -= CargarDashboard;
        GlobalDropdownRefreshManager.OnAnyDataChanged -= CargarDashboard;
    }

    private void OnEnable()
    {
        CargarDashboard();
    }

    private void Update()
    {
        // Cerrar el panel de notificaciones si el usuario hace click fuera de él
        if (panelNotificacionesDesplegable != null && panelNotificacionesDesplegable.activeSelf)
        {
            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                Vector2 pointerPos = pointer.position.ReadValue();

                // Verificar si el click fue DENTRO del panel o DENTRO del botón de campana
                bool clickEnPanel = RectTransformUtility.RectangleContainsScreenPoint(
                    panelNotificacionesDesplegable.GetComponent<RectTransform>(),
                    pointerPos,
                    null);
                bool clickEnCampana = botonCampanaNotificaciones != null && RectTransformUtility.RectangleContainsScreenPoint(
                    botonCampanaNotificaciones.GetComponent<RectTransform>(),
                    pointerPos,
                    null);

                if (!clickEnPanel && !clickEnCampana)
                    panelNotificacionesDesplegable.SetActive(false);
            }
        }
    }

    // ═════════════════════════════════════════════
    //  DASHBOARD — Carga principal
    // ═════════════════════════════════════════════

    public void CargarDashboard()
    {
        if (_cargandoDashboard) return; // Evitar doble carga por múltiples eventos simultáneos
        _cargandoDashboard = true;

        _dashboardTimeout++;
        LimpiarContenedor(contenedorAumentos);
        LimpiarContenedor(contenedorContratosVencer);
        LimpiarContenedor(contenedorInfoGenerica);
        datosCompletos.Clear();
        listaNotificaciones.Clear();

        // Obtener cantidad total de inquilinos en base de datos
        if (daoInquilino != null && totalInquilinosText != null)
        {
            daoInquilino.ObtenerTodosLosInquilinos((exito, json, error) =>
            {
                if (exito)
                {
                    int total = ParsearCantidadInquilinos(json);
                    totalInquilinosText.text = total.ToString();
                }
                else
                {
                    Debug.LogError("[MenuPrincipalUI] Error al obtener total de inquilinos: " + error);
                }
            });
        }

        daoContrato.ObtenerContratosActivos((exito, json, error) =>
        {
            if (!exito) { _cargandoDashboard = false; return; }

            List<ContratoJsonItem> contratos = ParsearContratos(json);
            if (contratos.Count == 0) { _cargandoDashboard = false; return; }

            ResolverDatosContratos(contratos);
        });
    }

    private int _dashboardTimeout = 0;
    private bool _cargandoDashboard = false;
    private void ResolverDatosContratos(List<ContratoJsonItem> contratos)
    {
        int pendientes = contratos.Count;
        if (pendientes == 0) return;

        foreach (var contrato in contratos)
        {
            FilaResumen fila = new FilaResumen
            {
                idContrato = contrato.id_Contrato,
                MontoAlquiler = contrato.MontoAlquiler,
                HonorarioPorcentaje = contrato.HonorarioPorcentaje,
                Comisiones = contrato.HonorarioPorcentaje, // monto fijo en pesos (no porcentaje)
                FechaInicio = contrato.FechaInicio,
                FechaFin = contrato.FechaFIn ?? "",
                MesesActualizacion = contrato.MesesActualizacion,
                TipoIndice = contrato.TipoIndice ?? "",
                ServiciosImpuestos = ""
            };

            bool subCompletado = false;
            int subPendientes = 4; // inquilino + dueño + inmueble + servicios
            Action checkSub = () =>
            {
                if (subCompletado) return; // Evitar llamadas múltiples
                subPendientes--;
                if (subPendientes > 0) return;
                subCompletado = true;

                datosCompletos.Add(fila);
                pendientes--;

                if (pendientes == 0)
                {
                    _cargandoDashboard = false;
                    RenderizarTablaResumen();
                    RenderizarAumentos();
                    RenderizarContratosVencer();
                    ActualizarCampanaNotificaciones();
                }
            };

            int idTimeout = _dashboardTimeout;
            StartCoroutine(TimeoutSubDashboard(idTimeout, () =>
            {
                if (subCompletado) return;
                subCompletado = true;
                datosCompletos.Add(fila);
                pendientes--;
                if (pendientes == 0)
                {
                    _cargandoDashboard = false;
                    RenderizarTablaResumen();
                    RenderizarAumentos();
                    RenderizarContratosVencer();
                    ActualizarCampanaNotificaciones();
                }
            }));

            // Inquilino
            daoInquilino.ObtenerInquilinoPorId(contrato.id_Inquilino,
                (ok, nombre, apellido, tel, err) =>
                {
                    fila.ApellidoInquilino = ok ? (apellido ?? "") : "—";
                    fila.NombreInquilino = ok ? (nombre ?? "") : "";
                    fila.TelefonoInquilino = ok ? tel : 0;
                    checkSub();
                });

            // Dueño
            daoDueno.ObtenerDuenoPorId(contrato.id_Duenos,
                (ok, nombre, apellido, tel, err) =>
                {
                    fila.ApellidoDueno = ok ? (apellido ?? "") : "—";
                    fila.NombreDueno = ok ? (nombre ?? "") : "";
                    fila.idDueno = ok ? contrato.id_Duenos : 0;
                    checkSub();
                });

            // Inmueble
            daoInmueble.ObtenerInmueblePorId(contrato.id_Inmueble,
                (ok, jsonInm, err) =>
                {
                    if (ok)
                    {
                        InmuebleJsonItem inm = ParsearPrimerInmueble(jsonInm);
                        fila.DireccionInmueble = inm != null
                            ? inm.Direccion + " " + inm.Numero_Direccion : "—";
                    }
                    else fila.DireccionInmueble = "—";
                    checkSub();
                });

            // Servicios de esta propiedad específica (agua, luz, gas, municipal, etc.)
            // Se filtra por id_Inmueble del contrato para que cada fila muestre
            // solo los servicios de ESA dirección, no todos los del dueño.
            long idInmuebleLocal = contrato.id_Inmueble;
            if (daoServicio != null)
            {
                daoServicio.ObtenerServiciosPorPropiedad(idInmuebleLocal, (ok, jsonSrv, err) =>
                {
                    if (ok && !string.IsNullOrEmpty(jsonSrv) && jsonSrv.Trim() != "[]")
                        fila.ServiciosImpuestos = ParsearResumenServicios(jsonSrv);
                    else
                        fila.ServiciosImpuestos = "";
                    checkSub();
                });
            }
            else
            {
                fila.ServiciosImpuestos = "";
                checkSub();
            }
        }
    }

    private System.Collections.IEnumerator TimeoutSubDashboard(int idTimeout, Action onTimeout)
    {
        yield return new WaitForSeconds(5f);
        if (idTimeout == _dashboardTimeout) onTimeout?.Invoke();
    }

    // ═════════════════════════════════════════════
    //  TABLA RESUMEN (Scroll View InfoGenerica)
    // ═════════════════════════════════════════════

    private void RenderizarTablaResumen()
    {
        if (contenedorInfoGenerica == null) return;
        LimpiarContenedor(contenedorInfoGenerica);

        string filtro = buscadorGenericoInput != null ? buscadorGenericoInput.text.ToLower().Trim() : "";
        List<FilaResumen> lista = new List<FilaResumen>();

        foreach (var f in datosCompletos)
        {
            if (string.IsNullOrEmpty(filtro))
            {
                lista.Add(f);
                continue;
            }

            // RF-01.04: Búsqueda unificada — propietario, inquilino, dirección o número de contrato
            string apellidoDueno = NormalizarTexto(f.ApellidoDueno);
            string nombreDueno = NormalizarTexto(f.NombreDueno);
            string apellidoInquil = NormalizarTexto(f.ApellidoInquilino);
            string nombreInquil = NormalizarTexto(f.NombreInquilino);
            string direccion = NormalizarTexto(f.DireccionInmueble ?? "");
            string filtroNorm = NormalizarTexto(filtro);

            string anioContrato = "";
            if (TryParseFecha(f.FechaInicio, out DateTime stF)) anioContrato = stF.Year.ToString();
            string numeroContrato = $"ct-{anioContrato}-{f.idContrato:D4}";

            if ((apellidoDueno + " " + nombreDueno).Contains(filtroNorm) ||
                apellidoDueno.Contains(filtroNorm) ||
                (apellidoInquil + " " + nombreInquil).Contains(filtroNorm) ||
                apellidoInquil.Contains(filtroNorm) ||
                nombreInquil.Contains(filtroNorm) ||
                direccion.Contains(filtroNorm) ||
                numeroContrato.Contains(filtroNorm))
            {
                lista.Add(f);
            }
        }

        // Ordenar por Inquilino (apellido)
        lista.Sort((a, b) => string.Compare(
            a.ApellidoInquilino, b.ApellidoInquilino, StringComparison.OrdinalIgnoreCase));

        string letraActual = "";

        foreach (var fila in lista)
        {
            // Agrupar por Inquilino (letra inicial del apellido)
            string primeraLetra = !string.IsNullOrEmpty(fila.ApellidoInquilino)
                ? fila.ApellidoInquilino.Substring(0, 1).ToUpper() : "#";

            if (primeraLetra != letraActual)
            {
                letraActual = primeraLetra;
                if (separadorAlfabeticoPrefab != null)
                {
                    GameObject sep = Instantiate(separadorAlfabeticoPrefab, contenedorInfoGenerica);
                    TMP_Text[] t = sep.GetComponentsInChildren<TMP_Text>();
                    if (t.Length >= 1) t[0].text = $"\t\t\t\t\t--- {letraActual} ---";
                }
            }

            if (itemResumenPrefab == null) continue;

            GameObject item = Instantiate(itemResumenPrefab, contenedorInfoGenerica);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();

            // PRIMER TXT = DUEÑO, SEGUNDO = INQUILINO, TERCERO = PROPIEDAD
            if (textos.Length >= 1) textos[0].text = (fila.ApellidoDueno + "\n" + fila.NombreDueno).ToUpper();
            if (textos.Length >= 2) textos[1].text = (fila.ApellidoInquilino + " " + fila.NombreInquilino).ToUpper();
            if (textos.Length >= 3) textos[2].text = fila.DireccionInmueble.ToUpper();
            if (textos.Length >= 4) textos[3].text = "$" + fila.MontoAlquiler.ToString("N0");
            if (textos.Length >= 5) textos[4].text = fila.ServiciosImpuestos;
            if (textos.Length >= 6) textos[5].text = "$" + fila.Comisiones.ToString("N0");
            if (textos.Length >= 7)
            {
                string fechaFinFormateada = "—";
                if (!string.IsNullOrEmpty(fila.FechaFin) && TryParseFecha(fila.FechaFin, out DateTime fechaFin))
                    fechaFinFormateada = fechaFin.ToString("dd/MM/yyyy");
                textos[6].text = fechaFinFormateada;
            }

            GameObject linea = new GameObject("Separador");
            linea.transform.SetParent(item.transform, false);

            Image img = linea.AddComponent<Image>();
            img.color = new Color(0.46f, 0.03f, 0.13f, 1f); // tu color burdó #750821

            RectTransform rt = linea.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);   // pivot abajo al centro
            rt.offsetMin = new Vector2(5, -8);
            rt.offsetMax = new Vector2(-5, -6);      // 2px de alto
        }
    }

    // ═════════════════════════════════════════════
    //  AUMENTOS POR APLICAR (Visualmente actualizado)
    //  Columnas: Contrato, Inquilino, Mes de aumento, Importe actual, Nuevo importe, Variación
    // ═════════════════════════════════════════════

    private void RenderizarAumentos()
    {
        if (contenedorAumentos == null || itemAumentoPrefab == null) return;
        LimpiarContenedor(contenedorAumentos);

        DateTime hoy = DateTime.Now.Date;
        var aumentos = new List<(long id, string inq, long tel, DateTime fecha, long monto, int anioInicio, long mesesActualizacion, string tipoIndice)>();

        foreach (var fila in datosCompletos)
        {
            if (fila.MesesActualizacion <= 0 || string.IsNullOrEmpty(fila.FechaInicio)) continue;
            if (!TryParseFecha(fila.FechaInicio, out DateTime inicio)) continue;

            DateTime fin;
            if (!string.IsNullOrEmpty(fila.FechaFin) && TryParseFecha(fila.FechaFin, out DateTime fp))
                fin = fp;
            else fin = inicio.AddYears(3);

            string nombreInq = (fila.ApellidoInquilino + " " + fila.NombreInquilino).ToUpper().Trim();
            DateTime fechaAjuste = inicio.AddMonths((int)fila.MesesActualizacion);

            while (fechaAjuste <= fin)
            {
                if (fechaAjuste >= hoy && fechaAjuste <= hoy.AddMonths(1)) // Aumento un mes antes
                {
                    aumentos.Add((fila.idContrato, nombreInq, fila.TelefonoInquilino, fechaAjuste, fila.MontoAlquiler, inicio.Year, fila.MesesActualizacion, fila.TipoIndice));

                    string notiId = $"AUMENTO_{fila.idContrato}_{fechaAjuste:yyyyMMdd}";
                    if (!EsNotificacionEliminada(notiId) && !listaNotificaciones.Exists(n => n.Id == notiId))
                    {
                        listaNotificaciones.Add(new NotificacionItem
                        {
                            Id = notiId,
                            FechaReferencia = fechaAjuste,
                            Titulo = $"Alerta de Aumento (1 mes)",
                            Subtitulo = $"{nombreInq}",
                            TiempoUrgencia = $"el {fechaAjuste:dd/MM/yyyy}",
                            EsPeligro = false
                        });
                    }
                }

                fechaAjuste = fechaAjuste.AddMonths((int)fila.MesesActualizacion);
            }
        }

        aumentos.Sort((a, b) => a.fecha.CompareTo(b.fecha));

        foreach (var aum in aumentos)
        {
            GameObject item = Instantiate(itemAumentoPrefab, contenedorAumentos);
            TMP_Text[] t = item.GetComponentsInChildren<TMP_Text>();

            if (t.Length >= 1) t[0].text = aum.inq;
            if (t.Length >= 2) t[1].text = MESES[aum.fecha.Month] + " " + aum.fecha.Year;
            if (t.Length >= 3) t[2].text = "$ " + aum.monto.ToString("N0");

            // RF-04.03: Botón WhatsApp para aviso de aumento
            string inqNombre = aum.inq;
            long telInq = aum.tel;
            string mesAum = MESES[aum.fecha.Month] + " " + aum.fecha.Year;
            string montoNuevoStr = ""; // Se actualiza cuando la API del BCRA responde

            // Nuevo importe: se consulta la API del BCRA de forma asíncrona
            if (t.Length >= 4)
            {
                TMP_Text txtNuevo = t[3]; // referencia capturada para el callback
                txtNuevo.text = "<color=#888888>Consultando...</color>";

                if (daoIndices != null)
                {
                    // Fecha del ajuste ANTERIOR = fechaAjuste - MesesActualizacion
                    DateTime fechaAjusteAnterior = aum.fecha.AddMonths(-(int)aum.mesesActualizacion);

                    string tipoIdx    = aum.tipoIndice;
                    long   montoLocal = aum.monto;

                    daoIndices.CalcularNuevoImporte(
                        tipoIdx,
                        montoLocal,
                        fechaAjusteAnterior,
                        aum.fecha,
                        (ok, nuevoMonto, err) =>
                        {
                            if (txtNuevo == null) return; // item ya fue destruido

                            if (ok)
                            {
                                bool esCasaPropia = (err == "~CER");
                                string label = esCasaPropia
                                    ? $"<color=#27AE60>$ {nuevoMonto:N0} (~CER)</color>"
                                    : $"<color=#27AE60>$ {nuevoMonto:N0}</color>";
                                txtNuevo.text = label;

                                // Actualizar el monto para el mensaje de WhatsApp
                                montoNuevoStr = "$ " + nuevoMonto.ToString("N0");
                            }
                            else
                            {
                                txtNuevo.text = "<color=#888888>— (sin datos)</color>";
                            }
                        });
                }
                else
                {
                    txtNuevo.text = "<color=#888888>— (sin DAO)</color>";
                }
            }

            if (t.Length >= 5) t[4].text = $"<color=#27AE60>{aum.tipoIndice}</color>";

            Button[] botonesAum = item.GetComponentsInChildren<Button>();
            foreach (Button btn in botonesAum)
            {
                string nameLower = btn.gameObject.name.ToLower();
                if (nameLower.Contains("whatsapp") || nameLower.Contains("whatapp"))
                    btn.onClick.AddListener(() => WhatsAppHelper.EnviarAvisoPago(telInq, inqNombre, mesAum, montoNuevoStr));
            }
        }
    }

    // ═════════════════════════════════════════════
    //  PRÓXIMOS CONTRATOS A VENCER (Visualmente actualizado)
    //  Columnas: Inquilino, Inmueble, Contrato, Vencimiento, Días restantes
    // ═════════════════════════════════════════════

    private void RenderizarContratosVencer()
    {
        if (contenedorContratosVencer == null || itemContratoVencerPrefab == null) return;
        LimpiarContenedor(contenedorContratosVencer);

        DateTime hoy = DateTime.Now.Date;
        DateTime limite = hoy.AddMonths(2); // Vencimiento dos meses antes (RF-04.01)

        var porVencer = new List<(FilaResumen fila, DateTime fechaFin)>();

        foreach (var fila in datosCompletos)
        {
            if (string.IsNullOrEmpty(fila.FechaFin)) continue;
            if (!TryParseFecha(fila.FechaFin, out DateTime ff)) continue;

            if (ff >= hoy && ff <= limite)
            {
                porVencer.Add((fila, ff));

                int dias = (int)(ff - hoy).TotalDays;
                Debug.Log($"[Notificaciones] Contrato por vencer detectado: {fila.ApellidoInquilino} vence el {ff:dd/MM/yyyy}");

                string notiId = $"VENCIMIENTO_{fila.idContrato}_{ff:yyyyMMdd}";
                if (!EsNotificacionEliminada(notiId) && !listaNotificaciones.Exists(n => n.Id == notiId))
                {
                    listaNotificaciones.Add(new NotificacionItem
                    {
                        Id = notiId,
                        FechaReferencia = ff,
                        Titulo = $"Alerta de Vencimiento (2 meses)",
                        Subtitulo = $"{(fila.ApellidoInquilino + " " + fila.NombreInquilino).ToUpper()}",
                        TiempoUrgencia = dias <= 0 ? "¡Vence hoy!" : $"vence en {dias} días",
                        EsPeligro = dias <= 15
                    });
                }
            }
        }

        porVencer.Sort((a, b) => a.fechaFin.CompareTo(b.fechaFin));

        foreach (var cv in porVencer)
        {
            GameObject item = Instantiate(itemContratoVencerPrefab, contenedorContratosVencer);
            TMP_Text[] t = item.GetComponentsInChildren<TMP_Text>();

            int diasRestantes = (int)(cv.fechaFin - hoy).TotalDays;
            string colorDias = diasRestantes <= 15 ? "#E74C3C" : "#F39C12";

            string anioDisplay = "0000";
            if (TryParseFecha(cv.fila.FechaInicio, out DateTime st)) anioDisplay = st.Year.ToString();

            if (t.Length >= 1) t[0].text = (cv.fila.ApellidoInquilino + " " + cv.fila.NombreInquilino).ToUpper();
            if (t.Length >= 2) t[1].text = cv.fila.DireccionInmueble.ToUpper();
            if (t.Length >= 3) t[2].text = cv.fechaFin.ToString("dd/MM/yyyy");
            if (t.Length >= 4) t[3].text = $"<color={colorDias}>{diasRestantes} días</color>";

            // RF-04.03: Botón WhatsApp para aviso de vencimiento
            string nombreVenc = (cv.fila.ApellidoInquilino + " " + cv.fila.NombreInquilino).Trim();
            long telVenc = cv.fila.TelefonoInquilino;
            string fechaVencStr = cv.fechaFin.ToString("dd/MM/yyyy");
            Button[] botonesVenc = item.GetComponentsInChildren<Button>();
            foreach (Button btn in botonesVenc)
            {
                string nameLower = btn.gameObject.name.ToLower();
                if (nameLower.Contains("whatsapp") || nameLower.Contains("whatapp"))
                    btn.onClick.AddListener(() => WhatsAppHelper.EnviarAvisoVencimiento(telVenc, nombreVenc, fechaVencStr));
            }
        }
    }

    // ═════════════════════════════════════════════
    //  SISTEMA DE CAMPANA Y PANEL DESPLEGABLE
    // ═════════════════════════════════════════════

    private void ActualizarCampanaNotificaciones()
    {
        Debug.Log($"[Notificaciones] Total calculadas: {listaNotificaciones.Count}");

        if (badgeNotificaciones == null || textoBadgeNotificaciones == null)
        {
            Debug.LogWarning("[Notificaciones] ERROR: Falta asignar 'Badge Notificaciones' o 'Texto Badge' en el Inspector de Unity.");
            return;
        }

        int total = listaNotificaciones.Count;
        if (total > 0)
        {
            badgeNotificaciones.SetActive(true);
            textoBadgeNotificaciones.text = total > 9 ? "9+" : total.ToString();
        }
        else
        {
            badgeNotificaciones.SetActive(false);
        }

        RenderizarPanelDesplegable();
    }

    public void TogglePanelNotificaciones()
    {
        if (panelNotificacionesDesplegable != null)
        {
            panelNotificacionesDesplegable.SetActive(!panelNotificacionesDesplegable.activeSelf);
        }
    }

    private void RenderizarPanelDesplegable()
    {
        if (contenedorListaNotificaciones == null || itemNotificacionElegantePrefab == null)
        {
            Debug.LogWarning("[Notificaciones] ERROR: Falta asignar 'Contenedor Lista' o 'Prefab' en el Inspector.");
            return;
        }
        LimpiarContenedor(contenedorListaNotificaciones);

        listaNotificaciones.Sort((a, b) => a.FechaReferencia.CompareTo(b.FechaReferencia));

        foreach (var noti in listaNotificaciones)
        {
            GameObject item = Instantiate(itemNotificacionElegantePrefab, contenedorListaNotificaciones);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();

            // Suponemos que el prefab tiene 3 textos: [0] Titulo, [1] Subtitulo, [2] Fecha/Urgencia
            if (textos.Length >= 1) textos[0].text = noti.Titulo;
            if (textos.Length >= 2) textos[1].text = noti.Subtitulo;
            if (textos.Length >= 3) textos[2].text = $"<color={(noti.EsPeligro ? "#E74C3C" : "#888888")}>{noti.TiempoUrgencia}</color>";

            // Buscar si hay un botón para eliminar esta notificación individualmente
            Button[] botones = item.GetComponentsInChildren<Button>(true);
            foreach (Button btn in botones)
            {
                string nameLower = btn.gameObject.name.ToLower();
                if (nameLower.Contains("eliminar") || nameLower.Contains("borrar") || nameLower.Contains("cerrar") || nameLower.Contains("close") || nameLower == "x" || nameLower.Contains("delete"))
                {
                    string tempId = noti.Id;
                    btn.onClick.AddListener(() =>
                    {
                        EliminarNotificacionIndividual(tempId);
                    });
                }
            }
        }
    }

    private void CargarNotificacionesEliminadas()
    {
        notificacionesEliminadas.Clear();
        string guardadas = PlayerPrefs.GetString("NotificacionesEliminadas", "");
        if (!string.IsNullOrEmpty(guardadas))
        {
            string[] ids = guardadas.Split('|');
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    notificacionesEliminadas.Add(id);
            }
        }
    }

    private void GuardarNotificacionesEliminadas()
    {
        string stringificada = string.Join("|", notificacionesEliminadas);
        PlayerPrefs.SetString("NotificacionesEliminadas", stringificada);
        PlayerPrefs.Save();
    }

    private bool EsNotificacionEliminada(string id)
    {
        return notificacionesEliminadas.Contains(id);
    }

    private void RegistrarNotificacionEliminada(string id)
    {
        if (notificacionesEliminadas.Add(id))
        {
            GuardarNotificacionesEliminadas();
        }
    }

    private void EliminarNotificacionIndividual(string id)
    {
        RegistrarNotificacionEliminada(id);
        listaNotificaciones.RemoveAll(n => n.Id == id);
        ActualizarCampanaNotificaciones();
    }

    private void BorrarTodasLasNotificaciones()
    {
        foreach (var noti in listaNotificaciones)
        {
            if (!string.IsNullOrEmpty(noti.Id))
            {
                notificacionesEliminadas.Add(noti.Id);
            }
        }
        GuardarNotificacionesEliminadas();
        listaNotificaciones.Clear();
        ActualizarCampanaNotificaciones();
    }

    // ═════════════════════════════════════════════
    //  HELPERS
    // ─────────────────────────────────────────────

    // (Método GenerarTextosAjustes eliminado para separar Inquilino y Propiedad en la tabla resumen)

    /// <summary>Convierte a minúsculas y elimina tildes para búsqueda tolerante.</summary>
    private string NormalizarTexto(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        string lower = texto.ToLowerInvariant();
        // Reemplazar vocales con tilde por su equivalente sin tilde
        lower = lower.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
                     .Replace('ó', 'o').Replace('ú', 'u').Replace('ü', 'u')
                     .Replace('ñ', 'n');
        return lower;
    }

    private void LimpiarContenedor(Transform cont)
    {
        if (cont != null) foreach (Transform t in cont) Destroy(t.gameObject);
    }

    private string FormatearFechaLarga(string yyyymmdd)
    {
        if (string.IsNullOrEmpty(yyyymmdd)) return "—";
        if (!TryParseFecha(yyyymmdd, out DateTime fecha)) return yyyymmdd;
        string mes = (fecha.Month >= 1 && fecha.Month <= 12) ? MESES[fecha.Month] : "?";
        return $"{fecha.Day} DE {mes} DE {fecha.Year}";
    }

    private bool TryParseFecha(string fechaStr, out DateTime fecha)
    {
        fecha = DateTime.MinValue;
        if (string.IsNullOrEmpty(fechaStr)) return false;

        string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "yyyy/MM/dd" };
        if (DateTime.TryParseExact(fechaStr, formatos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fecha))
            return true;

        if (DateTime.TryParse(fechaStr, new System.Globalization.CultureInfo("en-GB"), System.Globalization.DateTimeStyles.None, out fecha))
            return true;

        return DateTime.TryParse(fechaStr, out fecha);
    }

    // ═════════════════════════════════════════════
    //  CLASES JSON
    // ─────────────────────────────────────────────

    [Serializable]
    private class ContratoJsonItem
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
    [Serializable] private class ContratoJsonArray { public ContratoJsonItem[] items; }

    [Serializable]
    private class InmuebleJsonItem
    {
        public long id_Propiedad;
        public string Direccion;
        public int Numero_Direccion;
    }
    [Serializable] private class InmuebleJsonArray { public InmuebleJsonItem[] items; }

    private List<ContratoJsonItem> ParsearContratos(string json)
    {
        var r = new List<ContratoJsonItem>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return r;
        try
        {
            var arr = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
            if (arr?.items != null) r.AddRange(arr.items);
        }
        catch (Exception ex) { Debug.LogError("[MenuPrincipalUI] Error JSON: " + ex.Message); }
        return r;
    }

    private InmuebleJsonItem ParsearPrimerInmueble(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return null;
        try
        {
            var arr = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
            return (arr?.items != null && arr.items.Length > 0) ? arr.items[0] : null;
        }
        catch { return null; }
    }

    [Serializable]
    private class ServicioJsonItem
    {
        public long id;
        public string Nombre_servicio;
        public double MontoTotal;
        public float PorcentajePagar;
        public long id_Propietario;
    }
    [Serializable] private class ServicioJsonArray { public ServicioJsonItem[] items; }

    /// <summary>
    /// Convierte la lista de servicios de un propietario en un resumen de texto
    /// con formato "NOMBRE $monto" por línea, igual al Excel original.
    /// </summary>
    private string ParsearResumenServicios(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return "";
        try
        {
            var arr = JsonUtility.FromJson<ServicioJsonArray>("{\"items\":" + json + "}");
            if (arr?.items == null || arr.items.Length == 0) return "";

            var sb = new System.Text.StringBuilder();
            foreach (var srv in arr.items)
            {
                if (sb.Length > 0) sb.Append("\n");
                // Calcula el monto que paga el propietario según el porcentaje
                double montoPropietario = srv.MontoTotal * (srv.PorcentajePagar / 100.0);
                string nombreCorto = srv.Nombre_servicio?.ToUpper() ?? "";
                sb.Append($"{nombreCorto} ${montoPropietario:N0}");
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            Debug.LogError("[MenuPrincipalUI] Error al parsear servicios: " + ex.Message);
            return "";
        }
    }

    [Serializable]
    private class InquilinoCountItem
    {
        public long id_Inquilino;
    }
    [Serializable]
    private class InquilinoCountArray
    {
        public InquilinoCountItem[] items;
    }

    private int ParsearCantidadInquilinos(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return 0;
        try
        {
            var arr = JsonUtility.FromJson<InquilinoCountArray>("{\"items\":" + json + "}");
            return arr?.items != null ? arr.items.Length : 0;
        }
        catch { return 0; }
    }
}