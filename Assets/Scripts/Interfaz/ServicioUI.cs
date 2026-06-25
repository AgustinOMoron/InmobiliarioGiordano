using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Maneja la interfaz de usuario para la gestión de Servicios.
/// RF-05: Registro y seguimiento de servicios vinculados a propietarios y propiedades.
///
/// ═══════════════════════════════════════════════════════════════
///  GUÍA PARA ARMAR LA ESCENA EN UNITY (DISEÑO DASHBOARD)
/// ═══════════════════════════════════════════════════════════════
///
///  Canvas
///  └── Panel_Servicios                         ← Contenedor raíz
///      ├── Lado_Izquierdo_Lista                ← ~55% de la pantalla
///      │   ├── Filtros_Arriba
///      │   │   ├── Dropdown_FiltrosPropietario ← propietarioFiltroDropdown
///      │   │   │     (primer ítem: "-- Todos los Propietarios --")
///      │   │   ├── Button_Filtrar              ← filtrarBtn
///      │   │   └── Button_VerTodos             ← verTodosBtn
///      │   ├── ScrollView → Content            ← contenedorLista
///      │   └── Text_MensajeLista               ← mensajeListaText
///      │
///      ├── Lado_Derecho_Formulario             ← ~45% de la pantalla
///      │   ├── Text_TituloFormulario           ← tituloFormularioText
///      │   ├── InputField_NombreServicio       ← nombreServicioInput
///      │   ├── InputField_Monto                ← montoInput         (Decimal Number, ej: 15000.50)
///      │   ├── InputField_Fecha                ← fechaInput         (ej: "15/03/2024")
///      │   ├── InputField_Porcentaje           ← porcentajeInput    (Decimal Number, ej: 10 = 10%)
///      │   ├── Dropdown_Propietario            ← propietarioDropdown
///      │   │     (primer ítem: "-- Seleccionar Propietario --")
///      │   │     (se carga automáticamente con todos los dueños)
///      │   ├── Dropdown_Propiedad              ← propiedadDropdown
///      │   │     (primer ítem: "-- Seleccionar Propiedad --")
///      │   │     (se carga automáticamente al seleccionar un propietario)
///      │   ├── Text_MensajeFormulario          ← mensajeFormularioText
///      │   ├── Button_Guardar                  ← guardarBtn
///      │   └── Button_LimpiarFormulario        ← limpiarFormularioBtn
///      │
///      └── Panel_ConfirmacionEliminar          ← panelConfirmacionEliminar (POPUP)
///          ├── Text_MensajeConfirmacion        ← mensajeConfirmacionText
///          ├── Button_ConfirmarEliminar        ← confirmarEliminarBtn
///          └── Button_CancelarEliminar        ← cancelarEliminarBtn
///
///  PREFAB ÍTEM DE LISTA (ItemServicio) — un TMP_Text por dato:
///      ├── Text_NombreServicio                ← textos[0]  ej: "MUNICIPAL"
///      ├── Text_Fecha                         ← textos[1]  ej: "15/03/2024"
///      ├── Text_Monto                         ← textos[2]  ej: "$ 4.000,00"
///      ├── Text_Porcentaje                    ← textos[3]  ej: "100%"
///      ├── Text_Propiedad                     ← textos[4]  ej: "BV. CHACABUCO 253"
///      ├── Button_Editar                      ← nombre debe contener "editar"
///      └── Button_Eliminar                    ← nombre debe contener "eliminar"
///
///  NOTAS:
///  - Al seleccionar un propietario en el formulario, el dropdown de propiedades
///    se carga automáticamente con sus inmuebles.
///  - La propiedad es opcional; si no se selecciona se guarda sin FK de inmueble.
///  - La fecha se ingresa en formato DD/MM/YYYY y se convierte a YYYY-MM-DD para Supabase.
///  - El porcentaje representa la parte del servicio que paga el propietario (0–100).
///
/// ═══════════════════════════════════════════════════════════════
/// </summary>
public class ServicioUI : MonoBehaviour
{
    [Header("DAOs")]
    [SerializeField] private DAOServicio daoServicio;
    [SerializeField] private DAODueno daoDueno;
    [SerializeField] private DAOInmueble daoInmueble;

    [Header("Panel Izquierdo - Lista")]
    [SerializeField] private TMP_Dropdown propietarioFiltroDropdown;
    [SerializeField] private Button verTodosBtn;
    [SerializeField] private Button filtrarBtn;
    [SerializeField] private Transform contenedorLista;
    [SerializeField] private GameObject itemServicioPrefab;
    [SerializeField] private TMP_Text mensajeListaText;

    [Header("Panel Derecho - Formulario")]
    [SerializeField] private TMP_Text tituloFormularioText;
    [SerializeField] private TMP_InputField nombreServicioInput;
    [SerializeField] private TMP_InputField montoInput;
    [SerializeField] private TMP_InputField fechaInput;
    [SerializeField] private TMP_InputField porcentajeInput;
    [SerializeField] private TMP_Dropdown propietarioDropdown;
    [SerializeField] private TMP_Dropdown propiedadDropdown;   // ← NUEVO: dropdown de propiedades
    [SerializeField] private TMP_Text mensajeFormularioText;
    [SerializeField] private Button guardarBtn;
    [SerializeField] private Button limpiarFormularioBtn;

    [Header("Popup Confirmación")]
    [SerializeField] private GameObject panelConfirmacionEliminar;
    [SerializeField] private TMP_Text mensajeConfirmacionText;
    [SerializeField] private Button confirmarEliminarBtn;
    [SerializeField] private Button cancelarEliminarBtn;

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private long idServicioSeleccionado = -1;
    private bool modoEdicion = false;
    private bool _suprimirCambioPropietario = false; // Evita doble carga al asignar value programáticamente

    // Mapeos índice dropdown → ID real
    private List<long> idsPropietarios = new List<long>();
    private List<long> idsPropiedades  = new List<long>();
    
    // Mapeo global para la lista de servicios (id_propiedad -> Direccion)
    private Dictionary<long, string> mapaPropiedadesGlobal = new Dictionary<long, string>();

    /// <summary>
    /// Evento que se dispara cada vez que los servicios cambian (alta, baja o modificación).
    /// MenuPrincipalUI se suscribe para refrescar el dashboard automáticamente.
    /// </summary>
    public static event Action OnServiciosCambiados;

    // ─────────────────────────────────────────────
    //  Clases JSON
    // ─────────────────────────────────────────────

    [Serializable]
    private class ServicioJsonItem
    {
        public long id;
        public string Nombre_servicio;
        public float MontoTotal;
        public string FechaServicio;
        public float PorcentajePagar;
        public long id_Propietario;
        public long id_propiedad;   // nombre exacto de la columna en Supabase
    }
    [Serializable] private class ServicioJsonArray { public ServicioJsonItem[] items; }

    [Serializable]
    private class DuenoJsonItem { public long id_Dueno; public string Nombre_Dueno; public string Apellido_Dueno; }
    [Serializable] private class DuenoJsonArray { public DuenoJsonItem[] items; }

    [Serializable]
    private class InmuebleJsonItem { public long id_Propiedad; public string Direccion; public int Numero_Direccion; }
    [Serializable] private class InmuebleJsonArray { public InmuebleJsonItem[] items; }

    // ═════════════════════════════════════════════
    //  INICIALIZACIÓN
    // ═════════════════════════════════════════════

    private void Awake()
    {
        if (daoServicio == null) daoServicio = FindObjectOfType<DAOServicio>();
        if (daoDueno    == null) daoDueno    = FindObjectOfType<DAODueno>();
        if (daoInmueble == null) daoInmueble = FindObjectOfType<DAOInmueble>();

        if (verTodosBtn          != null) verTodosBtn.onClick.AddListener(CargarTodosLosServicios);
        if (filtrarBtn           != null) filtrarBtn.onClick.AddListener(OnFiltrarServicios);
        if (guardarBtn           != null) guardarBtn.onClick.AddListener(OnGuardar);
        if (limpiarFormularioBtn != null) limpiarFormularioBtn.onClick.AddListener(PrepararFormularioNuevo);
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.onClick.AddListener(OnConfirmarEliminar);
        if (cancelarEliminarBtn  != null) cancelarEliminarBtn.onClick.AddListener(CerrarPopupEliminar);

        if (propietarioDropdown != null)
            propietarioDropdown.onValueChanged.AddListener(OnPropietarioFormularioChanged);
    }

    private void OnEnable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged += RefrescarDropdowns;
        CerrarPopupEliminar();
        idsPropietarios.Clear();
        idsPropiedades.Clear();
        
        // Cargar mapa de propiedades primero, luego dueños, luego lista
        CargarMapaPropiedadesGlobal(() =>
        {
            CargarDropdownPropietarios(() =>
            {
                PrepararFormularioNuevo();
                CargarTodosLosServicios();
            });
        });
    }

    private void OnDisable()
    {
        GlobalDropdownRefreshManager.OnAnyDataChanged -= RefrescarDropdowns;
    }

    private void CargarMapaPropiedadesGlobal(Action onDone)
    {
        mapaPropiedadesGlobal.Clear();
        if (daoInmueble == null) { onDone?.Invoke(); return; }

        daoInmueble.ObtenerTodosLosInmuebles((exito, json, error) =>
        {
            if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
            {
                try
                {
                    var arr = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
                    if (arr?.items != null)
                    {
                        foreach (var inm in arr.items)
                        {
                            mapaPropiedadesGlobal[inm.id_Propiedad] = $"{inm.Direccion} {inm.Numero_Direccion}".ToUpper();
                        }
                    }
                }
                catch (Exception ex) { Debug.LogError("[ServicioUI] Error parseando mapa de propiedades: " + ex.Message); }
            }
            onDone?.Invoke();
        });
    }

    // ═════════════════════════════════════════════
    //  CARGA DE DROPDOWNS
    // ═════════════════════════════════════════════

    private void CargarDropdownPropietarios(Action onDone)
    {
        idsPropietarios.Clear();
        if (propietarioDropdown      != null) propietarioDropdown.ClearOptions();
        if (propietarioFiltroDropdown != null) propietarioFiltroDropdown.ClearOptions();

        var opciones       = new List<string> { "-- Seleccionar Propietario --" };
        var opcionesFiltro = new List<string> { "-- Todos los Propietarios --" };

        daoDueno.ObtenerTodosLosDuenos((exito, json, error) =>
        {
            if (exito && !string.IsNullOrEmpty(json) && json.Trim() != "[]")
            {
                try
                {
                    var arr = JsonUtility.FromJson<DuenoJsonArray>("{\"items\":" + json + "}");
                    if (arr?.items != null)
                        foreach (var d in arr.items)
                        {
                            idsPropietarios.Add(d.id_Dueno);
                            opciones.Add(d.Apellido_Dueno + ", " + d.Nombre_Dueno);
                            opcionesFiltro.Add(d.Apellido_Dueno + ", " + d.Nombre_Dueno);
                        }
                }
                catch (Exception ex) { Debug.LogError("[ServicioUI] Error parseando propietarios: " + ex.Message); }
            }

            if (propietarioDropdown != null)
            {
                propietarioDropdown.AddOptions(opciones);
                propietarioDropdown.value = 0;
                propietarioDropdown.RefreshShownValue();
            }
            if (propietarioFiltroDropdown != null)
            {
                propietarioFiltroDropdown.AddOptions(opcionesFiltro);
                propietarioFiltroDropdown.value = 0;
                propietarioFiltroDropdown.RefreshShownValue();
            }
            onDone?.Invoke();
        });
    }

    /// <summary>Se dispara al cambiar el propietario en el formulario; recarga el dropdown de propiedades.</summary>
    private void OnPropietarioFormularioChanged(int index)
    {
        // Si el cambio fue programático (desde AbrirEdicion), no recargar de nuevo
        if (_suprimirCambioPropietario) return;
        LimpiarDropdownPropiedades();
        if (index <= 0 || index - 1 >= idsPropietarios.Count) return;

        long idPropietario = idsPropietarios[index - 1];
        CargarPropiedadesDePropietario(idPropietario);
    }

    public void RefrescarDropdowns()
    {
        long idPropietarioSeleccionado = 0;
        if (propietarioDropdown != null && propietarioDropdown.value > 0 && propietarioDropdown.value - 1 < idsPropietarios.Count)
            idPropietarioSeleccionado = idsPropietarios[propietarioDropdown.value - 1];

        long idPropiedadSeleccionada = 0;
        if (propiedadDropdown != null && propiedadDropdown.value > 0 && propiedadDropdown.value - 1 < idsPropiedades.Count)
            idPropiedadSeleccionada = idsPropiedades[propiedadDropdown.value - 1];

        CargarMapaPropiedadesGlobal(() =>
        {
            CargarDropdownPropietarios(() =>
            {
                if (idPropietarioSeleccionado > 0)
                {
                    int idx = idsPropietarios.IndexOf(idPropietarioSeleccionado);
                    if (idx >= 0 && propietarioDropdown != null)
                    {
                        _suprimirCambioPropietario = true;
                        propietarioDropdown.value = idx + 1;
                        propietarioDropdown.RefreshShownValue();
                        _suprimirCambioPropietario = false;
                        CargarPropiedadesDePropietario(idPropietarioSeleccionado, idPropiedadSeleccionada);
                        return;
                    }
                }

                MostrarMensajeFormulario("✓ Listas actualizadas.", Color.green);
                Invoke(nameof(LimpiarMensajeFormulario), 2f);
            });
        });
    }

    private void CargarPropiedadesDePropietario(long idPropietario, long preseleccionarId = 0)
    {
        LimpiarDropdownPropiedades(); // ya agrega "-- Sin propiedad específica --" en índice 0

        daoInmueble.ObtenerInmueblesPorDueno(idPropietario, (exito, json, error) =>
        {
            if (!exito || string.IsNullOrEmpty(json) || json.Trim() == "[]") return;

            try
            {
                var arr = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
                if (arr?.items == null) return;

                // Solo agregar las direcciones reales — el placeholder ya está en el dropdown
                var opciones = new List<string>();
                foreach (var inm in arr.items)
                {
                    idsPropiedades.Add(inm.id_Propiedad);
                    opciones.Add(inm.Direccion + " " + inm.Numero_Direccion);
                }

                if (propiedadDropdown != null)
                {
                    propiedadDropdown.AddOptions(opciones);
                    // Pre-seleccionar si estamos en modo edición
                    // índice 0 = "Sin propiedad", índice 1..N = propiedades reales
                    if (preseleccionarId > 0)
                    {
                        int idx = idsPropiedades.IndexOf(preseleccionarId);
                        propiedadDropdown.value = idx >= 0 ? idx + 1 : 0;
                    }
                }
            }
            catch (Exception ex) { Debug.LogError("[ServicioUI] Error parseando propiedades: " + ex.Message); }
        });
    }

    private void LimpiarDropdownPropiedades()
    {
        idsPropiedades.Clear();
        if (propiedadDropdown != null)
        {
            propiedadDropdown.ClearOptions();
            propiedadDropdown.AddOptions(new List<string> { "-- Sin propiedad específica --" });
        }
    }

    // ═════════════════════════════════════════════
    //  LISTA
    // ═════════════════════════════════════════════

    private void CargarTodosLosServicios()
    {
        MostrarMensajeLista("Cargando servicios...", Color.gray);
        LimpiarLista();

        daoServicio.ObtenerTodosLosServicios((exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }
            var servicios = ParsearListaServicios(json);
            if (servicios.Count == 0) { MostrarMensajeLista("No hay servicios registrados.", Color.gray); return; }
            RenderizarLista(servicios);
            MostrarMensajeLista("", Color.white);
        });
    }

    private void OnFiltrarServicios()
    {
        if (propietarioFiltroDropdown == null || propietarioFiltroDropdown.value == 0)
        { CargarTodosLosServicios(); return; }

        long idPropietario = idsPropietarios[propietarioFiltroDropdown.value - 1];
        MostrarMensajeLista("Filtrando...", Color.gray);
        LimpiarLista();

        daoServicio.ObtenerServiciosPorPropietario(idPropietario, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }
            var servicios = ParsearListaServicios(json);
            if (servicios.Count == 0) { MostrarMensajeLista("No hay servicios para ese propietario.", Color.gray); return; }
            RenderizarLista(servicios);
            MostrarMensajeLista("", Color.white);
        });
    }

    private void RenderizarLista(List<ServicioJsonItem> servicios)
    {
        LimpiarLista();
        if (itemServicioPrefab == null || contenedorLista == null) return;

        foreach (var s in servicios)
        {
            var item   = Instantiate(itemServicioPrefab, contenedorLista);
            var textos = item.GetComponentsInChildren<TMP_Text>(true);

            // Un TMP_Text por columna — igual al formato del Excel
            if (textos.Length >= 1) textos[0].text = s.Nombre_servicio?.ToUpper() ?? "";
            if (textos.Length >= 2) textos[1].text = FormatoArgentino(s.FechaServicio);
            if (textos.Length >= 3) textos[2].text = "$ " + s.MontoTotal.ToString("N2");
            if (textos.Length >= 4) textos[3].text = s.PorcentajePagar.ToString("F0") + "%";
            if (textos.Length >= 5) 
            {
                if (s.id_propiedad > 0 && mapaPropiedadesGlobal.TryGetValue(s.id_propiedad, out string dir))
                    textos[4].text = dir;
                else
                    textos[4].text = "—";
            }

            foreach (var btn in item.GetComponentsInChildren<Button>(true))
            {
                string lower = btn.name.ToLowerInvariant();
                long sid = s.id;
                if (lower.Contains("editar"))   btn.onClick.AddListener(() => AbrirEdicion(sid));
                if (lower.Contains("eliminar")) btn.onClick.AddListener(() => MostrarPopupEliminar(sid));
            }
        }
    }

    private void LimpiarLista()
    {
        if (contenedorLista == null) return;
        foreach (Transform child in contenedorLista) Destroy(child.gameObject);
    }

    // ═════════════════════════════════════════════
    //  FORMULARIO
    // ═════════════════════════════════════════════

    public void PrepararFormularioNuevo()
    {
        modoEdicion = false;
        idServicioSeleccionado = -1;

        if (nombreServicioInput != null) nombreServicioInput.text = "";
        if (montoInput          != null) montoInput.text          = "";
        if (fechaInput          != null) fechaInput.text          = "";
        if (porcentajeInput     != null) porcentajeInput.text     = "";
        if (propietarioDropdown != null) propietarioDropdown.value = 0;
        LimpiarDropdownPropiedades();

        MostrarMensajeFormulario("", Color.white);
        if (tituloFormularioText != null) tituloFormularioText.text = "Nuevo Servicio";
    }

    private void AbrirEdicion(long idServicio)
    {
        modoEdicion = true;
        idServicioSeleccionado = idServicio;
        if (tituloFormularioText != null) tituloFormularioText.text = "Editar Servicio";
        MostrarMensajeFormulario("Cargando servicio...", Color.gray);

        daoServicio.ObtenerServicioPorId(idServicio, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeFormulario("Error: " + error, Color.red); return; }

            var lista = ParsearListaServicios(json);
            if (lista.Count == 0) { MostrarMensajeFormulario("Servicio no encontrado.", Color.red); return; }

            var s = lista[0];
            if (nombreServicioInput != null) nombreServicioInput.text  = s.Nombre_servicio;
            if (montoInput          != null) montoInput.text           = s.MontoTotal.ToString("F2");
            if (fechaInput          != null) fechaInput.text           = FormatoArgentino(s.FechaServicio);
            if (porcentajeInput     != null) porcentajeInput.text      = s.PorcentajePagar.ToString("F2");

        
            _suprimirCambioPropietario = true;
            try
            {
                if (propietarioDropdown != null)
    {
        int idxProp = idsPropietarios.IndexOf(s.id_Propietario);
        propietarioDropdown.value = idxProp >= 0 ? idxProp + 1 : 0;
    }
            }
            finally{ _suprimirCambioPropietario = false; }
            

            // Cargar propiedades del propietario y pre-seleccionar la FK de la propiedad
            if (s.id_Propietario > 0)
                CargarPropiedadesDePropietario(s.id_Propietario, preseleccionarId: s.id_propiedad);

            MostrarMensajeFormulario("", Color.white);
        });
    }

    // ═════════════════════════════════════════════
    //  GUARDAR
    // ═════════════════════════════════════════════

    private void OnGuardar()
    {
        string nombreServicio = nombreServicioInput != null ? nombreServicioInput.text.Trim() : "";
        if (string.IsNullOrEmpty(nombreServicio))
        { MostrarMensajeFormulario("El nombre del servicio es obligatorio.", Color.red); return; }

        if (!float.TryParse(montoInput?.text.Trim(), out float montoTotal) || montoTotal < 0f)
        { MostrarMensajeFormulario("El monto debe ser un número válido mayor o igual a 0.", Color.red); return; }

        string fechaTexto = fechaInput != null ? fechaInput.text.Trim() : "";
        if (string.IsNullOrEmpty(fechaTexto))
        { MostrarMensajeFormulario("La fecha del servicio es obligatoria. (ej: 15/03/2024)", Color.red); return; }

        string fechaSupabase = FormatoSupabase(fechaTexto);
        if (string.IsNullOrEmpty(fechaSupabase))
        { MostrarMensajeFormulario("Formato de fecha inválido. Usá DD/MM/YYYY.", Color.red); return; }

        if (!float.TryParse(porcentajeInput?.text.Trim(), out float porcentajePagar) || porcentajePagar < 0f || porcentajePagar > 100f)
        { MostrarMensajeFormulario("El porcentaje debe ser un número entre 0 y 100.", Color.red); return; }

        if (propietarioDropdown == null || propietarioDropdown.value == 0)
        { MostrarMensajeFormulario("Seleccioná un propietario.", Color.red); return; }

        long idPropietario = idsPropietarios[propietarioDropdown.value - 1];

        // Propiedad es opcional (índice 0 = "Sin propiedad")
        long idPropiedad = 0;
        if (propiedadDropdown != null && propiedadDropdown.value > 0 && propiedadDropdown.value - 1 < idsPropiedades.Count)
            idPropiedad = idsPropiedades[propiedadDropdown.value - 1];

        MostrarMensajeFormulario("Guardando...", Color.gray);

        if (modoEdicion && idServicioSeleccionado > 0)
        {
            daoServicio.ActualizarServicio(idServicioSeleccionado, nombreServicio, montoTotal, fechaSupabase, idPropietario, porcentajePagar, idPropiedad, (exito, error) =>
            {
                if (exito)
                {
                    MostrarMensajeFormulario("Servicio actualizado.", Color.green);
                    CargarTodosLosServicios();
                    OnServiciosCambiados?.Invoke(); // Notifica al dashboard
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                }
                else MostrarMensajeFormulario("Error: " + error, Color.red);
            });
        }
        else
        {
            daoServicio.RegistrarServicio(nombreServicio, montoTotal, fechaSupabase, idPropietario, porcentajePagar, idPropiedad, (exito, error) =>
            {
                if (exito)
                {
                    MostrarMensajeFormulario("Servicio registrado.", Color.green);
                    PrepararFormularioNuevo();
                    CargarTodosLosServicios();
                    OnServiciosCambiados?.Invoke(); // Notifica al dashboard
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                }
                else MostrarMensajeFormulario("Error: " + error, Color.red);
            });
        }
    }

    // ═════════════════════════════════════════════
    //  POPUP ELIMINAR
    // ═════════════════════════════════════════════

    private void MostrarPopupEliminar(long idServicio)
    {
        idServicioSeleccionado = idServicio;
        if (mensajeConfirmacionText  != null) mensajeConfirmacionText.text = $"¿Eliminar servicio #{idServicio}?\nEsta acción no se puede deshacer.";
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(true);
    }

    private void OnConfirmarEliminar()
    {
        if (idServicioSeleccionado <= 0) return;
        daoServicio.EliminarServicio(idServicioSeleccionado, (exito, error) =>
        {
            if (exito)
            {
                CerrarPopupEliminar();
                CargarTodosLosServicios();
                OnServiciosCambiados?.Invoke(); // Notifica al dashboard
                GlobalDropdownRefreshManager.NotifyDataChanged();
            }
            else MostrarMensajeLista("Error al eliminar: " + error, Color.red);
        });
    }

    private void CerrarPopupEliminar()
    {
        if (panelConfirmacionEliminar != null) panelConfirmacionEliminar.SetActive(false);
    }

    // ═════════════════════════════════════════════
    //  HELPERS
    // ═════════════════════════════════════════════

    private List<ServicioJsonItem> ParsearListaServicios(string json)
    {
        var lista = new List<ServicioJsonItem>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return lista;
        try
        {
            var arr = JsonUtility.FromJson<ServicioJsonArray>("{\"items\":" + json + "}");
            if (arr?.items != null) lista.AddRange(arr.items);
        }
        catch (Exception ex) { Debug.LogError("[ServicioUI] Error parseando servicios: " + ex.Message); }
        return lista;
    }

    private void MostrarMensajeFormulario(string m, Color c)
    { if (mensajeFormularioText != null) { mensajeFormularioText.text = m; mensajeFormularioText.color = c; } }

    private void LimpiarMensajeFormulario()
    { if (mensajeFormularioText != null) { mensajeFormularioText.text = ""; mensajeFormularioText.color = Color.white; } }

    private void MostrarMensajeLista(string m, Color c)
    { if (mensajeListaText != null) { mensajeListaText.text = m; mensajeListaText.color = c; } }

    private string FormatoSupabase(string ddmmyyyy)
    {
        if (string.IsNullOrEmpty(ddmmyyyy)) return null;
        string[] p = ddmmyyyy.Split('-', '/');
        if (p.Length != 3) return null;
        return $"{p[2]}-{p[1]}-{p[0]}";
    }

    private string FormatoArgentino(string yyyymmdd)
    {
        if (string.IsNullOrEmpty(yyyymmdd)) return "";
        string[] p = yyyymmdd.Split('-');
        if (p.Length >= 3) return $"{p[2]}/{p[1]}/{p[0]}";
        return yyyymmdd;
    }
}
