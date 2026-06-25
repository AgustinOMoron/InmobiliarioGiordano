using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Panel Unificado: Dueños + sus Inmuebles en una sola pantalla.
///
/// JERARQUÍA EN UNITY:
/// Canvas
/// └── Panel_DuenoInmueble
///     ├── Lado_Izquierdo
///     │   ├── InputField_Busqueda       ← busquedaInput
///     │   ├── Button_Buscar             ← buscarBtn
///     │   ├── Button_VerTodos           ← verTodosBtn
///     │   ├── ScrollView/Content        ← contenedorListaDuenos
///     │   └── Text_MensajeLista         ← mensajeListaText
///     │
///     ├── Lado_Derecho
///     │   ├── Text_Titulo               ← tituloDuenoText
///     │   ├── InputField_Nombre         ← nombreInput
///     │   ├── InputField_Apellido       ← apellidoInput
///     │   ├── InputField_Telefono       ← telefonoInput
///     │   ├── Text_MensajeDueno         ← mensajeDuenoText
///     │   ├── Button_GuardarDueno       ← guardarDuenoBtn
///     │   ├── Button_Limpiar            ← limpiarDuenoBtn
///     │   └── Panel_Inmuebles           ← seccionInmuebles (se activa al seleccionar dueño)
///     │       ├── Button_AgregarInm     ← agregarInmuebleBtn
///     │       ├── ScrollView/Content    ← contenedorListaInmuebles
///     │       └── Text_MensajeInm       ← mensajeInmueblesText
///     │
///     ├── Panel_Confirmacion (POPUP)    ← panelConfirmacion
///     │   ├── Text_Mensaje              ← mensajeConfirmacionText
///     │   ├── Button_Confirmar          ← confirmarEliminarBtn
///     │   └── Button_Cancelar           ← cancelarEliminarBtn
///     │
///     └── Panel_ModalInmueble (POPUP)   ← panelModalInmueble
///         ├── Text_Titulo               ← tituloModalText
///         ├── InputField_Direccion      ← direccionInput
///         ├── InputField_Numero         ← numeroInput
///         ├── Dropdown_Tipo             ← tipoDropdown
///         ├── InputField_Piso           ← pisoInput  (solo Depto)
///         ├── InputField_Unidad         ← unidadInput (solo Depto)
///         ├── Text_MensajeModal         ← mensajeModalText
///         ├── Button_GuardarInm         ← guardarInmuebleBtn
///         └── Button_CancelarModal      ← cancelarModalBtn
///
/// PREFABS:
///   ItemDueno:    2x TMP_Text + Button "editar" + Button "eliminar"
///   ItemInmueble: 1x TMP_Text + Button "editar" + Button "eliminar"
/// </summary>
public partial class DuenoInmuebleUI : MonoBehaviour
{
    // ── DAOs ──────────────────────────────────────
    [Header("DAOs")]
    [SerializeField] private DAODueno daoDueno;
    [SerializeField] private DAOInmueble daoInmueble;
    [SerializeField] private DAODepartamento daoDepartamento;

    // ── Lista Dueños ──────────────────────────────
    [Header("Panel Izquierdo - Lista")]
    [SerializeField] private TMP_InputField busquedaInput;
    [SerializeField] private Button buscarBtn;
    [SerializeField] private Button verTodosBtn;
    [SerializeField] private Transform contenedorListaDuenos;
    [SerializeField] private GameObject itemDuenoPrefab;
    [SerializeField] private TMP_Text mensajeListaText;

    // ── Formulario Dueño ──────────────────────────
    [Header("Sección Dueño")]
    [SerializeField] private TMP_Text tituloDuenoText;
    [SerializeField] private TMP_InputField nombreInput;
    [SerializeField] private TMP_InputField apellidoInput;
    [SerializeField] private TMP_InputField telefonoInput;
    [SerializeField] private TMP_Text mensajeDuenoText;
    [SerializeField] private Button guardarDuenoBtn;
    [SerializeField] private Button limpiarDuenoBtn;

    // ── Sección Inmuebles ─────────────────────────
    [Header("Sección Inmuebles del Dueño")]
    [SerializeField] private GameObject seccionInmuebles;
    [SerializeField] private Button agregarInmuebleBtn;
    [SerializeField] private Transform contenedorListaInmuebles;
    [SerializeField] private GameObject itemInmueblePrefab;
    [SerializeField] private TMP_Text mensajeInmueblesText;

    // ── Popup Confirmación ────────────────────────
    [Header("Popup Confirmación")]
    [SerializeField] private GameObject panelConfirmacion;
    [SerializeField] private TMP_Text mensajeConfirmacionText;
    [SerializeField] private Button confirmarEliminarBtn;
    [SerializeField] private Button cancelarEliminarBtn;

    // ── Modal Inmueble ────────────────────────────
    [Header("Modal Formulario Inmueble")]
    [SerializeField] private GameObject panelModalInmueble;
    [SerializeField] private TMP_Text tituloModalText;
    [SerializeField] private TMP_InputField direccionInput;
    [SerializeField] private TMP_InputField numeroInput;
    [Tooltip("Barrio del inmueble (ej: General Paz, Nueva Córdoba, etc.)")]
    [SerializeField] private TMP_InputField barrioInput;
    [SerializeField] private TMP_Dropdown tipoDropdown;
    [SerializeField] private TMP_InputField pisoInput;
    [SerializeField] private TMP_InputField unidadInput;
    [SerializeField] private TMP_Text mensajeModalText;
    [SerializeField] private Button guardarInmuebleBtn;
    [SerializeField] private Button cancelarModalBtn;

    // ── Menú Principal ────────────────────────────
    [Header("Menú Principal")]
    [SerializeField] private TMP_Text totalDuenos;
    [SerializeField] private TMP_Text totalInmuebles;

    // ── Notificaciones ────────────────────────────
    [Header("Notificaciones")]
    [Tooltip("Arrastrá el componente ContratoUI para actualizar sus dropdowns al guardar o eliminar un dueño/inmueble.")]
    [SerializeField] private ContratoUI contratoUI;

    [Tooltip("Arrastrá el componente ServicioUI para actualizar sus dropdowns cuando cambian los propietarios.")]
    [SerializeField] private ServicioUI servicioUI;

    // ── Estado interno ────────────────────────────
    private long idDuenoActual = -1;
    private bool modoEdicionDueno = false;
    private long idInmuebleEnEdicion = -1;
    private long idDepartamentoEnEdicion = -1;
    private bool modoEdicionInmueble = false;
    private enum TipoEliminar { Dueno, Inmueble }
    private TipoEliminar tipoEliminarPendiente;
    private long idEliminarPendiente = -1;

    // ═════════════════════════════════════════════
    //  INICIALIZACIÓN
    // ═════════════════════════════════════════════

    private void Awake()
    {
        if (daoDueno == null) daoDueno = FindObjectOfType<DAODueno>();
        if (daoInmueble == null) daoInmueble = FindObjectOfType<DAOInmueble>();
        if (daoDepartamento == null) daoDepartamento = FindObjectOfType<DAODepartamento>();

        if (buscarBtn != null) buscarBtn.onClick.AddListener(OnBuscar);
        if (verTodosBtn != null) verTodosBtn.onClick.AddListener(CargarListaDuenos);
        if (busquedaInput != null) busquedaInput.onSubmit.AddListener(_ => OnBuscar());
        if (guardarDuenoBtn != null) guardarDuenoBtn.onClick.AddListener(OnGuardarDueno);
        if (limpiarDuenoBtn != null) limpiarDuenoBtn.onClick.AddListener(PrepararFormularioNuevo);
        if (agregarInmuebleBtn != null) agregarInmuebleBtn.onClick.AddListener(AbrirModalNuevoInmueble);
        if (guardarInmuebleBtn != null) guardarInmuebleBtn.onClick.AddListener(OnGuardarInmueble);
        if (cancelarModalBtn != null) cancelarModalBtn.onClick.AddListener(CerrarModalInmueble);
        if (tipoDropdown != null) tipoDropdown.onValueChanged.AddListener(OnTipoChanged);
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.onClick.AddListener(OnConfirmarEliminar);
        if (cancelarEliminarBtn != null) cancelarEliminarBtn.onClick.AddListener(CerrarPopupConfirmacion);

        if (contratoUI == null) contratoUI = FindObjectOfType<ContratoUI>();
        if (contratoUI == null)
            Debug.LogWarning("[DuenoInmuebleUI] ContratoUI no asignado. Los dropdowns de Contrato no se actualizarán automáticamente.");

        ConfigurarDropdownTipo();
    }

    private void OnEnable()
    {
        CerrarPopupConfirmacion();
        CerrarModalInmueble();
        PrepararFormularioNuevo();
        CargarListaDuenos();
        ActualizarContadorInmuebles();
    }

    private void ConfigurarDropdownTipo()
    {
        if (tipoDropdown == null) return;
        tipoDropdown.ClearOptions();
        tipoDropdown.AddOptions(new List<string> {
            "-- Seleccionar Tipo --", "Casa", "Departamento", "Salón Comercial"
        });
    }

    // ═════════════════════════════════════════════
    //  LISTA DE DUEÑOS
    // ═════════════════════════════════════════════

    public void CargarListaDuenos()
    {
        MostrarMensajeLista("Cargando...", Color.gray);
        LimpiarContenedor(contenedorListaDuenos);

        daoDueno.ObtenerTodosLosDuenos((exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }

            var lista = ParsearDuenos(json);
            if (totalDuenos != null) totalDuenos.text = lista.Count.ToString();
            if (lista.Count == 0) { MostrarMensajeLista("No hay propietarios registrados.", Color.gray); return; }

            MostrarMensajeLista("", Color.white);
            RenderizarListaDuenos(lista);
        });
    }

    private void OnBuscar()
    {
        string t = busquedaInput != null ? busquedaInput.text.Trim() : "";
        if (string.IsNullOrEmpty(t)) { CargarListaDuenos(); return; }

        MostrarMensajeLista("Buscando...", Color.gray);
        LimpiarContenedor(contenedorListaDuenos);

        daoDueno.BuscarDuenos(t, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeLista("Error: " + error, Color.red); return; }
            var lista = ParsearDuenos(json);
            if (lista.Count == 0) { MostrarMensajeLista($"Sin resultados para \"{t}\".", Color.gray); return; }
            MostrarMensajeLista("", Color.white);
            RenderizarListaDuenos(lista);
        });
    }

    private void RenderizarListaDuenos(List<DuenoItemData> lista)
    {
        if (itemDuenoPrefab == null || contenedorListaDuenos == null) return;

        foreach (var d in lista)
        {
            GameObject item = Instantiate(itemDuenoPrefab, contenedorListaDuenos);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();
            if (textos.Length >= 1) textos[0].text = d.apellido + ", " + d.nombre;

            long id = d.id;
            string nombreCompleto = d.apellido + ", " + d.nombre;
            TMP_Text textoInmuebles = textos.Length >= 2 ? textos[1] : null;

            // Cargar cantidad de inmuebles de forma asíncrona
            daoInmueble.ObtenerInmueblesPorDueno(id, (ok, jsonInm, err) =>
            {
                if (textoInmuebles == null) return;
                var cant = ok ? ParsearInmuebles(jsonInm).Count : 0;
                textoInmuebles.text = cant == 1 ? "1 Inmueble" : cant + " Inmuebles";
            });

            Button[] botones = item.GetComponentsInChildren<Button>();
            foreach (Button btn in botones)
            {
                string n = btn.gameObject.name.ToLower();
                if (n.Contains("editar")) btn.onClick.AddListener(() => AbrirEdicionDueno(id));
                else if (n.Contains("eliminar")) btn.onClick.AddListener(() => AbrirPopupEliminarDueno(id, nombreCompleto));
            }
        }
    }

    // ═════════════════════════════════════════════
    //  FORMULARIO DUEÑO
    // ═════════════════════════════════════════════

    public void PrepararFormularioNuevo()
    {
        modoEdicionDueno = false;
        idDuenoActual = -1;

        if (nombreInput != null) nombreInput.text = "";
        if (apellidoInput != null) apellidoInput.text = "";
        if (telefonoInput != null) telefonoInput.text = "";

        MostrarMensajeDueno("", Color.white);
        if (tituloDuenoText != null) tituloDuenoText.text = "Nuevo Propietario";
        if (seccionInmuebles != null) seccionInmuebles.SetActive(false);
        LimpiarContenedor(contenedorListaInmuebles);
    }

    public void AbrirEdicionDueno(long idDueno)
    {
        modoEdicionDueno = true;
        idDuenoActual = idDueno;

        if (tituloDuenoText != null) tituloDuenoText.text = "Editar Propietario";
        MostrarMensajeDueno("Cargando...", Color.gray);

        daoDueno.ObtenerDuenoPorId(idDueno, (exito, nombre, apellido, telefono, error) =>
        {
            if (!exito) { MostrarMensajeDueno("Error: " + error, Color.red); return; }

            if (nombreInput != null) nombreInput.text = nombre;
            if (apellidoInput != null) apellidoInput.text = apellido;
            if (telefonoInput != null) telefonoInput.text = telefono.ToString();

            MostrarMensajeDueno("", Color.white);
            if (seccionInmuebles != null) seccionInmuebles.SetActive(true);
            CargarInmueblesDeDueno(idDueno);
        });
    }

    private void OnGuardarDueno()
    {
        string nombre = nombreInput != null ? nombreInput.text.Trim() : "";
        string apellido = apellidoInput != null ? apellidoInput.text.Trim() : "";
        string telStr = telefonoInput != null ? telefonoInput.text.Trim() : "";

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        { MostrarMensajeDueno("El nombre y apellido son obligatorios.", Color.red); return; }

        if (!long.TryParse(telStr, out long telefono) || telefono <= 0)
        { MostrarMensajeDueno("Ingresá un número de teléfono válido.", Color.red); return; }

        if (guardarDuenoBtn != null) guardarDuenoBtn.interactable = false;
        MostrarMensajeDueno("Guardando...", Color.gray);

        if (modoEdicionDueno)
        {
            daoDueno.ActualizarDueno(idDuenoActual, nombre, apellido, telefono, (exito, error) =>
            {
                if (guardarDuenoBtn != null) guardarDuenoBtn.interactable = true;
                if (exito)
                {
                    MostrarMensajeDueno("Propietario actualizado.", Color.green);
                    CargarListaDuenos();
                    contratoUI?.RefrescarDropdowns();
                    servicioUI?.RefrescarDropdowns();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                }
                else MostrarMensajeDueno("Error: " + error, Color.red);
            });
        }
        else
        {
            daoDueno.RegistrarDueno(nombre, apellido, telefono, (exito, error) =>
            {
                if (guardarDuenoBtn != null) guardarDuenoBtn.interactable = true;
                if (exito)
                {
                    MostrarMensajeDueno("Propietario registrado. Ahora podés agregar sus inmuebles.", Color.green);
                    CargarListaDuenos();
                    contratoUI?.RefrescarDropdowns(); // Sincroniza dropdown de Contratos
                    servicioUI?.RefrescarDropdowns();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                    BuscarIdDuenoRecienCreado(nombre, apellido);
                }
                else MostrarMensajeDueno("Error: " + error, Color.red);
            });
        }
    }

    // Después de crear un dueño nuevo, buscamos su ID para habilitar la sección de inmuebles
    private void BuscarIdDuenoRecienCreado(string nombre, string apellido)
    {
        daoDueno.BuscarDuenos(apellido, (exito, json, error) =>
        {
            if (!exito) return;
            var lista = ParsearDuenos(json);
            var d = lista.Find(x => x.nombre == nombre && x.apellido == apellido);
            if (d != null)
            {
                modoEdicionDueno = true;
                idDuenoActual = d.id;
                if (seccionInmuebles != null) seccionInmuebles.SetActive(true);
                CargarInmueblesDeDueno(d.id);
            }
        });
    }
}