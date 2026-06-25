using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Parte 2 de DuenoInmuebleUI:
/// Mini-lista de inmuebles, Modal de Inmueble, Popup Confirmación y Helpers/JSON.
/// IMPORTANTE: Este archivo es la continuación de DuenoInmuebleUI.cs (Parte 1).
/// Ambos usan "partial class" para que Unity los compile como una sola clase.
/// </summary>
public partial class DuenoInmuebleUI
{
    // ═════════════════════════════════════════════
    //  MINI-LISTA DE INMUEBLES
    // ═════════════════════════════════════════════

    private void CargarInmueblesDeDueno(long idDueno)
    {
        MostrarMensajeInmuebles("Cargando inmuebles...", Color.gray);
        LimpiarContenedor(contenedorListaInmuebles);

        daoInmueble.ObtenerInmueblesPorDueno(idDueno, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeInmuebles("Error al cargar inmuebles.", Color.red); return; }

            var lista = ParsearInmuebles(json);

            if (lista.Count == 0)
            {
                MostrarMensajeInmuebles("Sin inmuebles. Tocá '+ Agregar Inmueble'.", Color.gray);
                return;
            }
            MostrarMensajeInmuebles("", Color.white);
            RenderizarMiniListaInmuebles(lista);
        });
    }

    /// <summary>
    /// Consulta el total GLOBAL de inmuebles en la BD y actualiza el contador del Menú Principal.
    /// </summary>
    private void ActualizarContadorInmuebles()
    {
        daoInmueble.ObtenerTodosLosInmuebles((exito, json, error) =>
        {
            if (!exito) return;
            var total = ParsearInmuebles(json).Count;
            if (totalInmuebles != null) totalInmuebles.text = total.ToString();
        });
    }

    private void RenderizarMiniListaInmuebles(List<InmuebleItemData> lista)
    {
        if (itemInmueblePrefab == null || contenedorListaInmuebles == null) return;

        foreach (var inm in lista)
        {
            GameObject item = Instantiate(itemInmueblePrefab, contenedorListaInmuebles);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();

            // texto[0]: Dirección, número y barrio
            if (textos.Length >= 1)
            {
                string barrio = !string.IsNullOrEmpty(inm.barrio) ? $" ({inm.barrio})" : "";
                textos[0].text = $"{inm.direccion} {inm.numero}{barrio}";
            }

            // texto[1]: Tipo. Si es Departamento, carga Piso/Unidad de forma async
            string tipoStr = inm.tipo == 1 ? "Casa" : inm.tipo == 2 ? "Departamento" : "Salón Comercial";
            TMP_Text textoTipo = textos.Length >= 2 ? textos[1] : null;
            if (textoTipo != null) textoTipo.text = tipoStr;

            if (inm.tipo == 2 && textoTipo != null)
            {
                long idInm = inm.id;
                daoDepartamento.ObtenerDepartamentosPorPropiedad(idInm, (ok, jsonD, err) =>
                {
                    if (!ok || textoTipo == null) return;
                    var deptos = ParsearDeptos(jsonD);
                    if (deptos.Count > 0)
                        textoTipo.text = $"Departamento  ·  Piso {deptos[0].Piso}, Unidad {deptos[0].Unidad}";
                });
            }

            long id = inm.id;
            string desc = $"{inm.direccion} {inm.numero}";

            Button[] botones = item.GetComponentsInChildren<Button>();
            foreach (Button btn in botones)
            {
                string n = btn.gameObject.name.ToLower();
                if (n.Contains("editar")) btn.onClick.AddListener(() => AbrirModalEditarInmueble(id));
                else if (n.Contains("eliminar")) btn.onClick.AddListener(() => AbrirPopupEliminarInmueble(id, desc));
            }
        }
    }

    // ═════════════════════════════════════════════
    //  MODAL FORMULARIO INMUEBLE
    // ═════════════════════════════════════════════

    private void AbrirModalNuevoInmueble()
    {
        if (idDuenoActual <= 0) { MostrarMensajeDueno("Guardá primero el propietario.", Color.red); return; }

        modoEdicionInmueble = false;
        idInmuebleEnEdicion = -1;
        idDepartamentoEnEdicion = -1;

        if (tituloModalText != null) tituloModalText.text = "Nuevo Inmueble";
        if (direccionInput != null) direccionInput.text = "";
        if (numeroInput != null) numeroInput.text = "";
        if (barrioInput != null) barrioInput.text = "";
        if (tipoDropdown != null) tipoDropdown.value = 0;
        if (pisoInput != null) { pisoInput.text = ""; pisoInput.interactable = false; }
        if (unidadInput != null) { unidadInput.text = ""; unidadInput.interactable = false; }

        MostrarMensajeModal("", Color.white);
        if (panelModalInmueble != null) panelModalInmueble.SetActive(true);
    }

    private void AbrirModalEditarInmueble(long idInmueble)
    {
        modoEdicionInmueble = true;
        idInmuebleEnEdicion = idInmueble;
        idDepartamentoEnEdicion = -1;

        if (tituloModalText != null) tituloModalText.text = "Editar Inmueble";
        MostrarMensajeModal("Cargando...", Color.gray);
        if (panelModalInmueble != null) panelModalInmueble.SetActive(true);

        daoInmueble.ObtenerInmueblePorId(idInmueble, (exito, json, error) =>
        {
            if (!exito) { MostrarMensajeModal("Error: " + error, Color.red); return; }

            var lista = ParsearInmuebles(json);
            if (lista.Count == 0) { MostrarMensajeModal("Error al cargar datos.", Color.red); return; }

            var inm = lista[0];
            if (direccionInput != null) direccionInput.text = inm.direccion;
            if (numeroInput != null) numeroInput.text = inm.numero.ToString();
            if (barrioInput != null) barrioInput.text = inm.barrio ?? "";
            if (tipoDropdown != null) tipoDropdown.value = (int)inm.tipo;

            if (inm.tipo == 2)
            {
                daoDepartamento.ObtenerDepartamentosPorPropiedad(idInmueble, (exD, jsonD, errD) =>
                {
                    if (exD)
                    {
                        var deptos = ParsearDeptos(jsonD);
                        if (deptos.Count > 0)
                        {
                            idDepartamentoEnEdicion = deptos[0].id_Inmueble;
                            if (pisoInput != null) pisoInput.text = deptos[0].Piso.ToString();
                            if (unidadInput != null) unidadInput.text = deptos[0].Unidad;
                        }
                    }
                    MostrarMensajeModal("", Color.white);
                });
            }
            else MostrarMensajeModal("", Color.white);
        });
    }

    private void CerrarModalInmueble()
    {
        if (panelModalInmueble != null) panelModalInmueble.SetActive(false);
    }

    private void OnTipoChanged(int index)
    {
        bool esDepto = (index == 2);
        if (pisoInput != null) { pisoInput.interactable = esDepto; if (!esDepto) pisoInput.text = ""; }
        if (unidadInput != null) { unidadInput.interactable = esDepto; if (!esDepto) unidadInput.text = ""; }
    }

    private void OnGuardarInmueble()
    {
        string dir = direccionInput != null ? direccionInput.text.Trim() : "";
        string numStr = numeroInput != null ? numeroInput.text.Trim() : "";
        string barrio = barrioInput != null ? barrioInput.text.Trim() : "";
        int tipoIdx = tipoDropdown != null ? tipoDropdown.value : 0;

        if (string.IsNullOrEmpty(dir) || tipoIdx == 0)
        { MostrarMensajeModal("Completá la dirección y el tipo.", Color.red); return; }

        if (!int.TryParse(numStr, out int numero) || numero <= 0)
        { MostrarMensajeModal("Número de dirección inválido.", Color.red); return; }

        long tipoId = tipoIdx;
        if (guardarInmuebleBtn != null) guardarInmuebleBtn.interactable = false;
        MostrarMensajeModal("Guardando...", Color.gray);

        if (modoEdicionInmueble)
        {
            daoInmueble.ActualizarInmueble(idInmuebleEnEdicion, dir, numero, barrio, tipoId, idDuenoActual, (exito, error) =>
            {
                if (exito) ProcesarDepartamento(idInmuebleEnEdicion, tipoId);
                else TerminarGuardadoInmueble(false, error);
            });
        }
        else
        {
            daoInmueble.RegistrarInmueble(dir, numero, barrio, tipoId, idDuenoActual, (exito, nuevoId, error) =>
            {
                if (exito) ProcesarDepartamento(nuevoId, tipoId);
                else TerminarGuardadoInmueble(false, error);
            });
        }
    }

    private void ProcesarDepartamento(long idPropiedad, long tipoId)
    {
        if (tipoId != 2) { TerminarGuardadoInmueble(true, null); return; }

        long.TryParse(pisoInput?.text.Trim(), out long piso);
        string unidad = unidadInput != null ? unidadInput.text.Trim() : "-";

        if (idDepartamentoEnEdicion > 0)
        {
            daoDepartamento.ActualizarDepartamento(idDepartamentoEnEdicion, piso, unidad, idPropiedad,
                (exito, error) => TerminarGuardadoInmueble(exito, error));
        }
        else
        {
            daoDepartamento.RegistrarDepartamento(piso, unidad, idPropiedad,
                (exito, error) => TerminarGuardadoInmueble(exito, error));
        }
    }

    private void TerminarGuardadoInmueble(bool exito, string error)
    {
        if (guardarInmuebleBtn != null) guardarInmuebleBtn.interactable = true;

        if (exito)
        {
            MostrarMensajeModal("✓ Inmueble guardado.", Color.green);
            Invoke(nameof(CerrarModalInmueble), 1f);
            CargarInmueblesDeDueno(idDuenoActual);
            CargarListaDuenos();
            ActualizarContadorInmuebles(); // Total global del menú
            contratoUI?.RefrescarDropdowns(); // Sincroniza dropdown de Contratos
        }
        else MostrarMensajeModal("Error: " + error, Color.red);
    }

    // ═════════════════════════════════════════════
    //  POPUP CONFIRMACIÓN ELIMINAR
    // ═════════════════════════════════════════════

    private void AbrirPopupEliminarDueno(long id, string nombre)
    {
        idEliminarPendiente = id;
        tipoEliminarPendiente = TipoEliminar.Dueno;
        if (mensajeConfirmacionText != null)
            mensajeConfirmacionText.text = $"¿Eliminar al propietario \"{nombre}\"?\n\nSi tiene inmuebles asociados, debés eliminarlos primero.";
        if (panelConfirmacion != null) panelConfirmacion.SetActive(true);
    }

    private void AbrirPopupEliminarInmueble(long id, string desc)
    {
        idEliminarPendiente = id;
        tipoEliminarPendiente = TipoEliminar.Inmueble;
        if (mensajeConfirmacionText != null)
            mensajeConfirmacionText.text = $"¿Eliminar el inmueble \"{desc}\"?\nEsta acción no se puede deshacer.";
        if (panelConfirmacion != null) panelConfirmacion.SetActive(true);
    }

    private void CerrarPopupConfirmacion()
    {
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
    }

    private void OnConfirmarEliminar()
    {
        if (idEliminarPendiente <= 0) return;
        if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = false;

        if (tipoEliminarPendiente == TipoEliminar.Dueno)
        {
            daoDueno.EliminarDueno(idEliminarPendiente, (exito, error) =>
            {
                if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;
                if (exito)
                {
                    CargarListaDuenos();
                    contratoUI?.RefrescarDropdowns();
                    servicioUI?.RefrescarDropdowns();
                    GlobalDropdownRefreshManager.NotifyDataChanged();
                    PrepararFormularioNuevo();
                    CerrarPopupConfirmacion();
                }
                else if (mensajeConfirmacionText != null) mensajeConfirmacionText.text = "Error: " + error;
            });
        }
        else
        {
            // Primero intentamos borrar el Departamento asociado (si existe),
            // luego borramos el Inmueble para evitar el error 409 de FK constraint.
            long idInmuebleABorrar = idEliminarPendiente;
            daoDepartamento.ObtenerDepartamentosPorPropiedad(idInmuebleABorrar, (exD, jsonD, errD) =>
            {
                var deptos = exD ? ParsearDeptos(jsonD) : new System.Collections.Generic.List<DeptoItemData>();

                if (deptos.Count > 0)
                {
                    // Es un Departamento — primero borramos el registro de Depto
                    daoDepartamento.EliminarDepartamento(deptos[0].id_Inmueble, (exDepto, errorDepto) =>
                    {
                        if (!exDepto)
                        {
                            if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;
                            if (mensajeConfirmacionText != null)
                                mensajeConfirmacionText.text = "Error al eliminar departamento: " + errorDepto;
                            return;
                        }
                        // Depto borrado → ahora borramos el Inmueble
                        EliminarInmuebleFinal(idInmuebleABorrar);
                    });
                }
                else
                {
                    // No es departamento → borramos directamente
                    EliminarInmuebleFinal(idInmuebleABorrar);
                }
            });
        }
    }

    private void EliminarInmuebleFinal(long idInmueble)
    {
        daoInmueble.EliminarInmueble(idInmueble, (exito, error) =>
        {
            if (confirmarEliminarBtn != null) confirmarEliminarBtn.interactable = true;
            if (exito)
            {
                CargarInmueblesDeDueno(idDuenoActual);
                CargarListaDuenos();
                ActualizarContadorInmuebles();
                contratoUI?.RefrescarDropdowns(); // Sincroniza dropdown de Contratos
                CerrarPopupConfirmacion();
            }
            else if (mensajeConfirmacionText != null) mensajeConfirmacionText.text = "Error: " + error;
        });
    }

    // ═════════════════════════════════════════════
    //  HELPERS
    // ═════════════════════════════════════════════

    private void LimpiarContenedor(Transform contenedor)
    {
        if (contenedor == null) return;
        foreach (Transform hijo in contenedor) Destroy(hijo.gameObject);
    }

    private void MostrarMensajeLista(string m, Color c)
    { if (mensajeListaText != null) { mensajeListaText.gameObject.SetActive(!string.IsNullOrEmpty(m)); mensajeListaText.text = m; mensajeListaText.color = c; } }

    private void MostrarMensajeDueno(string m, Color c)
    { if (mensajeDuenoText != null) { mensajeDuenoText.text = m; mensajeDuenoText.color = c; } }

    private void MostrarMensajeInmuebles(string m, Color c)
    { if (mensajeInmueblesText != null) { mensajeInmueblesText.gameObject.SetActive(!string.IsNullOrEmpty(m)); mensajeInmueblesText.text = m; mensajeInmueblesText.color = c; } }

    private void MostrarMensajeModal(string m, Color c)
    { if (mensajeModalText != null) { mensajeModalText.text = m; mensajeModalText.color = c; } }

    // ═════════════════════════════════════════════
    //  CLASES JSON Y PARSERS
    // ═════════════════════════════════════════════

    [Serializable] private class DuenoItemData { public long id; public string nombre; public string apellido; }
    [Serializable] private class DuenoJsonItem { public long id_Dueno; public string Nombre_Dueno; public string Apellido_Dueno; public long Num_Telefono; }
    [Serializable] private class DuenoJsonArray { public DuenoJsonItem[] items; }

    [Serializable] private class InmuebleItemData { public long id; public string direccion; public long numero; public long tipo; public string barrio; }
    [Serializable] private class InmuebleJsonItem { public long id_Propiedad; public string Direccion; public int Numero_Direccion; public long Tipo; public string Barrio; }
    [Serializable] private class InmuebleJsonArray { public InmuebleJsonItem[] items; }

    [Serializable] private class DeptoItemData { public long id_Inmueble; public long Piso; public string Unidad; }
    [Serializable] private class DeptoJsonArray { public DeptoItemData[] items; }

    private List<DuenoItemData> ParsearDuenos(string json)
    {
        var r = new List<DuenoItemData>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return r;
        try
        {
            var a = JsonUtility.FromJson<DuenoJsonArray>("{\"items\":" + json + "}");
            if (a?.items != null)
                foreach (var d in a.items)
                    r.Add(new DuenoItemData { id = d.id_Dueno, nombre = d.Nombre_Dueno, apellido = d.Apellido_Dueno });
        }
        catch (Exception ex) { Debug.LogError("[DuenoInmuebleUI] JSON Dueno: " + ex.Message); }
        return r;
    }

    private List<InmuebleItemData> ParsearInmuebles(string json)
    {
        var r = new List<InmuebleItemData>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return r;
        try
        {
            var a = JsonUtility.FromJson<InmuebleJsonArray>("{\"items\":" + json + "}");
            if (a?.items != null)
                foreach (var m in a.items)
                    r.Add(new InmuebleItemData { id = m.id_Propiedad, direccion = m.Direccion, numero = m.Numero_Direccion, tipo = m.Tipo, barrio = m.Barrio });
        }
        catch (Exception ex) { Debug.LogError("[DuenoInmuebleUI] JSON Inmueble: " + ex.Message); }
        return r;
    }

    private List<DeptoItemData> ParsearDeptos(string json)
    {
        var r = new List<DeptoItemData>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return r;
        try
        {
            var a = JsonUtility.FromJson<DeptoJsonArray>("{\"items\":" + json + "}");
            if (a?.items != null) r.AddRange(a.items);
        }
        catch (Exception ex) { Debug.LogError("[DuenoInmuebleUI] JSON Depto: " + ex.Message); }
        return r;
    }
}