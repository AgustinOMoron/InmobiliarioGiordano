using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Maneja la interfaz de usuario para la gestión de Recibos.
///
/// ═══════════════════════════════════════════════════════════════
///  GUÍA PARA ARMAR LA ESCENA EN UNITY (DISEÑO DASHBOARD)
/// ═══════════════════════════════════════════════════════════════
///
///  Canvas
///  └── Panel_Recibos                          ← Contenedor raíz
///      ├── Lado_Izquierdo_Lista               ← ~55% de la pantalla
///      │   ├── Filtros_Arriba
///      │   │   ├── InputField_FechaDesde      ← fechaDesdeInput  (ej: "01/01/2024")
///      │   │   ├── InputField_FechaHasta      ← fechaHastaInput  (ej: "31/12/2024")
///      │   │   ├── Button_Filtrar             ← filtrarPeriodoBtn
///      │   │   └── Button_VerTodos            ← verTodosBtn
///      │   ├── ScrollView → Content           ← contenedorLista
///      │   └── Text_MensajeLista              ← mensajeListaText
///      │
///      ├── Lado_Derecho_Formulario            ← ~45% de la pantalla
///      │   ├── Text_TituloFormulario          ← tituloFormularioText
///      │   ├── InputField_Fecha               ← fechaInput       (ej: "15/03/2024")
///      │   ├── InputField_Monto               ← montoInput       (Integer Number)
///      │   ├── InputField_TotalAbonar         ← totalAbonarInput (Integer Number)
///      │   ├── Dropdown_Tipo                  ← tipoDropdown
///      │   │     Opciones: "-- Seleccionar Tipo --", "Alquiler", "Expensas", "Otros"
///      │   ├── Dropdown_Contrato              ← contratoDropdown
///      │   │     (se carga automático con todos los contratos)
///      │   ├── Text_MensajeFormulario         ← mensajeFormularioText
///      │   ├── Button_Guardar                 ← guardarBtn
///      │   └── Button_LimpiarFormulario       ← limpiarFormularioBtn
///      │
///      └── Panel_ConfirmacionEliminar         ← panelConfirmacionEliminar (POPUP)
///          ├── Text_MensajeConfirmacion       ← mensajeConfirmacionText
///          ├── Button_ConfirmarEliminar       ← confirmarEliminarBtn
///          └── Button_CancelarEliminar        ← cancelarEliminarBtn
///
///  PREFAB ÍTEM DE LISTA (ItemRecibo):
///      ├── Text_NumeroRecibo                  ← textos[0]  "# {id}"
///      ├── Text_Fecha                         ← textos[1]  "DD/MM/YYYY"
///      ├── Text_Contrato                      ← textos[2]  "Contrato #{idContrato}"
///      ├── Text_Monto                         ← textos[3]  "$ {monto:N0}"
///      ├── Text_TotalAbonar                   ← textos[4]  "$ {totalAbonar:N0}"
///      ├── Text_Tipo                          ← textos[5]  "Alquiler / Expensas / Otros"
///      ├── Button_Editar                      ← nombre debe contener "editar"
///      ├── Button_Eliminar                    ← nombre debe contener "eliminar"
///      └── Button_PDF                         ← nombre debe contener "pdf" o "imprimir"
///
/// ═══════════════════════════════════════════════════════════════
/// </summary>
public class ReciboUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Referencias a los DAOs
    // ─────────────────────────────────────────────

    [Header("DAOs")]
    [SerializeField] private DAORecibo daoRecibo;
    [SerializeField] private DAOContrato daoContrato;
    [SerializeField] private DAOInquilino daoInquilino;
    [SerializeField] private DAOInmueble daoInmueble;
    [SerializeField] private DAOServicio daoServicio;

    // ─────────────────────────────────────────────
    //  Popup Confirmación Eliminar
    // ─────────────────────────────────────────────

    [Header("Popup Confirmación")]
    [SerializeField] private GameObject panelConfirmacionEliminar;
    [SerializeField] private TMP_Text mensajeConfirmacionText;
    [SerializeField] private Button confirmarEliminarBtn;
    [SerializeField] private Button cancelarEliminarBtn;

    // ─────────────────────────────────────────────
    //  Panel Izquierdo — Lista y Filtros
    // ─────────────────────────────────────────────

    [Header("Panel Izquierdo - Lista")]
    [SerializeField] private Transform contenedorLista;
    [SerializeField] private GameObject itemReciboPrefab;
    [SerializeField] private TMP_Text mensajeListaText;
    [Tooltip("Campo de búsqueda por nombre/apellido de inquilino")]
    [SerializeField] private TMP_InputField buscadorInquilinoInput;
    [SerializeField] private Button buscarPorInquilinoBtn;
    [SerializeField] private Button limpiarBusquedaBtn;

    // ─────────────────────────────────────────────
    //  Panel Derecho — Formulario
    // ─────────────────────────────────────────────

    [Header("Panel Derecho - Formulario")]
    [SerializeField] private TMP_Text tituloFormularioText;

    [Tooltip("Fecha del recibo en formato DD/MM/YYYY (ej: 15/03/2024)")]
    [SerializeField] private TMP_InputField fechaInput;

    [Tooltip("Monto base del recibo")]
    [SerializeField] private TMP_InputField montoInput;

    [Tooltip("Total a abonar (puede incluir recargos, etc.)")]
    [SerializeField] private TMP_InputField totalAbonarInput;

    [Tooltip("Tipo de recibo: Alquiler, Expensas, Otros")]
    [SerializeField] private TMP_Dropdown tipoDropdown;

    [Tooltip("Medio de pago: Efectivo, Transferencia o ambos")]
    [SerializeField] private TMP_Dropdown tipoPagoDropdown;

    [Tooltip("Se carga automáticamente con todos los contratos (índice 0 = placeholder)")]
    [SerializeField] private TMP_Dropdown contratoDropdown;

    [SerializeField] private TMP_Text mensajeFormularioText;
    [SerializeField] private Button guardarBtn;
    [SerializeField] private Button limpiarFormularioBtn;

    [Header("Sección Servicios Asociados (dinámicos)")]
    [Tooltip("Contenedor ScrollView donde se instancian los renglones de servicio")]
    [SerializeField] private Transform contenedorServicios;
    [Tooltip("Prefab del renglón: TMP_InputField[0]=Nombre, [1]=Fecha, [2]=Monto, [3]=Porcentaje + Button con 'eliminar'")]
    [SerializeField] private GameObject servicioItemPrefab;
    [Tooltip("Botón '+' para agregar un nuevo renglón de servicio")]
    [SerializeField] private Button agregarServicioBtn;

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private long idReciboSeleccionado = -1;
    private bool modoEdicion = false;

    // IDs del inmueble y propietario del contrato seleccionado (para guardar los servicios)
    private long _contratoInmuebleId    = 0;
    private long _contratoPropietarioId = 0;

    // Lista dinámica de renglones de servicio: (nombre, fecha, monto, porcentaje)
    private List<(TMP_InputField nombre, TMP_InputField fecha, TMP_InputField monto, TMP_InputField porcentaje)>
        _renglones = new List<(TMP_InputField, TMP_InputField, TMP_InputField, TMP_InputField)>();

    // Lista para mapear índice del dropdown → ID real del contrato
    private List<long> idsContratos = new List<long>();
    // Mapa de id_Contrato → nombre del inquilino (para búsqueda y display)
    private Dictionary<long, string> nombreInquilinoPorContrato = new Dictionary<long, string>();
    // Lista completa de recibos cargados (para filtrar localmente por inquilino)
    private List<ReciboItemData> _todosLosRecibos = new List<ReciboItemData>();

    // ─────────────────────────────────────────────
    //  Tipos de recibo (mapeo índice → valor BD)
    //  Índice 0: Placeholder
    //  Índice 1: Alquiler  → valor 1
    //  Índice 2: Expensas  → valor 2
    //  Índice 3: Otros     → valor 3
    //
    //  Tipos de pago (tipoPagoDropdown)
    //  Índice 0: Placeholder
    //  Índice 1: EFECTIVO
    //  Índice 2: TRANSFERENCIA
    //  Índice 3: EFEC/TRANSF
    // ─────────────────────────────────────────────

    // ─────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (daoRecibo == null)   daoRecibo   = FindObjectOfType<DAORecibo>();
        if (daoContrato == null)  daoContrato = FindObjectOfType<DAOContrato>();
        if (daoInquilino == null) daoInquilino= FindObjectOfType<DAOInquilino>();
        if (daoInmueble == null)  daoInmueble = FindObjectOfType<DAOInmueble>();
        if (daoServicio == null)  daoServicio = FindObjectOfType<DAOServicio>();



        // Listeners Formulario
        if (guardarBtn != null) guardarBtn.onClick.AddListener(OnGuardar);
        if (limpiarFormularioBtn != null) limpiarFormularioBtn.onClick.AddListener(PrepararFormularioNuevo);

        // Listeners Popup
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.onClick.AddListener(OnConfirmarEliminar);
        if (cancelarEliminarBtn != null) cancelarEliminarBtn.onClick.AddListener(CerrarPopupEliminar);

        // Listener al seleccionar contrato → auto-llenar monto (RF-02.02)
        if (contratoDropdown != null) contratoDropdown.onValueChanged.AddListener(OnContratoSeleccionado);

        // Listener cálculo automático del total a abonar (RF-02.02)
        if (montoInput != null) montoInput.onValueChanged.AddListener((s) => RecalcularTotal());

        // Listener buscador por inquilino
        if (buscarPorInquilinoBtn != null) buscarPorInquilinoBtn.onClick.AddListener(OnBuscarPorInquilino);
        if (buscadorInquilinoInput != null) buscadorInquilinoInput.onSubmit.AddListener(_ => OnBuscarPorInquilino());
        if (limpiarBusquedaBtn != null) limpiarBusquedaBtn.onClick.AddListener(OnLimpiarBusqueda);

        // Listener para agregar renglón de servicio dinámico
        if (agregarServicioBtn != null) agregarServicioBtn.onClick.AddListener(AgregarRenglonServicio);

        // Configurar dropdowns fijos (no vienen de la BD)
        ConfigurarDropdownTipo();
        ConfigurarDropdownTipoPago();
    }

    private void OnEnable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged += RefrescarDropdowns;
        CerrarPopupEliminar();
        
        MostrarMensajeLista("Cargando...", Color.gray);
        CargarMapInquilinos(() =>
        {
            CargarDropdownContratos(() =>
            {
                PrepararFormularioNuevo();
                CargarTodosLosRecibos();
            });
        });
    }

    private void OnDisable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged -= RefrescarDropdowns;
    }

    public void RefrescarDropdowns()
    {
        long idRecuperarContrato = 0;
        if (contratoDropdown != null && contratoDropdown.value > 0 && contratoDropdown.value - 1 < idsContratos.Count)
            idRecuperarContrato = idsContratos[contratoDropdown.value - 1];

        CargarMapInquilinos(() =>
        {
            CargarDropdownContratos(() =>
            {
                if (idRecuperarContrato > 0)
                {
                    int idx = idsContratos.IndexOf(idRecuperarContrato);
                    if (idx >= 0 && contratoDropdown != null)
                    {
                        contratoDropdown.value = idx + 1;
                        contratoDropdown.RefreshShownValue();
                    }
                }
            });
        });
    }

    // ─────────────────────────────────────────────
    //  Configuración del Dropdown de Tipo (fijo)
    // ─────────────────────────────────────────────

    private void ConfigurarDropdownTipo()
    {
        if (tipoDropdown == null) return;
        tipoDropdown.ClearOptions();
        tipoDropdown.AddOptions(new List<string>
        {
            "-- Seleccionar Tipo --",
            "Alquiler",
            "Expensas",
            "Otros"
        });
    }

    private void ConfigurarDropdownTipoPago()
    {
        if (tipoPagoDropdown == null) return;
        tipoPagoDropdown.ClearOptions();
        tipoPagoDropdown.AddOptions(new List<string>
        {
            "-- Tipo de Pago --",
            "EFECTIVO",
            "TRANSFERENCIA",
            "EFEC/TRANSF"
        });
    }

    // ─────────────────────────────────────────────
    //  Carga del Dropdown de Contratos (desde BD)
    // ─────────────────────────────────────────────

    // Carga el mapa de inquilinos Y el dropdown de contratos en un único GET de contratos.
    // Antes se hacían dos GET separados al mismo endpoint, lo que causaba el error 400
    // y podía generar entradas duplicadas en el dropdown.
    private void CargarMapInquilinos(Action onDone = null)
    {
        // Solo cargamos el mapa; el dropdown se rellena en CargarDropdownContratos
        // que reutiliza los datos ya en nombreInquilinoPorContrato.
        // Esta función se llama primero para tener los nombres listos.
        onDone?.Invoke();
    }

    private void CargarDropdownContratos(Action onDone = null)
    {
        idsContratos.Clear();
        nombreInquilinoPorContrato.Clear();
        if (contratoDropdown != null) contratoDropdown.ClearOptions();

        var opciones = new List<string> { "-- Seleccionar Contrato --" };

        // Un solo GET de contratos
        daoContrato.ObtenerTodosLosContratos((exitoC, jsonC, errorC) =>
        {
            if (!exitoC || string.IsNullOrEmpty(jsonC) || jsonC.Trim() == "[]")
            {
                if (contratoDropdown != null) contratoDropdown.AddOptions(opciones);
                onDone?.Invoke();
                return;
            }

            ContratoJsonArray arr;
            try { arr = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + jsonC + "}"); }
            catch (Exception ex)
            {
                Debug.LogError("[ReciboUI] Error parsear contratos: " + ex.Message);
                if (contratoDropdown != null) contratoDropdown.AddOptions(opciones);
                onDone?.Invoke();
                return;
            }

            if (arr?.items == null || arr.items.Length == 0)
            {
                if (contratoDropdown != null) contratoDropdown.AddOptions(opciones);
                onDone?.Invoke();
                return;
            }

            // Cargar IDs de contratos
            foreach (var c in arr.items) idsContratos.Add(c.id_contrato);

            // Obtener inquilinos para construir los nombres del dropdown
            daoInquilino.ObtenerTodosLosInquilinos((exitoI, jsonI, errorI) =>
            {
                // Construir mapa inquilino
                if (exitoI && !string.IsNullOrEmpty(jsonI) && jsonI.Trim() != "[]")
                {
                    var listaInquilinos = ParsearListaInquilinos(jsonI);
                    var mapaInquilinos = new Dictionary<long, string>();
                    foreach (var i in listaInquilinos)
                        mapaInquilinos[i.id_Inquilino] = $"{i.Apellido_Inquilinos}, {i.Nombre_Inquilinos}";

                    foreach (var c in arr.items)
                    {
                        nombreInquilinoPorContrato[c.id_contrato] = mapaInquilinos.ContainsKey(c.id_Inquilino)
                            ? mapaInquilinos[c.id_Inquilino]
                            : $"Inquilino #{c.id_Inquilino}";
                    }
                }
                else
                {
                    foreach (var c in arr.items)
                        nombreInquilinoPorContrato[c.id_contrato] = $"Inquilino #{c.id_Inquilino}";
                }

                // Construir opciones del dropdown
                var opcionesFinales = new List<string> { "-- Seleccionar Contrato --" };
                foreach (var ct in arr.items)
                {
                    string nomInq = nombreInquilinoPorContrato.ContainsKey(ct.id_contrato)
                        ? nombreInquilinoPorContrato[ct.id_contrato]
                        : $"Inquilino #{ct.id_Inquilino}";
                    opcionesFinales.Add($"{nomInq}  |  $ {ct.MontoAlquiler:N0}");
                }

                if (contratoDropdown != null)
                {
                    contratoDropdown.ClearOptions();
                    contratoDropdown.AddOptions(opcionesFinales);
                }

                onDone?.Invoke();
            });
        });
    }

    // ═════════════════════════════════════════════
    //  FORMULARIO
    // ═════════════════════════════════════════════

    public void PrepararFormularioNuevo()
    {
        if (this == null || gameObject == null) return;
        modoEdicion = false;
        idReciboSeleccionado = -1;
        _contratoInmuebleId    = 0;
        _contratoPropietarioId = 0;

        if (fechaInput        != null) fechaInput.text        = "";
        if (montoInput        != null) montoInput.text        = "";
        if (totalAbonarInput  != null) totalAbonarInput.text  = "";
        if (tipoDropdown      != null) tipoDropdown.value     = 0;
        if (tipoPagoDropdown  != null) tipoPagoDropdown.value = 0;
        if (contratoDropdown  != null) contratoDropdown.value = 0;

        // Limpiar renglones y agregar uno vacío por defecto
        LimpiarRenglones();
        AgregarRenglonServicio();

        MostrarMensajeFormulario("", Color.white);

        if (tituloFormularioText != null)
            tituloFormularioText.text = "Nuevo Recibo";
    }

    // ═════════════════════════════════════════════
    //  RENGLONES DE SERVICIO DINÁMICOS
    // ═════════════════════════════════════════════

    /// <summary>
    /// Agrega un renglón de servicio al formulario.
    /// El prefab debe tener 4 TMP_InputField (nombre, fecha, monto, porcentaje)
    /// y un Button con "eliminar" o "borrar" en el nombre.
    /// </summary>
    private void AgregarRenglonServicio()
    {
        if (contenedorServicios == null || servicioItemPrefab == null) return;

        GameObject renglon = Instantiate(servicioItemPrefab, contenedorServicios);
        TMP_InputField[] inputs = renglon.GetComponentsInChildren<TMP_InputField>(true);

        TMP_InputField inputNombre     = inputs.Length > 0 ? inputs[0] : null;
        TMP_InputField inputFecha      = inputs.Length > 1 ? inputs[1] : null;
        TMP_InputField inputMonto      = inputs.Length > 2 ? inputs[2] : null;
        TMP_InputField inputPorcentaje = inputs.Length > 3 ? inputs[3] : null;

        // El monto de cada renglón actualiza el total
        if (inputMonto != null)
            inputMonto.onValueChanged.AddListener((s) => RecalcularTotal());

        var tupla = (inputNombre, inputFecha, inputMonto, inputPorcentaje);
        _renglones.Add(tupla);

        // Botón de eliminar este renglón
        foreach (Button btn in renglon.GetComponentsInChildren<Button>(true))
        {
            string n = btn.gameObject.name.ToLowerInvariant();
            if (n.Contains("eliminar") || n.Contains("borrar") || n.Contains("quitar"))
            {
                GameObject renglonRef = renglon;
                var tuplaRef = tupla;
                btn.onClick.AddListener(() =>
                {
                    _renglones.Remove(tuplaRef);
                    Destroy(renglonRef);
                    RecalcularTotal();
                });
            }
        }
    }

    private void LimpiarRenglones()
    {
        if (contenedorServicios != null)
            foreach (Transform hijo in contenedorServicios) Destroy(hijo.gameObject);
        _renglones.Clear();
    }

    /// <summary>Suma los montos de todos los renglones de servicio.</summary>
    private long ObtenerTotalServiciosMonto()
    {
        long total = 0;
        foreach (var r in _renglones)
        {
            if (r.monto != null && long.TryParse(r.monto.text.Trim(), out long v) && v > 0)
                total += v;
        }
        return total;
    }


    // ═════════════════════════════════════════════
    //  AUTO-LLENAR MONTO AL SELECCIONAR CONTRATO (RF-02.02)
    // ═════════════════════════════════════════════

    // ═════════════════════════════════════════════
    //  AUTO-LLENAR MONTO AL SELECCIONAR CONTRATO (RF-02.02)
    //  También captura id_Inmueble y el propietario del inmueble
    //  para poder guardar el servicio asociado después.
    // ═════════════════════════════════════════════

    private void CargarDatosContratoParaServicios(long idContrato)
    {
        _contratoInmuebleId = 0;
        _contratoPropietarioId = 0;

        if (idContrato <= 0) return;

        daoContrato.ObtenerContratoPorId(idContrato, (exito, json, error) =>
        {
            if (!exito) return;
            try
            {
                ContratoJsonArray arr = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
                if (arr?.items == null || arr.items.Length == 0) return;

                var c = arr.items[0];
                _contratoInmuebleId = c.id_Inmueble;

                if (_contratoInmuebleId > 0 && daoInmueble != null)
                {
                    daoInmueble.ObtenerInmueblePorId(_contratoInmuebleId, (exI, jsonI, errI) =>
                    {
                        if (!exI) return;
                        try
                        {
                            var inm = ParsearPrimerItem<InmuebleJsonItem>(jsonI);
                            if (inm != null) _contratoPropietarioId = inm.id_Duenos;
                            Debug.Log($"[ReciboUI] CargarDatosContratoParaServicios → id_Inmueble={_contratoInmuebleId}, id_Propietario={_contratoPropietarioId}");
                        }
                        catch (Exception ex) { Debug.LogWarning("[ReciboUI] Error al leer propietario: " + ex.Message); }
                    });
                }
            }
            catch (Exception ex) { Debug.LogError("[ReciboUI] Error al cargar contrato para servicios: " + ex.Message); }
        });
    }

    private void OnContratoSeleccionado(int index)
    {
        _contratoInmuebleId    = 0;
        _contratoPropietarioId = 0;

        if (index <= 0 || index - 1 >= idsContratos.Count) return;

        long idContrato = idsContratos[index - 1];

        daoContrato.ObtenerContratoPorId(idContrato, (exito, json, error) =>
        {
            if (!exito) return;
            try
            {
                ContratoJsonArray arr = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
                if (arr?.items == null || arr.items.Length == 0) return;

                var c = arr.items[0];
                if (montoInput != null) montoInput.text = c.MontoAlquiler.ToString();
                RecalcularTotal();
            }
            catch (Exception ex) { Debug.LogError("[ReciboUI] Error al cargar contrato: " + ex.Message); }
        });

        CargarDatosContratoParaServicios(idContrato);
    }

    // ═════════════════════════════════════════════
    //  CÁLCULO AUTOMÁTICO DEL TOTAL (RF-02.02)
    // ═════════════════════════════════════════════

    private void RecalcularTotal()
    {
        if (!long.TryParse(montoInput?.text.Trim(), out long monto) || monto < 0) monto = 0;
        long totalServicios = ObtenerTotalServiciosMonto();
        long total = monto + totalServicios;
        if (totalAbonarInput != null) totalAbonarInput.text = total.ToString();
    }

    public void GenerarPDFRecibo(long idRecibo)
    {
        if (idRecibo <= 0)
        {
            MostrarMensajeLista("ID de recibo inválido para generar PDF.", Color.red);
            return;
        }

        PrepararDatosParaPDF(idRecibo);
    }

    public void GenerarPDFRecibo(string idRecibo)
    {
        if (!long.TryParse(idRecibo, out long id) || id <= 0)
        {
            MostrarMensajeLista("ID de recibo inválido para generar PDF.", Color.red);
            return;
        }

        PrepararDatosParaPDF(id);
    }

    public void AbrirEdicion(long idRecibo)
    {
        modoEdicion = true;
        idReciboSeleccionado = idRecibo;

        if (tituloFormularioText != null)
            tituloFormularioText.text = "Editar Recibo";

        MostrarMensajeFormulario("Cargando datos...", Color.gray);

        daoRecibo.ObtenerReciboPorId(idRecibo, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeFormulario("Error: " + error, Color.red); return; }

            try
            {
                ReciboJsonArray arr = JsonUtility.FromJson<ReciboJsonArray>("{\"items\":" + json + "}");
                if (arr?.items == null || arr.items.Length == 0)
                { MostrarMensajeFormulario("Error al procesar los datos.", Color.red); return; }

                ReciboJsonItem r = arr.items[0];

                if (fechaInput != null) fechaInput.text = FormatoArgentino(r.Fecha);
                if (montoInput != null) montoInput.text = r.Monto.ToString();
                if (totalAbonarInput != null) totalAbonarInput.text = r.Total_Abonar.ToString();

                // Tipo: el índice en el dropdown es igual al valor (1=Alquiler, 2=Expensas, 3=Otros)
                if (tipoDropdown != null)
                    tipoDropdown.value = (r.Tipo >= 1 && r.Tipo <= 3) ? (int)r.Tipo : 0;

                // Tipo Pago: 1=Efectivo, 2=Transferencia, 3=Efec/Transf
                if (tipoPagoDropdown != null)
                    tipoPagoDropdown.value = (r.Tipo_Pago >= 1 && r.Tipo_Pago <= 3) ? (int)r.Tipo_Pago : 0;

                // Contrato: +1 por el placeholder en índice 0
                if (contratoDropdown != null)
                {
                    int idx = idsContratos.IndexOf(r.id_Contrato);
                    contratoDropdown.value = idx >= 0 ? idx + 1 : 0;
                }

                // Cargar explícitamente los datos del contrato para obtener inmueble y dueño,
                // asegurando que estén listos para guardar cualquier servicio asociado.
                CargarDatosContratoParaServicios(r.id_Contrato);

                // ── Cargar servicios asociados al recibo en el scroll ──
                CargarServiciosDelRecibo(idRecibo);

                MostrarMensajeFormulario("", Color.white);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ReciboUI] Error al cargar recibo: " + ex.Message);
                MostrarMensajeFormulario("Error interno al cargar datos.", Color.red);
            }
        });
    }

    // ─────────────────────────────────────────────
    //  Carga los servicios vinculados a un recibo y los muestra en el scroll
    // ─────────────────────────────────────────────
    [Serializable] private class ServicioEdicionItem
    {
        public long id;
        public string Nombre_servicio;
        public float MontoTotal;
        public string FechaServicio;
        public float PorcentajePagar;
    }
    [Serializable] private class ServicioEdicionArray { public ServicioEdicionItem[] items; }

    private void CargarServiciosDelRecibo(long idRecibo)
    {
        LimpiarRenglones();

        if (daoServicio == null) { AgregarRenglonServicio(); return; }

        daoServicio.ObtenerServiciosPorRecibo(idRecibo, (exito, json, error) =>
        {
            if (!exito || string.IsNullOrEmpty(json) || json.Trim() == "[]")
            {
                // Sin servicios → poner un renglón vacío
                AgregarRenglonServicio();
                return;
            }

            try
            {
                var arr = JsonUtility.FromJson<ServicioEdicionArray>("{\"items\":" + json + "}");
                if (arr?.items == null || arr.items.Length == 0)
                {
                    AgregarRenglonServicio();
                    return;
                }

                foreach (var s in arr.items)
                {
                    AgregarRenglonServicio();
                    var ultimo = _renglones[_renglones.Count - 1];
                    if (ultimo.nombre     != null) ultimo.nombre.text     = s.Nombre_servicio ?? "";
                    if (ultimo.fecha      != null) ultimo.fecha.text      = FormatoArgentino(s.FechaServicio);
                    if (ultimo.monto      != null) ultimo.monto.text      = s.MontoTotal.ToString("0");
                    if (ultimo.porcentaje != null) ultimo.porcentaje.text = s.PorcentajePagar.ToString("0");
                }

                RecalcularTotal();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ReciboUI] Error al parsear servicios para edición: " + ex.Message);
                AgregarRenglonServicio();
            }
        });
    }

    // ═════════════════════════════════════════════
    //  GUARDAR
    // ═════════════════════════════════════════════

    private void OnGuardar()
    {
        string fecha = fechaInput != null ? fechaInput.text.Trim() : "";

        if (string.IsNullOrEmpty(fecha))
        { MostrarMensajeFormulario("La fecha es obligatoria. (ej: 15/03/2024)", Color.red); return; }

        string fechaSupabase = FormatoSupabase(fecha);
        if (string.IsNullOrEmpty(fechaSupabase))
        { MostrarMensajeFormulario("Formato de fecha inválido. Usá DD/MM/YYYY.", Color.red); return; }

        if (!long.TryParse(montoInput?.text.Trim(), out long monto) || monto < 0)
        { MostrarMensajeFormulario("El monto debe ser un número válido.", Color.red); return; }

        if (!long.TryParse(totalAbonarInput?.text.Trim(), out long totalAbonar) || totalAbonar < 0)
        { MostrarMensajeFormulario("El total a abonar debe ser un número válido.", Color.red); return; }

        int idxTipo = tipoDropdown != null ? tipoDropdown.value : 0;
        if (idxTipo == 0)
        { MostrarMensajeFormulario("Seleccioná el tipo de recibo.", Color.red); return; }

        int idxTipoPago = tipoPagoDropdown != null ? tipoPagoDropdown.value : 0;
        if (idxTipoPago == 0)
        { MostrarMensajeFormulario("Seleccioná el tipo de pago.", Color.red); return; }

        int idxContrato = contratoDropdown != null ? contratoDropdown.value : 0;
        if (idxContrato == 0)
        { MostrarMensajeFormulario("Seleccioná un contrato.", Color.red); return; }

        long tipo = idxTipo;
        long tipoPago = idxTipoPago;
        long idContrato = idsContratos.Count > 0 && idxContrato - 1 < idsContratos.Count ? idsContratos[idxContrato - 1] : -1;

        if (idContrato <= 0)
        { MostrarMensajeFormulario("Error al obtener el contrato. Intentá de nuevo.", Color.red); return; }

        if (guardarBtn != null) guardarBtn.interactable = false;
        MostrarMensajeFormulario("Guardando...", Color.gray);

        if (modoEdicion)
        {
            daoRecibo.ActualizarRecibo(idReciboSeleccionado, fechaSupabase, monto, totalAbonar, tipo, tipoPago, idContrato, (exito, error) =>
            {
                if (guardarBtn != null) guardarBtn.interactable = true;
                if (exito)
                {
                    MostrarMensajeFormulario("✓ Recibo actualizado.", Color.green);
                    CargarTodosLosRecibos();

                    // Borrar servicios viejos y guardar los nuevos con el id del recibo
                    if (daoServicio != null)
                    {
                        daoServicio.EliminarServiciosPorRecibo(idReciboSeleccionado, (exSvc, errSvc) =>
                        {
                            GuardarServicioAsociado(idReciboSeleccionado, () =>
                            {
                                GlobalDropdownRefreshManager.NotifyDataChanged();
                            });
                        });
                    }
                    else
                    {
                        GlobalDropdownRefreshManager.NotifyDataChanged();
                    }

                    Invoke(nameof(PrepararFormularioNuevo), 1.5f);
                }
                else MostrarMensajeFormulario("Error: " + error, Color.red);
            });
        }
        else
        {
            daoRecibo.RegistrarRecibo(fechaSupabase, monto, totalAbonar, tipo, tipoPago, idContrato, (exito, idReciboCreado, error) =>
            {
                if (guardarBtn != null) guardarBtn.interactable = true;
                if (exito)
                {
                    MostrarMensajeFormulario("Recibo registrado.", Color.green);
                    CargarTodosLosRecibos();

                    // Guardar servicios asociados con el id del recibo recién creado
                    if (daoServicio != null)
                    {
                        GuardarServicioAsociado(idReciboCreado, () =>
                        {
                            GlobalDropdownRefreshManager.NotifyDataChanged();
                        });
                    }
                    else
                    {
                        GlobalDropdownRefreshManager.NotifyDataChanged();
                    }

                    Invoke(nameof(PrepararFormularioNuevo), 1.5f);
                }
                else MostrarMensajeFormulario("Error: " + error, Color.red);
            });
        }
    }

    // ═════════════════════════════════════════════
    //  GUARDAR SERVICIOS ASOCIADOS AL RECIBO
    //  Guarda todos los renglones con nombre completo.
    //  El inmueble y propietario se toman del contrato seleccionado.
    // ═════════════════════════════════════════════

    private void GuardarServicioAsociado(long idRecibo = 0, Action onDone = null)
    {
        if (_contratoPropietarioId <= 0)
        {
            Debug.LogWarning("[ReciboUI] Servicios no guardados: propietario desconocido.");
            onDone?.Invoke();
            return;
        }

        string fechaReciboSupabase = FormatoSupabase(fechaInput != null ? fechaInput.text.Trim() : "");

        var renglonesValidos = new List<(TMP_InputField nombre, TMP_InputField fecha, TMP_InputField monto, TMP_InputField porcentaje)>();
        foreach (var r in _renglones)
        {
            string nombreServicio = r.nombre != null ? r.nombre.text.Trim() : "";
            if (string.IsNullOrEmpty(nombreServicio)) continue; // renglón vacío: saltar
            renglonesValidos.Add(r);
        }

        if (renglonesValidos.Count == 0)
        {
            onDone?.Invoke();
            return;
        }

        int pendientes = renglonesValidos.Count;
        foreach (var r in renglonesValidos)
        {
            string nombreServicio = r.nombre.text.Trim();
            string fechaSvc = r.fecha != null ? FormatoSupabase(r.fecha.text.Trim()) : null;
            if (string.IsNullOrEmpty(fechaSvc)) fechaSvc = fechaReciboSupabase; // fallback: fecha del recibo
            if (string.IsNullOrEmpty(fechaSvc))
            {
                pendientes--;
                if (pendientes == 0) onDone?.Invoke();
                continue;
            }

            float.TryParse(r.monto?.text.Trim(),      out float montoSvc);
            float.TryParse(r.porcentaje?.text.Trim(), out float porcentajeSvc);
            porcentajeSvc = Mathf.Clamp(porcentajeSvc, 0f, 100f);

            string nomCaptura = nombreServicio; // captura para el lambda
            daoServicio.RegistrarServicio(
                nomCaptura,
                montoSvc,
                fechaSvc,
                _contratoPropietarioId,
                porcentajeSvc,
                _contratoInmuebleId,
                (exitoSvc, errorSvc) =>
                {
                    if (exitoSvc) Debug.Log($"[ReciboUI] Servicio '{nomCaptura}' guardado.");
                    else          Debug.LogWarning($"[ReciboUI] Error al guardar '{nomCaptura}': {errorSvc}");

                    pendientes--;
                    if (pendientes == 0) onDone?.Invoke();
                },
                idRecibo  // ← Vincula el servicio al recibo
            );
        }
    }

    // ═════════════════════════════════════════════
    //  LISTA Y FILTROS
    // ═════════════════════════════════════════════

    public void CargarTodosLosRecibos()
    {
        MostrarMensajeLista("Cargando...", Color.gray);
        LimpiarContenedorLista();

        daoRecibo.ObtenerTodosLosRecibos((exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }

            _todosLosRecibos = ParsearLista(json);
            if (_todosLosRecibos == null || _todosLosRecibos.Count == 0)
            { MostrarMensajeLista("No hay recibos registrados.", Color.gray); return; }

            MostrarMensajeLista("", Color.white);
            RenderizarLista(_todosLosRecibos);
        });
    }

    private void OnBuscarPorInquilino()
    {
        string termino = buscadorInquilinoInput != null ? buscadorInquilinoInput.text.Trim().ToLowerInvariant() : "";
        if (string.IsNullOrEmpty(termino))
        {
            // Sin filtro: mostrar todos
            LimpiarContenedorLista();
            RenderizarLista(_todosLosRecibos);
            return;
        }

        // Filtrar localmente por el nombre del inquilino asociado al contrato
        var filtrados = _todosLosRecibos.FindAll(r =>
        {
            if (!nombreInquilinoPorContrato.ContainsKey(r.idContrato)) return false;
            string nombre = nombreInquilinoPorContrato[r.idContrato].ToLowerInvariant();
            // Normalizar tildes
            nombre = nombre.Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u');
            string t2 = termino.Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u');
            return nombre.Contains(t2);
        });

        LimpiarContenedorLista();
        if (filtrados.Count == 0)
            MostrarMensajeLista($"Sin recibos para \"{buscadorInquilinoInput.text.Trim()}\".", Color.gray);
        else
            RenderizarLista(filtrados);
    }

    private void OnLimpiarBusqueda()
    {
        if (buscadorInquilinoInput != null) buscadorInquilinoInput.text = "";
        LimpiarContenedorLista();
        if (_todosLosRecibos.Count == 0)
        {
            MostrarMensajeLista("No hay recibos registrados.", Color.gray);
        }
        else
        {
            MostrarMensajeLista("", Color.white);
            RenderizarLista(_todosLosRecibos);
        }
    }

    private void RenderizarLista(List<ReciboItemData> lista)
    {
        if (itemReciboPrefab == null || contenedorLista == null) return;

        foreach (ReciboItemData r in lista)
        {
            GameObject item = Instantiate(itemReciboPrefab, contenedorLista);

            string tipoStr     = r.tipo     == 1 ? "Alquiler"      : r.tipo     == 2 ? "Expensas"      : "Otros";
            string tipoPagoStr = r.tipoPago == 1 ? "Efectivo"      : r.tipoPago == 2 ? "Transferencia" : r.tipoPago == 3 ? "Efec/Transf" : "—";

            // Nombre del inquilino vinculado al contrato de este recibo
            string nomInq = nombreInquilinoPorContrato.ContainsKey(r.idContrato)
                ? nombreInquilinoPorContrato[r.idContrato]
                : $"Contrato #{r.idContrato}";

            // ── Asignar cada campo por nombre exacto del GameObject ──────────
            TMP_Text textoServicios = null;
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text t in textos)
            {
                switch (t.gameObject.name)
                {
                    case "TxtNumeroRecibo":      t.text = nomInq;                    break;
                    case "TxtNumeroRecibo (1)":  t.text = nomInq;                    break;
                    case "TxtFechaContrato":     t.text = FormatoArgentino(r.fecha); break;
                    case "TxtFechaContrato (1)": t.text = "Fecha del Contrato";      break;
                    case "TxtContrato":          t.text = nomInq;                    break;
                    case "TxtContrato (1)":      t.text = "Contrato";                break;
                    case "TxtMonto":             t.text = $"$ {r.monto:N0}";         break;
                    case "TxtMonto (1)":         t.text = "Subtotal";                break;
                    case "TxtTipoAPagar":        t.text = $"$ {r.totalAbonar:N0}";   break;
                    case "TxtTipo (1)":          t.text = tipoStr;                   break;
                    case "TxtFormaPago":         t.text = tipoPagoStr;               break;
                    case "TxtFormaPago (1)":     t.text = "Forma de Pago";           break;
                    case "TxtTotal Abonar":      t.text = $"$ {r.totalAbonar:N0}";   break;
                    case "TxtTotal Abonar (1)":  t.text = "Total";                   break;
                    case "TxtServicios":         textoServicios = t;                 break;
                }
            }

            // ── Cargar servicios del recibo en TxtServicios (asíncrono) ──
            if (textoServicios != null && daoServicio != null)
            {
                TMP_Text txtSvcRef = textoServicios;
                txtSvcRef.text = "...";

                // Capturar referencia al TMP_Text del Total para actualizarlo al sumar servicios
                TMP_Text txtTotalRef = null;
                foreach (TMP_Text t in textos)
                    if (t.gameObject.name == "TxtTipoAPagar" || t.gameObject.name == "TxtTotal Abonar")
                    { txtTotalRef = t; break; }

                long totalBase = r.totalAbonar;
                long idRecibo  = r.id;

                daoServicio.ObtenerServiciosPorRecibo(idRecibo, (exSvc, jsonSvc, errSvc) =>
                {
                    if (txtSvcRef == null) return;
                    if (!exSvc || string.IsNullOrEmpty(jsonSvc) || jsonSvc.Trim() == "[]")
                    {
                        txtSvcRef.text = "—";
                        return;
                    }
                    try
                    {
                        var arr = JsonUtility.FromJson<ServicioEdicionArray>("{\"items\":" + jsonSvc + "}");
                        if (arr?.items == null || arr.items.Length == 0) { txtSvcRef.text = "—"; return; }

                        var sb = new System.Text.StringBuilder();
                        long totalServicios = 0;
                        foreach (var s in arr.items)
                        {
                            if (sb.Length > 0) sb.Append("\n");
                            sb.Append($"{s.Nombre_servicio}  $ {s.MontoTotal:N0}");
                            totalServicios += (long)s.MontoTotal;
                        }
                        txtSvcRef.text = sb.ToString();

                        // Actualizar el Total sumando base + servicios
                        if (txtTotalRef != null)
                            txtTotalRef.text = $"$ {(totalBase + totalServicios):N0}";
                    }
                    catch { txtSvcRef.text = "—"; }
                });
            }

            long id = r.id;

            Button[] botones = item.GetComponentsInChildren<Button>();
            foreach (Button btn in botones)
            {
                string nombre = btn.gameObject.name.ToLower();
                if (nombre.Contains("editar"))
                    btn.onClick.AddListener(() => AbrirEdicion(id));
                else if (nombre.Contains("eliminar"))
                    btn.onClick.AddListener(() => AbrirPopupEliminar(id, ""));
                else if (nombre.Contains("pdf") || nombre.Contains("imprimir"))
                    btn.onClick.AddListener(() => PrepararDatosParaPDF(id));
            }
        }
    }

    private void LimpiarContenedorLista()
    {
        if (contenedorLista == null) return;
        foreach (Transform hijo in contenedorLista) Destroy(hijo.gameObject);
    }

    // ═════════════════════════════════════════════
    //  POPUP ELIMINAR
    // ═════════════════════════════════════════════

    private void AbrirPopupEliminar(long id, string resumen)
    {
        idReciboSeleccionado = id;
        if (mensajeConfirmacionText != null)
            mensajeConfirmacionText.text = "¿Estás seguro de que querés eliminar este recibo?\nEsta acción no se puede deshacer.";
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(true);
    }

    private void CerrarPopupEliminar()
    {
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(false);
    }

    private void OnConfirmarEliminar()
    {
        if (idReciboSeleccionado <= 0) return;
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = false;

        // Primero borrar los servicios asociados, luego el recibo
        Action borrarRecibo = () =>
        {
            daoRecibo.EliminarRecibo(idReciboSeleccionado, (exito, error) =>
            {
                if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;
                if (exito)
                {
                    CargarTodosLosRecibos();
                    if (modoEdicion) PrepararFormularioNuevo();
                    CerrarPopupEliminar();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                }
                else
                {
                    if (mensajeConfirmacionText != null)
                        mensajeConfirmacionText.text = "Error: " + error;
                }
            });
        };

        // Borrar servicios antes de borrar el recibo (complementa CASCADE)
        if (daoServicio != null)
        {
            daoServicio.EliminarServiciosPorRecibo(idReciboSeleccionado, (exitoSvc, errorSvc) =>
            {
                if (!exitoSvc) Debug.LogWarning($"[ReciboUI] Error borrando servicios: {errorSvc}");
                borrarRecibo();
            });
        }
        else
        {
            borrarRecibo();
        }
    }

    // ═════════════════════════════════════════════
    //  GENERACIÓN DE PDF (RF-03.02)
    // ═════════════════════════════════════════════
    private ReciboJsonItem  _pdfRecibo;
    private ContratoJsonItem _pdfContrato;
    private string           _pdfNombreInquilino;
    private InmuebleJsonItem _pdfInmueble; // guardamos el inmueble para buscar sus servicios

    private void PrepararDatosParaPDF(long idRecibo)
    {
        MostrarMensajeLista("Generando PDF...", Color.cyan);
        daoRecibo.ObtenerReciboPorId(idRecibo, OnReciboPDFObtenido);
    }

    private void OnReciboPDFObtenido(bool exito, string json, string error)
    {
        if (!exito) { MostrarMensajeLista("Error PDF: " + error, Color.red); return; }
        _pdfRecibo = ParsearPrimerItem<ReciboJsonItem>(json);
        daoContrato.ObtenerContratoPorId(_pdfRecibo.id_Contrato, OnContratoPDFObtenido);
    }

    private void OnContratoPDFObtenido(bool exito, string json, string error)
    {
        if (!exito) { MostrarMensajeLista("Error PDF: " + error, Color.red); return; }
        _pdfContrato = ParsearPrimerItem<ContratoJsonItem>(json);
        Debug.Log($"[ReciboUI-PDF] Contrato obtenido: id_contrato={_pdfContrato.id_contrato}, id_Inmueble={_pdfContrato.id_Inmueble}, id_Inquilino={_pdfContrato.id_Inquilino}");
        daoInquilino.ObtenerInquilinoPorId(_pdfContrato.id_Inquilino, OnInquilinoPDFObtenido);
    }

    private void OnInquilinoPDFObtenido(bool exito, string nombre, string apellido, long tel, string error)
    {
        if (!exito) { MostrarMensajeLista("Error PDF: " + error, Color.red); return; }
        _pdfNombreInquilino = $"{nombre} {apellido}";
        daoInmueble.ObtenerInmueblePorId(_pdfContrato.id_Inmueble, OnInmueblePDFObtenido);
    }

    private void OnInmueblePDFObtenido(bool exito, string json, string error)
    {
        if (!exito) { MostrarMensajeLista("Error PDF: " + error, Color.red); return; }
        _pdfInmueble = ParsearPrimerItem<InmuebleJsonItem>(json);
        Debug.Log($"[ReciboUI-PDF] Inmueble obtenido: id_Propiedad={(_pdfInmueble != null ? _pdfInmueble.id_Propiedad.ToString() : "NULL")}, id_Duenos={_pdfInmueble?.id_Duenos}, Direccion={(_pdfInmueble?.Direccion ?? "NULL")}");

        // Buscar servicios por PROPIETARIO del inmueble (id_Duenos == id_Propietario en tabla Servicios).
        // Esto cubre tanto servicios con id_propiedad asignado como los registrados sin propiedad.
        long idPropietario = _pdfInmueble?.id_Duenos ?? 0;
        if (daoServicio != null && idPropietario > 0)
        {
            Debug.Log($"[ReciboUI-PDF] Buscando servicios para id_Propietario={idPropietario}...");
            daoServicio.ObtenerServiciosPorPropietario(idPropietario, OnServiciosPDFObtenidos);
        }
        else
        {
            Debug.LogWarning($"[ReciboUI-PDF] No se buscan servicios. daoServicio={daoServicio != null}, id_Duenos={idPropietario}");
            GenerarPDFConServicios(null);
        }
    }

    // Clases JSON para parsear servicios en el contexto de PDF
    [Serializable] private class ServicioPDFJsonItem { public string Nombre_servicio; public float MontoTotal; public string FechaServicio; public float PorcentajePagar; }
    [Serializable] private class ServicioPDFJsonArray { public ServicioPDFJsonItem[] items; }

    private void OnServiciosPDFObtenidos(bool exito, string json, string error)
    {
        Debug.Log($"[ReciboUI-PDF] Servicios respuesta: exito={exito}, json='{json}', error='{error}'");
        List<Inmobiliaria.Modelos.ServicioReporteItem> servicios = null;

        if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
        {
            try
            {
                var arr = JsonUtility.FromJson<ServicioPDFJsonArray>("{\"items\":" + json + "}");
                if (arr?.items != null && arr.items.Length > 0)
                {
                    servicios = new List<Inmobiliaria.Modelos.ServicioReporteItem>();
                    foreach (var s in arr.items)
                        servicios.Add(new Inmobiliaria.Modelos.ServicioReporteItem
                        {
                            NombreServicio = s.Nombre_servicio,
                            Fecha          = s.FechaServicio,
                            Monto          = s.MontoTotal,
                            Porcentaje     = s.PorcentajePagar
                        });
                    Debug.Log($"[ReciboUI-PDF] Se cargaron {servicios.Count} servicio(s) para el PDF.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ReciboUI] No se pudieron parsear servicios para PDF: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning($"[ReciboUI-PDF] Sin servicios: exito={exito} | json vacío o []");
        }

        GenerarPDFConServicios(servicios);
    }

    private void GenerarPDFConServicios(List<Inmobiliaria.Modelos.ServicioReporteItem> servicios)
    {
        var dataReporte = new Inmobiliaria.Modelos.ReciboReporteData
        {
            NumeroRecibo      = _pdfRecibo.id_Recibo,
            Fecha             = _pdfRecibo.Fecha,
            Monto             = _pdfRecibo.Monto,
            TotalAbonar       = _pdfRecibo.Total_Abonar,
            TipoRecibo        = _pdfRecibo.Tipo == 1 ? "ALQUILER" : _pdfRecibo.Tipo == 2 ? "EXPENSAS" : "OTROS",
            Concepto          = "Pago correspondiente al contrato #" + _pdfRecibo.id_Contrato,
            NombreInquilino   = _pdfNombreInquilino,
            DomicilioInmueble = _pdfInmueble != null
                ? $"{_pdfInmueble.Direccion} {_pdfInmueble.Numero_Direccion}"
                : "",
            Servicios         = servicios
        };

        string path = Inmobiliaria.Servicios.ReciboPDFGenerator.GenerarReciboPDF(dataReporte);
        if (!string.IsNullOrEmpty(path))
        {
            MostrarMensajeLista("PDF Generado con éxito.", Color.green);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        else MostrarMensajeLista("Error al generar el archivo PDF.", Color.red);
    }

    private T ParsearPrimerItem<T>(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return default;
        try
        {
            string wrappedJson = "{\"items\":" + json + "}";
            var wrapper = JsonUtility.FromJson<JsonArrayWrapper<T>>(wrappedJson);
            if (wrapper != null && wrapper.items != null && wrapper.items.Length > 0)
                return wrapper.items[0];
        }
        catch (Exception ex) { Debug.LogError("[ReciboUI] Error ParsearItem: " + ex.Message); }
        return default;
    }

    [Serializable] private class JsonArrayWrapper<T> { public T[] items; }
    // id_Duenos es el propietario del inmueble → coincide con id_Propietario en Servicios
    [Serializable] private class InmuebleJsonItem { public long id_Propiedad; public long id_Duenos; public string Direccion; public int Numero_Direccion; }

    // ── Conversión de fechas: DD/MM/YYYY ↔ YYYY-MM-DD ──

    /// <summary>Convierte DD/MM/YYYY (argentino) → YYYY-MM-DD (Supabase). Retorna null si el formato es inválido.</summary>
    private string FormatoSupabase(string ddmmyyyy)
    {
        if (string.IsNullOrEmpty(ddmmyyyy)) return null;
        string[] partes = ddmmyyyy.Split('-', '/');
        if (partes.Length != 3) return null;
        return $"{partes[2]}-{partes[1]}-{partes[0]}";
    }

    /// <summary>Convierte YYYY-MM-DD (Supabase) → DD/MM/YYYY (argentino). Si no puede, devuelve el original.</summary>
    private string FormatoArgentino(string yyyymmdd)
    {
        if (string.IsNullOrEmpty(yyyymmdd)) return "";
        string[] partes = yyyymmdd.Split('-');
        if (partes.Length >= 3) return $"{partes[2]}/{partes[1]}/{partes[0]}";
        return yyyymmdd;
    }

    // ═════════════════════════════════════════════
    //  HELPERS & JSON
    // ═════════════════════════════════════════════

    private void MostrarMensajeFormulario(string m, Color c)
    { if (mensajeFormularioText != null) { mensajeFormularioText.text = m; mensajeFormularioText.color = c; } }

    private void MostrarMensajeLista(string m, Color c)
    { if (mensajeListaText != null) { mensajeListaText.gameObject.SetActive(!string.IsNullOrEmpty(m)); mensajeListaText.text = m; mensajeListaText.color = c; } }

    // Clase interna para la lista renderizada
    private class ReciboItemData { public long id; public string fecha; public long monto; public long totalAbonar; public long tipo; public long tipoPago; public long idContrato; }

    // Clases JSON Recibo — nombres exactos que devuelve Supabase
    [Serializable] private class ReciboJsonItem { public long id_Recibo; public string Fecha; public long Monto; public long Total_Abonar; public long Tipo; public long Tipo_Pago; public long id_Contrato; }
    [Serializable] private class ReciboJsonArray { public ReciboJsonItem[] items; }

    // Clases JSON Contrato — nombres exactos que devuelve Supabase
    // NOTA: Supabase devuelve id_contrato (minúscula) pero id_Inquilino e id_Inmueble (mayúscula)
    [Serializable] private class ContratoJsonItem { public long id_contrato; public string FechaInicio; public long MontoAlquiler; public long id_Inquilino; public long id_Inmueble; }
    [Serializable] private class ContratoJsonArray { public ContratoJsonItem[] items; }

    [Serializable]
    private class InquilinoJsonItem
    {
        public long id_Inquilino;
        public string Nombre_Inquilinos;
        public string Apellido_Inquilinos;
    }
    [Serializable] private class InquilinoJsonArray { public InquilinoJsonItem[] items; }

    private List<ReciboItemData> ParsearLista(string json)
    {
        var resultado = new List<ReciboItemData>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return resultado;
        try
        {
            ReciboJsonArray arr = JsonUtility.FromJson<ReciboJsonArray>("{\"items\":" + json + "}");
            if (arr?.items == null) return resultado;
            foreach (var r in arr.items)
                resultado.Add(new ReciboItemData { id = r.id_Recibo, fecha = r.Fecha, monto = r.Monto, totalAbonar = r.Total_Abonar, tipo = r.Tipo, tipoPago = r.Tipo_Pago, idContrato = r.id_Contrato });
        }
        catch (Exception ex) { Debug.LogError("[ReciboUI] Error JSON: " + ex.Message); }
        return resultado;
    }

    private List<ContratoJsonItem> ParsearListaContratos(string json)
    {
        try { var a = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}"); return new List<ContratoJsonItem>(a.items); }
        catch { return new List<ContratoJsonItem>(); }
    }

    private List<InquilinoJsonItem> ParsearListaInquilinos(string json)
    {
        try { var a = JsonUtility.FromJson<InquilinoJsonArray>("{\"items\":" + json + "}"); return new List<InquilinoJsonItem>(a.items); }
        catch { return new List<InquilinoJsonItem>(); }
    }
}
