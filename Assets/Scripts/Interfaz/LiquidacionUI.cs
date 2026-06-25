using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Inmobiliaria.Modelos;
using Inmobiliaria.Servicios;

/// <summary>
/// Maneja la interfaz de usuario para la gestión de Liquidaciones Mensuales.
/// RF-03.03: Liquidación Mensual para Propietarios
/// RF-03.04: Histórico de Liquidaciones
///
/// ═══════════════════════════════════════════════════════════════
///  GUÍA PARA ARMAR LA ESCENA EN UNITY (DISEÑO DASHBOARD)
/// ═══════════════════════════════════════════════════════════════
///
///  Canvas
///  └── Panel_Liquidaciones
///      ├── Lado_Izquierdo_Historico
///      │   ├── Filtros_Periodo
///      │   │   ├── InputField_FechaDesde       ← fechaDesdeInput   (ej: "01/01/2024")
///      │   │   ├── InputField_FechaHasta       ← fechaHastaInput   (ej: "31/12/2024")
///      │   │   ├── Button_Filtrar              ← filtrarBtn
///      │   │   └── Button_MostrarTodas         ← limpiarFiltroBtn
///      │   ├── ScrollView
///      │   │   └── Viewport/Content            ← contenedorLista
///      │   └── Text_MensajeLista               ← mensajeListaText
///      │
///      ├── Lado_Derecho_Formulario
///      │   ├── Text_TituloFormulario           ← tituloFormularioText
///      │   ├── Dropdown_Contrato               ← contratoDropdown
///      │   ├── InputField_Fecha                ← fechaInput        (ej: "15/03/2024")
///      │   ├── InputField_MontoAlquiler        ← montoAlquilerInput
///      │   ├── InputField_Honorarios           ← honorariosInput
///      │   ├── InputField_DescuentoAdicional   ← descuentoAdicionalInput
///      │   ├── InputField_DescuentoDescripcion ← descuentoDescripcionInput
///      │   ├── Text_NetoPropietario            ← netoPropietarioText
///      │   ├── Dropdown_Estado                 ← estadoDropdown
///      │   ├── Text_MensajeFormulario          ← mensajeFormularioText
///      │   ├── Button_Guardar                  ← guardarBtn
///      │   └── Button_LimpiarFormulario        ← limpiarFormularioBtn
///      │
///      └── Panel_ConfirmacionEliminar          ← panelConfirmacionEliminar
///          ├── Text_MensajeConfirmacion        ← mensajeConfirmacionText
///          ├── Button_ConfirmarEliminar        ← confirmarEliminarBtn
///          └── Button_CancelarEliminar         ← cancelarEliminarBtn
///
///  PREFAB ÍTEM DE LISTA (ItemLiquidacion):
///      ├── Text_Fecha                          ← textos[0]  "DD-MM-YYYY"
///      ├── Text_Contrato                       ← textos[1]  "Contrato #5"
///      ├── Text_MontoAlquiler                  ← textos[2]  "$ 150.000"
///      ├── Text_Honorarios                     ← textos[3]  "$ 15.000"
///      ├── Text_Descuento                      ← textos[4]  "$ 5.000" (0 si no hay)
///      ├── Text_DescuentoDesc                  ← textos[5]  Descripción del descuento
///      ├── Text_Neto                           ← textos[6]  "$ 130.000"
///      ├── Toggle_Estado                       ← Toggle  (isOn=Pagada, off=Pendiente)
///      ├── Text_EstadoLabel                    ← TMP_Text al lado del Toggle
///      ├── Button_Editar                       ← botón con "editar" en el nombre
///      ├── Button_Eliminar                     ← botón con "eliminar" en el nombre
///      └── Button_PDF                          ← botón con "pdf" en el nombre
///
/// ═══════════════════════════════════════════════════════════════
/// </summary>
public class LiquidacionUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Referencias a DAOs
    // ─────────────────────────────────────────────

    [Header("DAOs")]
    [SerializeField] private DAOLiquidacion daoLiquidacion;
    [SerializeField] private DAOContrato daoContrato;
    [SerializeField] private DAODueno daoDueno;
    [SerializeField] private DAOInmueble daoInmueble;

    // ─────────────────────────────────────────────
    //  Panel Izquierdo — Histórico y Filtros
    // ─────────────────────────────────────────────

    [Header("Panel Izquierdo - Histórico")]
    [SerializeField] private Transform contenedorLista;
    [SerializeField] private GameObject itemLiquidacionPrefab;
    [SerializeField] private TMP_Text mensajeListaText;
    [Tooltip("Campo de búsqueda por nombre/apellido del dueño")]
    [SerializeField] private TMP_InputField buscadorDuenoInput;
    [SerializeField] private Button buscarPorDuenoBtn;
    [SerializeField] private Button limpiarBusquedaBtn;

    // ─────────────────────────────────────────────
    //  Panel Derecho — Formulario
    // ─────────────────────────────────────────────

    [Header("Panel Derecho - Formulario")]
    [SerializeField] private TMP_Dropdown contratoDropdown;
    [SerializeField] private TMP_InputField fechaInput;
    [SerializeField] private TMP_InputField montoAlquilerInput;
    [SerializeField] private TMP_InputField honorariosInput;
    [Tooltip("Contenedor donde se instanciarán los renglones de descuento")]
    [SerializeField] private Transform contenedorDescuentos;
    [Tooltip("Prefab del renglón de descuento: 2 TMP_InputField (monto, descripción) + Button con 'eliminar' en el nombre")]
    [SerializeField] private GameObject descuentoItemPrefab;
    [SerializeField] private Button agregarDescuentoBtn;
    [Space(10)]
    [SerializeField] private TMP_Text netoPropietarioText;

    [Tooltip("Toggle de estado: marcado = Pagada, desmarcado = Pendiente")]
    [SerializeField] private Toggle estadoToggle;

    [Tooltip("TMP_Text que muestra 'Pagada' o 'Pendiente' al lado del Toggle")]
    [SerializeField] private TMP_Text estadoToggleLabel;

    [Space(10)]
    [SerializeField] private TMP_Text mensajeFormularioText;
    [SerializeField] private Button guardarBtn;
    [SerializeField] private Button limpiarFormularioBtn;

    // ─────────────────────────────────────────────
    //  Popup Confirmación
    // ─────────────────────────────────────────────

    [Header("Popup Confirmación")]
    [SerializeField] private GameObject panelConfirmacionEliminar;
    [SerializeField] private TMP_Text mensajeConfirmacionText;
    [SerializeField] private Button confirmarEliminarBtn;
    [SerializeField] private Button cancelarEliminarBtn;

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private long idLiquidacionSeleccionada = -1;
    private bool modoEdicion = false;
    private List<long> listaIdsContratos = new List<long>();
    // Mapa id_Contrato → nombre del dueño (para búsqueda)
    private Dictionary<long, string> nombreDuenoPorContrato = new Dictionary<long, string>();
    // Lista de todos los items cargados (para filtrado local)
    private List<LiquidacionJsonItem> _todasLasLiquidaciones = new List<LiquidacionJsonItem>();
    // Lista dinámica de renglones de descuentos adicionales
    private List<(TMP_InputField monto, TMP_InputField descripcion)> _rengloneDescuentos
        = new List<(TMP_InputField, TMP_InputField)>();

    // ─────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────

    private void Awake()
    {
        ValidarDAOs();

        // Listeners Formulario
        if (guardarBtn != null) guardarBtn.onClick.AddListener(OnGuardar);
        if (limpiarFormularioBtn != null) limpiarFormularioBtn.onClick.AddListener(PrepararFormularioNuevo);

        // Listeners cálculo automático del neto
        if (montoAlquilerInput != null) montoAlquilerInput.onValueChanged.AddListener((s) => RecalcularNeto());
        if (honorariosInput != null) honorariosInput.onValueChanged.AddListener((s) => RecalcularNeto());

        // Listener al seleccionar contrato → auto-llenar honorarios
        if (contratoDropdown != null) contratoDropdown.onValueChanged.AddListener(OnContratoSeleccionado);

        // Listener del Toggle de estado
        if (estadoToggle != null)
            estadoToggle.onValueChanged.AddListener(ActualizarEtiquetaEstado);

        // Listeners Popup
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.onClick.AddListener(OnConfirmarEliminar);
        if (cancelarEliminarBtn != null) cancelarEliminarBtn.onClick.AddListener(CerrarPopupEliminar);

        // Listener buscador por dueño
        if (buscarPorDuenoBtn != null) buscarPorDuenoBtn.onClick.AddListener(OnBuscarPorDueno);
        if (buscadorDuenoInput != null) buscadorDuenoInput.onSubmit.AddListener(_ => OnBuscarPorDueno());
        if (limpiarBusquedaBtn != null) limpiarBusquedaBtn.onClick.AddListener(OnLimpiarBusqueda);

        // Listener para agregar descuento adicional
        if (agregarDescuentoBtn != null) agregarDescuentoBtn.onClick.AddListener(AgregarRenglonDescuento);
    }

    private void OnEnable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged += RefrescarDropdowns;
        CerrarPopupEliminar();
        PrepararFormularioNuevo();

        MostrarMensajeLista("Cargando...", Color.gray);
        CargarMapDuenos(() =>
        {
            CargarListaLiquidaciones();
            CargarContratosDropdown();
        });
    }

    private void OnDisable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged -= RefrescarDropdowns;
    }

    private void ValidarDAOs()
    {
        if (daoLiquidacion == null) daoLiquidacion = FindObjectOfType<DAOLiquidacion>();
        if (daoContrato == null) daoContrato = FindObjectOfType<DAOContrato>();
        if (daoDueno == null) daoDueno = FindObjectOfType<DAODueno>();
        if (daoInmueble == null) daoInmueble = FindObjectOfType<DAOInmueble>();
    }

    // ─────────────────────────────────────────────
    //  Toggle Estado — Actualizar etiqueta
    // ─────────────────────────────────────────────

    private void ActualizarEtiquetaEstado(bool pagada)
    {
        if (estadoToggleLabel == null) return;
        estadoToggleLabel.text = pagada ? "Pagada" : "Pendiente";
        estadoToggleLabel.color = pagada ? Color.green : Color.yellow;
    }

    // ═════════════════════════════════════════════
    //  CÁLCULO AUTOMÁTICO DEL NETO
    // ═════════════════════════════════════════════

    private void RecalcularNeto()
    {
        long monto      = ObtenerValorInput(montoAlquilerInput);
        long honorarios = ObtenerValorInput(honorariosInput);
        long descuento  = ObtenerTotalDescuentos();

        long neto = monto - honorarios - descuento;
        if (neto < 0) neto = 0;

        if (netoPropietarioText != null)
            netoPropietarioText.text = "Neto Propietario: $" + neto.ToString("N0");
    }

    private long ObtenerValorInput(TMP_InputField input)
    {
        if (input == null || string.IsNullOrEmpty(input.text)) return 0;
        long.TryParse(input.text.Trim(), out long valor);
        return valor;
    }

    // ═════════════════════════════════════════════
    //  AUTO-LLENAR HONORARIOS AL SELECCIONAR CONTRATO
    // ═════════════════════════════════════════════

    private void OnContratoSeleccionado(int index)
    {
        if (index <= 0 || index - 1 >= listaIdsContratos.Count) return;

        long idContrato = listaIdsContratos[index - 1]; // -1 por el placeholder

        daoContrato.ObtenerContratoPorId(idContrato, (exito, json, error) =>
        {
            if (exito)
            {
                ContratoJsonItem contrato = ParsearUnContrato(json);
                if (contrato != null)
                {
                    // Auto-llenar monto alquiler
                    if (montoAlquilerInput != null)
                        montoAlquilerInput.text = contrato.MontoAlquiler.ToString();

                    // Auto-llenar honorarios: se usa el valor guardado en HonorarioPorcentaje
                    // como monto FIJO en pesos (no se calcula porcentaje)
                    if (honorariosInput != null)
                        honorariosInput.text = contrato.HonorarioPorcentaje.ToString();

                    RecalcularNeto();
                }
            }
        });
    }

    // ═════════════════════════════════════════════
    //  LÓGICA DEL FORMULARIO
    // ═════════════════════════════════════════════

    public void PrepararFormularioNuevo()
    {
        if (this == null || gameObject == null) return;
        modoEdicion = false;
        idLiquidacionSeleccionada = -1;

        if (contratoDropdown != null) contratoDropdown.value = 0;
        if (fechaInput != null) fechaInput.text = "";
        if (montoAlquilerInput != null) montoAlquilerInput.text = "";
        if (honorariosInput != null) honorariosInput.text = "";
        if (netoPropietarioText != null) netoPropietarioText.text = "Neto Propietario: $0";

        // Limpiar todos los renglones y agregar uno vacío por defecto
        LimpiarRenglonesDescuento();
        AgregarRenglonDescuento();

        // Toggle: Pendiente por defecto
        if (estadoToggle != null) estadoToggle.isOn = false;
        ActualizarEtiquetaEstado(false);

        MostrarMensajeFormulario("", Color.white);
    }

    // ═════════════════════════════════════════════
    //  DESCUENTOS ADICIONALES DINÁMICOS
    // ═════════════════════════════════════════════

    /// <summary>
    /// Agrega un nuevo renglón de descuento al formulario.
    /// Requiere que 'contenedorDescuentos' y 'descuentoItemPrefab' estén asignados en el Inspector.
    /// El prefab debe tener al menos 2 TMP_InputField (monto y descripción) y un Button con "eliminar" en el nombre.
    /// </summary>
    private void AgregarRenglonDescuento()
    {
        if (contenedorDescuentos == null || descuentoItemPrefab == null) return;

        GameObject renglon = Instantiate(descuentoItemPrefab, contenedorDescuentos);
        TMP_InputField[] inputs = renglon.GetComponentsInChildren<TMP_InputField>();
        TMP_InputField inputMonto = inputs.Length > 0 ? inputs[0] : null;
        TMP_InputField inputDesc = inputs.Length > 1 ? inputs[1] : null;

        if (inputMonto != null) inputMonto.onValueChanged.AddListener((s) => RecalcularNeto());

        var tupla = (inputMonto, inputDesc);
        _rengloneDescuentos.Add(tupla);

        // Botón de eliminar este renglón
        Button[] btns = renglon.GetComponentsInChildren<Button>();
        foreach (Button btn in btns)
        {
            if (btn.gameObject.name.ToLower().Contains("eliminar") || btn.gameObject.name.ToLower().Contains("borrar"))
            {
                GameObject renglonRef = renglon;
                var tuplaRef = tupla;
                btn.onClick.AddListener(() =>
                {
                    _rengloneDescuentos.Remove(tuplaRef);
                    Destroy(renglonRef);
                    RecalcularNeto();
                });
            }
        }
    }

    private void LimpiarRenglonesDescuento()
    {
        if (contenedorDescuentos != null)
            foreach (Transform hijo in contenedorDescuentos) Destroy(hijo.gameObject);
        _rengloneDescuentos.Clear();
    }

    /// <summary>Suma los montos de todos los renglones de descuento.</summary>
    private long ObtenerTotalDescuentos()
    {
        long total = 0;
        foreach (var r in _rengloneDescuentos) total += ObtenerValorInput(r.monto);
        return total;
    }

    /// <summary>Concatena todas las descripciones de descuento no vacías con " | ".</summary>
    private string ObtenerDescripcionesDescuentos()
    {
        var partes = new System.Text.StringBuilder();
        foreach (var r in _rengloneDescuentos)
        {
            string d = r.descripcion != null ? r.descripcion.text.Trim() : "";
            if (!string.IsNullOrEmpty(d))
            {
                if (partes.Length > 0) partes.Append(" | ");
                partes.Append(d);
            }
        }
        return partes.ToString();
    }

    public void AbrirEdicion(long idLiquidacion)
    {
        modoEdicion = true;
        idLiquidacionSeleccionada = idLiquidacion;

        MostrarMensajeFormulario("Cargando...", Color.gray);

        daoLiquidacion.ObtenerLiquidacionPorId(idLiquidacion, (exito, json, error) =>
        {
            if (exito)
            {
                LiquidacionJsonItem data = ParsearUnaLiquidacion(json);
                if (data != null)
                {
                    if (fechaInput != null) fechaInput.text = FormatoArgentino(data.Fecha);
                    if (montoAlquilerInput != null) montoAlquilerInput.text = data.Monto_Alquiler.ToString();
                    if (honorariosInput != null) honorariosInput.text = data.Honorarios.ToString();

                    // Cargar el descuento guardado en el primer renglón dinámico
                    LimpiarRenglonesDescuento();
                    AgregarRenglonDescuento();
                    if (_rengloneDescuentos.Count > 0)
                    {
                        if (_rengloneDescuentos[0].monto != null)
                            _rengloneDescuentos[0].monto.text = data.DescuentoAdicional > 0 ? data.DescuentoAdicional.ToString() : "";
                        if (_rengloneDescuentos[0].descripcion != null)
                            _rengloneDescuentos[0].descripcion.text = data.DescuentoDescripcion ?? "";
                    }

                    if (estadoToggle != null) estadoToggle.isOn = (data.Estado == 2);
                    ActualizarEtiquetaEstado(estadoToggle != null && estadoToggle.isOn);

                    // Seleccionar contrato en dropdown
                    int indexContrato = listaIdsContratos.IndexOf(data.id_Contratos) + 1;
                    if (contratoDropdown != null) contratoDropdown.value = indexContrato >= 0 ? indexContrato : 0;

                    RecalcularNeto();
                    MostrarMensajeFormulario("", Color.white);
                }
            }
            else MostrarMensajeFormulario("Error: " + error, Color.red);
        });
    }

    // ═════════════════════════════════════════════
    //  GUARDAR (REGISTRAR O ACTUALIZAR)
    // ═════════════════════════════════════════════

    private void OnGuardar()
    {
        int contratoIdx = contratoDropdown != null ? contratoDropdown.value : 0;
        string fechaUsuario = fechaInput != null ? fechaInput.text.Trim() : "";

        // Validaciones
        if (contratoIdx == 0)
        {
            MostrarMensajeFormulario("Seleccioná un contrato.", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(fechaUsuario))
        {
            MostrarMensajeFormulario("Ingresá una fecha (DD/MM/YYYY).", Color.red);
            return;
        }

        long estado = (estadoToggle != null && estadoToggle.isOn) ? 2 : 1; // 2=Pagada, 1=Pendiente

        long montoAlquiler = ObtenerValorInput(montoAlquilerInput);
        long honorarios = ObtenerValorInput(honorariosInput);
        // Sumar TODOS los descuentos (campo fijo + renglones dinámicos)
        long descuentoAdicional = ObtenerTotalDescuentos();
        // Concatenar TODAS las descripciones de descuentos
        string descuentoDescripcion = ObtenerDescripcionesDescuentos();

        if (montoAlquiler <= 0)
        {
            MostrarMensajeFormulario("El monto de alquiler debe ser mayor a cero.", Color.red);
            return;
        }

        // Convertir fecha de DD/MM/YYYY a YYYY-MM-DD para Supabase
        string fecha = FormatoSupabase(fechaUsuario);
        if (string.IsNullOrEmpty(fecha))
        {
            MostrarMensajeFormulario("Formato de fecha inválido. Usá DD/MM/YYYY.", Color.red);
            return;
        }

        long netoPropietario = montoAlquiler - honorarios - descuentoAdicional;
        if (netoPropietario < 0) netoPropietario = 0;

        long idContrato = listaIdsContratos[contratoIdx - 1]; // -1 por placeholder

        if (guardarBtn != null) guardarBtn.interactable = false;
        MostrarMensajeFormulario("Guardando...", Color.gray);

        if (modoEdicion)
        {
            daoLiquidacion.ActualizarLiquidacion(idLiquidacionSeleccionada, fecha, montoAlquiler, honorarios,
                descuentoAdicional, descuentoDescripcion, netoPropietario, estado, idContrato, (exito, error) =>
                {
                    TerminarGuardado(exito, error);
                });
        }
        else
        {
            daoLiquidacion.RegistrarLiquidacion(fecha, montoAlquiler, honorarios,
                descuentoAdicional, descuentoDescripcion, netoPropietario, estado, idContrato, (exito, error) =>
                {
                    TerminarGuardado(exito, error);
                });
        }
    }

    private void TerminarGuardado(bool exito, string error)
    {
        if (guardarBtn != null) guardarBtn.interactable = true;

        if (exito)
        {
            MostrarMensajeFormulario("Liquidación guardada correctamente.", Color.green);
            CargarListaLiquidaciones();
            Invoke(nameof(PrepararFormularioNuevo), 1.5f);
        }
        else MostrarMensajeFormulario("Error: " + error, Color.red);
    }

    // ═════════════════════════════════════════════
    //  LISTA — HISTÓRICO DE LIQUIDACIONES (RF-03.04)
    // ═════════════════════════════════════════════

    public void CargarListaLiquidaciones()
    {
        MostrarMensajeLista("Cargando...", Color.gray);
        LimpiarContenedorLista();

        daoLiquidacion.ObtenerTodasLasLiquidaciones((exito, json, error) =>
        {
            if (exito)
            {
                _todasLasLiquidaciones = ParsearListaLiquidaciones(json);
                if (_todasLasLiquidaciones.Count == 0)
                    MostrarMensajeLista("No hay liquidaciones registradas.", Color.gray);
                else
                {
                    MostrarMensajeLista("", Color.white);
                    RenderizarLista(_todasLasLiquidaciones);
                }
            }
            else MostrarMensajeLista("Error: " + error, Color.red);
        });
    }

    private void OnBuscarPorDueno()
    {
        string termino = buscadorDuenoInput != null ? buscadorDuenoInput.text.Trim().ToLowerInvariant() : "";
        if (string.IsNullOrEmpty(termino))
        {
            LimpiarContenedorLista();
            RenderizarLista(_todasLasLiquidaciones);
            return;
        }

        var filtrados = _todasLasLiquidaciones.FindAll(liq =>
        {
            if (!nombreDuenoPorContrato.ContainsKey(liq.id_Contratos)) return false;
            string nombre = nombreDuenoPorContrato[liq.id_Contratos].ToLowerInvariant();
            nombre = nombre.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u');
            string t2 = termino.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u');
            return nombre.Contains(t2);
        });

        LimpiarContenedorLista();
        if (filtrados.Count == 0)
            MostrarMensajeLista($"Sin liquidaciones para \"{buscadorDuenoInput.text.Trim()}\".", Color.gray);
        else
            RenderizarLista(filtrados);
    }

    private void OnLimpiarBusqueda()
    {
        if (buscadorDuenoInput != null) buscadorDuenoInput.text = "";
        LimpiarContenedorLista();
        if (_todasLasLiquidaciones.Count == 0)
        {
            MostrarMensajeLista("No hay liquidaciones registradas.", Color.gray);
        }
        else
        {
            MostrarMensajeLista("", Color.white);
            RenderizarLista(_todasLasLiquidaciones);
        }
    }



    // ═════════════════════════════════════════════
    //  RENDERIZAR LISTA
    // ═════════════════════════════════════════════

    private void RenderizarLista(List<LiquidacionJsonItem> lista)
    {
        if (itemLiquidacionPrefab == null || contenedorLista == null) return;

        foreach (var itemData in lista)
        {
            GameObject item = Instantiate(itemLiquidacionPrefab, contenedorLista);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>(true);

            // ── Textos individuales ──────────────────────────────────────────────
            // textos[0] → Fecha
            // textos[1] → Contrato
            // textos[2] → Monto Alquiler
            // textos[3] → Honorarios
            // textos[4] → Descuento
            // textos[5] → Descripción descuento
            // textos[6] → Neto Propietario
            // (El estado se maneja con Toggle, no texto)
            // ────────────────────────────────────────────────────────────
            if (textos.Length >= 1) textos[0].text = FormatoArgentino(itemData.Fecha);
            if (textos.Length >= 2) textos[1].text = "Contrato #" + itemData.id_Contratos;
            if (textos.Length >= 3) textos[2].text = "$ " + itemData.Monto_Alquiler.ToString("N0");
            if (textos.Length >= 4) textos[3].text = "$ " + itemData.Honorarios.ToString("N0");
            if (textos.Length >= 5) textos[4].text = itemData.DescuentoAdicional > 0
                ? "- $ " + itemData.DescuentoAdicional.ToString("N0") : "$0";
            if (textos.Length >= 6) textos[5].text = !string.IsNullOrEmpty(itemData.DescuentoDescripcion)
                ? itemData.DescuentoDescripcion : "";
            if (textos.Length >= 7) textos[6].text = "$ " + itemData.NetoPropietario.ToString("N0");

            // Toggle de estado (isOn = Pagada, off = Pendiente)
            Toggle[] toggles = item.GetComponentsInChildren<Toggle>(true);
            if (toggles.Length > 0)
            {
                Toggle tog = toggles[0];
                bool pagada = itemData.Estado == 2;
                tog.isOn = pagada;
                tog.interactable = false; // Solo lectura en la lista

                // Buscar etiqueta del toggle (primer TMP_Text DENTRO del toggle)
                TMP_Text[] togTexts = tog.GetComponentsInChildren<TMP_Text>(true);
                if (togTexts.Length > 0)
                {
                    togTexts[0].text = pagada ? "Pagada" : "Pendiente";
                    togTexts[0].color = pagada ? Color.green : Color.yellow;
                }
            }

            long id = itemData.id_Liquidacion;
            long idContrato = itemData.id_Contratos;
            string desc = $"Liquidación {FormatoArgentino(itemData.Fecha)} (Contrato #{itemData.id_Contratos})";

            Button[] btns = item.GetComponentsInChildren<Button>();
            foreach (var btn in btns)
            {
                string n = btn.gameObject.name.ToLower();

                if (n.Contains("editar")) btn.onClick.AddListener(() => AbrirEdicion(id));
                else if (n.Contains("eliminar")) btn.onClick.AddListener(() => AbrirPopupEliminar(id, desc));
                else if (n.Contains("pdf")) btn.onClick.AddListener(() => GenerarPDFLiquidacion(itemData));
            }
        }
    }

    // ═════════════════════════════════════════════
    //  GENERACIÓN DE PDF
    // ═════════════════════════════════════════════

    private LiquidacionJsonItem _pdfLiquidacionItem;
    private void GenerarPDFLiquidacion(LiquidacionJsonItem itemData)
    {
        MostrarMensajeLista("Generando PDF...", Color.gray);

        _pdfLiquidacionItem = itemData;
        daoContrato.ObtenerContratoPorId(itemData.id_Contratos, OnContratoPDFObtenido);
    }
    private void OnContratoPDFObtenido(bool exito, string json, string error)
    {
        if (!exito) { MostrarMensajeLista("Error al obtener contrato para PDF.", Color.red); return; }
        ContratoJsonItem contrato = ParsearUnContrato(json);
        if (contrato == null) { MostrarMensajeLista("Contrato no encontrado.", Color.red); return; }
        daoDueno.ObtenerDuenoPorId(contrato.id_Duenos, (ok, nom, ape, tel, err) => OnDuenoPDFObtenido(ok, nom, ape, err, contrato));
    }
    private void OnDuenoPDFObtenido(bool exito, string nombre, string apellido, string error, ContratoJsonItem contrato)
    {
        if (!exito) { MostrarMensajeLista("Error al obtener propietario para PDF.", Color.red); return; }
        string nomProp = $"{nombre} {apellido}";
        daoInmueble.ObtenerInmueblePorId(contrato.id_Inmueble, (ok, j, err) => OnInmueblePDFObtenido(ok, j, err, nomProp));
    }
    private void OnInmueblePDFObtenido(bool exito, string json, string error, string nomProp)
    {
        if (!exito) { MostrarMensajeLista("Error al obtener inmueble para PDF.", Color.red); return; }
        InmuebleJsonItem inmueble = ParsearUnInmueble(json);
        if (inmueble == null) { MostrarMensajeLista("Inmueble no encontrado.", Color.red); return; }
        LiquidacionReporteData dataPDF = new LiquidacionReporteData
        {
            NumeroLiquidacion = _pdfLiquidacionItem.id_Liquidacion,
            Fecha = FormatoArgentino(_pdfLiquidacionItem.Fecha),
            NombrePropietario = nomProp,
            DireccionInmueble = $"{inmueble.Direccion} {inmueble.Numero_Direccion}",
            MontoAlquiler = _pdfLiquidacionItem.Monto_Alquiler,
            Honorarios = _pdfLiquidacionItem.Honorarios,
            DescuentoAdicional = _pdfLiquidacionItem.DescuentoAdicional,
            DescuentoDescripcion = _pdfLiquidacionItem.DescuentoDescripcion,
            NetoPropietario = _pdfLiquidacionItem.NetoPropietario,
            NumeroContrato = _pdfLiquidacionItem.id_Contratos
        };

        string pdfPath = LiquidacionPDFGenerator.GenerarLiquidacionPDF(dataPDF);
        if (!string.IsNullOrEmpty(pdfPath))
        {
            MostrarMensajeLista("PDF generado con éxito.", Color.green);
            Application.OpenURL("file://" + pdfPath.Replace("\\", "/"));
        }
        else MostrarMensajeLista("Error al generar el PDF.", Color.red);
    }

    // ═════════════════════════════════════════════
    //  DROPDOWN DE CONTRATOS
    // ═════════════════════════════════════════════

    public void RefrescarDropdowns()
    {
        long idContratoSeleccionado = 0;
        if (contratoDropdown != null && contratoDropdown.value > 0 && contratoDropdown.value - 1 < listaIdsContratos.Count)
            idContratoSeleccionado = listaIdsContratos[contratoDropdown.value - 1];

        CargarMapDuenos(() =>
        {
            CargarContratosDropdown(() =>
            {
                if (idContratoSeleccionado > 0)
                {
                    int index = listaIdsContratos.IndexOf(idContratoSeleccionado);
                    if (index >= 0 && contratoDropdown != null)
                    {
                        contratoDropdown.value = index + 1;
                        contratoDropdown.RefreshShownValue();
                    }
                }
            });
        });
    }

    private void CargarMapDuenos(Action onDone = null)
    {
        daoContrato.ObtenerTodosLosContratos((exitoC, jsonC, errorC) =>
        {
            if (!exitoC)
            {
                onDone?.Invoke();
                return;
            }
            daoDueno.ObtenerTodosLosDuenos((exitoD, jsonD, errorD) =>
            {
                if (exitoD)
                {
                    var listaContratos = ParsearListaContratos(jsonC);
                    var listaDuenos = ParsearListaDuenos(jsonD);

                    var mapaDuenos = new Dictionary<long, string>();
                    foreach (var d in listaDuenos)
                    {
                        mapaDuenos[d.id_Dueno] = $"{d.Apellido_Dueno}, {d.Nombre_Dueno}";
                    }

                    nombreDuenoPorContrato.Clear();
                    foreach (var c in listaContratos)
                    {
                        if (mapaDuenos.ContainsKey(c.id_Duenos))
                        {
                            nombreDuenoPorContrato[c.id_contrato] = mapaDuenos[c.id_Duenos];
                        }
                        else
                        {
                            nombreDuenoPorContrato[c.id_contrato] = $"Contrato #{c.id_contrato}";
                        }
                    }
                }
                onDone?.Invoke();
            });
        });
    }

    private void CargarContratosDropdown(Action onDone = null)
    {
        daoContrato.ObtenerContratosActivos((exito, json, error) =>
        {
            if (exito)
            {
                listaIdsContratos.Clear();
                List<string> opciones = new List<string> { "-- seleccione contrato --" };

                try
                {
                    ContratoJsonArray array = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
                    if (array != null && array.items != null)
                    {
                        foreach (var c in array.items) listaIdsContratos.Add(c.id_contrato);

                        List<string> opFinales = new List<string> { "-- seleccione contrato --" };
                        foreach (var ct in array.items)
                        {
                            string nd = nombreDuenoPorContrato.ContainsKey(ct.id_contrato) ? nombreDuenoPorContrato[ct.id_contrato] : $"Contrato #{ct.id_contrato}";
                            opFinales.Add($"{nd}  |  $ {ct.MontoAlquiler:N0}");
                        }
                        if (contratoDropdown != null)
                        {
                            contratoDropdown.ClearOptions();
                            contratoDropdown.AddOptions(opFinales);
                        }
                    }
                    else { if (contratoDropdown != null) { contratoDropdown.ClearOptions(); contratoDropdown.AddOptions(opciones); } }
                }
                catch { if (contratoDropdown != null) { contratoDropdown.ClearOptions(); contratoDropdown.AddOptions(opciones); } }
            }
            onDone?.Invoke();
        });
    }

    // ═════════════════════════════════════════════
    //  ELIMINAR
    // ═════════════════════════════════════════════

    private void AbrirPopupEliminar(long id, string desc)
    {
        idLiquidacionSeleccionada = id;
        if (mensajeConfirmacionText != null) mensajeConfirmacionText.text = "¿Estás seguro de que querés eliminar esta liquidación?\nEsta acción no se puede deshacer.";
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(true);
    }

    private void CerrarPopupEliminar()
    {
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(false);
    }

    private void OnConfirmarEliminar()
    {
        if (idLiquidacionSeleccionada <= 0) return;

        if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = false;

        daoLiquidacion.EliminarLiquidacion(idLiquidacionSeleccionada, (exito, error) =>
        {
            if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;

            if (exito)
            {
                CargarListaLiquidaciones();
                if (modoEdicion) PrepararFormularioNuevo();
                CerrarPopupEliminar();
            }
            else
            {
                if (mensajeConfirmacionText != null) mensajeConfirmacionText.text = "Error: " + error;
            }
        });
    }

    // ═════════════════════════════════════════════
    //  HELPERS & PARSING
    // ═════════════════════════════════════════════

    private void MostrarMensajeFormulario(string m, Color c) { if (mensajeFormularioText) { mensajeFormularioText.text = m; mensajeFormularioText.color = c; } }
    private void MostrarMensajeLista(string m, Color c) { if (mensajeListaText) { mensajeListaText.text = m; mensajeListaText.color = c; } }
    private void LimpiarContenedorLista() { if (contenedorLista != null) foreach (Transform t in contenedorLista) Destroy(t.gameObject); }

    // ── Conversión de fechas: DD-MM-YYYY ↔ YYYY-MM-DD ──

    /// <summary>Convierte DD-MM-YYYY (argentino) → YYYY-MM-DD (Supabase). Retorna null si el formato es inválido.</summary>
    private string FormatoSupabase(string ddmmyyyy)
    {
        if (string.IsNullOrEmpty(ddmmyyyy)) return null;
        string[] partes = ddmmyyyy.Split('-', '/');
        if (partes.Length != 3) return null;
        return $"{partes[2]}-{partes[1]}-{partes[0]}";
    }

    /// <summary>Convierte YYYY-MM-DD (Supabase) → DD-MM-YYYY (argentino). Si no puede, devuelve el original.</summary>
    private string FormatoArgentino(string yyyymmdd)
    {
        if (string.IsNullOrEmpty(yyyymmdd)) return "";
        string[] partes = yyyymmdd.Split('-');
        if (partes.Length >= 3) return $"{partes[2]}-{partes[1]}-{partes[0]}";
        return yyyymmdd;
    }

    // ── Clases de parseo ──

    [Serializable]
    private class LiquidacionJsonItem
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
    [Serializable] private class LiquidacionJsonArray { public LiquidacionJsonItem[] items; }

    [Serializable]
    private class ContratoJsonItem
    {
        public long id_contrato;
        public string FechaInicio;
        public string FechaFIn;
        public long MontoAlquiler;
        public long HonorarioPorcentaje;
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

    [Serializable]
    private class DuenoJsonItem
    {
        public long id_Dueno;
        public string Nombre_Dueno;
        public string Apellido_Dueno;
    }
    [Serializable] private class DuenoJsonArray { public DuenoJsonItem[] items; }

    private List<LiquidacionJsonItem> ParsearListaLiquidaciones(string json)
    {
        try { var a = JsonUtility.FromJson<LiquidacionJsonArray>("{\"items\":" + json + "}"); return new List<LiquidacionJsonItem>(a.items); }
        catch { return new List<LiquidacionJsonItem>(); }
    }

    private LiquidacionJsonItem ParsearUnaLiquidacion(string json)
    {
        var list = ParsearListaLiquidaciones(json);
        return list.Count > 0 ? list[0] : null;
    }

    private ContratoJsonItem ParsearUnContrato(string json)
    {
        try
        {
            var a = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
            return (a != null && a.items != null && a.items.Length > 0) ? a.items[0] : null;
        }
        catch { return null; }
    }

    private List<ContratoJsonItem> ParsearListaContratos(string json)
    {
        try { var a = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}"); return new List<ContratoJsonItem>(a.items); }
        catch { return new List<ContratoJsonItem>(); }
    }

    private List<DuenoJsonItem> ParsearListaDuenos(string json)
    {
        try { var a = JsonUtility.FromJson<DuenoJsonArray>("{\"items\":" + json + "}"); return new List<DuenoJsonItem>(a.items); }
        catch { return new List<DuenoJsonItem>(); }
    }

    private InmuebleJsonItem ParsearUnInmueble(string json)
    {
        try
        {
            var a = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
            return (a != null && a.items != null && a.items.Length > 0) ? a.items[0] : null;
        }
        catch { return null; }
    }
}
