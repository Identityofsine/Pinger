using BepInEx;
using HarmonyLib;
using Pinger.Overrider;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;

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
	  _harmony.PatchAll(typeof(HUDManager_MeetsScanNodeRequirements));
	  _harmony.PatchAll(typeof(HUDManager_NodeIsNotVisible));
	  StartLogicLoop();
	  DummyKeybind();
	}

	private async void DummyKeybind()
	{
	  while (true)
	  {
		await Task.Delay(5000);
		if (_mainPlayer == null) continue;
		RaycastHit hit = shootRay(_mainPlayer.localVisorTargetPoint.position.x, _mainPlayer.localVisorTargetPoint.position.y, _mainPlayer.localVisorTargetPoint.position.z);
		if (_mainPlayer.gameplayCamera == null)
		{
		  Logger.LogWarning("Gameplay camera is null");
		  continue;
		}
		float x = _mainPlayer.gameplayCamera.transform.position.x;
		float y = _mainPlayer.gameplayCamera.transform.position.y;
		float z = _mainPlayer.gameplayCamera.transform.position.z;
		createPing(x, y, z, hit);
	  }


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

	private bool createPing(float x, float y, float z, in RaycastHit hit)
	{

	  string header = "r/bbcworship";
	  string sub = "r/bbcworship ping";

	  GameObject gOBJ = new GameObject("PlayerPing");

	  ScanNodeProperties[] scanNodeProperties = ScanNodeProperties.FindObjectsByType<ScanNodeProperties>(FindObjectsSortMode.None);

	  //copy scanNodeOne
	  ScanNodeProperties copy = Instantiate(scanNodeProperties[0]);
	  copy.headerText = header;
	  copy.subText = sub;
	  copy.transform.position = new Vector3(x, y, z);


	  foreach (ScanNodeProperties scanNodeProperty in scanNodeProperties)
	  {
		//debug	
		try
		{
		  Logger.LogInfo($"ScanNodeProperties (active : {scanNodeProperty.isActiveAndEnabled}): {scanNodeProperty.name}, header:{scanNodeProperty.headerText}, pos: {scanNodeProperty.transform.position.x}, {scanNodeProperty.transform.position.y}, {scanNodeProperty.transform.position.z}, max_range:{scanNodeProperty.maxRange}");
		  scanNodeProperty.maxRange = 999;
		  scanNodeProperty.requiresLineOfSight = false;
		}
		catch
		{
		  Logger.LogError($"Error with {scanNodeProperty.name}");
		}
	  }


	  return true;
	}

	private RaycastHit shootRay(float x, float y, float z)
	{
	  RaycastHit hit;
	  if (Physics.Raycast(_mainPlayer.localVisorTargetPoint.position, _mainPlayer.localVisorTargetPoint.forward, out hit, 1000f))
	  {
	  }
	  return hit;
	}
  }
}
