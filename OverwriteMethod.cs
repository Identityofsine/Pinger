using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using GameNetcodeStuff;
using System.Reflection;
using System.Collections;

namespace Pinger.Overrider
{
  //override Awake() method in StartOfRound class
  [HarmonyPatch(typeof(StartOfRound), "Awake")]
  internal class StartOfRound_Awake
  {
	[HarmonyPatch(typeof(StartOfRound), "Awake")]
	[HarmonyPrefix]
	static bool Prefix()
	{
	  Debug.Log("StartOfRound.Awake() is called");
	  //do something before original method
	  //run original
	  return true;
	}
  }

	/*
  [HarmonyPatch(typeof(HUDManager))]
  internal class HUDManager_MeetsScanNodeRequirements
  {

	[HarmonyPrefix]
	static bool Postfix(ref bool __result)
	{
	  return true;
	}

	private static MethodInfo TargetMethod()
	{
	  return typeof(HUDManager).GetMethod("MeetsScanNodeRequirements", BindingFlags.Instance | BindingFlags.NonPublic);
	}

  }

  [HarmonyPatch(typeof(HUDManager))]
  internal class HUDManager_NodeIsNotVisible
  {

	[HarmonyPrefix]
	static bool Postfix(ref bool __result)
	{
	  //... return whatever
	  return true;
	}

	private static MethodInfo TargetMethod()

	  return typeof(HUDManager).GetMethod("NodeIsNotVisible", BindingFlags.Instance | BindingFlags.NonPublic);
	}

  }

	*/
	
  [HarmonyPatch]
  internal class KeyboardPing
  {
	private static float lastQPress = 0;
	const float PING_RESET = .3f; // one second
	private static bool isWaiting = false;
	private static bool pingPressed = false;

	private static IEnumerator WaitForNextQ()
	{
	  if (!isWaiting)
	  {
		isWaiting = true;
		yield return new WaitForSeconds(PING_RESET);

		if (!pingPressed)
		  Plugin.Instance.createPingWherePlayerIsLooking(false);

		isWaiting = false;
	  }

	}


	[HarmonyPatch(typeof(PlayerControllerB), "Update")]
	[HarmonyPostfix]
	static void PingCommand(PlayerControllerB __instance)
	{
	  long CURRENT_TIME = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
	  bool flag = false;
	  if (!__instance.IsOwner || !__instance.isPlayerControlled || __instance.inTerminalMenu || __instance.isTypingChat || __instance.isPlayerDead)
	  {
		flag = true;
	  }
	  if (!flag)
	  {
		if (Keyboard.current.qKey.wasPressedThisFrame)
		{
		  if (!pingPressed)
		  {
			pingPressed = true;
			__instance.StartCoroutine(WaitForNextQ());
		  }
		  else
		  {

			bool response = Plugin.Instance.createPingWherePlayerIsLooking(true);
		  }
		  return;
		}
		if (pingPressed)
		{
		  lastQPress += Time.deltaTime;

		  if (lastQPress >= PING_RESET)
		  {
			pingPressed = false;
			isWaiting = false;
			lastQPress = 0;
		  }
		}
	  }
	}
  }
}

