using Myra.Utility;
using System;
using System.Collections.Generic;

#if !STRIDE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#else
using Stride.Core.Mathematics;
using Stride.Graphics;
#endif

namespace Myra.Graphics2D.Text
{
	public class TextLine
	{
		public int Count { get; internal set; }

		public Point Size;

		public int LineIndex
		{
			get; internal set;
		}

		public int Top
		{
			get; internal set;
		}

		public int TextStartIndex
		{
			get; internal set;
		}

		public List<ITextChunk> Chunks { get; } = new List<ITextChunk>();

		public GlyphInfo GetGlyphInfoByIndex(int index)
		{
			foreach (var c in Chunks)
			{
				if (c is TextChunk si) 
				{
					if (index >= si.Count) 
					{
						index -= si.Count;
					}
					else 
					{
						return si.GetGlyphInfoByIndex(index);
					}
				}
			}

			return null;
		}

		public int? GetGlyphIndexByX(int startX)
		{
			if (Chunks.Count == 0)
			{
				return null;
			}

			var x = startX;
			for(var i = 0; i < Chunks.Count; ++i)
			{
				var chunk = Chunks[i];

				if (x >= chunk.Size.X)
				{
					x -= chunk.Size.X;
				}
				else if (chunk is TextChunk textChunk)
				{
					if (textChunk.Glyphs.Count > 0 && x < textChunk.Glyphs[0].Bounds.X)
					{
						// Before first glyph
						return 0;
					}

					return textChunk.GetGlyphIndexByX(x);
				}
			}

			// Use last chunk
			x = startX;
			if (Chunks[Chunks.Count - 1] is TextChunk tc) 
			{
				return tc.GetGlyphIndexByX(startX);
			}
			else return null;
		}

		public Color Draw(SpriteBatch batch, Point pos, Color color, bool useChunkColor, float opacity = 1.0f)
		{
			foreach (var chunk in Chunks)
			{
				if (useChunkColor && chunk.Color != null)
				{
					color = chunk.Color.Value;
				}

				chunk.Draw(batch, pos, color, opacity);
				pos.X += chunk.Size.X;
			}

			return color;
		}
	}
}
