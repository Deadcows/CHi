using System;
using System.Linq;
using CHi;
using UnityEditor;
using UnityEngine;

public class CHiWindow : EditorWindow
{

	// This vars used to emulate Drag&Drop on icons
	private static bool _previousMouseState;
	private static bool _mousePressed;
	private static bool _previousOverState;
	private static bool _mouseOver;
	private static Vector2 _mousePressedPosition;
	private static CHiIcon _selectedIcon;

	private static Vector2 _scrollViewPosition;

	private static string[] _layers;


	private static string SayHello
	{
		get
		{
			if (string.IsNullOrEmpty(_sayHello))
			{
				int r = UnityEngine.Random.Range(0, chiWords.Length - 1);
				_sayHello = chiWords[r];
			}
			return _sayHello;
		}
	}
	private static string _sayHello;

	private static string[] chiWords =
		{
			"Welcome to CHi my friend",
			"May CHi be with you",
			"I'm glad you're back",
			"Call me Chihuahua today",
			"You're pretty",
			"Your game looks cool!",
			"Shake it up",
			"213 new icons will come with next update!",
			"Give us Smooth! Give us Silky!",
			"Say hello",
			"Say what",
			"I'll get it done for you",
			"It's dangerous to go alone! Take CHi!",
			"Every day is a new opportunity",
			"Insert Header here",
			"<text>",
			"And thanks for all the fish!",
			"Mathematical!",
			"You know nothing, Jon Snow",
			"A dragon is not a slave",
			"Sweetness and light",
			"Oh, you!",
			"<Excited beeping>",
			"CHi. CHi Never Changes"
		};




	[MenuItem("Window/CHi Manager")]
	static void Init()
	{
		CHiManager.LoadBase(true);

		var instance = GetWindow<CHiWindow>();
		instance.minSize = new Vector2(355, 400);
		instance.maxSize = new Vector2(355, instance.maxSize.y);
		instance.title = "CHi Manager";

		instance.ShowPopup();
	}

	private static Texture2D textureWhite;
	private static Texture2D textureBlack;

	private static Texture2D GetEmptyTexture(bool white)
	{
		if (white)
		{
			if (textureWhite == null)
			{
				textureWhite = new Texture2D(1, 1);
				textureWhite.SetPixel(1, 1, new Color(1, 1, 1, 1));
				textureWhite.Apply();
			}
			return textureWhite;
		}
		else
		{
			if (textureBlack == null)
			{
				textureBlack = new Texture2D(1, 1);
				textureBlack.SetPixel(1, 1, new Color(0, 0, 0, 1));
				textureBlack.Apply();
			}
			return textureBlack;
		}

	}

	void Update()
	{
		if (_mousePressed != _previousMouseState ||
			_mouseOver != _previousOverState)
		{
			Repaint();
			_previousMouseState = _mousePressed;
			_previousOverState = _mouseOver;
		}
	}


	private void OnGUI()
	{

		#region Header Box

		if (_layers == null || _layers.Length == 0)
			_layers = CHiManager.GetLayers();

		EditorGUILayout.Space();

		var boxStyle = new GUIStyle(GUI.skin.box);
		boxStyle.normal.textColor = Color.black;
		boxStyle.padding.top = 5;
		boxStyle.fixedWidth = 350;
		boxStyle.fixedHeight = 25;
		GUILayout.Box(SayHello, boxStyle);

		EditorGUILayout.Space();

		#endregion

		var defaultContentColor = GUI.contentColor;
		var defaultBackgroundCOlor = GUI.backgroundColor;


		#region Mouse Click Detection

		bool mouseDropped = false;
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			if (_mousePressed) return;
			_mousePressed = true;

			_mousePressedPosition = Event.current.mousePosition;
		}
		else if (Event.current.type == EventType.MouseUp && _mousePressed)
		{
			if (!_mousePressed) return;

			if (_selectedIcon != CHiIcon.None)
				mouseDropped = true;

			_mousePressed = false;

		}
		if (_selectedIcon != CHiIcon.None)
			EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), MouseCursor.MoveArrow);

		#endregion


		#region Default Icons Set

		EditorGUILayout.BeginHorizontal();
		GUI.contentColor = CHiManager.Base.DefaultIconTint;

		int iRow = 0;
		foreach (var defaultSprite in CHiManager.DefaultSprites)
		{
			if (iRow % 14 == 0)
			{
				iRow = 0;
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
			}

			iRow++;

			var iconStyle = new GUIStyle { fixedWidth = 20, fixedHeight = 20, margin = new RectOffset(5, 5, 5, 5) };

			var iconContent = new GUIContent(defaultSprite.Value.texture, defaultSprite.Key.ToString());
			var iconRect = GUILayoutUtility.GetRect(iconContent, iconStyle);

			if (_mousePressed && iconRect.Contains(_mousePressedPosition))
				_selectedIcon = defaultSprite.Key;

			if (_mousePressed && _selectedIcon == defaultSprite.Key)
			{
				GUI.backgroundColor = Color.gray;
				iconStyle.normal.background = GetEmptyTexture(true);
			}

			GUI.Label(iconRect, iconContent, iconStyle);

			GUI.backgroundColor = defaultBackgroundCOlor;
		}
		GUI.contentColor = defaultContentColor;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		CHiManager.Base.DefaultIconTint = EditorGUILayout.ColorField(CHiManager.Base.DefaultIconTint);

		EditorGUILayout.Space();

		#endregion


		#region Add New Rule Button

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Tag", EditorStyles.toolbarButton))
			AddRule(CHiRuleType.Tag);
		if (GUILayout.Button("Layer", EditorStyles.toolbarButton))
			AddRule(CHiRuleType.Layer);

		if (CHiManager.Base.CustomConditions.All(c => c.Type != CHiRuleType.Enabled))
		{
			if (GUILayout.Button("Enabled", EditorStyles.toolbarButton))
				AddRule(CHiRuleType.Enabled);
		}
		if (CHiManager.Base.CustomConditions.All(c => c.Type != CHiRuleType.Disabled))
		{
			if (GUILayout.Button("Disabled", EditorStyles.toolbarButton))
				AddRule(CHiRuleType.Disabled);
		}

		EditorGUILayout.EndHorizontal();

		#endregion


		#region Registered Rules

		_mouseOver = false;
		_scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
		for (int i = 0; i < CHiManager.Base.CustomConditions.Length; i++)
		{
			var rule = CHiManager.Base.CustomConditions[i];
			if (DrawRule(rule, mouseDropped))
			{
				CHiManager.Base.CustomConditions = CHiManager.Base.CustomConditions.Where(c => c != rule).ToArray();
				EditorUtility.SetDirty(CHiManager.Base);
				return;
			}
		}
		EditorGUILayout.EndScrollView();

		#endregion


		if (mouseDropped)
			_selectedIcon = CHiIcon.None;

		if (GUI.changed)
		{
			EditorUtility.SetDirty(CHiManager.Base);
			CHiManager.SortBase();
		}
	}

	#region Add Rule To Base

	private void AddRule(CHiRuleType type)
	{
		int length = CHiManager.Base.CustomConditions.Length;
		Array.Resize(ref CHiManager.Base.CustomConditions, length + 1);

		var newCondition = new CHiRule { Type = type };
		if (type == CHiRuleType.Tag)
			newCondition.Value = "Untagged";
		if (type == CHiRuleType.Layer)
			newCondition.Value = "Default";

		CHiManager.Base.CustomConditions[length] = newCondition;
	}

	#endregion



	private bool DrawRule(CHiRule rule, bool mouseDropped)
	{
		var defaultBackgroundColor = GUI.backgroundColor;
		var defaultColor = GUI.color;


		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();

		#region Draw Remove Icon Button

		var butrtonStyle = new GUIStyle(EditorStyles.toolbarButton)
		{
			fixedWidth = 20,
			fixedHeight = 20,
			margin = new RectOffset(5, 10, 0, 0)
		};
		butrtonStyle.contentOffset = new Vector2(0, -1);
		if (GUILayout.Button("—", butrtonStyle))
			return true;

		#endregion


		#region Draw Type Lable & Selection


		var labelRect = EditorGUILayout.GetControlRect(false, 18, EditorStyles.label);
		labelRect.width = 355;
		if (rule.UseBackground)
		{
			GUI.color = rule.Background;
			labelRect.yMin -= 3;
			GUI.DrawTexture(labelRect, EditorGUIUtility.whiteTexture);
			labelRect.yMin += 3;
		}
		GUI.color = defaultColor;

		var labelStyle = new GUIStyle(EditorStyles.label);

		if (rule.UseForeground)
			labelStyle.normal.textColor = rule.Foreground;
		EditorGUI.LabelField(labelRect, rule.Type.ToString(), labelStyle);

		labelRect.xMin += 100;
		labelRect.width = 150;

		if (rule.Type == CHiRuleType.Tag)
		{
			var tags = CHiManager.GetTags();

			if (!tags.Contains(rule.Value))
				rule.Value = tags.First();
			rule.Value = EditorGUI.TagField(labelRect, rule.Value);
		}
		else if (rule.Type == CHiRuleType.Layer)
		{
			var layer = Array.IndexOf(_layers, rule.Value);
			if (layer < 0)
				layer = 0;

			rule.Value = _layers[EditorGUI.Popup(labelRect, layer, _layers)];
		}

		#endregion


		#region Draw Icon

		var iconStyle = new GUIStyle { fixedWidth = 20, fixedHeight = 20, margin = new RectOffset(5, 10, 0, 0) };

		var iconTexture = rule.Icon != null ? rule.Icon.texture : GetEmptyTexture(false);

		var iconContent = new GUIContent(iconTexture);

		var iconRect = GUILayoutUtility.GetRect(iconContent, iconStyle);

		if (rule.UseIconTint)
			GUI.color = rule.IconTint;
		else if (rule.DefaultIcon)
			GUI.color = CHiManager.Base.DefaultIconTint;

		if (rule.Icon == null)
		{
			GUI.backgroundColor = Color.black;
			iconStyle.normal.background = GetEmptyTexture(true);
		}
		if (iconRect.Contains(Event.current.mousePosition))
		{
			if (_mousePressed && _selectedIcon != CHiIcon.None)
			{
				GUI.backgroundColor = Color.gray;
				iconStyle.normal.background = GetEmptyTexture(true);

				_mouseOver = true;
			}
		}

		if (mouseDropped && iconRect.Contains(Event.current.mousePosition) && _selectedIcon != CHiIcon.None)
		{
			var icon = CHiManager.DefaultSprites[_selectedIcon];

			if (icon != null)
				rule.IconPath = AssetDatabase.GetAssetPath(icon);
			rule.Icon = icon;
			rule.DefaultIcon = true;
			GUI.changed = true;
		}
		GUI.Label(iconRect, iconContent, iconStyle);

		GUI.backgroundColor = defaultBackgroundColor;
		GUI.color = defaultColor;

		#endregion

		EditorGUILayout.EndHorizontal();

		#region Colours Pickers

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();

		var rect = EditorGUILayout.GetControlRect(false, 20, EditorStyles.colorField);
		rect.xMin += 10;

		rect.width = 20;

		rule.UseBackground = EditorGUI.Toggle(rect, rule.UseBackground);
		if (rule.UseBackground && rule.Background.a == 0)
			rule.Background = Color.white;

		rect.xMin += 25;
		rect.width = 80;
		if (rule.UseBackground)
		{
			rule.UseForeground = true;
			rule.Background = EditorGUI.ColorField(rect, rule.Background);
		}
		else
			GUI.Label(rect, "Background");

		rect.xMin += 85;
		rect.width = 20;
		rule.UseForeground = EditorGUI.Toggle(rect, rule.UseForeground);
		if (rule.UseForeground && rule.Foreground.a == 0)
			rule.Foreground = Color.black;


		rect.xMin += 25;
		rect.width = 80;
		if (rule.UseForeground)
			rule.Foreground = EditorGUI.ColorField(rect, rule.Foreground);
		else
			GUI.Label(rect, "Font Color");


		rect.xMin += 85;
		rect.width = 20;
		rule.UseIconTint = EditorGUI.Toggle(rect, rule.UseIconTint);
		if (rule.UseIconTint && rule.IconTint.a == 0)
			rule.IconTint = Color.white;


		rect.xMin += 25;
		rect.width = 80;
		if (rule.UseIconTint)
			rule.IconTint = EditorGUI.ColorField(rect, rule.IconTint);
		else
			GUI.Label(rect, "Icon Tint");

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		#endregion

		return false;
	}

}
