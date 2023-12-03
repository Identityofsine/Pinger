using UnityEngine;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

namespace Settings.Waiter {
	//extend monospace
	public static class Extension {
		
		public delegate bool WaitCondition();
		public delegate void Callback(bool success);

		public static void changeText(this GameObject obj, string text) {
			//make sure TextMeshProUGUI exists
			if(obj.GetComponent<TextMeshProUGUI>() == null) {
				Debug.Log("[Pinger::SettingsAPI] TextMeshProUGUI not found!");
				return;
			}
			obj.GetComponent<TextMeshProUGUI>().text = text;
		}

		public static async void WaitUntilComponentExists(WaitCondition wait_func, Callback callback, int limit = 0) {

			const int SLEEP_TIME = 100;
			bool limited = limit > 0;
			bool failure = false;

			bool result = wait_func();
			if (result) {
				await Task.Delay(SLEEP_TIME);
			}

			Debug.Log("[Pinger::WaitUntilComponentExists] Waiting for component to exist...");
			//adjust limit to be one seconds worth despite sleep time
			int limit_copy = limit / ((int)(SLEEP_TIME / 1000f) + 1);	
			while(!result) {
				if(limited) {
					limit--;
					if(limit <= 0) {
						failure = true;
						break;
					}
				}
				result = wait_func();
				await Task.Delay(SLEEP_TIME);
			}
			callback(!failure);

		}

		public static void WaitUntilComponentExists(this MonoBehaviour mono, WaitCondition wait_func, Callback callback, int limit = 0) {

			IEnumerator loop_logic() {
				const int SLEEP_TIME = 100;
				bool limited = limit > 0;
				bool failure = false;

				bool result = wait_func();
				if (result) {
					yield return null;
				}
				
				Debug.Log("[Pinger::WaitUntilComponentExists] Waiting for component to exist...");
				//adjust limit to be one seconds worth despite sleep time
				int limit_copy = limit / ((int)(SLEEP_TIME / 1000f) + 1);	
				while(!result) {
					if(limited) {
						limit--;
						if(limit <= 0) {
							failure = true;
							break;
						}
					}
					result = wait_func();
					yield return new WaitForSeconds(SLEEP_TIME / 1000f);
				}
				callback(!failure);
			}

			mono.StartCoroutine(loop_logic());

		}
	}
}


