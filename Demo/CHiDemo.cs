using UnityEngine;
using System.Collections;

public class CHiDemo : MonoBehaviour, ICHi {

	public Sprite DemoSprite;

	public CHiCondition CHiThis()
	{
		return new CHiCondition()

			// This style will be applied to all objects with CHiDemo script attached
			.Add(() => true, new CHiStyle().WithForeground(new Color(0.4f, 0.1f, 0.4f)))

			// This style will override the previous color and set the Sprite
			.Add(() => name.StartsWith("Angry"), new CHiStyle()
				.WithForeground(CHiColor.Red)
				.WithIcon(DemoSprite))

			// This one will override previous colours and set default icon
			.Add(() => name.StartsWith("Nice"), new CHiStyle()
				.WithForeground(CHiColor.Green)
				.WithColoredIcon(CHiIcon.Heart, Color.black))

			// And this one will set background and icon if object have some shilds
			.Add(() => transform.childCount > 0, new CHiStyle()
				.WithBackground(new Color(.5f, .8f, .5f))
				.WithIcon(CHiIcon.Layers));
	}
}
