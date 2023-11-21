
namespace Pinger.Adder
{
  static class Adder
  {

	public static void AddToClass<T>(this T obj, System.Action action)
	{
	  //extend the class with a new method
	  obj.GetType().GetMethod("AddToClass").Invoke(obj, new object[] { action });
	}

  }
}
