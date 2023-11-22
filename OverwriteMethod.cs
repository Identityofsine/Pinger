using HarmonyLib;
using UnityEngine;

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
}

