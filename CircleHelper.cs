using UnityEngine;
using BepInEx.Logging;

namespace Pinger.CircleHelper
{
  public static class Helper
  {
	private const string TAG = "[Pinger::CircleHelper]";

	public static void debug_DisplayEntireComponentTree(Transform obj)
	{
	  if (obj == null) return;
	  int children_size = obj.childCount;
	  if (children_size <= 0) return;
	  for (int i = 0; i < children_size; i++)
	  {
		var child = obj.GetChild(i);
		Debug.Log($"{TAG}: {obj.name}/{child.name}");
		debug_DisplayComponent(obj);
		debug_DisplayEntireComponentTree(child);
	  }
	  Debug.Log($"EOF - {obj.name}");

	}
	private static void debug_DisplayComponent(Transform obj)
	{
	  Debug.Log($"{TAG}: {obj.name} :: typeof {obj.GetType()}");
	}

	public static Transform debug_GrabInnerCircle(Transform obj)
	{
	  //look for the name Inner Circle
	  if (obj == null) return null;
	  int children_size = obj.childCount;
	  if (children_size <= 0) return null;
	  Transform innerCircle = null;
	  for (int i = 0; i < children_size; i++)
	  {
		var child = obj.GetChild(i);
		if (child.name == "Inner")
		{
		  innerCircle = child;
		  break;
		}
	  }
	  return innerCircle;
	}


  }
}
