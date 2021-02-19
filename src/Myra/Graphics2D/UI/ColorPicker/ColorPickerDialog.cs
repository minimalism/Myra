using Myra.Utility;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace Myra.Graphics2D.UI.ColorPicker
{
	public class ColorPickerDialog : Dialog
	{
		public ColorPickerPanel ColorPickerPanel { get; }

		public Color Color
		{
			get
			{
				return ColorPickerPanel.Color;
			}

			set
			{
				ColorPickerPanel.Color = value;
			}
		}

		public ColorPickerDialog()
		{
			ColorPickerPanel = new ColorPickerPanel();

			Title = "Color Picker";
			Content = ColorPickerPanel;
		}

		public override void Close()
		{
			base.Close();

			for (var i = 0; i < ColorPickerPanel.UserColors.Length; ++i)
			{
				var colorDisplay = ColorPickerPanel.GetUserColorImage(i);
				var color = colorDisplay.Color;
				var alpha = (int) (colorDisplay.Opacity * 255);
				ColorPickerPanel.UserColors[i] = ColorStorage.CreateColor(color.R, color.G, color.B, alpha);
			}
		}
	}
}