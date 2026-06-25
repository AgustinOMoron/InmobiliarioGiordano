using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UnityEngine;
using System;
using System.IO;
using Inmobiliaria.Modelos;

namespace Inmobiliaria.Servicios
{
    public class LiquidacionPDFGenerator
    {
        static LiquidacionPDFGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static string GenerarLiquidacionPDF(LiquidacionReporteData data)
        {
            try
            {
                string fileName = $"Liquidacion_{data.NumeroLiquidacion}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string folderPath = Path.Combine(Application.persistentDataPath, "LiquidacionesPDF");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);

                var document = Document.Create(container =>
                {
                    // Página 1 - ORIGINAL
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                        page.Content().Component(new LiquidacionComponent(data, "ORIGINAL"));
                    });

                    // Página 2 - DUPLICADO
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                        page.Content().Component(new LiquidacionComponent(data, "DUPLICADO"));
                    });
                });

                document.GeneratePdf(filePath);

                Debug.Log($"[LiquidacionPDF] PDF generado con éxito en: {filePath}");
                return filePath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LiquidacionPDF] Error al generar PDF: {ex.Message}");
                return null;
            }
        }
    }

    public class LiquidacionComponent : IComponent
    {
        private LiquidacionReporteData _data;
        private string _tipoCopia;

        public LiquidacionComponent(LiquidacionReporteData data, string tipoCopia)
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
                        Debug.Log($"[LiquidacionPDF] Logo encontrado en: {ruta}");
                        return ruta;
                    }
                }
            }

            Debug.LogWarning($"[LiquidacionPDF] No se encontró el logo en las rutas buscadas.");
            return null;
        }

        public void Compose(IContainer container)
        {
            container.Border(1).Padding(10).Column(col =>
            {
                // HEADER
                col.Item().Row(row =>
                {
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

                    row.RelativeItem().AlignRight().Border(1).Padding(5).Column(numCol =>
                    {
                        numCol.Item().Text("LIQUIDACIÓN").FontSize(14).Bold().AlignCenter();
                        numCol.Item().Text($"FECHA: {_data.Fecha}").FontSize(10);
                        numCol.Item().Text($"C.U.I.T.: {_data.InmobiliariaCUIT}").FontSize(8);
                        numCol.Item().PaddingTop(5).Text(_tipoCopia).FontSize(12).Bold().FontColor(Colors.Grey.Medium).AlignCenter();
                    });
                });

                col.Item().PaddingVertical(5).LineHorizontal(1);

                // DATOS CLIENTE (PROPIETARIO)
                col.Item().Grid(grid =>
                {
                    grid.Columns(4);
                    grid.Item(1).Text("Propietario:").Bold();
                    grid.Item(3).Text(_data.NombrePropietario).Underline();

                    grid.Item(1).Text("Inmueble:").Bold();
                    grid.Item(3).Text(_data.DireccionInmueble).Underline();

                });

                // DETALLE DE LA LIQUIDACIÓN
                col.Item().PaddingVertical(10).Border(0.5f).Padding(10).MinHeight(100).Column(bodyCol =>
                {
                    bodyCol.Item().Text("Detalle de Liquidación:").Bold().FontSize(12);
                    bodyCol.Item().PaddingVertical(5).LineHorizontal(0.5f);

                    bodyCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Monto Alquiler Cobrado:");
                        row.ConstantItem(100).AlignRight().Text($"$ {_data.MontoAlquiler:N0}");
                    });

                    bodyCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Honorarios Administrativos:");
                        row.ConstantItem(100).AlignRight().Text($"$ -{_data.Honorarios:N0}");
                    });

                    if (_data.DescuentoAdicional > 0)
                    {
                        string descText = string.IsNullOrEmpty(_data.DescuentoDescripcion) ? "Descuentos Adicionales:" : $"Descuentos ({_data.DescuentoDescripcion}):";
                        bodyCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text(descText);
                            row.ConstantItem(100).AlignRight().Text($"$ -{_data.DescuentoAdicional:N0}");
                        });
                    }

                    bodyCol.Item().PaddingVertical(5).LineHorizontal(0.5f);

                    bodyCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Total a Cobrar:").Bold();
                        row.ConstantItem(100).AlignRight().Text($"$ {_data.NetoPropietario:N0}").Bold();
                    });
                });

                // FOOTER
                col.Item().PaddingTop(15).Column(footerCol =>
                {
                    string mes = "_____";
                    string anio = "____";
                    if (DateTime.TryParseExact(_data.Fecha, new[] { "dd-MM-yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd" }, 
                        System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dt))
                    {
                        mes = dt.ToString("MMMM", new System.Globalization.CultureInfo("es-AR"));
                        anio = dt.Year.ToString();
                    }
                    else
                    {
                        string[] partes = _data.Fecha.Split('-', '/');
                        if (partes.Length == 3)
                        {
                            string mesNum = partes[1];
                            string anioStr = partes[2];
                            if (partes[0].Length == 4)
                            {
                                mesNum = partes[1];
                                anioStr = partes[0];
                            }
                            int.TryParse(mesNum, out int m);
                            string[] meses = { "", "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };
                            if (m >= 1 && m <= 12) mes = meses[m];
                            anio = anioStr;
                        }
                    }

                    string montoEnLetras = NumeroALetras.Convertir(_data.NetoPropietario);

                    footerCol.Item().PaddingBottom(20).Text(t =>
                    {
                        t.Span("Sin otro particular saludo a usted muy Atte.\n\n").Italic();
                        t.Span("Recibi del Sr Luis Alfredo Giordano la suma de pesos ");
                        t.Span($"{montoEnLetras}").Bold();
                        t.Span($" ($ {_data.NetoPropietario:N0}). ").Bold();
                        t.Span($"En concepto de alquiler mes de {mes} {anio}.");
                    });

                    footerCol.Item().Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(250).Column(firmaCol =>
                        {
                            firmaCol.Item().PaddingTop(25).LineHorizontal(1);
                            firmaCol.Item().Text("Firma y Aclaración (Propietario)").FontSize(8).AlignCenter();
                        });
                    });
                });
            });
        }
    }

    public static class NumeroALetras
    {
        public static string Convertir(long numero)
        {
            if (numero == 0) return "CERO";
            if (numero < 0) return "MENOS " + Convertir(Math.Abs(numero));

            string literal = "";

            if (numero >= 1000000)
            {
                long millones = numero / 1000000;
                long resto = numero % 1000000;
                if (millones == 1)
                    literal = "UN MILLÓN ";
                else
                    literal = Convertir(millones) + " MILLONES ";

                if (resto > 0)
                    literal += Convertir(resto);
            }
            else if (numero >= 1000)
            {
                long miles = numero / 1000;
                long resto = numero % 100;
                // Dejemos que maneje resto de miles completo
                long restoMiles = numero % 1000;
                if (miles == 1)
                    literal = "MIL ";
                else
                    literal = Convertir(miles) + " MIL ";

                if (restoMiles > 0)
                    literal += Convertir(restoMiles);
            }
            else if (numero >= 100)
            {
                long centenas = numero / 100;
                long resto = numero % 100;
                if (centenas == 1 && resto == 0)
                    literal = "CIEN";
                else if (centenas == 1)
                    literal = "CIENTO " + Convertir(resto);
                else if (centenas == 2)
                    literal = "DOSCIENTOS " + Convertir(resto);
                else if (centenas == 3)
                    literal = "TRESCIENTOS " + Convertir(resto);
                else if (centenas == 4)
                    literal = "CUATROCIENTOS " + Convertir(resto);
                else if (centenas == 5)
                    literal = "QUINIENTOS " + Convertir(resto);
                else if (centenas == 6)
                    literal = "SEISCIENTOS " + Convertir(resto);
                else if (centenas == 7)
                    literal = "SETECIENTOS " + Convertir(resto);
                else if (centenas == 8)
                    literal = "OCHOCIENTOS " + Convertir(resto);
                else if (centenas == 9)
                    literal = "NOVECIENTOS " + Convertir(resto);
            }
            else if (numero >= 10)
            {
                long decenas = numero / 10;
                long resto = numero % 10;
                switch (numero)
                {
                    case 10: literal = "DIEZ"; break;
                    case 11: literal = "ONCE"; break;
                    case 12: literal = "DOCE"; break;
                    case 13: literal = "TRECE"; break;
                    case 14: literal = "CATORCE"; break;
                    case 15: literal = "QUINCE"; break;
                    default:
                        if (numero < 20)
                            literal = "DIECI" + Convertir(resto);
                        else if (numero == 20)
                            literal = "VEINTE";
                        else if (numero < 30)
                            literal = "VEINTI" + Convertir(resto);
                        else
                        {
                            string[] nombresDecenas = { "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
                            literal = nombresDecenas[decenas];
                            if (resto > 0)
                                literal += " Y " + Convertir(resto);
                        }
                        break;
                }
            }
            else
            {
                string[] nombresUnidades = { "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
                literal = nombresUnidades[numero];
            }

            return literal.Trim();
        }
    }
}
