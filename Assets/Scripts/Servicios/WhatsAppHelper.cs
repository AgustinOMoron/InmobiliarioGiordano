using UnityEngine;

/// <summary>
/// Utilidad estática para generar y abrir links de WhatsApp con mensajes prediseñados.
/// Usa la API de wa.me para abrir conversaciones directamente desde la app.
///
/// RF-04.03: Comunicación Automática con Inquilinos
/// Genera mensajes prediseñados para enviar por WhatsApp recordatorios o avisos de pago.
/// </summary>
public static class WhatsAppHelper
{
    // ═══════════════════════════════════════════════
    //  MENSAJES PREDISEÑADOS
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Genera un mensaje de recordatorio de pago de alquiler.
    /// </summary>
    public static string MensajeRecordatorioPago(string nombreInquilino, string direccion = "", string monto = "")
    {
        string saludo = $"Hola {nombreInquilino}, le escribimos desde Inmobiliaria Giordano.";
        string cuerpo = "Le recordamos que se encuentra pendiente el pago del alquiler correspondiente al mes en curso.";

        if (!string.IsNullOrEmpty(direccion))
            cuerpo += $"\n\nPropiedad: {direccion}";

        if (!string.IsNullOrEmpty(monto))
            cuerpo += $"\nMonto: ${monto}";

        string cierre = "\n\nPor favor, comuníquese con nosotros ante cualquier consulta.\n¡Muchas gracias!";

        return saludo + "\n\n" + cuerpo + cierre;
    }

    /// <summary>
    /// Genera un mensaje de aviso de aumento de alquiler.
    /// </summary>
    public static string MensajeAvisoPago(string nombreInquilino, string mesAumento = "", string montoNuevo = "")
    {
        string saludo = $"Hola {nombreInquilino}, le escribimos desde Inmobiliaria Giordano.";
        string cuerpo = "Le informamos que según lo estipulado en el contrato de locación, corresponde un ajuste en el valor del alquiler.";

        if (!string.IsNullOrEmpty(mesAumento))
            cuerpo += $"\n\nMes de ajuste: {mesAumento}";

        if (!string.IsNullOrEmpty(montoNuevo))
            cuerpo += $"\nNuevo monto: ${montoNuevo}";

        string cierre = "\n\nQuedamos a disposición ante cualquier consulta.\n¡Saludos cordiales!";

        return saludo + "\n\n" + cuerpo + cierre;
    }

    /// <summary>
    /// Genera un mensaje de vencimiento de contrato.
    /// </summary>
    public static string MensajeVencimientoContrato(string nombreInquilino, string fechaVencimiento = "")
    {
        string saludo = $"Hola {nombreInquilino}, le escribimos desde Inmobiliaria Giordano.";
        string cuerpo = "Le informamos que su contrato de locación se encuentra próximo a su fecha de vencimiento.";

        if (!string.IsNullOrEmpty(fechaVencimiento))
            cuerpo += $"\n\nFecha de vencimiento: {fechaVencimiento}";

        cuerpo += "\n\nLe solicitamos que se comunique con nosotros a la brevedad para coordinar la renovación o finalización del mismo.";

        string cierre = "\n\n¡Muchas gracias!";

        return saludo + "\n\n" + cuerpo + cierre;
    }

    /// <summary>
    /// Genera un mensaje genérico personalizable.
    /// </summary>
    public static string MensajeGenerico(string nombreInquilino, string asunto)
    {
        return $"Hola {nombreInquilino}, le escribimos desde Inmobiliaria Giordano.\n\n{asunto}\n\nQuedamos a disposición ante cualquier consulta.\n¡Saludos cordiales!";
    }

    // ═══════════════════════════════════════════════
    //  ABRIR WHATSAPP
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Abre WhatsApp Web/App con el número y mensaje indicado.
    /// El número debe estar en formato internacional sin "+" (ej: 5493511234567).
    /// </summary>
    public static void AbrirWhatsApp(long telefono, string mensaje)
    {
        // Formatear número: asegurar prefijo de Argentina si es un número local
        string numero = FormatearNumeroArgentina(telefono);
        string mensajeEncoded = UnityEngine.Networking.UnityWebRequest.EscapeURL(mensaje);
        string url = $"https://wa.me/{numero}?text={mensajeEncoded}";

        Debug.Log($"[WhatsApp] Abriendo: {url}");
        Application.OpenURL(url);
    }

    /// <summary>
    /// Abre WhatsApp directamente con un recordatorio de pago prediseñado.
    /// </summary>
    public static void EnviarRecordatorioPago(long telefono, string nombreInquilino, string direccion = "", string monto = "")
    {
        string mensaje = MensajeRecordatorioPago(nombreInquilino, direccion, monto);
        AbrirWhatsApp(telefono, mensaje);
    }

    /// <summary>
    /// Abre WhatsApp directamente con un aviso de aumento prediseñado.
    /// </summary>
    public static void EnviarAvisoPago(long telefono, string nombreInquilino, string mesAumento = "", string montoNuevo = "")
    {
        string mensaje = MensajeAvisoPago(nombreInquilino, mesAumento, montoNuevo);
        AbrirWhatsApp(telefono, mensaje);
    }

    /// <summary>
    /// Abre WhatsApp directamente con un aviso de vencimiento de contrato.
    /// </summary>
    public static void EnviarAvisoVencimiento(long telefono, string nombreInquilino, string fechaVencimiento = "")
    {
        string mensaje = MensajeVencimientoContrato(nombreInquilino, fechaVencimiento);
        AbrirWhatsApp(telefono, mensaje);
    }

    // ═══════════════════════════════════════════════
    //  HELPERS INTERNOS
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Formatea un número de teléfono argentino para WhatsApp.
    /// Si el número empieza con 0 (prefijo local), lo convierte al formato internacional.
    /// Si ya tiene código de país (54), lo deja como está.
    /// </summary>
    private static string FormatearNumeroArgentina(long telefono)
    {
        string num = telefono.ToString();

        // Si ya tiene prefijo internacional argentino (54...)
        if (num.StartsWith("54"))
            return num;

        // Si empieza con 0 (ej: 0351...), quitar el 0 y agregar 54
        if (num.StartsWith("0"))
            return "54" + num.Substring(1);

        // Si empieza con 15 (número celular local), agregar 549 + código de área
        if (num.StartsWith("15"))
            return "549" + num.Substring(2);

        // Asumir que es un número sin prefijo, agregar 549
        return "549" + num;
    }
}
