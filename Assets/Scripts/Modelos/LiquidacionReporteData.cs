using System;

namespace Inmobiliaria.Modelos
{
    [Serializable]
    public class LiquidacionReporteData
    {
        public long NumeroLiquidacion;
        public string Fecha;
        public string NombrePropietario;
        public string DireccionInmueble;
        public long MontoAlquiler;
        public long Honorarios;
        public long DescuentoAdicional;
        public string DescuentoDescripcion;
        public long NetoPropietario;
        public long NumeroContrato;
        
        // Datos de la Inmobiliaria
        public string InmobiliariaNombre = "Luis Alfredo Giordano";
        public string InmobiliariaDireccion = "Bv. Los Granaderos 2115 - 5008 - Córdoba";
        public string InmobiliariaTelefono = "(0351) 153028876";
        public string InmobiliariaEmail = "inmobiliarialuisgiordano@gmail.com";
        public string InmobiliariaCUIT = "23-12874545-9";
    }
}
