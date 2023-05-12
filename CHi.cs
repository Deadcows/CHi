using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using CHi;
#endif


#region Hierarchy Color Preset Enum

public enum CHiColor
{
	Red,
	Green,
	Gray
}

#endregion

#region Hierarchy Icons Preset Enum

public enum CHiIcon
{
	None,
	Audio,
	Bell,
	Camera,
	Check,
	CircleDouble,
	Clock,
	Command,
	Cross,
	Dot,
	Exchange,
	Exclamation,
	Flag,
	Folder,
	Gamepad,
	Gear,
	Grid,
	Heart,
	Hidden,
	Home,
	Infinity,
	Layers,
	LeftBracket,
	Letter,
	List,
	Lock,
	Mark,
	Message,
	Microphone,
	Minus,
	Person,
	Photocamera,
	Plus,
	Question,
	Restart,
	Ribbon,
	RightBracket,
	Star,
	Sun,
	Unlock,
	Visible,
	Web,
	Zipper
}

#endregion


public interface ICHi
{
	CHiCondition CHiThis();
}

public struct CHiStyle
{
	public Sprite Icon;

	public bool UseIcon;

	public bool DefaultIconUsed;

	public Color Foreground;

	public bool UseForeground;

	public Color Background;

	public bool UseBackground;

	public Color IconTint;

	public bool UseIconTint;


	#region Set Icon

	/// <summary>
	/// Set Regular Icon
	/// </summary>
	public CHiStyle WithIcon(Sprite icon)
	{
		Icon = icon;
		UseIcon = true;

		return this;
	}

	/// <summary>
	/// Set Regular Icon
	/// </summary>
	public CHiStyle WithIcon(CHiIcon icon)
	{
		DefaultIconUsed = true;
		var i = WithIcon(ToIcon(icon));

		return i;
	}

	#endregion


	#region Set Coloured Icon

	/// <summary>
	/// Set Coloured Icon
	/// </summary>
	public CHiStyle WithColoredIcon(Sprite icon, Color tint)
	{
		WithIcon(icon);
		IconTint = tint;
		UseIconTint = true;

		return this;
	}

	/// <summary>
	/// Set Coloured Icon
	/// </summary>
	public CHiStyle WithColoredIcon(CHiIcon icon, Color tint)
	{
		return WithColoredIcon(ToIcon(icon), tint);
	}

	/// <summary>
	/// Set Coloured Icon
	/// </summary>
	public CHiStyle WithColoredIcon(Sprite icon, CHiColor tint)
	{
		return WithColoredIcon(icon, ToColor(tint));
	}

	/// <summary>
	/// Set Coloured Icon
	/// </summary>
	public CHiStyle WithColoredIcon(CHiIcon icon, CHiColor tint)
	{
		return WithColoredIcon(ToIcon(icon), ToColor(tint));
	}

	#endregion


	#region Set Foreground

	/// <summary>
	/// Set Hierarchy Item Text Color
	/// </summary>
	public CHiStyle WithForeground(Color color)
	{
		Foreground = color;
		UseForeground = true;

		return this;
	}

	/// <summary>
	/// Set Hierarchy Item Text Color
	/// </summary>
	public CHiStyle WithForeground(CHiColor color)
	{
		return WithForeground(ToColor(color));
	}

	#endregion


	#region Set Background

	/// <summary>
	/// Set Hierarchy Item Background Color
	/// </summary>
	public CHiStyle WithBackground(Color color)
	{
		Background = color;
		UseBackground = true;

		if (!UseForeground)
			return WithForeground(Color.black);

		return this;
	}

	/// <summary>
	/// Set Hierarchy Item Background Color
	/// </summary>
	public CHiStyle WithBackground(CHiColor color)
	{
		return WithBackground(ToColor(color));
	}

	#endregion



	#region Helper Functions

	private Color ToColor(CHiColor color)
	{
		switch (color)
		{
			case CHiColor.Green:
				return new Color(.2f, .6f, .2f);
			case CHiColor.Red:
				return new Color(.8f, .4f, .4f);
			case CHiColor.Gray:
				return Color.gray;
			default:
				return Color.white;
		}
	}

	private Sprite ToIcon(CHiIcon icon)
	{
#if UNITY_EDITOR
		if (CHiManager.DefaultSprites.Keys.Contains(icon))
			return CHiManager.DefaultSprites[icon];
#endif
		return null;
	}

	#endregion
}

public class CHiCondition
{
	public delegate bool Condition();

	private readonly List<KeyValuePair<Condition, CHiStyle>> _conditions = new List<KeyValuePair<Condition, CHiStyle>>();


	public CHiCondition Add(Condition condition, CHiStyle style)
	{
		_conditions.Add(new KeyValuePair<Condition, CHiStyle>(condition, style));
		return this;
	}


	public List<KeyValuePair<Condition, CHiStyle>> GetRawConditions()
	{
		return _conditions;
	}
}
