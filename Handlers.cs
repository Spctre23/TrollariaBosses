using TerrariaApi.Server;
using TShockAPI.Hooks;

namespace TrollariaBosses;

public class Handlers
{
    public static void InitializeHandlers(TerrariaPlugin registrator)
    {
        GeneralHooks.ReloadEvent += OnReload;
    }

    public static void DisposeHandlers(TerrariaPlugin deregistrator)
    {
        GeneralHooks.ReloadEvent -= OnReload;
    }

    private static void OnReload(ReloadEventArgs args)
    {
        TrollariaBosses.Instance.Config = Configuration.Reload();
    }
}