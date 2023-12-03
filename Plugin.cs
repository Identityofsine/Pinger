using BepInEx;
using HarmonyLib;
using Pinger.Overrider;
using System.Threading.Tasks;
using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;
using Newtonsoft.Json;
using LC_API.ServerAPI;
using Settings.Hook;

namespace Pinger
{

  struct CustomScanNode
  {
	public long created { get; set; }
	public ScanNodeProperties scanNode { get; set; }
	public string owner { get; set; }
  }

  [JsonObject(MemberSerialization.OptIn)]
  class PingData
  {
	[JsonProperty]
	public float x { get; set; }
	[JsonProperty]
	public float y { get; set; }
	[JsonProperty]
	public float z { get; set; }
	[JsonProperty]
	public long created { get; set; }
	[JsonProperty]
	public string owner { get; set; }
	[JsonProperty]
	public bool isDanger { get; set; }
  }



  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  public class Plugin : BaseUnityPlugin
  {

	public static Plugin Instance { get; private set; }

	private const string SIGNATURE = "player_ping";
	private const int LIFESPAN = 10000;

	private Harmony _harmony;
	private ScanNodeProperties _scanNodeMaster;


	//make linked list
	private LinkedList<CustomScanNode> _scanNodes = new LinkedList<CustomScanNode>();
	private static bool _isPatched = false;
	private static bool _isPatching = false;


	//gameobjects
	private static PlayerControllerB _mainPlayer = null;
	private static HUDManager _hudManager = null;


	private void Awake()
	{
	  // Plugin startup logic
	  Plugin.Instance = this;
	  Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
	  _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
	  _harmony.PatchAll(typeof(StartOfRound_Awake));
	  _harmony.PatchAll(typeof(KeyboardPing));
		SettingsAPI.getInstance().LoadSettings("Pinger");
	  StartLogicLoop();
	}

	private async void StartLogicLoop()
	{

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

	  while (_hudManager == null)
	  {
		await Task.Delay(250);
		_hudManager = HUDManager.Instance;
	  }

	  _isPatched = true;
	  handleIncomingPings();

	}

	private void handleIncomingPings()
	{
	  Networking.GetString += (string message, string signature) =>
	  {
		if (signature.Equals(SIGNATURE))
		{
		  //try to parse the message
		  PingData pingData = JsonConvert.DeserializeObject<PingData>(message);
		  if (pingData == null)
		  {
			Logger.LogWarning("Failed to parse ping data");
		  }
		  else
		  {
			Logger.LogMessage($"Received ping from {pingData.owner} at {pingData.x} {pingData.y} {pingData.z}");
			createPing(pingData.x, pingData.y, pingData.z, new RaycastHit(), pingData.isDanger, pingData.owner);
		  }
		}
	  };
	}

	private void DeletePings(string owner) {
		ScanNodeProperties[] scanNodeProperties = ScanNodeProperties.FindObjectsByType<ScanNodeProperties>(FindObjectsSortMode.None);
		for (var node = _scanNodes.First; node != null; node = node.Next)
		{
			if(node.Value.scanNode == null) continue;
			if(node.Value.owner != owner) {
				Debug.Log($"Skipping ping at {node.Value.scanNode.transform.position} (Predicate failed: {node.Value.owner} != {owner})");
				continue;
			}
			Debug.Log($"Deleting ping at {node.Value.scanNode.transform.position}");
			Destroy(node.Value.scanNode);
			//find all the scan nodes and delete them
			for (int i = 0; i < scanNodeProperties.Length; i++)
			{
				if (scanNodeProperties[i].transform.position == node.Value.scanNode.transform.position)
				{
					Destroy(scanNodeProperties[i]);
					break;
				}
			}
		}
	}

	private async void checkAndDeleteOldPings()
	{
	  int lifespan = 10000; // 10 seconds
	  while (true)
	  {
		await Task.Delay(1000);
		long now = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
		for (var node = _scanNodes.First; node != null; node = node.Next)
		{
		  if (node.Value.scanNode == null) continue;
		  if (now - node.Value.created > lifespan)
		  {
			
			Logger.LogMessage($"Deleting ping at {node.Value.scanNode.transform.position}");
			Destroy(node.Value.scanNode);
			_scanNodes.Remove(node);
		  }
		}
	  }
	}


	private void OnDestroy()
	{
	  // Plugin cleanup logic
	  //_harmony.UnpatchSelf();
	  ScanNodeProperties[] scanNodeProperties = ScanNodeProperties.FindObjectsByType<ScanNodeProperties>(FindObjectsSortMode.None);
	  for (var node = _scanNodes.First; node != null; node = node.Next)
	  {
		Destroy(node.Value.scanNode);
		//find all the scan nodes and delete them
		for (int i = 0; i < scanNodeProperties.Length; i++)
		{
		  if (scanNodeProperties[i].transform.position == node.Value.scanNode.transform.position)
		  {
			Destroy(scanNodeProperties[i]);
			break;
		  }
		}
	  }

	}

	public bool createPingWherePlayerIsLooking(bool is_danger = false)
	{
	  if (_mainPlayer == null || _hudManager == null)
	  {
		if (_isPatching) return false;
		StartLogicLoop();
		_isPatching = true;
		return false;
	  };
	  float camera_x, camera_y, camera_z, point_x, point_y, point_z;
	  camera_x = _mainPlayer.gameplayCamera.transform.position.x;
	  camera_y = _mainPlayer.gameplayCamera.transform.position.y;
	  camera_z = _mainPlayer.gameplayCamera.transform.position.z;
	  RaycastHit hit = this.shootRay(camera_x, camera_y, camera_z);

	  //get the point where the raycast hit
	  point_x = hit.point.x;
	  point_y = hit.point.y;
	  point_z = hit.point.z;

	  Logger.LogMessage($"Creating Ping on the surface of {hit.transform.name}");
	  CustomScanNode ping_obj = createPing(point_x, point_y, point_z, hit, is_danger);

	  if (ping_obj.scanNode == null)
	  {
		return false;
	  }


	  string message = JsonConvert.SerializeObject(new PingData
	  {
		x = point_x,
		y = point_y,
		z = point_z,
		created = ping_obj.created,
		owner = _mainPlayer.playerUsername,
		isDanger = is_danger
	  });

	  Networking.Broadcast(message, SIGNATURE);

	  return true;
	}

	private CustomScanNode createPing(float x, float y, float z, in RaycastHit hit)
	{
	  return createPing(x, y, z, hit, false, _mainPlayer.playerUsername);
	}

	private CustomScanNode createPing(float x, float y, float z, in RaycastHit hit, bool isDanger)
	{
	  return createPing(x, y, z, hit, isDanger, _mainPlayer.playerUsername);
	}

	private CustomScanNode createPing(float x, float y, float z, in RaycastHit hit, string playerName)
	{
	  return createPing(x, y, z, hit, false, playerName);
	}

	private CustomScanNode createPing(float x, float y, float z, in RaycastHit hit, bool isDanger, string playerName)
	{
		if(hasPlayerPinged(playerName)) {
			DeletePings(playerName);
		} 

	  string header = playerName + "'s ping";
	  string sub = "Player Ping <!>";

	  Logger.LogMessage($"Creating Ping at : {x} {y} {z}");

	  ScanNodeProperties[] scanNodeProperties = ScanNodeProperties.FindObjectsByType<ScanNodeProperties>(FindObjectsSortMode.None);

	  if (_scanNodeMaster == null)
	  {
		if (scanNodeProperties.Length == 0)
		{
		  Logger.LogWarning("No scan node master found");
		  return new CustomScanNode();
		}
		else
		{
		  _scanNodeMaster = scanNodeProperties[0];
		  checkAndDeleteOldPings();
		}
	  }

	  //copy scanNodeOne
	  ScanNodeProperties copy = Instantiate(_scanNodeMaster);
	  copy.name = "PlayerPing";
	  copy.headerText = header;
	  copy.subText = sub;
	  copy.transform.position = new Vector3(x, y, z);
	  copy.maxRange = 200;
	  copy.minRange = 0;
	  copy.requiresLineOfSight = false;
	  if (isDanger)
	  {
		copy.nodeType = 1;
		copy.subText = "DANGER <!>";
	  }
	  else
	  {
		copy.nodeType = 2;
	  }

	  CustomScanNode customScanNode = new CustomScanNode();
	  long now = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
	  customScanNode.created = now;
	  customScanNode.scanNode = copy;
		customScanNode.owner = playerName;

	  this._scanNodes.AddLast(customScanNode);

	  if (_hudManager == null)
	  {
		//well... do nothing.
		Logger.LogWarning("HUDManager is null");
	  }
	  else
	  {
		if(customScanNode.owner == _mainPlayer.playerUsername)
			_hudManager.UIAudio.PlayOneShot(_hudManager.scanSFX);
	  }

	  return customScanNode;
	}

	private bool hasPlayerPinged(string name) {
		for (var node = _scanNodes.First; node != null; node = node.Next)
		{
			if(node.Value.owner == name) {
				return true;
			}
		}
		return false;
	} 

	private RaycastHit shootRay(float x, float y, float z)
	{
	  RaycastHit hit;
	  float someOffset = 2.5f;

	  // Assuming _mainPlayer is the player GameObject
	  Transform playerTransform = _mainPlayer.gameplayCamera.transform;
	  // Offset the starting position of the ray to be in front of the player

	  Vector3 rayStart = playerTransform.position + playerTransform.forward * someOffset;

	  Physics.Raycast(rayStart, playerTransform.forward, out hit, 1000f);

	  return hit;
	}
  }
}
