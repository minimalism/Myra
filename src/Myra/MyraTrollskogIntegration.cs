using Microsoft.Xna.Framework;
using System;

namespace Myra
{
	
	/// <summary>
	/// Trying to keep the various hacks coordinated in one place...
	/// </summary>
	public static class TrollskogIntegration
	{
		public static bool ShowGridLines { get; set; }
		
		public static bool ForceCalculateGlyphs { get; set; }
	}
	
	public interface IButtonWidget {
		Rectangle Bounds { get; }
		bool IsPressed { get; set; }
		event EventHandler LayoutUpdated;
		event EventHandler PressedChanged;
	}
}