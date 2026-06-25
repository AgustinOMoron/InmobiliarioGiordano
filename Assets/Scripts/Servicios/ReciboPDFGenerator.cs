using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Inmobiliaria.Modelos;

/// <summary>
/// Servicio estático para generar PDFs de Recibos de pago.
/// RF-03.02: Emisión y Descarga de Recibos en formato PDF.
///
/// ═══════════════════════════════════════════════════════════════
///  CONFIGURACIÓN REQUERIDA (NO es un MonoBehaviour)
/// ═══════════════════════════════════════════════════════════════
///
///  Esta clase NO se agrega a ningún GameObject en la escena.
///  Es una clase estática de utilidad llamada directamente desde ReciboUI.
///
///  ── DEPENDENCIA DE PAQUETE ──
///  Requiere el paquete NuGet: QuestPDF (versión Community)
///  Importado desde: Window → Package Manager → Add package from disk
///  o via NuGet for Unity (herramienta de terceros).
///
///  ── ARCHIVO DE LOGO REQUERIDO ──
///  El logo de la inmobiliaria debe existir en la siguiente ruta:
///  Assets/Imagenes/Giordano Isotipo.png
///  (Si no existe, el PDF se genera igual pero sin logo)
///
///  ── SALIDA DE ARCHIVOS ──
///  Los PDFs se guardan automáticamente en:
///  Application.persistentDataPath/RecibosPDF/
///  En Windows → C:/Users/<usuario>/AppData/LocalLow/<empresa>/<proyecto>/RecibosPDF/
///  El archivo se abre automáticamente al terminar la generación.
///
///  ── CÓMO LLAMARLA DESDE ReciboUI ──
///  var data = new ReciboReporteData { ... };
///  string path = ReciboPDFGenerator.GenerarReciboPDF(data);
///  if (path != null) Application.OpenURL("file://" + path);
///
///  ── CONTENIDO DEL PDF ──
///  Genera 2 páginas en un solo archivo:
///  - Página 1: ORIGINAL  (para el inquilino)
///  - Página 2: DUPLICADO (para la inmobiliaria)
///  Cada página incluye: Logo + datos inmobiliaria, datos del inquilino,
///  domicilio del inmueble, monto recibido, concepto y total a abonar.
///
/// ═══════════════════════════════════════════════════════════════
/// </summary>
namespace Inmobiliaria.Servicios
{
    public class ReciboPDFGenerator
    {
        static ReciboPDFGenerator()
        {
            // Es obligatorio aceptar la licencia para que funcione QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static string GenerarReciboPDF(ReciboReporteData data)
        {
            try
            {
                // Definir la ruta de guardado (Carpeta Documentos del usuario)
                string fileName = $"Recibo_{data.NumeroRecibo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string folderPath = Path.Combine(Application.persistentDataPath, "RecibosPDF");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);

                // Crear el documento con dos páginas separadas: ORIGINAL y DUPLICADO
                var document = Document.Create(container =>
                {
                    // Página 1 - ORIGINAL
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                        page.Content().Component(new ReciboComponent(data, "ORIGINAL"));
                    });

                    // Página 2 - DUPLICADO
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                        page.Content().Component(new ReciboComponent(data, "DUPLICADO"));
                    });
                });

                // Generar el archivo
                document.GeneratePdf(filePath);

                Debug.Log($"[ReciboPDF] PDF generado con éxito en: {filePath}");
                return filePath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReciboPDF] Error al generar PDF: {ex.Message}");
                return null;
            }
        }

        // ── FOOTER: TOTAL + Firma — siempre al pie de la página ──
        public static void RenderFooter(IContainer container, ReciboReporteData data)
        {
            // Calcular total = monto alquiler + suma de servicios
            long totalPDF = data.Monto;
            if (data.Servicios != null)
                foreach (var s in data.Servicios)
                    totalPDF += (long)s.Monto;

            container.PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Border(1).Padding(5).Column(totalCol =>
                {
                    totalCol.Item().Text("TOTAL").FontSize(12).Bold();
                    totalCol.Item().Text($"$ {totalPDF:N0}").FontSize(16).Bold().AlignCenter();
                });

                row.RelativeItem(2).PaddingLeft(20).Column(firmaCol =>
                {
                    firmaCol.Item().PaddingTop(20).LineHorizontal(1);
                    firmaCol.Item().Text("Firma y Aclaración").FontSize(8).AlignCenter();
                });
            });
        }
    }

    // Componente reutilizable para el diseño del recibo
    public class ReciboComponent : IComponent
    {
        private ReciboReporteData _data;
        private string _tipoCopia; // "ORIGINAL" o "DUPLICADO"

        public ReciboComponent(ReciboReporteData data, string tipoCopia)
        {
            _data = data;
            _tipoCopia = tipoCopia;
        }

        private string ObtenerRutaLogo()
        {
            string[] carpetasBusqueda = new string[]
            {
                Path.Combine(Application.dataPath, "Imagenes"),
                Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? "", "Imagenes"),
                Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? "", "Assets", "Imagenes")
            };

            string[] nombresArchivo = new string[] { "Giordano Isotipo.png", "GiordanoIsotipo.png" };

            foreach (var carpeta in carpetasBusqueda)
            {
                if (string.IsNullOrEmpty(carpeta)) continue;

                foreach (var nombre in nombresArchivo)
                {
                    string ruta = Path.GetFullPath(Path.Combine(carpeta, nombre));
                    if (File.Exists(ruta))
                    {
                        Debug.Log($"[ReciboPDF] Logo encontrado en: {ruta}");
                        return ruta;
                    }
                }
            }

            Debug.LogWarning($"[ReciboPDF] No se encontró el logo en las rutas buscadas.");
            return null;
        }

        public void Compose(IContainer container)
        {
            container.Border(1).Padding(10).Column(col =>
            {
                // HEADER
                col.Item().Row(row =>
                {
                    // Izquierda: Logo y Datos Inmobiliaria
                    row.RelativeItem().Row(izqRow =>
                    {
                        string logoPath = ObtenerRutaLogo();
                        if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                        {
                            izqRow.ConstantItem(60).PaddingRight(10).Image(logoPath);
                        }

                        izqRow.RelativeItem().Column(headerCol =>
                        {
                            headerCol.Item().Text("Inmobiliaria").FontSize(14).Bold().FontColor("#750821");
                            headerCol.Item().Text("Luis A. Giordano").FontSize(14).Bold().FontColor("#750821");
                            headerCol.Item().Text(_data.InmobiliariaDireccion).FontSize(9);
                            headerCol.Item().Text($"Tel: {_data.InmobiliariaTelefono}").FontSize(9);
                            headerCol.Item().Text(_data.InmobiliariaEmail).FontSize(9);
                            headerCol.Item().Text("RESPONSABLE MONOTRIBUTO").FontSize(8).Italic();
                        });
                    });

                    // Derecha: Número y Fecha
                    row.RelativeItem().AlignRight().Border(1).Padding(5).Column(numCol =>
                    {
                        numCol.Item().Text("RECIBO").FontSize(16).Bold().AlignCenter();
                        //numCol.Item().Text("DOCUMENTO NO VÁLIDO COMO FACTURA").FontSize(7).AlignCenter();
                        //numCol.Item().PaddingTop(5).Text($"Nº 0001 - {_data.NumeroRecibo:D8}").FontSize(12).Bold();
                        numCol.Item().Text($"FECHA: {_data.Fecha}").FontSize(10);
                        numCol.Item().Text($"C.U.I.T.: {_data.InmobiliariaCUIT}").FontSize(8);
                        numCol.Item().PaddingTop(5).Text(_tipoCopia).FontSize(12).Bold().FontColor(Colors.Grey.Medium).AlignCenter();
                    });
                });

                col.Item().PaddingVertical(5).LineHorizontal(1);

                // DATOS CLIENTE
                col.Item().Grid(grid =>
                {
                    grid.Columns(4);
                    grid.Item(1).Text("Señor(es):").Bold();
                    grid.Item(3).Text(_data.NombreInquilino).Underline();

                    grid.Item(1).Text("Domicilio:").Bold();
                    grid.Item(3).Text(_data.DomicilioInmueble).Underline();
                });


                // ── TABLA DE SERVICIOS ──────────────────────────────────
                col.Item().PaddingTop(8).Column(svcSection =>
                {
                    svcSection.Item().Text("SERVICIOS y ALQUILER ASOCIADOS AL INMUEBLE")
                        .FontSize(9).Bold().FontColor("#750821");

                    svcSection.Item().PaddingTop(3).Table(table =>
                    {
                        // Definir columnas
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3); // Nombre
                            cols.RelativeColumn(2); // Fecha
                            cols.RelativeColumn(2); // Monto
                        });

                        // Encabezado
                        static IContainer HeaderCell(IContainer c) =>
                            c.Background("#750821").Padding(4);

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("SERVICIO Y ALQUILER").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCell).Text("FECHA").FontSize(8).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCell).Text("MONTO").FontSize(8).Bold().FontColor(Colors.White);
                        });

                        // Fila de Alquiler (siempre presente)
                        string bgColor = Colors.White;
                        string fechaAlquilerDisplay = _data.Fecha ?? "";
                        if (!string.IsNullOrEmpty(fechaAlquilerDisplay))
                        {
                            var p = fechaAlquilerDisplay.Split('-');
                            if (p.Length >= 3) fechaAlquilerDisplay = $"{p[2]}/{p[1]}/{p[0]}";
                        }

                        IContainer AlquilerCell(IContainer c) =>
                            c.Background(bgColor).BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4);

                        table.Cell().Element(AlquilerCell).Text("ALQUILER").FontSize(8);
                        table.Cell().Element(AlquilerCell).Text(fechaAlquilerDisplay).FontSize(8);
                        table.Cell().Element(AlquilerCell).Text($"$ {_data.Monto:N2}").FontSize(8);

                        // Filas de servicios (si los hubiera, debajo del alquiler)
                        bool tieneServicios = _data.Servicios != null && _data.Servicios.Count > 0;

                        if (tieneServicios)
                        {
                            bool esFilaPar = true; // Empezamos en true porque la primera fila (Alquiler) fue blanca (impar)
                            foreach (var svc in _data.Servicios)
                            {
                                string bgSvcColor = esFilaPar ? "#F5F5F5" : Colors.White;
                                esFilaPar = !esFilaPar;

                                // Convertir fecha YYYY-MM-DD → DD/MM/YYYY
                                string fechaDisplay = svc.Fecha ?? "";
                                if (!string.IsNullOrEmpty(fechaDisplay))
                                {
                                    var p = fechaDisplay.Split('-');
                                    if (p.Length >= 3) fechaDisplay = $"{p[2]}/{p[1]}/{p[0]}";
                                }

                                IContainer DataCell(IContainer c) =>
                                    c.Background(bgSvcColor).BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4);

                                table.Cell().Element(DataCell).Text(svc.NombreServicio?.ToUpper() ?? "—").FontSize(8);
                                table.Cell().Element(DataCell).Text(fechaDisplay).FontSize(8);
                                table.Cell().Element(DataCell).Text($"$ {svc.Monto:N2}").FontSize(8);
                            }
                        }
                    });
                });

                // TOTAL y Firma debajo de la tabla
                col.Item().PaddingTop(15).Element(c => ReciboPDFGenerator.RenderFooter(c, _data));
            });
        }
    }
}
