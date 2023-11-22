using BepInEx;
using HarmonyLib;
using Pinger.Overrider;
using System.Threading.Tasks;
using GameNetcodeStuff;

namespace Pinger
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  public class Plugin : BaseUnityPlugin
  {
	private Harmony _harmony;
	private static bool _isPatched = false;
	private static PlayerControllerB _mainPlayer = null;


	private void Awake()
	{
	  // Plugin startup logic
	  Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	  _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
	  _harmony.PatchAll(typeof(StartOfRound_Awake));
	  StartLogicLoop();
	}

	private async void StartLogicLoop()
	{
	  if (_isPatched) return;

	  while (StartOfRound.Instance == null)
	  {
		await Task.Delay(1000);
	  };
	  Logger.LogInfo("StartOfRound.Instance found...");
	  _mainPlayer = StartOfRound.Instance.localPlayerController;

	  while (_mainPlayer == null)
	  {
		_mainPlayer = StartOfRound.Instance.localPlayerController;
	  }

	  _isPatched = true;

	  UpdatePlayerCamera();
	}

	private async void UpdatePlayerCamera()
	{
	  while (true)
	  {
		await Task.Delay(250);
		if (_mainPlayer == null)
		{
		  Logger.LogInfo("Main player is null");
		  return;
		}
		float x = _mainPlayer.localVisorTargetPoint.eulerAngles.x;
		float y = _mainPlayer.localVisorTargetPoint.eulerAngles.y;
		float z = _mainPlayer.localVisorTargetPoint.eulerAngles.z;

		Logger.LogInfo($"Updating Camera (x:{x},y:{y},z:{z})");
	  }
	}

  }
}
