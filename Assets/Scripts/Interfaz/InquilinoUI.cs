using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Maneja toda la interfaz de usuario para la gestión de inquilinos.
/// Conecta los elementos UI de la escena con el DAOInquilino para operaciones CRUD.
///
/// ═══════════════════════════════════════════════════════════════
///  GUÍA PARA ARMAR LA ESCENA EN UNITY (DISEÑO DASHBOARD)
/// ═══════════════════════════════════════════════════════════════
///
///  Estructura de Canvas recomendada (Vista dividida / Lado a lado):
///
///  Canvas
///  └── Panel_Inquilinos                       ← Contenedor raíz
///      ├── Lado_Izquierdo_Lista               ← Ocupa ej: 60% de la pantalla
///      │   ├── Buscador_Arriba
///      │   │   ├── InputField_Busqueda        ← busquedaInput
///      │   │   ├── Button_Buscar              ← buscarBtn
///      │   │   └── Button_MostrarTodos        ← limpiarBusquedaBtn
///      │   ├── ScrollView                     ← el ScrollView estándar de Unity
///      │   │   └── Viewport/Content           ← contenedorLista (el Content del Scroll)
///      │   └── Text_MensajeLista              ← mensajeListaText (ej: "Cargando...")
///      │
///      ├── Lado_Derecho_Formulario            ← Ocupa ej: 40% de la pantalla
///      │   ├── Text_TituloFormulario          ← tituloFormularioText ("Nuevo Inquilino" / "Editar")
///      │   ├── InputField_Nombre              ← nombreInput
///      │   ├── InputField_Apellido            ← apellidoInput
///      │   ├── InputField_Telefono            ← telefonoInput  (Content Type: Integer Number)
///      │   ├── Text_MensajeFormulario         ← mensajeFormularioText
///      │   ├── Button_Guardar                 ← guardarBtn
///      │   └── Button_LimpiarFormulario       ← limpiarFormularioBtn (Para salir de edición)
///      │
///      └── Panel_ConfirmacionEliminar         ← panelConfirmacionEliminar (POPUP OSCURO)
///          ├── Image Oscura (Fondo)           ← para tapar/oscurecer el dashboard atrás
///          └── Ventana_Centro
///              ├── Text_MensajeConfirmacion   ← mensajeConfirmacionText
///              ├── Button_ConfirmarEliminar   ← confirmarEliminarBtn
///              └── Button_CancelarEliminar    ← cancelarEliminarBtn
///
///  PREFAB ÍTEM DE LISTA:
///  Crear un prefab llamado "ItemInquilino" con:
///      ├── Text_NombreApellido                ← (primer TMP_Text encontrado)
///      ├── Text_Telefono                      ← (segundo TMP_Text)
///      ├── Button_Editar                      ← botón que contenga "editar" en el nombre
///      └── Button_Eliminar                    ← botón que contenga "eliminar" en el nombre
///
/// ═══════════════════════════════════════════════════════════════
/// </summary>
public class InquilinoUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Referencias al DAO
    // ─────────────────────────────────────────────

    [Header("DAO")]
    [Tooltip("Arrastrá el GameObject que tiene el componente DAOInquilino")]
    [SerializeField] private DAOInquilino daoInquilino;

    // ─────────────────────────────────────────────
    //  Panel Confirmación (El único que es Popup)
    // ─────────────────────────────────────────────

    [Header("Popup Confirmación")]
    [Tooltip("Panel de confirmación antes de eliminar (con fondo semitransparente)")]
    [SerializeField] private GameObject panelConfirmacionEliminar;

    [Tooltip("Texto que muestra el nombre del inquilino a eliminar")]
    [SerializeField] private TMP_Text mensajeConfirmacionText;
    [SerializeField] private Button confirmarEliminarBtn;
    [SerializeField] private Button cancelarEliminarBtn;

    // ─────────────────────────────────────────────
    //  Lado Izquierdo — Lista y Búsqueda
    // ─────────────────────────────────────────────

    [Header("Panel Izquierdo - Lista")]
    [SerializeField] private TMP_InputField busquedaInput;
    [SerializeField] private Button buscarBtn;
    [SerializeField] private Button limpiarBusquedaBtn;
    [SerializeField] private Transform contenedorLista;
    [SerializeField] private GameObject itemInquilinoPrefab;
    [SerializeField] private TMP_Text mensajeListaText;

    // ─────────────────────────────────────────────
    //  Lado Derecho — Formulario
    // ─────────────────────────────────────────────

    [Header("Panel Derecho - Formulario")]
    [SerializeField] private TMP_Text tituloFormularioText;
    [SerializeField] private TMP_InputField nombreInput;
    [SerializeField] private TMP_InputField apellidoInput;
    [SerializeField] private TMP_InputField telefonoInput;
    [SerializeField] private TMP_Text mensajeFormularioText;
    [SerializeField] private Button guardarBtn;

    [Tooltip("Botón para limpiar los campos y volver a 'Nuevo Inquilino'")]
    [SerializeField] private Button limpiarFormularioBtn;

    // ─────────────────────────────────────────────
    //  Notificación a otros paneles
    // ─────────────────────────────────────────────
    [Header("Notificaciones")]
    [Tooltip("Arrastrá el componente ContratoUI para que se actualicen sus dropdowns al guardar un inquilino.")]
    [SerializeField] private ContratoUI contratoUI;

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private long idInquilinoSeleccionado = -1;
    private bool modoEdicion = false;

    // ─────────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Validar DAO
        if (daoInquilino == null)
        {
            daoInquilino = FindObjectOfType<DAOInquilino>();
        }

        // Listeners Lista
        if (buscarBtn != null) buscarBtn.onClick.AddListener(OnBuscar);
        if (limpiarBusquedaBtn != null) limpiarBusquedaBtn.onClick.AddListener(OnLimpiarBusqueda);
        if (busquedaInput != null) busquedaInput.onSubmit.AddListener(OnSubmitBusqueda);

        // Listeners Formulario
        if (guardarBtn != null) guardarBtn.onClick.AddListener(OnGuardar);
        if (limpiarFormularioBtn != null) limpiarFormularioBtn.onClick.AddListener(PrepararFormularioNuevo);

        if (contratoUI == null) contratoUI = FindObjectOfType<ContratoUI>();
        if (contratoUI == null)
            Debug.LogWarning("[InquilinoUI] ContratoUI no asignado. Los dropdowns de Contrato no se actualizarán automáticamente.");

        // Listeners Popup Eliminar
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.onClick.AddListener(OnConfirmarEliminar);
        if (cancelarEliminarBtn != null) cancelarEliminarBtn.onClick.AddListener(CerrarPopupEliminar);
    }

    private void OnEnable()
    {
        // Al entrar a la sección, aseguramos que el popup esté cerrado, form limpio y cargar lista
        CerrarPopupEliminar();
        PrepararFormularioNuevo();
        CargarListaInquilinos();
    }

    // ═════════════════════════════════════════════
    //  MANEJO DEL FORMULARIO (LADO DERECHO)
    // ═════════════════════════════════════════════

    /// <summary>
    /// Limpia los campos y lo pone en modo "Nuevo Inquilino".
    /// </summary>
    public void PrepararFormularioNuevo()
    {
        modoEdicion = false;
        idInquilinoSeleccionado = -1;

        if (nombreInput != null) nombreInput.text = "";
        if (apellidoInput != null) apellidoInput.text = "";
        if (telefonoInput != null) telefonoInput.text = "";

        MostrarMensajeFormulario("", Color.white);

        if (tituloFormularioText != null)
            tituloFormularioText.text = "Nuevo Inquilino";
    }

    /// <summary>
    /// Se llama al tocar "Editar" en un ítem de la lista.
    /// Carga los datos en el formulario del costado.
    /// </summary>
    public void AbrirEdicion(long idInquilino)
    {
        modoEdicion = true;
        idInquilinoSeleccionado = idInquilino;

        if (tituloFormularioText != null)
            tituloFormularioText.text = "Editar Inquilino";

        MostrarMensajeFormulario("Cargando datos...", Color.gray);

        daoInquilino.ObtenerInquilinoPorId(idInquilino, (exito, nombre, apellido, telefono, error) =>
        {
            if (exito)
            {
                if (nombreInput != null) nombreInput.text = nombre;
                if (apellidoInput != null) apellidoInput.text = apellido;
                if (telefonoInput != null) telefonoInput.text = telefono.ToString();
                MostrarMensajeFormulario("", Color.white);
            }
            else
            {
                MostrarMensajeFormulario("Error: " + error, Color.red);
            }
        });
    }

    // ═════════════════════════════════════════════
    //  GUARDAR (REGISTRAR O ACTUALIZAR)
    // ═════════════════════════════════════════════

    private void OnGuardar()
    {
        string nombre = nombreInput != null ? nombreInput.text.Trim() : "";
        string apellido = apellidoInput != null ? apellidoInput.text.Trim() : "";
        string telStr = telefonoInput != null ? telefonoInput.text.Trim() : "";

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        {
            MostrarMensajeFormulario("El nombre y apellido son obligatorios.", Color.red);
            return;
        }

        if (!long.TryParse(telStr, out long telefono) || telefono <= 0)
        {
            MostrarMensajeFormulario("Ingresá un número de teléfono válido.", Color.red);
            return;
        }

        if (guardarBtn != null) guardarBtn.interactable = false;
        MostrarMensajeFormulario("Guardando...", Color.gray);

        if (modoEdicion)
        {
            daoInquilino.ActualizarInquilino(idInquilinoSeleccionado, nombre, apellido, telefono, (exito, error) =>
            {
                if (guardarBtn != null) guardarBtn.interactable = true;

                if (exito)
                {
                    MostrarMensajeFormulario("✓ Inquilino actualizado.", Color.green);
                    CargarListaInquilinos();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                    Invoke(nameof(PrepararFormularioNuevo), 1.5f);
                }
                else
                {
                    MostrarMensajeFormulario("Error: " + error, Color.red);
                }
            });
        }
        else
        {
            daoInquilino.RegistrarInquilino(nombre, apellido, telefono, (exito, error) =>
            {
                if (guardarBtn != null) guardarBtn.interactable = true;

                if (exito)
                {
                    MostrarMensajeFormulario("✓ Inquilino registrado.", Color.green);
                    CargarListaInquilinos();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                    Invoke(nameof(PrepararFormularioNuevo), 1.5f);
                }
                else
                {
                    MostrarMensajeFormulario("Error: " + error, Color.red);
                }
            });
        }
    }

    // ═════════════════════════════════════════════
    //  CARGAR Y RENDERIZAR LISTA (LADO IZQUIERDO)
    // ═════════════════════════════════════════════

    public void CargarListaInquilinos()
    {
        MostrarMensajeLista("Cargando...", Color.gray);
        LimpiarContenedorLista();

        daoInquilino.ObtenerTodosLosInquilinos((exito, json, error) =>
        {
            if (!exito)
            {
                MostrarMensajeLista("Error: " + error, Color.red);
                return;
            }

            List<InquilinoItemData> lista = ParsearListaInquilinos(json);

            if (lista == null || lista.Count == 0)
            {
                MostrarMensajeLista("No hay inquilinos registrados.", Color.gray);
                return;
            }

            MostrarMensajeLista("", Color.white);
            RenderizarLista(lista);
        });
    }

    private void OnBuscar()
    {
        string termino = busquedaInput != null ? busquedaInput.text.Trim() : "";

        if (string.IsNullOrEmpty(termino))
        {
            CargarListaInquilinos();
            return;
        }

        MostrarMensajeLista("Buscando...", Color.gray);
        LimpiarContenedorLista();

        daoInquilino.BuscarInquilinos(termino, (exito, json, error) =>
        {
            if (!exito)
            {
                MostrarMensajeLista("Error: " + error, Color.red);
                return;
            }

            List<InquilinoItemData> lista = ParsearListaInquilinos(json);

            if (lista == null || lista.Count == 0)
            {
                MostrarMensajeLista($"Sin resultados para \"{termino}\".", Color.gray);
                return;
            }

            MostrarMensajeLista("", Color.white);
            RenderizarLista(lista);
        });
    }

    private void OnSubmitBusqueda(string valor)
    {
        OnBuscar();
    }

    private void OnLimpiarBusqueda()
    {
        if (busquedaInput != null) busquedaInput.text = "";
        CargarListaInquilinos();
    }

    private void RenderizarLista(List<InquilinoItemData> lista)
    {
        if (itemInquilinoPrefab == null || contenedorLista == null) return;

        foreach (InquilinoItemData inquilino in lista)
        {
            GameObject item = Instantiate(itemInquilinoPrefab, contenedorLista);

            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();
            if (textos.Length >= 1) textos[0].text = inquilino.apellido + ", " + inquilino.nombre;
            if (textos.Length >= 2) textos[1].text = "Tel: " + inquilino.telefono;

            long id = inquilino.id;
            string nombreApellido = inquilino.apellido + ", " + inquilino.nombre;

            Button[] botones = item.GetComponentsInChildren<Button>();
            foreach (Button btn in botones)
            {
                string nombreBtn = btn.gameObject.name.ToLower();
                if (nombreBtn.Contains("editar"))
                    btn.onClick.AddListener(() => AbrirEdicion(id));
                else if (nombreBtn.Contains("eliminar"))
                    btn.onClick.AddListener(() => AbrirPopupEliminar(id, nombreApellido));
            }
        }
    }

    private void LimpiarContenedorLista()
    {
        if (contenedorLista == null) return;
        foreach (Transform hijo in contenedorLista)
            Destroy(hijo.gameObject);
    }

    // ═════════════════════════════════════════════
    //  POPUP DE ELIMINAR (ÚNICA VENTANA MODAL)
    // ═════════════════════════════════════════════

    private void AbrirPopupEliminar(long idInquilino, string nombreApellido)
    {
        idInquilinoSeleccionado = idInquilino;

        if (mensajeConfirmacionText != null)
            mensajeConfirmacionText.text = $"¿Eliminar al inquilino \"{nombreApellido}\"?\nEsta acción no se puede deshacer.";

        if (panelConfirmacionEliminar != null)
            panelConfirmacionEliminar.SetActive(true);
    }

    private void CerrarPopupEliminar()
    {
        if (panelConfirmacionEliminar != null)
            panelConfirmacionEliminar.SetActive(false);
    }

    private void OnConfirmarEliminar()
    {
        if (idInquilinoSeleccionado <= 0) return;

        if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = false;

        daoInquilino.EliminarInquilino(idInquilinoSeleccionado, (exito, error) =>
        {
            if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;

            if (exito)
            {
                CargarListaInquilinos();
                GlobalDropdownRefreshManager.NotifyDataChanged();

                // Si justo estábamos editando a este inquilino, limpiamos el formulario
                if (modoEdicion) PrepararFormularioNuevo();

                CerrarPopupEliminar();
            }
            else
            {
                if (mensajeConfirmacionText != null)
                    mensajeConfirmacionText.text = "Error: " + error;
            }
        });
    }

    // ═════════════════════════════════════════════
    //  HELPERS & JSON PARSING
    // ═════════════════════════════════════════════

    private void MostrarMensajeFormulario(string mensaje, Color color)
    {
        if (mensajeFormularioText == null) return;
        mensajeFormularioText.text = mensaje;
        mensajeFormularioText.color = color;
    }

    private void MostrarMensajeLista(string mensaje, Color color)
    {
        if (mensajeListaText == null) return;
        mensajeListaText.gameObject.SetActive(!string.IsNullOrEmpty(mensaje));
        mensajeListaText.text = mensaje;
        mensajeListaText.color = color;
    }

    [Serializable] private class InquilinoItemData { public long id; public string nombre; public string apellido; public long telefono; }
    [Serializable] private class InquilinoJsonItem { public long id_Inquilino; public string Nombre_Inquilinos; public string Apellido_Inquilinos; public long Num_Telefono; }
    [Serializable] private class InquilinoJsonArray { public InquilinoJsonItem[] items; }

    private List<InquilinoItemData> ParsearListaInquilinos(string json)
    {
        List<InquilinoItemData> resultado = new List<InquilinoItemData>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return resultado;

        try
        {
            InquilinoJsonArray array = JsonUtility.FromJson<InquilinoJsonArray>("{\"items\":" + json + "}");
            if (array == null || array.items == null) return resultado;

            foreach (InquilinoJsonItem item in array.items)
                resultado.Add(new InquilinoItemData { id = item.id_Inquilino, nombre = item.Nombre_Inquilinos, apellido = item.Apellido_Inquilinos, telefono = item.Num_Telefono });
        }
        catch (Exception ex) { Debug.LogError("[InquilinoUI] Error JSON: " + ex.Message); }

        return resultado;
    }
}