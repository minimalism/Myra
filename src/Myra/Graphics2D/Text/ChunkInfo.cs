#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace Myra.Graphics2D.Text
{
	public struct ChunkInfo
	{
		public int X;
		public int Y;
		public int StartIndex;
		public int CharsCount;
		public int SkipChars;
		public Color? Color;
		public bool LineEnd;
		public string? SpriteId;
	}
}