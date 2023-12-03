using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using System.Collections.Generic;
using HarmonyLib;
using Settings.Waiter;

namespace Settings.Crafter {

	public static class SettingsElementCreator {

		public delegate void Callback(GameObject obj);

		private static Vector3 ReturnPosition(GameObject container, int index) {
			//get first child
			GameObject first_child = container.transform.GetChild(0).gameObject;
			//get height of first child
			float height = first_child.GetComponent<RectTransform>().rect.height;
			//get position of first child
			Vector3 position = first_child.transform.position;
			//adjust position
			position.y -= height * index;
			return position;
		}

		public static GameObject CreateCheckbox(GameObject container, string name, bool default_value) {

			Vector3 position = ReturnPosition(container, 1);
			GameObject checkbox = GameObject.Instantiate(GameObject.Find("InvertYAxis"), container.transform);
			float height = checkbox.GetComponent<RectTransform>().rect.height;
			float offset = 0f;
			//adjust for offset based on text object
			if(container.transform.childCount > 1) {
				GameObject txt_container = checkbox.GetComponentInChildren<TextMeshProUGUI>().transform.gameObject;
				offset = txt_container.transform.localPosition.y;
			}

			checkbox.name = name;
			checkbox.transform.position = position;
			checkbox.transform.localPosition = new Vector3(0 - (offset), -(height / 2f) - 10f, 0);

			return checkbox;

		}

		public static void CreateContainer(Vector3 position, string name, Callback callback) {

			GameObject execute() {
				GameObject copy = GameObject.Find("ControlsOptions");
				GameObject container = GameObject.Instantiate(GameObject.Find("ControlsOptions"), GameObject.Find("SettingsPanel").transform);
				container.name = "settings-" + name.ToLower();
				GameObject header = container.transform.GetChild(0).gameObject;
				header.transform.localPosition = Vector3.zeroVector;
				//delete all children except for header
				for(int i = 1; i < container.transform.childCount; i++) {
					GameObject.Destroy(container.transform.GetChild(i).gameObject);
				}
				header.changeText(name);
				container.transform.position = position;
				container.transform.localPosition = Vector3.zeroVector;
				return container;
			}

			Extension.WaitUntilComponentExists(() => GameObject.Find("ControlsOptions") != null, (bool success) => {
				if(success) {
					Debug.Log("[Pinger::SettingsAPI] Found ControlsOptions!");
					callback(execute());
				} else {
					Debug.Log("[Pinger::SettingsAPI] Failed to find ControlsOptions!");
				}
			});

		}

	}
}

namespace Settings.Hook {

	public enum SettingType {
		Slider,
		Toggle,
		Dropdown,
		Button
	}

	//make a interface that is generic that only takes in Float,Bool,or Int

	public class SettingComponent{
		public GameObject setting {get; set;}
		public string name {get; set;}
		private string default_value {get;}
		public SettingType type {get; set;}
		public delegate void ActionFloat(int value);
		public delegate void ActionBool(bool value);
		public delegate void ActionInt(int value);
		//events
		private event ActionFloat OnSliderChange;
		private event ActionBool OnToggleChange;
		private event ActionInt OnDropdownChange;
		private event ActionInt OnButtonClick;

		public SettingComponent(GameObject settings, SettingType type) {
			//grab setting
			this.setting = settings;
			this.type = type;
		}

		public void onSliderChange(ActionFloat evnt) {
			this.OnSliderChange += evnt;
		}

		public void onToggleChange(ActionBool evnt) {
			this.OnToggleChange += evnt;
		} 

		public void onDropdownChange(ActionInt evnt) {
			this.OnDropdownChange += evnt;
		}

		public void onButtonClick(ActionInt evnt) {
			this.OnButtonClick += evnt;
		}

		void changeSetting() {
			switch(this.type) {
				case SettingType.Slider:
					break;
				case SettingType.Toggle:
					break;
				case SettingType.Dropdown:
					break;
				case SettingType.Button:
					break;
			}
		}

	}

	public class SettingsComponent {

		GameObject container {get; set;}
		List<SettingComponent> settings {get; set;}
		private GameObject header;
		private string Plugin;
		public delegate void SettingChange(SettingComponent setting);
		public event SettingChange OnSettingChange;


		public SettingsComponent(GameObject container) {
			this.container = container;
			this.settings = new List<SettingComponent>();
			this.header = container.transform.GetChild(0).gameObject;
		}

		//TODO : add loading of settings, maybe JSON?
		private List<GameObject> loadSettings() {
			List<GameObject> settings = new List<GameObject>();
			return settings;
		}

		public void addSetting(SettingComponent setting) {
			this.settings.Add(setting);
		}

		public void addSetting(GameObject setting, SettingType type) {
			this.settings.Add(new SettingComponent(setting, type));
		}

		public void addSetting(GameObject setting, SettingType type, string name) {
			SettingComponent setting_component = new SettingComponent(setting, type);
			setting_component.name = name;
			this.settings.Add(setting_component);
		}

		public void addSetting(GameObject setting, SettingType type, string name, string default_value) {
			SettingComponent setting_component = new SettingComponent(setting, type);
			setting_component.name = name;
			this.settings.Add(setting_component);
		}

		public void addSetting(string name, SettingType type) {
			GameObject setting = Crafter.SettingsElementCreator.CreateCheckbox(this.container, name, false);
			this.settings.Add(new SettingComponent(setting, type));
		}


	}

	//copy settings component
	public class SettingsAPI {
		public static IngamePlayerSettings igps_instance {get;set;} = null;
		private static SettingsAPI instance {get;set;} = null; 
		private Dictionary<string, SettingsComponent> settings {get;set;} = new Dictionary<string, SettingsComponent>();
		private Queue<string> settings_queue {get;set;} = new Queue<string>();
		private bool is_grabbing {get;set;} = false;


		private SettingsAPI() {
			//grab instance
			async_grabbing_loop();	
		} 

		private async void async_grabbing_loop() {
			const int SLEEP_TIME = 125;
			SettingsAPI.igps_instance = IngamePlayerSettings.Instance;
			while(SettingsAPI.igps_instance == null) {
				this.is_grabbing = true;
				SettingsAPI.igps_instance = IngamePlayerSettings.Instance;
				Debug.Log("[Pinger::SettingsAPI] Waiting for IngamePlayerSettings.Instance...");
				await Task.Delay(SLEEP_TIME);
			}	
			this.is_grabbing = false;
			Debug.Log("[Pinger::SettingsAPI] IngamePlayerSettings.Instance found!");

			//deque settings
			while(this.settings_queue.Count > 0) {
				string plugin = this.settings_queue.Dequeue();
				this.LoadSettings(plugin);
			}
		}

		public static SettingsAPI getInstance() {
			if(SettingsAPI.instance == null) {
				SettingsAPI.instance = new SettingsAPI();
			}
			return SettingsAPI.instance; 
		}	

		public void LoadSettings(string plugin) {
			if(SettingsAPI.igps_instance == null) {
				this.settings_queue.Enqueue(plugin);
				return;
			}
			//check if plugin exists
			if(this.settings.ContainsKey(plugin)) {
				Debug.Log("[Pinger::SettingsAPI] Plugin already exists!");
				return;
			}
			
			//get the last child of the SettingsPanel

			Extension.WaitUntilComponentExists(() => GameObject.Find("SettingsPanel") != null, (bool success) => {
				if(success) {
					Debug.Log("[Pinger::SettingsAPI] Found SettingsPanel!");
					//get last child
					GameObject settings_panel = GameObject.Find("SettingsPanel");
					GameObject last_child = settings_panel.transform.GetChild(settings_panel.transform.childCount - 1).gameObject;
					//get position of last child
					Vector3 position = last_child.transform.position;
					//adjust position
					position.y -= last_child.GetComponent<RectTransform>().rect.height;
					//create new settings component
					Crafter.SettingsElementCreator.CreateContainer(position, plugin, (obj) => {
						Debug.Log("[Pinger::SettingsAPI] Created new settings component!");
						SettingsComponent settings_component = new SettingsComponent(obj);
						this.settings.Add(plugin, settings_component);
						settings_component.addSetting("Test", SettingType.Toggle);
					});	
				} else {
					Debug.Log("[Pinger::SettingsAPI] Failed to find SettingsPanel!");
				}
			});
		}

	

		private void CopyandMoveSettings() {
			if(SettingsAPI.instance == null) {
				Debug.Log("[Pinger::SettingsAPI] SettingsAPI.instance is null!");
				return;
			}

			void CopySetting(GameObject menucontainer) {
				GameObject source = GameObject.Find("InvertYAxis");
				GameObject container = GameObject.Find("ControlsOptions");

				GameObject container_copy = GameObject.Instantiate(container, menucontainer.transform);
				container_copy.name = "PingerSettings";
				//clear children passed the first child
				for(int i = 1; i < container_copy.transform.childCount; i++) {
					GameObject.Destroy(container_copy.transform.GetChild(i).gameObject);
				}
				GameObject header = container_copy.transform.GetChild(0).gameObject;
				header.changeText("Pinger Settings");
				header.name = "PingerSettingsHeader";

				//copy source into container_copy
				GameObject source_copy = GameObject.Instantiate(source, container_copy.transform);
				float header_height = header.GetComponent<RectTransform>().rect.height;
				float source_height = source_copy.GetComponent<RectTransform>().rect.height;
				
				//shift source_copy to right under the last child
				source_copy.transform.position = new Vector3(
					source_copy.transform.position.x, 
					source_copy.transform.position.y - header_height / 2 - source_height / 2,
					source_copy.transform.position.z
				);
				//set local position to 0 (y)
				source_copy.transform.localPosition = new Vector3(
					source_copy.transform.localPosition.x, 
					0,
					source_copy.transform.localPosition.z
				);

				//1235.007 585.1533 -36.935 : inject_site
				container_copy.transform.position = new Vector3(1235.007f, 585.1533f, -36.935f);

			}
			
			//get GameObject with name SettingsPanel
			igps_instance.WaitUntilComponentExists(
				() => {
					return GameObject.Find("SettingsPanel") != null; 
				},
				(success) => {
					if(success) {
						Debug.Log("[Pinger::SettingsAPI] SettingsPanel found!");
						
					} else {
						Debug.Log("[Pinger::SettingsAPI] SettingsPanel not found!");
					}
				}, 1000);
		}

	}

}
