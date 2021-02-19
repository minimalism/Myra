﻿using System.ComponentModel;
using Myra.Graphics2D.UI.Styles;
using Myra.Utility;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace Myra.Graphics2D.UI
{
	public class Image : Widget
	{
		private IImage _image, _overImage, _pressedImage;

		[Category("Appearance")]
		public IImage Renderable
		{
			get
			{
				return _image;
			}

			set
			{
				if (value == _image)
				{
					return;
				}

				_image = value;
				InvalidateMeasure();
			}
		}

		[Category("Appearance")]
		public IImage OverRenderable
		{
			get
			{
				return _overImage;
			}

			set
			{
				if (value == _overImage)
				{
					return;
				}

				_overImage = value;
				InvalidateMeasure();
			}
		}

		internal IImage PressedRenderable
		{
			get
			{
				return _pressedImage;
			}

			set
			{
				if (value == _pressedImage)
				{
					return;
				}

				_pressedImage = value;
				InvalidateMeasure();
			}
		}

		internal bool IsPressed
		{
			get; set;
		}

		[Category("Appearance")]
		[DefaultValue("#FFFFFFFF")]
		public Color Color { get; set; } = Color.White;

		[Category("Behavior")]
		[DefaultValue(ImageResizeMode.Stretch)]
		public ImageResizeMode ResizeMode { get; set; }

		protected override Point InternalMeasure(Point availableSize)
		{
			var result = _image != null ? _image.Size : Mathematics.PointZero;

			var overSize = _overImage != null ? _overImage.Size : Mathematics.PointZero;
			if (overSize.X > result.X)
			{
				result.X = overSize.X;
			}

			if (overSize.Y > result.Y)
			{
				result.Y = overSize.Y;
			}

			var pressedSize = _pressedImage != null ? _pressedImage.Size : Mathematics.PointZero;
			if (pressedSize.X > result.X)
			{
				result.X = pressedSize.X;
			}

			if (pressedSize.Y > result.Y)
			{
				result.Y = pressedSize.Y;
			}

			return result;
		}

		public override void InternalRender(RenderContext context)
		{
			var image = Renderable;

			if (UseHoverRenderable && OverRenderable != null)
			{
				image = OverRenderable;
			}

			if (IsPressed && PressedRenderable != null)
			{
				image = PressedRenderable;
			}

			if (image != null)
			{
				var bounds = ActualBounds;

				if (ResizeMode == ImageResizeMode.KeepAspectRatio)
				{
					var aspect = (float)image.Size.X / image.Size.Y;
					bounds.Height = (int)(bounds.Width * aspect);
				}

				image.Draw(context, bounds, Color);
			}
		}

		public void ApplyPressableImageStyle(PressableImageStyle imageStyle)
		{
			ApplyWidgetStyle(imageStyle);

			Renderable = imageStyle.Image;
			OverRenderable = imageStyle.OverImage;
			PressedRenderable = imageStyle.PressedImage;
		}
	}
}