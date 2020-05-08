﻿#if !STRIDE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#else
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#endif

namespace Myra.Graphics2D.TextureAtlases
{
	public class NinePatchRegion : TextureRegion
	{
		private readonly Thickness _info;

		private readonly TextureRegion _topLeft,
			_topCenter,
			_topRight,
			_centerLeft,
			_center,
			_centerRight,
			_bottomLeft,
			_bottomCenter,
			_bottomRight;

		public Thickness Info
		{
			get { return _info; }
		}

		public NinePatchRegion(Texture2D texture, Rectangle bounds, Thickness info) : base(texture, bounds)
		{
			_info = info;

			var centerWidth = bounds.Width - info.Left - info.Right;
			var centerHeight = bounds.Height - info.Top - info.Bottom;

			var y = bounds.Y;
			if (info.Top > 0)
			{
				if (info.Left > 0)
				{
					_topLeft = new TextureRegion(texture,
						new Rectangle(bounds.X,
							y,
							info.Left,
							info.Top));
				}

				if (centerWidth > 0)
				{
					_topCenter = new TextureRegion(texture,
						new Rectangle(bounds.X + info.Left,
							y,
							centerWidth,
							info.Top));
				}

				if (info.Right > 0)
				{
					_topRight = new TextureRegion(texture,
						new Rectangle(bounds.X + info.Left + centerWidth,
							y,
							info.Right,
							info.Top));
				}
			}

			y += info.Top;
			if (centerHeight > 0)
			{
				if (info.Left > 0)
				{
					_centerLeft = new TextureRegion(texture,
						new Rectangle(bounds.X,
							y,
							info.Left,
							centerHeight));
				}

				if (centerWidth > 0)
				{
					_center = new TextureRegion(texture,
						new Rectangle(bounds.X + info.Left,
							y,
							centerWidth,
							centerHeight));
				}

				if (info.Right > 0)
				{
					_centerRight = new TextureRegion(texture,
						new Rectangle(bounds.X + info.Left + centerWidth,
							y,
							info.Right,
							centerHeight));
				}
			}

			y += centerHeight;
			if (info.Bottom > 0)
			{
				if (info.Left > 0)
				{
					_bottomLeft = new TextureRegion(texture,
						new Rectangle(bounds.X,
							y,
							info.Left,
							info.Bottom));
				}

				if (centerWidth > 0)
				{
					_bottomCenter = new TextureRegion(texture,
						new Rectangle(bounds.X + info.Left,
							y,
							centerWidth,
							info.Bottom));
				}

				if (info.Right > 0)
				{
					_bottomRight = new TextureRegion(texture,
						new Rectangle(bounds.X + info.Left + centerWidth,
							y,
							info.Right,
							info.Bottom));
				}
			}
		}

		public override void Draw(SpriteBatch batch, Rectangle dest, Color color)
		{
			var y = dest.Y;

			var centerWidth = dest.Width - _info.Left - _info.Right;
			var centerHeight = dest.Height - _info.Top - _info.Bottom;

			if (_topLeft != null)
			{
				_topLeft.Draw(batch,
					new Rectangle(dest.X,
						y,
						_info.Left,
						_info.Top),
					color);
			}

			if (_topCenter != null)
			{
				_topCenter.Draw(batch,
					new Rectangle(dest.X + _info.Left,
						y,
						centerWidth,
						_info.Top),
					color);
			}

			if (_topRight != null)
			{
				_topRight.Draw(batch,
					new Rectangle(dest.X + Info.Left + centerWidth,
						y,
						_info.Right,
						_info.Top),
					color);
			}

			y += _info.Top;
			if (_centerLeft != null)
			{
				_centerLeft.Draw(batch,
					new Rectangle(dest.X,
						y,
						_info.Left,
						centerHeight),
					color);
			}

			if (_center != null)
			{
				_center.Draw(batch,
					new Rectangle(dest.X + _info.Left,
						y,
						centerWidth,
						centerHeight),
					color);
			}

			if (_centerRight != null)
			{
				_centerRight.Draw(batch,
					new Rectangle(dest.X + Info.Left + centerWidth,
						y,
						_info.Right,
						centerHeight),
					color);
			}

			y += centerHeight;
			if (_bottomLeft != null)
			{
				_bottomLeft.Draw(batch,
					new Rectangle(dest.X,
						y,
						_info.Left,
						_info.Bottom),
					color);
			}

			if (_bottomCenter != null)
			{
				_bottomCenter.Draw(batch,
					new Rectangle(dest.X + _info.Left,
						y,
						centerWidth,
						_info.Bottom),
					color);
			}

			if (_bottomRight != null)
			{
				_bottomRight.Draw(batch,
					new Rectangle(dest.X + Info.Left + centerWidth,
						y,
						_info.Right,
						_info.Bottom),
					color);
			}
		}
	}
}