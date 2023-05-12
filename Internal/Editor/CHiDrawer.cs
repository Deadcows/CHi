using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


namespace CHi
{
	[InitializeOnLoad]
	public class CHiDrawer
	{

		private static readonly Dictionary<ICHi, CHiCondition> ItemsList = new Dictionary<ICHi, CHiCondition>();

		public static bool IsDirty;

		private static bool _mousePressed;
		private static Vector2 _mousePressedPosition;


		static CHiDrawer()
		{
#if UNITY_EDITOR
			EditorApplication.update += Update;

			EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItemIcon;

			EditorApplication.playmodeStateChanged += () => IsDirty = true;
#endif
		}

		private static void Update()
		{
			if (EditorApplication.isCompiling)
				IsDirty = true;
		}



		static void DrawHierarchyItemIcon(int instanceId, Rect selectionRect)
		{
			if (IsDirty)
			{
				ItemsList.Clear();
				IsDirty = false;
			}

			if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if (_mousePressed) return;
				_mousePressed = true;
				_mousePressedPosition = Event.current.mousePosition;
			}
			else if (Event.current.type == EventType.MouseUp && _mousePressed)
			{
				if (!_mousePressed) return;
				_mousePressed = false;
			}

			GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			if (go == null) return;

			List<CHiStyle> styles = CHiManager.GetStylesOfObject(go);

			ICHi item = (ICHi)go.GetComponent(typeof(ICHi));
			if (item != null)
			{
				if (!ItemsList.Keys.Contains(item))
					ItemsList.Add(item, item.CHiThis());

				styles.AddRange(ItemsList[item].GetRawConditions().Where(c => TryInvoke(c.Key, go)).Select(c => c.Value).ToList());
			}

			bool currentSelected = (!_mousePressed && Selection.Contains(go.GetInstanceID())) ||
					(_mousePressed && selectionRect.Contains(_mousePressedPosition));

			var applyBackground = styles.Any(c => c.UseBackground);
			var background = styles.Where(c => c.UseBackground).Select(c => c.Background).LastOrDefault();

			var applyForeground = styles.Any(c => c.UseForeground);
			var foreground = styles.Where(c => c.UseForeground).Select(c => c.Foreground).LastOrDefault();

			var icons =
				styles.Where(c => c.UseIcon && c.Icon != null)
					.Select(c => new { c.Icon, UseTint = c.UseIconTint, Tint = c.IconTint, DefaultTint = c.DefaultIconUsed }).ToArray();

			if (!currentSelected && applyForeground)
			{
				var defColor = GUI.color;
				GUI.color = applyBackground
					? background
					: EditorGUIUtility.isProSkin
						? (Color)new Color32(56, 56, 56, 255)
						: (Color)new Color32(194, 194, 194, 255);

				if (!EditorApplication.isPlayingOrWillChangePlaymode || applyBackground)
				GUI.DrawTexture(selectionRect, EditorGUIUtility.whiteTexture);

				GUI.color = defColor;
			}

			if (applyForeground)
			{
				GUIStyle style = new GUIStyle((GUIStyle)"Hi Label")
				{
					padding = { left = EditorStyles.label.padding.left },
					normal = { textColor = (currentSelected) ? (Color)new Color32(175, 199, 254, 255) : foreground }
				};
				GUI.Label(selectionRect, go.name, style);
			}

			if (icons.Any())
			{
				for (int i = 1; i <= icons.Length; i++)
				{
					var icon = icons[i - 1];
					Rect iconRect = new Rect(selectionRect);
					iconRect.x += iconRect.width - 22 * i;
					iconRect.width = 20;
					iconRect.height = 20;
					var defaultColor = GUI.color;
					if (icon.UseTint)
						GUI.color = icon.Tint;
					else if (icon.DefaultTint)
						GUI.color = CHiManager.Base.DefaultIconTint;
					GUI.Label(iconRect, icon.Icon.texture);
					GUI.color = defaultColor;
				}
			}

		}

		private static bool TryInvoke(CHiCondition.Condition condition, GameObject conditionObject)
		{
			try
			{
				bool result = condition.Invoke();
				return result;
			}
			catch (MissingReferenceException)
			{
				IsDirty = true;
				return false;
			}
			catch (Exception ex)
			{
				Debug.LogWarning(conditionObject.name + " caused: Condition Invocation failed with exception: " + ex, conditionObject);
				IsDirty = true;
				return false;
			}
		}
	}
}
