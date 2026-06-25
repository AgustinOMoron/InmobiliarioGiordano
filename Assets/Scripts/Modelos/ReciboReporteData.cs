using System;
using System.Collections.Generic;

namespace Inmobiliaria.Modelos
{
    /// <summary>
    /// Ítem de servicio para incluir en el reporte PDF del recibo.
    /// </summary>
    [Serializable]
    public class ServicioReporteItem
    {
        public string NombreServicio;
        public string Fecha;
        public float  Monto;
        public float  Porcentaje;
    }

    [Serializable]
    public class ReciboReporteData
    {
        public long   NumeroRecibo;
        public string Fecha;
        public string NombreInquilino;
        public string DomicilioInmueble;
        public long   Monto;
        public long   TotalAbonar;
        public string TipoRecibo; // Alquiler, Expensas, etc.
        public string Concepto;

        /// <summary>
        /// Servicios vinculados al inmueble del contrato.
        /// Si es null o vacía, el PDF muestra "Sin servicios asociados."
        /// </summary>
        public List<ServicioReporteItem> Servicios;

        // Datos de la Inmobiliaria (pueden ser fijos o venir de configuración)
        public string InmobiliariaNombre    = "Luis Alfredo Giordano";
        public string InmobiliariaDireccion = "Bv. Los Granaderos 2115 - 5008 - Córdoba";
        public string InmobiliariaTelefono  = "(0351) 153028876";
        public string InmobiliariaEmail     = "inmobiliarialuisgiordano@gmail.com";
        public string InmobiliariaCUIT      = "23-12874545-9";
    }
}
