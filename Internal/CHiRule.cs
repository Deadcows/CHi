using System;
using UnityEngine;

namespace CHi
{
	public enum CHiRuleType
	{
		None,
		Tag = 7,
		Layer = 8,
		Enabled = 9,
		Disabled = 10
	}

	[Serializable]
	public class CHiRule
	{
		public CHiRuleType Type;
		public string Value;

		public Sprite Icon;

		public string IconPath;

		public bool DefaultIcon;

		public Color Foreground;

		public bool UseForeground;

		public Color Background;

		public bool UseBackground;

		public Color IconTint;

		public bool UseIconTint;

		public CHiStyle GetStyle()
		{
			var style = new CHiStyle();
			if (Icon != null)
			{
				if (!UseIconTint && DefaultIcon)
				{
					style.WithColoredIcon(Icon, CHiManager.Base.DefaultIconTint);
					style.DefaultIconUsed = true;
				}
				else if (UseIconTint)
					style.WithColoredIcon(Icon, IconTint);
				else
					style.WithIcon(Icon);
			}
			if (UseBackground)
				style.WithBackground(Background);
			if (UseForeground)
				style.WithForeground(Foreground);

			return style;
		}
	}

}

