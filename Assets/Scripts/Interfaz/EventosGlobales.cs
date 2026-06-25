using System;

public static class EventosGlobales
{
    public static event Action OnDatosCambiados;

    public static void NotificarCambio()
    {
        OnDatosCambiados?.Invoke();
    }
}
