using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace CHi
{
	public class CHiManager
	{

		public static CHiBase Base
		{
			get
			{
				LoadBase();
				return _base;
			}
		}

		public static readonly Dictionary<CHiIcon, Sprite> DefaultSprites = new Dictionary<CHiIcon, Sprite>();



		private static CHiBase _base;
		private static bool _baseLoaded;




		#region Public: Get Styles Of Object

		public static List<CHiStyle> GetStylesOfObject(GameObject go)
		{
			var styles = new List<CHiStyle>();

			foreach (var condition in Base.CustomConditions)
			{
				switch (condition.Type)
				{
					case CHiRuleType.Enabled:
						if (go.activeInHierarchy)
							styles.Add(condition.GetStyle());
						break;
					case CHiRuleType.Disabled:
						if (!go.activeInHierarchy)
							styles.Add(condition.GetStyle());
						break;
					case CHiRuleType.Tag:
						if (go.tag == condition.Value)
							styles.Add(condition.GetStyle());
						break;
					case CHiRuleType.Layer:
						if (go.layer == LayerMask.NameToLayer(condition.Value))
							styles.Add(condition.GetStyle());
						break;
				}
			}
			return styles;
		}

		#endregion




		#region Load Base

		public static void LoadBase(bool force = false)
		{
#if UNITY_EDITOR
			if (_baseLoaded && !force) return;
			var defaultSprites = Enum.GetValues(typeof(CHiIcon));

			DefaultSprites.Clear();

			foreach (var defaultSprite in defaultSprites)
			{
				if (defaultSprite.ToString() == "None") continue;

				var sprite = Resources.Load<Sprite>("Icons/ci_" + defaultSprite);
				DefaultSprites.Add((CHiIcon)defaultSprite, sprite);
			}

			_base = Resources.LoadAll("", typeof(CHiBase)).SingleOrDefault() as CHiBase;

			if (_base == null)
				CreateNew();

			SortBase();

			for (int i = 0; i < _base.CustomConditions.Length; i++)
			{
				var rule = _base.CustomConditions[i];
				if (string.IsNullOrEmpty(rule.IconPath)) continue;

				var sprite = AssetDatabase.LoadAssetAtPath(rule.IconPath, typeof(Sprite)) as Sprite;
				rule.Icon = sprite;
			}


			//	_mousePressed = false;


			_baseLoaded = true;

#else
			_base = null;
#endif
		}

		private static void CreateNew()
		{
			Debug.LogError("CHiBase is missing. Need to create new one");
#if UNITY_EDITOR
			string filePath = EditorUtility.SaveFilePanelInProject(
				"CHiBase is missing, save new in Resources folder",
				"CHiBase",
				"asset",
				"CHiBase is missing, save new in Resources folder");

			if (string.IsNullOrEmpty(filePath)) return;

			if (!filePath.Contains("Resources/"))
			{
				Debug.LogError("CHiBase creation failed: path " + filePath + " not contains Resource folder");
				return;
			}

			//string resourcesPath = Path.Combine("Assets", "Resources");
			//string filePath = Path.Combine(resourcesPath, LocalizationManager.ResourceName);

			var instance = ScriptableObject.CreateInstance<CHiBase>();
			AssetDatabase.CreateAsset(instance, filePath);
			AssetDatabase.SaveAssets();

			_base = instance;

			Debug.Log("CHiBase Created");
#endif
		}

		public static void SortBase()
		{
			_base.CustomConditions = _base.CustomConditions.OrderByDescending(c => (int)c.Type).ToArray();
		}


		public static string[] GetLayers()
		{
			List<string> layers = new List<string>();
			for (int i = 0; i <= 31; i++) //user defined layers start with layer 8 and unity supports 31 layers
			{
				var layer = LayerMask.LayerToName(i); //get the name of the layer
				if (layer.Length > 0) //only add the layer if it has been named (comment this line out if you want every layer)
					layers.Add(layer);
			}
			return layers.ToArray();
		}

		public static string[] GetTags()
		{
			return UnityEditorInternal.InternalEditorUtility.tags;
		}

		#endregion

	}
}

