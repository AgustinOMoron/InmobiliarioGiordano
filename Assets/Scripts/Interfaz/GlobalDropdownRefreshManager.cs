using System;

public static class GlobalDropdownRefreshManager
{
    public static event Action OnAnyDataChanged;

    public static void NotifyDataChanged()
    {
        OnAnyDataChanged?.Invoke();
    }
}
