using BepInEx;
using HarmonyLib;
using Pinger.Overrider;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pinger
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  public class Plugin : BaseUnityPlugin
  {

	public static Plugin Instance { get; private set; }

	private Harmony _harmony;
	private static bool _isPatched = false;
	private static PlayerControllerB _mainPlayer = null;


	private void Awake()
	{
	  // Plugin startup logic
	  Plugin.Instance = this;
	  Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	  _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
	  _harmony.PatchAll(typeof(StartOfRound_Awake));
	  _harmony.PatchAll(typeof(HUDManager_MeetsScanNodeRequirements));
	  _harmony.PatchAll(typeof(HUDManager_NodeIsNotVisible));
	  _harmony.PatchAll(typeof(KeyboardPing));
	  StartLogicLoop();
	}

	private async void DummyKeybind()
	{
	  while (true)
	  {
		await Task.Delay(5000);
		if (_mainPlayer == null) continue;


		float x = _mainPlayer.gameplayCamera.transform.position.x;
		float y = _mainPlayer.gameplayCamera.transform.position.y;
		float z = _mainPlayer.gameplayCamera.transform.position.z;


		RaycastHit hit = shootRay(x, y, z);
		if (_mainPlayer.gameplayCamera == null)
		{
		  Logger.LogWarning("Gameplay camera is null");
		  continue;
		}
		float x_pre = hit.transform.position.x;
		float y_pre = hit.transform.position.y;
		float z_pre = hit.transform.position.z;
		createPing(x_pre, y_pre, z_pre, hit);
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
	  }
	}

	public bool createPingWherePlayerIsLooking()
	{
	  if (_mainPlayer == null) return false;
	  float camera_x, camera_y, camera_z, point_x, point_y, point_z;
	  camera_x = _mainPlayer.gameplayCamera.transform.position.x;
	  camera_y = _mainPlayer.gameplayCamera.transform.position.y;
	  camera_z = _mainPlayer.gameplayCamera.transform.position.z;
	  RaycastHit hit = this.shootRay(camera_x, camera_y, camera_z);

	  //get the point where the raycast hit
	  point_x = hit.transform.position.x;
	  point_y = hit.transform.position.y;
	  point_z = hit.transform.position.z;

	  return createPing(point_x, point_y, point_z, hit);
	}

	private bool createPing(float x, float y, float z, in RaycastHit hit)
	{

	  string header = "r/bbcworship";
	  string sub = "r/bbcworship ping";

	  Logger.LogMessage($"Creating Ping at : {x} {y} {z}");

	  ScanNodeProperties[] scanNodeProperties = ScanNodeProperties.FindObjectsByType<ScanNodeProperties>(FindObjectsSortMode.None);

	  //copy scanNodeOne
	  ScanNodeProperties copy = Instantiate(scanNodeProperties[0]);
	  copy.headerText = header;
	  copy.subText = sub;
	  copy.transform.position = new Vector3(x, y, z);

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
