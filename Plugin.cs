using BepInEx;
using HarmonyLib;
using Pinger.Overrider;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

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
		await Task.Delay(250);
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

		/* Raytracing is not working 

		RaycastHit hit = shootRay(x, y, z);
		float x_hit, y_hit, z_hit;
		//get xyz of the object in space
		if (hit.collider != null)
		{
		  x_hit = hit.collider.transform.position.x;
		  y_hit = hit.collider.transform.position.y;
		  z_hit = hit.collider.transform.position.z;
		  Logger.LogInfo($"Hit: {hit.collider.name} at {x_hit}, {y_hit}, {z_hit}");
		}
		*/

	  }
	}

	private RaycastHit shootRay(float x, float y, float z)
	{
	  RaycastHit hit;
	  if (Physics.Raycast(_mainPlayer.localVisorTargetPoint.position, _mainPlayer.localVisorTargetPoint.forward, out hit, 1000f))
	  {
	  }
	  else
	  {
	  }

	  return hit;
	}

  }
}
