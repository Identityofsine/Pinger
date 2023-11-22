using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using GameNetcodeStuff;
using System.Reflection;

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

  [HarmonyPatch(typeof(HUDManager))]
  internal class HUDManager_MeetsScanNodeRequirements
  {

	[HarmonyPrefix]
	static bool Postfix(ref bool __result)
	{
	  __result = true;
	  return false;
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
	  return true;
	}

	private static MethodInfo TargetMethod()
	{
	  return typeof(HUDManager).GetMethod("NodeIsNotVisible", BindingFlags.Instance | BindingFlags.NonPublic);
	}

  }

  [HarmonyPatch]
  internal class KeyboardPing
  {
	[HarmonyPatch(typeof(PlayerControllerB), "Update")]
	[HarmonyPostfix]
	static void PingCommand(PlayerControllerB __instance)
	{
	  bool flag = false;
	  if (!__instance.IsOwner || !__instance.isPlayerControlled || __instance.inTerminalMenu || __instance.isTypingChat || __instance.isPlayerDead)
	  {
		flag = true;
	  }
	  if (!flag)
	  {
		if (Keyboard.current.qKey.wasPressedThisFrame)
		{
		  Plugin.Instance.createPingWherePlayerIsLooking();
		}
	  }
	  return;
	}
  }
}

