using HarmonyLib;
using UnityEngine;
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

}

