using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Maneja la interfaz de usuario para la gestión de Contratos.
///
/// ═══════════════════════════════════════════════════════════════
///  GUÍA PARA ARMAR LA ESCENA EN UNITY (DISEÑO DASHBOARD)
/// ═══════════════════════════════════════════════════════════════
///
///  Canvas
///  └── Panel_Contratos                        ← Contenedor raíz
///      ├── Lado_Izquierdo_Lista               ← ~55% de la pantalla
///      │   ├── Filtros_Arriba
///      │   │   ├── Button_VerTodos            ← verTodosBtn
///      │   │   ├── Button_VerSoloActivos      ← verActivosBtn
///      │   │   ├── InputField_Buscador        ← buscadorInquilinoInput
///      │   │   ├── Button_Buscar              ← buscarPorInquilinoBtn
///      │   │   └── Button_Limpiar             ← limpiarBusquedaBtn
///      │   ├── ScrollView → Content           ← contenedorLista
///      │   └── Text_MensajeLista              ← mensajeListaText
///      │
///      ├── Lado_Derecho_Formulario            ← ~45% de la pantalla
///      │   ├── Text_TituloFormulario          ← tituloFormularioText
///      │   ├── InputField_FechaInicio         ← fechaInicioInput  (ej: "15/01/2024")
///      │   ├── InputField_FechaFin            ← fechaFinInput     (opcional, ej: "15/01/2026")
///      │   ├── InputField_MontoAlquiler       ← montoAlquilerInput (Integer Number)
///      │   ├── InputField_Honorario           ← honorarioInput    (Integer Number, monto fijo en $)
///      │   ├── InputField_IndiceIPC           ← indiceIPCInput    (Integer Number)
///      │   ├── Toggle_Estado                  ← estadoToggle      (marcado=Activo, desmarcado=Inactivo)
///      │   ├── Dropdown_Dueno                 ← duenoDropdown     (primer ítem: "-- Seleccionar Dueño --")
///      │   ├── Dropdown_Inquilino             ← inquilinoDropdown (primer ítem: "-- Seleccionar Inquilino --")
///      │   ├── Dropdown_Inmueble              ← inmuebleDropdown  (primer ítem: "-- Seleccionar Inmueble --")
///      │   ├── Text_MensajeFormulario         ← mensajeFormularioText
///      │   ├── Button_Guardar                 ← guardarBtn
///      │   └── Button_LimpiarFormulario       ← limpiarFormularioBtn
///      │
///      └── Panel_ConfirmacionEliminar         ← panelConfirmacionEliminar (POPUP)
///          ├── Text_MensajeConfirmacion       ← mensajeConfirmacionText
///          ├── Button_ConfirmarEliminar       ← confirmarEliminarBtn
///          └── Button_CancelarEliminar        ← cancelarEliminarBtn
///
///  PREFAB ÍTEM DE LISTA (ItemContrato):
///      ├── Text_IdFecha                       ← (primer TMP_Text) muestra ID y fecha
///      ├── Text_Monto                         ← (segundo TMP_Text) muestra monto y estado
///      ├── Button_Editar                      ← nombre debe contener "editar"
///      ├── Button_Eliminar                    ← nombre debe contener "eliminar"
///      └── Button_WhatsApp                    ← nombre debe contener "whatsapp" (RF-04.03)
///
/// ═══════════════════════════════════════════════════════════════
/// </summary>
public class ContratoUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Referencias a los DAOs
    // ─────────────────────────────────────────────

    [Header("DAOs")]
    [SerializeField] private DAOContrato daoContrato;
    [SerializeField] private DAODueno daoDueno;
    [SerializeField] private DAOInquilino daoInquilino;
    [SerializeField] private DAOInmueble daoInmueble;

    // ─────────────────────────────────────────────
    //  Popup Confirmación Eliminar
    // ─────────────────────────────────────────────

    [Header("Popup Confirmación")]
    [SerializeField] private GameObject panelConfirmacionEliminar;
    [SerializeField] private TMP_Text mensajeConfirmacionText;
    [SerializeField] private Button confirmarEliminarBtn;
    [SerializeField] private Button cancelarEliminarBtn;

    // ─────────────────────────────────────────────
    //  Panel Izquierdo — Lista
    // ─────────────────────────────────────────────

    [Header("Panel Izquierdo - Lista")]
    [SerializeField] private Button verTodosBtn;
    [SerializeField] private Button verActivosBtn;
    [SerializeField] private Transform contenedorLista;
    [SerializeField] private GameObject itemContratoPrefab;
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
    [SerializeField] private TMP_InputField fechaInicioInput;
    [SerializeField] private TMP_InputField fechaFinInput;
    [SerializeField] private TMP_InputField montoAlquilerInput;
    [Tooltip("Honorario en PESOS (monto fijo, no porcentaje)")]
    [SerializeField] private TMP_InputField honorarioInput;
    
    [Header("Datos Visuales ARquiler")]
    [SerializeField] private TMP_Dropdown frecuenciaMesesDropdown;
    [SerializeField] private TMP_Dropdown tipoIndiceDropdown;

    [Tooltip("Bot\u00f3n que abre https://arquiler.com en el navegador para calcular actualizaciones de alquiler")]
    [SerializeField] private Button arquilerBtn;

    [Header("Menu Principal - Contadores")]
    [SerializeField] private TMP_Text totalContratos;

    [Tooltip("Marcado = Activo (1), Desmarcado = Inactivo (0)")]
    [SerializeField] private Toggle estadoToggle;

    [Tooltip("TMP_Text que muestra 'Activo' o 'Inactivo' al lado del Toggle")]
    [SerializeField] private TMP_Text estadoToggleLabel;

    [Tooltip("Se carga automáticamente con todos los dueños (índice 0 = placeholder)")]
    [SerializeField] private TMP_Dropdown duenoDropdown;

    [Tooltip("Se carga automáticamente con todos los inquilinos (índice 0 = placeholder)")]
    [SerializeField] private TMP_Dropdown inquilinoDropdown;

    [Tooltip("Se carga automáticamente con todos los inmuebles (índice 0 = placeholder)")]
    [SerializeField] private TMP_Dropdown inmuebleDropdown;

    [SerializeField] private TMP_Text mensajeFormularioText;
    [SerializeField] private Button guardarBtn;
    [SerializeField] private Button limpiarFormularioBtn;

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private long idContratoSeleccionado = -1;
    private bool modoEdicion = false;

    // Listas para mapear índice del dropdown → ID real en la BD
    private List<long> idsDuenos = new List<long>();
    private List<long> idsInquilinos = new List<long>();
    private List<long> idsInmuebles = new List<long>();

    // Caché de TODOS los inmuebles (para filtrar por dueño en memoria)
    private List<InmuebleJsonItem> _todosLosInmuebles = new List<InmuebleJsonItem>();
    // idDueno de cada inmueble en caché (mismo orden que _todosLosInmuebles)
    private List<long> _idDuenoDeInmueble = new List<long>();

    // Lista completa de contratos cargados (para filtrar localmente por inquilino)
    private List<ContratoItemData> _todosLosContratos = new List<ContratoItemData>();

    // ─────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (daoContrato == null) daoContrato = FindObjectOfType<DAOContrato>();
        if (daoDueno == null) daoDueno = FindObjectOfType<DAODueno>();
        if (daoInquilino == null) daoInquilino = FindObjectOfType<DAOInquilino>();
        if (daoInmueble == null) daoInmueble = FindObjectOfType<DAOInmueble>();

        if (daoInquilino == null) Debug.LogError("[ContratoUI] No se encontró DAOInquilino en la escena.");
        if (daoDueno == null) Debug.LogError("[ContratoUI] No se encontró DAODueno en la escena.");
        if (daoInmueble == null) Debug.LogError("[ContratoUI] No se encontró DAOInmueble en la escena.");

        if (verTodosBtn != null) verTodosBtn.onClick.AddListener(CargarTodosLosContratos);
        if (verActivosBtn != null) verActivosBtn.onClick.AddListener(CargarContratosActivos);

        // Listener buscador por inquilino
        if (buscarPorInquilinoBtn != null) buscarPorInquilinoBtn.onClick.AddListener(OnBuscarPorInquilino);
        if (buscadorInquilinoInput != null) buscadorInquilinoInput.onSubmit.AddListener(_ => OnBuscarPorInquilino());
        if (limpiarBusquedaBtn != null) limpiarBusquedaBtn.onClick.AddListener(OnLimpiarBusqueda);

        if (guardarBtn != null) guardarBtn.onClick.AddListener(OnGuardar);
        if (limpiarFormularioBtn != null) limpiarFormularioBtn.onClick.AddListener(PrepararFormularioNuevo);

        if (confirmarEliminarBtn != null) confirmarEliminarBtn.onClick.AddListener(OnConfirmarEliminar);
        if (cancelarEliminarBtn != null) cancelarEliminarBtn.onClick.AddListener(CerrarPopupEliminar);

        // Listener del Toggle: actualizar etiqueta cada vez que cambia
        if (estadoToggle != null)
            estadoToggle.onValueChanged.AddListener(ActualizarEtiquetaEstado);

        // Botón Calculadora ARquiler → abre el navegador
        if (arquilerBtn != null)
            arquilerBtn.onClick.AddListener(() => Application.OpenURL("https://arquiler.com"));

        // Listener del dropdown de dueño: filtrar inmuebles
        if (duenoDropdown != null)
            duenoDropdown.onValueChanged.AddListener(OnDuenoDropdownChanged);

        InicializarDropdownsARquiler();
    }

    private void InicializarDropdownsARquiler()
    {
        if (frecuenciaMesesDropdown != null)
        {
            frecuenciaMesesDropdown.ClearOptions();
            var opcionesMeses = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
            frecuenciaMesesDropdown.AddOptions(opcionesMeses);
        }

        if (tipoIndiceDropdown != null)
        {
            tipoIndiceDropdown.ClearOptions();
            var opcionesIndice = new List<string> { "ICL", "IPC", "CasaPropia", "CAC", "CER", "IS", "IPIM", "UVA" };
            tipoIndiceDropdown.AddOptions(opcionesIndice);
        }
    }

    private void OnEnable()
    {
        Debug.Log("[ContratoUI] OnEnable disparado — recargando dropdowns y lista.");
        GlobalDropdownRefreshManager.OnAnyDataChanged += RefrescarDropdowns;
        CerrarPopupEliminar();
        CargarDropdowns(() =>
        {
            PrepararFormularioNuevo();
            CargarTodosLosContratos();
        });
    }

    private void OnDisable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged -= RefrescarDropdowns;
    }

    /// <summary>
    /// Método público para forzar la recarga de los 3 dropdowns desde la BD.
    /// Útil cuando se agregan dueños/inquilinos/inmuebles desde otros paneles.
    /// </summary>
    public void RefrescarDropdowns()
    {
        Debug.Log("[ContratoUI] Refrescando dropdowns manualmente...");
        CargarDropdowns(() =>
        {
            MostrarMensajeFormulario("✓ Listas actualizadas.", Color.green);
            Invoke(nameof(LimpiarMensajeFormulario), 2f);
        });
    }

    private void LimpiarMensajeFormulario()
    {
        MostrarMensajeFormulario("", Color.white);
    }

    // ═════════════════════════════════════════════
    //  CARGA DE DROPDOWNS (Dueño, Inquilino, Inmueble)
    // ═════════════════════════════════════════════

    /// <summary>
    /// Carga en paralelo los tres dropdowns y llama al callback cuando terminan todos.
    /// </summary>
    private void CargarDropdowns(Action onComplete)
    {
        int pendientes = 3;
        bool completo = false;
        Action check = () =>
        {
            if (completo) return;
            pendientes--;
            if (pendientes == 0) { completo = true; onComplete?.Invoke(); }
        };

        CargarDropdownDuenos(check);
        CargarDropdownInquilinos(check);
        CargarDropdownInmuebles(check);

        StartCoroutine(TimeoutDropdowns(5f, () =>
        {
            if (!completo) { completo = true; Debug.LogWarning("[ContratoUI] Timeout cargando dropdowns"); onComplete?.Invoke(); }
        }));
    }

    private System.Collections.IEnumerator TimeoutDropdowns(float segundos, Action onTimeout)
    {
        yield return new WaitForSeconds(segundos);
        onTimeout?.Invoke();
    }

    private void CargarDropdownDuenos(Action onDone)
    {
        daoDueno.ObtenerTodosLosDuenos((exito, json, error) =>
        {
            idsDuenos.Clear();
            if (duenoDropdown != null) duenoDropdown.ClearOptions();

            // Índice 0 siempre es el placeholder — no tiene ID en la lista
            var opciones = new List<string> { "-- Seleccionar Propietario --" };

            if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
            {
                try
                {
                    DuenoJsonArray arr = JsonUtility.FromJson<DuenoJsonArray>("{\"items\":" + json + "}");
                    if (arr?.items != null)
                        foreach (var d in arr.items)
                        {
                            idsDuenos.Add(d.id_Dueno);
                            opciones.Add(d.Apellido_Dueno + ", " + d.Nombre_Dueno);
                        }
                }
                catch (Exception ex) { Debug.LogError("[ContratoUI] Error dropdown dueños: " + ex.Message); }
            }

            if (duenoDropdown != null)
            {
                duenoDropdown.AddOptions(opciones);
                duenoDropdown.value = 0;
                duenoDropdown.RefreshShownValue();
            }
            onDone?.Invoke();
        });
    }

    private void CargarDropdownInquilinos(Action onDone)
    {
        if (daoInquilino == null)
        {
            Debug.LogError("[ContratoUI] No se puede cargar inquilinos: daoInquilino es nulo.");
            onDone?.Invoke();
            return;
        }

        daoInquilino.ObtenerTodosLosInquilinos((exito, json, error) =>
        {
            idsInquilinos.Clear();
            if (inquilinoDropdown != null) inquilinoDropdown.ClearOptions();

            var opciones = new List<string> { "-- Seleccionar Inquilino --" };

            Debug.Log($"[ContratoUI] Cargando inquilinos. Exito: {exito}. JSON: {json}");

            if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
            {
                try
                {
                    InquilinoJsonArray arr = JsonUtility.FromJson<InquilinoJsonArray>("{\"items\":" + json + "}");
                    if (arr?.items != null)
                    {
                        foreach (var i in arr.items)
                        {
                            idsInquilinos.Add(i.id_Inquilino);
                            opciones.Add(i.Apellido_Inquilinos + ", " + i.Nombre_Inquilinos);
                        }
                        Debug.Log($"[ContratoUI] Se cargaron {arr.items.Length} inquilinos.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[ContratoUI] Error parseando dropdown inquilinos: " + ex.Message);
                }
            }
            else if (!exito)
            {
                Debug.LogError("[ContratoUI] Error DAO inquilinos: " + error);
            }

            if (inquilinoDropdown != null)
            {
                inquilinoDropdown.AddOptions(opciones);
                inquilinoDropdown.value = 0;
                inquilinoDropdown.RefreshShownValue();
            }
            onDone?.Invoke();
        });
    }

    private void CargarDropdownInmuebles(Action onDone)
    {
        daoInmueble.ObtenerTodosLosInmuebles((exito, json, error) =>
        {
            idsInmuebles.Clear();
            if (inmuebleDropdown != null) inmuebleDropdown.ClearOptions();

            var opciones = new List<string> { "-- Seleccionar Inmueble --" };

            if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
            {
                try
                {
                    InmuebleJsonArray arr = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
                    if (arr?.items != null)
                        foreach (var m in arr.items)
                        {
                            idsInmuebles.Add(m.id_Propiedad);
                            opciones.Add(m.Direccion + " " + m.Numero_Direccion);
                        }
                }
                catch (Exception ex) { Debug.LogError("[ContratoUI] Error dropdown inmuebles: " + ex.Message); }
            }

            if (inmuebleDropdown != null)
            {
                inmuebleDropdown.AddOptions(opciones);
                inmuebleDropdown.value = 0;
                inmuebleDropdown.RefreshShownValue();
            }
            onDone?.Invoke();
        });
    }

    /// <summary>
    /// Se dispara cada vez que el usuario cambia el dropdown de dueño.
    /// Recarga el dropdown de inmueble mostrando solo los del dueño seleccionado.
    /// Si se elige el placeholder (índice 0) vuelve a mostrar todos.
    /// </summary>
    private void OnDuenoDropdownChanged(int dropdownIndex)
    {
        if (dropdownIndex <= 0 || idsDuenos.Count < dropdownIndex)
        {
            // Placeholder seleccionado: recargar todos los inmuebles
            CargarDropdownInmuebles(() => { });
            return;
        }

        long idDueno = idsDuenos[dropdownIndex - 1];
        FiltrarInmueblesPorDueno(idDueno);
    }

    /// <summary>
    /// Consulta los inmuebles del dueño y los carga en el dropdown de inmueble.
    /// Resetea la selección al placeholder.
    /// </summary>
    private void FiltrarInmueblesPorDueno(long idDueno, Action onComplete = null)
    {
        idsInmuebles.Clear();
        if (inmuebleDropdown != null) inmuebleDropdown.ClearOptions();

        var opciones = new List<string> { "-- Seleccionar Inmueble --" };

        daoInmueble.ObtenerInmueblesPorDueno(idDueno, (exito, json, error) =>
        {
            if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
            {
                try
                {
                    InmuebleJsonArray arr = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
                    if (arr?.items != null)
                        foreach (var m in arr.items)
                        {
                            idsInmuebles.Add(m.id_Propiedad);
                            opciones.Add(m.Direccion + " " + m.Numero_Direccion);
                        }
                }
                catch (Exception ex) { Debug.LogError("[ContratoUI] Error filtrando inmuebles por dueño: " + ex.Message); }
            }

            if (inmuebleDropdown != null)
            {
                inmuebleDropdown.AddOptions(opciones);
                inmuebleDropdown.value = 0;
                inmuebleDropdown.RefreshShownValue();
            }

            Debug.Log($"[ContratoUI] Inmuebles filtrados para dueño {idDueno}: {idsInmuebles.Count}");
            onComplete?.Invoke();
        });
    }

    // ═════════════════════════════════════════════
    //  FORMULARIO
    // ═════════════════════════════════════════════

    public void PrepararFormularioNuevo()
    {
        if (this == null || gameObject == null) return;
        modoEdicion = false;
        idContratoSeleccionado = -1;

        if (fechaInicioInput != null) fechaInicioInput.text = "";
        if (fechaFinInput != null) fechaFinInput.text = "";
        if (montoAlquilerInput != null) montoAlquilerInput.text = "";
        if (honorarioInput != null) honorarioInput.text = "";
        if (frecuenciaMesesDropdown != null) frecuenciaMesesDropdown.value = 0;
        if (tipoIndiceDropdown != null) tipoIndiceDropdown.value = 0;
        if (estadoToggle != null) estadoToggle.isOn = true; // Activo por defecto
        ActualizarEtiquetaEstado(estadoToggle != null && estadoToggle.isOn);
        if (duenoDropdown != null) duenoDropdown.value = 0;     // 0 = placeholder
        if (inquilinoDropdown != null) inquilinoDropdown.value = 0;
        // Al resetear el dueño, recargar todos los inmuebles
        CargarDropdownInmuebles(() => { });

        MostrarMensajeFormulario("", Color.white);

        if (tituloFormularioText != null)
            tituloFormularioText.text = "Nuevo Contrato";
    }

    public void AbrirEdicion(long idContrato)
    {
        modoEdicion = true;
        idContratoSeleccionado = idContrato;

        if (tituloFormularioText != null)
            tituloFormularioText.text = "Editar Contrato";

        MostrarMensajeFormulario("Cargando datos...", Color.gray);

        daoContrato.ObtenerContratoPorId(idContrato, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeFormulario("Error: " + error, Color.red); return; }

            try
            {
                ContratoJsonArray arr = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
                if (arr?.items == null || arr.items.Length == 0)
                { MostrarMensajeFormulario("Error al procesar los datos.", Color.red); return; }

                ContratoJsonItem c = arr.items[0];

                if (fechaInicioInput != null) fechaInicioInput.text = FormatoArgentino(c.FechaInicio);
                if (fechaFinInput != null) fechaFinInput.text = FormatoArgentino(c.FechaFIn ?? "");
                if (montoAlquilerInput != null) montoAlquilerInput.text = c.MontoAlquiler.ToString();
                if (honorarioInput != null) honorarioInput.text = c.HonorarioPorcentaje.ToString();
                if (frecuenciaMesesDropdown != null)
                {
                    int index = frecuenciaMesesDropdown.options.FindIndex(opt => opt.text == c.MesesActualizacion.ToString());
                    frecuenciaMesesDropdown.value = index >= 0 ? index : 0;
                }
                if (tipoIndiceDropdown != null && !string.IsNullOrEmpty(c.TipoIndice))
                {
                    int index = tipoIndiceDropdown.options.FindIndex(opt => opt.text.Equals(c.TipoIndice, StringComparison.OrdinalIgnoreCase));
                    tipoIndiceDropdown.value = index >= 0 ? index : 0;
                }
                if (estadoToggle != null) estadoToggle.isOn = (c.Estado == 1);
                ActualizarEtiquetaEstado(c.Estado == 1);

                // +1 porque el índice 0 es el placeholder en cada dropdown
                if (duenoDropdown != null)
                {
                    int idx = idsDuenos.IndexOf(c.id_Duenos);
                    // Quitar listener temporalmente para que setear el valor no dispare el filtro
                    duenoDropdown.onValueChanged.RemoveListener(OnDuenoDropdownChanged);
                    duenoDropdown.value = idx >= 0 ? idx + 1 : 0;
                    duenoDropdown.onValueChanged.AddListener(OnDuenoDropdownChanged);
                }
                if (inquilinoDropdown != null)
                {
                    int idx = idsInquilinos.IndexOf(c.id_Inquilino);
                    inquilinoDropdown.value = idx >= 0 ? idx + 1 : 0;
                }

                // Filtrar inmuebles por el dueño del contrato y luego seleccionar el inmueble correcto
                long idInmuebleContrato = c.id_Inmueble;
                FiltrarInmueblesPorDueno(c.id_Duenos, () =>
   {
       if (inmuebleDropdown != null)
       {
           int idx = idsInmuebles.IndexOf(idInmuebleContrato);
           inmuebleDropdown.value = idx >= 0 ? idx + 1 : 0;
           inmuebleDropdown.RefreshShownValue();
       }
   });

                MostrarMensajeFormulario("", Color.white);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ContratoUI] Error al cargar contrato: " + ex.Message);
                MostrarMensajeFormulario("Error interno al cargar datos.", Color.red);
            }
        });
    }

    // ═════════════════════════════════════════════
    //  GUARDAR
    // ═════════════════════════════════════════════

    private void OnGuardar()
    {
        string fechaInicioUsuario = fechaInicioInput != null ? fechaInicioInput.text.Trim() : "";
        string fechaFinUsuario = fechaFinInput != null ? fechaFinInput.text.Trim() : "";

        if (string.IsNullOrEmpty(fechaInicioUsuario))
        { MostrarMensajeFormulario("La fecha de inicio es obligatoria. (ej: 15/01/2024)", Color.red); return; }

        // Convertir fechas de DD/MM/YYYY a YYYY-MM-DD para Supabase
        string fechaInicio = FormatoSupabase(fechaInicioUsuario);
        if (string.IsNullOrEmpty(fechaInicio))
        { MostrarMensajeFormulario("Formato de fecha inicio inválido. Usá DD/MM/YYYY.", Color.red); return; }

        string fechaFin = !string.IsNullOrEmpty(fechaFinUsuario) ? FormatoSupabase(fechaFinUsuario) : "";
        if (!string.IsNullOrEmpty(fechaFinUsuario) && string.IsNullOrEmpty(fechaFin))
        { MostrarMensajeFormulario("Formato de fecha fin inválido. Usá DD/MM/YYYY.", Color.red); return; }

        if (!long.TryParse(montoAlquilerInput?.text.Trim(), out long monto) || monto <= 0)
        { MostrarMensajeFormulario("El monto de alquiler debe ser un número mayor a cero.", Color.red); return; }

        long.TryParse(honorarioInput?.text.Trim(), out long honorario);
        long.TryParse(frecuenciaMesesDropdown != null && frecuenciaMesesDropdown.options.Count > 0 ? frecuenciaMesesDropdown.options[frecuenciaMesesDropdown.value].text : "0", out long mesesActualizacion);
        string tipoIndice = tipoIndiceDropdown != null && tipoIndiceDropdown.options.Count > 0 ? tipoIndiceDropdown.options[tipoIndiceDropdown.value].text : "";
        long estado = (estadoToggle != null && estadoToggle.isOn) ? 1 : 0;

        // Índice 0 en cada dropdown = placeholder, así que restamos 1 para obtener el ID real
        int idxDueno = duenoDropdown != null ? duenoDropdown.value : 0;
        int idxInquil = inquilinoDropdown != null ? inquilinoDropdown.value : 0;
        int idxInmueble = inmuebleDropdown != null ? inmuebleDropdown.value : 0;

        // Validar que no sea el placeholder (índice 0)
        if (idxDueno == 0 || idxInquil == 0 || idxInmueble == 0)
        { MostrarMensajeFormulario("Seleccioná un dueño, inquilino e inmueble.", Color.red); return; }

        long idDueno = idsDuenos.Count >= idxDueno ? idsDuenos[idxDueno - 1] : -1;
        long idInquil = idsInquilinos.Count >= idxInquil ? idsInquilinos[idxInquil - 1] : -1;
        long idInmueble = idsInmuebles.Count >= idxInmueble ? idsInmuebles[idxInmueble - 1] : -1;

        if (idDueno <= 0 || idInquil <= 0 || idInmueble <= 0)
        { MostrarMensajeFormulario("Error al obtener los IDs. Intentá de nuevo.", Color.red); return; }

        if (guardarBtn != null) guardarBtn.interactable = false;
        MostrarMensajeFormulario("Guardando...", Color.gray);

        if (modoEdicion)
        {
            daoContrato.ActualizarContrato(idContratoSeleccionado, fechaInicio, fechaFin, monto, honorario, mesesActualizacion, tipoIndice, estado, idDueno, idInquil, idInmueble, (exito, error) =>
            {
                if (guardarBtn != null) guardarBtn.interactable = true;
                if (exito)
                {
                    MostrarMensajeFormulario("Contrato actualizado.", Color.green);
                    CargarTodosLosContratos();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                    Invoke(nameof(PrepararFormularioNuevo), 1.5f);
                }
                else MostrarMensajeFormulario("Error: " + error, Color.red);
            });
        }
        else
        {
            daoContrato.RegistrarContrato(fechaInicio, fechaFin, monto, honorario, mesesActualizacion, tipoIndice, estado, idDueno, idInquil, idInmueble, (exito, error) =>
            {
                if (guardarBtn != null) guardarBtn.interactable = true;
                if (exito)
                {
                    MostrarMensajeFormulario("Contrato registrado.", Color.green);
                    CargarTodosLosContratos();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                    Invoke(nameof(PrepararFormularioNuevo), 1.5f);
                }
                else MostrarMensajeFormulario("Error: " + error, Color.red);
            });
        }
    }

    // ═════════════════════════════════════════════
    //  LISTA
    // ═════════════════════════════════════════════

    public void CargarTodosLosContratos()
    {
        MostrarMensajeLista("Cargando...", Color.gray);
        LimpiarContenedorLista();

        daoContrato.ObtenerTodosLosContratos((exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }

            List<ContratoItemData> lista = ParsearLista(json);

            // Actualizar contador en el Menú Principal
            if (totalContratos != null) totalContratos.text = (lista != null ? lista.Count : 0).ToString();

            if (lista == null || lista.Count == 0)
            {
                _todosLosContratos.Clear();
                MostrarMensajeLista("No hay contratos registrados.", Color.gray);
                return;
            }

            MostrarMensajeLista("", Color.white);
            RenderizarLista(lista);
        });
    }

    public void CargarContratosActivos()
    {
        MostrarMensajeLista("Cargando activos...", Color.gray);
        LimpiarContenedorLista();

        daoContrato.ObtenerContratosActivos((exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }

            List<ContratoItemData> lista = ParsearLista(json);

            // Actualizar contador en el Menú Principal (opcional: o solo en ObtenerTodos)
            if (totalContratos != null) totalContratos.text = (lista != null ? lista.Count : 0).ToString();

            if (lista == null || lista.Count == 0)
            {
                _todosLosContratos.Clear();
                MostrarMensajeLista("No hay contratos activos.", Color.gray);
                return;
            }

            MostrarMensajeLista("", Color.white);
            RenderizarLista(lista);
        });
    }

    private void RenderizarLista(List<ContratoItemData> lista)
    {
        if (itemContratoPrefab == null || contenedorLista == null) return;

        // ── Fase 1: resolver nombre de inquilino para cada contrato en paralelo ──
        int pendientes = lista.Count;

        if (pendientes == 0) return;

        foreach (ContratoItemData c in lista)
        {
            long idInq = c.idInquilino;
            ContratoItemData captura = c;

            if (idInq <= 0)
            {
                captura.apellidoInquilino = "";
                captura.nombreInquilino = "";
                pendientes--;
                if (pendientes == 0) RenderizarListaOrdenada(lista, false);
                continue;
            }

            daoInquilino.ObtenerInquilinoPorId(idInq, (ok, nombre, apellido, telefono, err) =>
            {
                captura.apellidoInquilino = ok ? (apellido ?? "") : "";
                captura.nombreInquilino = ok ? (nombre ?? "") : "";
                pendientes--;
                if (pendientes == 0) RenderizarListaOrdenada(lista, false);
            });
        }
    }

    /// <summary>
    /// Ordena la lista por apellido + nombre de inquilino (A→Z) e instancia los prefabs.
    /// Se llama cuando todos los nombres ya fueron resueltos.
    /// </summary>
    private void RenderizarListaOrdenada(List<ContratoItemData> lista, bool esFiltro = false)
    {
        if (!esFiltro)
        {
            _todosLosContratos = new List<ContratoItemData>(lista);
        }

        lista.Sort((a, b) =>
        {
            int cmp = string.Compare(a.apellidoInquilino, b.apellidoInquilino, System.StringComparison.OrdinalIgnoreCase);
            if (cmp != 0) return cmp;
            return string.Compare(a.nombreInquilino, b.nombreInquilino, System.StringComparison.OrdinalIgnoreCase);
        });

        foreach (ContratoItemData c in lista)
        {
            GameObject item = Instantiate(itemContratoPrefab, contenedorLista);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();

            // texto[0]: Dirección Inmueble (se carga async)
            if (textos.Length >= 1) textos[0].text = "Cargando Inmueble...";

            // texto[1]: Nombre del Inquilino — ya lo tenemos
            if (textos.Length >= 2)
                textos[1].text = string.IsNullOrEmpty(c.apellidoInquilino)
                    ? $"Inquilino #{c.idInquilino}"
                    : $"{c.apellidoInquilino}, {c.nombreInquilino}";

            // texto[2]: Fechas
            string fechaFin = string.IsNullOrEmpty(c.fechaFin) ? "Sin fecha fin" : FormatoArgentino(c.fechaFin);
            if (textos.Length >= 3) textos[2].text = $"{FormatoArgentino(c.fechaInicio)}  →  {fechaFin}";

            // texto[3]: Monto
            if (textos.Length >= 4)
                textos[3].text = $"$ {c.montoAlquiler:N0}";

            // texto[4]: Estado con color
            bool activo = c.estado == 1;
            string colorHex = activo ? "#27AE60" : "#E74C3C";
            if (textos.Length >= 5)
            {
                textos[4].text = $"<color={colorHex}>● {(activo ? "Activo" : "Inactivo")}</color>";
                textos[4].richText = true;
            }

            // texto[5]: Frecuencia de Actualización (Meses)
            if (textos.Length >= 6)
                textos[5].text = c.mesesActualizacion > 0 ? $"Cada {c.mesesActualizacion} meses" : "Sin act.";

            // texto[6]: Tipo de Índice
            if (textos.Length >= 7)
                textos[6].text = !string.IsNullOrEmpty(c.tipoIndice) ? c.tipoIndice : "-";

            // ── Datos para WhatsApp ──
            string[] datosWA = new string[3];
            datosWA[0] = $"{c.nombreInquilino} {c.apellidoInquilino}".Trim();
            long montoContrato = c.montoAlquiler;

            // Cargar teléfono del inquilino (aún lo necesitamos para WhatsApp)
            long idInq = c.idInquilino;
            if (idInq > 0)
            {
                daoInquilino.ObtenerInquilinoPorId(idInq, (ok, nombre, apellido, telefono, err) =>
                {
                    if (ok) datosWA[1] = telefono.ToString();
                });
            }

            // Cargar dirección del inmueble de forma async
            long idInm = c.idInmueble;
            TMP_Text textoInmueble = textos.Length >= 1 ? textos[0] : null;
            if (idInm > 0 && textoInmueble != null)
            {
                daoInmueble.ObtenerInmueblePorId(idInm, (ok, jsonInm, errInm) =>
                {
                    if (!ok || textoInmueble == null) return;
                    try
                    {
                        InmuebleJsonArray arrI = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + jsonInm + "}");
                        if (arrI?.items != null && arrI.items.Length > 0)
                        {
                            string dir = $"{arrI.items[0].Direccion} {arrI.items[0].Numero_Direccion}";
                            textoInmueble.text = dir;
                            datosWA[2] = dir;
                        }
                        else textoInmueble.text = $"Inmueble #{idInm}";
                    }
                    catch { textoInmueble.text = $"Inmueble #{idInm}"; }
                });
            }

            long id = c.id;
            string resumen = $"Contrato #{c.id}";

            Button[] botones = item.GetComponentsInChildren<Button>();
            Debug.Log($"[ContratoUI] Prefab contrato #{c.id}: encontrados {botones.Length} botones: {string.Join(", ", System.Array.ConvertAll(botones, b => b.gameObject.name))}");
            foreach (Button btn in botones)
            {
                string nombre = btn.gameObject.name.ToLower();
                if (nombre.Contains("editar"))
                    btn.onClick.AddListener(() => AbrirEdicion(id));
                else if (nombre.Contains("eliminar"))
                    btn.onClick.AddListener(() => AbrirPopupEliminar(id, resumen));
                else if (nombre.Contains("whatsapp") || nombre.Contains("whatapp"))
                {
                    btn.onClick.AddListener(() =>
                    {
                        if (string.IsNullOrEmpty(datosWA[1]) || datosWA[1] == "0")
                        {
                            Debug.LogWarning("[ContratoUI] No se puede enviar WhatsApp: teléfono del inquilino no disponible.");
                            return;
                        }
                        long tel = long.Parse(datosWA[1]);
                        string nombreInq = datosWA[0] ?? "Inquilino";
                        string direccion = datosWA[2] ?? "";
                        string monto = montoContrato.ToString("N0");
                        WhatsAppHelper.EnviarRecordatorioPago(tel, nombreInq, direccion, monto);
                    });
                }
            }
        }
    }

    private void LimpiarContenedorLista()
    {
        if (contenedorLista == null) return;
        foreach (Transform hijo in contenedorLista) Destroy(hijo.gameObject);
    }

    private void OnBuscarPorInquilino()
    {
        string termino = buscadorInquilinoInput != null ? buscadorInquilinoInput.text.Trim().ToLowerInvariant() : "";
        if (string.IsNullOrEmpty(termino))
        {
            LimpiarContenedorLista();
            if (_todosLosContratos.Count == 0)
            {
                MostrarMensajeLista("No hay contratos registrados.", Color.gray);
            }
            else
            {
                MostrarMensajeLista("", Color.white);
                RenderizarListaOrdenada(_todosLosContratos, true);
            }
            return;
        }

        var filtrados = _todosLosContratos.FindAll(c =>
        {
            string nombre = $"{c.apellidoInquilino} {c.nombreInquilino}".ToLowerInvariant();
            nombre = nombre.Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u');
            string t2 = termino.Replace('á','a').Replace('é','e').Replace('í','i').Replace('ó','o').Replace('ú','u');
            return nombre.Contains(t2);
        });

        LimpiarContenedorLista();
        if (filtrados.Count == 0)
        {
            MostrarMensajeLista($"Sin contratos para \"{buscadorInquilinoInput.text.Trim()}\".", Color.gray);
        }
        else
        {
            MostrarMensajeLista("", Color.white);
            RenderizarListaOrdenada(filtrados, true);
        }
    }

    private void OnLimpiarBusqueda()
    {
        if (buscadorInquilinoInput != null) buscadorInquilinoInput.text = "";
        LimpiarContenedorLista();
        if (_todosLosContratos.Count == 0)
        {
            MostrarMensajeLista("No hay contratos registrados.", Color.gray);
        }
        else
        {
            MostrarMensajeLista("", Color.white);
            RenderizarListaOrdenada(_todosLosContratos, true);
        }
    }

    // ═════════════════════════════════════════════
    //  POPUP ELIMINAR
    // ═════════════════════════════════════════════

    private void AbrirPopupEliminar(long id, string resumen)
    {
        Debug.Log($"[ContratoUI] AbrirPopupEliminar llamado — id={id}, resumen={resumen}");
        Debug.Log($"[ContratoUI] panelConfirmacionEliminar es null: {panelConfirmacionEliminar == null}");
        Debug.Log($"[ContratoUI] mensajeConfirmacionText es null: {mensajeConfirmacionText == null}");
        Debug.Log($"[ContratoUI] confirmarEliminarBtn es null: {confirmarEliminarBtn == null}");
        idContratoSeleccionado = id;
        if (mensajeConfirmacionText != null)
            mensajeConfirmacionText.text = "¿Estás seguro de que querés eliminar este contrato?\nEsta acción no se puede deshacer.";
        if (panelConfirmacionEliminar != null)
            panelConfirmacionEliminar.SetActive(true);
        else
            Debug.LogError("[ContratoUI] ¡panelConfirmacionEliminar NO ESTÁ ASIGNADO en el Inspector! El popup no puede mostrarse.");
    }

    private void CerrarPopupEliminar()
    {
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(false);
    }

    private void OnConfirmarEliminar()
    {
        Debug.Log($"[ContratoUI] OnConfirmarEliminar llamado — idContratoSeleccionado={idContratoSeleccionado}");
        if (idContratoSeleccionado <= 0)
        {
            Debug.LogError("[ContratoUI] ID de contrato inválido (<= 0), cancelando eliminación.");
            return;
        }
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = false;

        daoContrato.EliminarContrato(idContratoSeleccionado, (exito, error) =>
        {
            Debug.Log($"[ContratoUI] Resultado EliminarContrato — exito={exito}, error={error}");
            if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;
            if (exito)
            {
                CargarTodosLosContratos();
                GlobalDropdownRefreshManager.NotifyDataChanged();
                if (modoEdicion) PrepararFormularioNuevo();
                CerrarPopupEliminar();
            }
            else
            {
                if (mensajeConfirmacionText != null)
                    mensajeConfirmacionText.text = "Error: " + error;
                Debug.LogError("[ContratoUI] Error al eliminar contrato: " + error);
            }
        });
    }

    // ═════════════════════════════════════════════
    //  HELPERS & JSON
    // ═════════════════════════════════════════════

    private void MostrarMensajeFormulario(string m, Color c) { if (mensajeFormularioText != null) { mensajeFormularioText.text = m; mensajeFormularioText.color = c; } }
    private void MostrarMensajeLista(string m, Color c) { if (mensajeListaText != null) { mensajeListaText.gameObject.SetActive(!string.IsNullOrEmpty(m)); mensajeListaText.text = m; mensajeListaText.color = c; } }

    /// <summary>
    /// Actualiza el texto de la etiqueta del Toggle a "Activo" o "Inactivo".
    /// Se llama automáticamente cuando el usuario toca el Toggle.
    /// </summary>
    private void ActualizarEtiquetaEstado(bool activo)
    {
        if (estadoToggleLabel == null) return;
        estadoToggleLabel.text = activo ? "Contrato Activo" : "Contrato Inactivo";
        estadoToggleLabel.color = activo ? Color.green : Color.red;
    }

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

    // ── Clases de datos internos ──
    private class ContratoItemData { public long id; public string fechaInicio; public string fechaFin; public long montoAlquiler; public long estado; public long idInquilino; public long idInmueble; public string apellidoInquilino = ""; public string nombreInquilino = ""; public long mesesActualizacion; public string tipoIndice; }

    // ── Clases JSON Contrato ──
    [Serializable] private class ContratoJsonItem { public long id_contrato; public string FechaInicio; public string FechaFIn; public long MontoAlquiler; public long HonorarioPorcentaje; public long MesesActualizacion; public string TipoIndice; public long Estado; public long id_Duenos; public long id_Inquilino; public long id_Inmueble; }
    [Serializable] private class ContratoJsonArray { public ContratoJsonItem[] items; }

    // ── Clases JSON Dueño ──
    [Serializable] private class DuenoJsonItem { public long id_Dueno; public string Nombre_Dueno; public string Apellido_Dueno; }
    [Serializable] private class DuenoJsonArray { public DuenoJsonItem[] items; }

    // ── Clases JSON Inquilino ──
    [Serializable] private class InquilinoJsonItem { public long id_Inquilino; public string Nombre_Inquilinos; public string Apellido_Inquilinos; }
    [Serializable] private class InquilinoJsonArray { public InquilinoJsonItem[] items; }

    // ── Clases JSON Inmueble ──
    [Serializable] private class InmuebleJsonItem { public long id_Propiedad; public string Direccion; public int Numero_Direccion; }
    [Serializable] private class InmuebleJsonArray { public InmuebleJsonItem[] items; }

    private List<ContratoItemData> ParsearLista(string json)
    {
        var resultado = new List<ContratoItemData>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return resultado;
        try
        {
            ContratoJsonArray arr = JsonUtility.FromJson<ContratoJsonArray>("{\"items\":" + json + "}");
            if (arr?.items == null) return resultado;
            foreach (var c in arr.items)
                resultado.Add(new ContratoItemData
                {
                    id = c.id_contrato,
                    fechaInicio = c.FechaInicio,
                    fechaFin = c.FechaFIn,
                    montoAlquiler = c.MontoAlquiler,
                    estado = c.Estado,
                    idInquilino = c.id_Inquilino,
                    idInmueble = c.id_Inmueble,
                    mesesActualizacion = c.MesesActualizacion,
                    tipoIndice = c.TipoIndice
                });
        }
        catch (Exception ex) { Debug.LogError("[ContratoUI] Error JSON: " + ex.Message); }
        return resultado;
    }
}