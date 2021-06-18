using FontStashSharp;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using System;

namespace Myra
{
	
	/// <summary>
	/// Trying to keep the various hacks coordinated in one place...
	/// </summary>
	public static class TrollskogIntegration
	{
		public delegate IImage SpriteFunc(string spriteId);
		public delegate Point MeasureSpriteFunc(string spriteId, SpriteFontBase font);
		public static SpriteFunc GetSprite { get; set; }
		public static MeasureSpriteFunc MeasureSprite { get; set; }
		public static bool ShowGridLines { get; set; }
		public static bool ForceCalculateGlyphs { get; set; }
	}
	
	public interface IButtonWidget {
		Rectangle Bounds { get; }
		bool IsPressed { get; set; }
		event EventHandler LayoutUpdated;
		event EventHandler PressedChanged;
		event EventHandler<bool> PlacedChanged;
	}
}