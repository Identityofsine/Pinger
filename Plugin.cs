using BepInEx;
using System.Threading.Tasks;

namespace Pinger
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  public class Plugin : BaseUnityPlugin
  {

	private void Awake()
	{
	  // Plugin startup logic
	  Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	}

	private async void StartLogicLoop()
	{
	  while (StartOfRound.Instance == null)
	  {
		await Task.Delay(1000);
	  };
	}

  }
}
