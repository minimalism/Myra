using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Myra.Graphics2D.Text
{
	public class SpriteChunk : ITextChunk
	{
		private readonly IImage _image;
		public Point Size { get; }

		public Color? Color { get; set; }

		public int LineIndex { get; set; }
		public int ChunkIndex { get; set; }
		public int Top { get; set; }

		/// <summary>
		/// Sprites are considered non-characters for now.
		/// </summary>
		public int Count => 0;

		public static string GetSpriteId(string text, int startIndex, int endIndex)
		{
			return text.Substring(startIndex, endIndex - startIndex);
		}
		
		public static SpriteChunk GetSpriteChunk(in ChunkInfo chunkInfo)
		{
			IImage sprite = TrollskogIntegration.GetSprite(chunkInfo.SpriteId);
			return new SpriteChunk(sprite, new Point(chunkInfo.X, chunkInfo.Y)) {Color = chunkInfo.Color};
		}
		
		public SpriteChunk(IImage image, Point size)
		{
			_image = image;
			Size = size;
		}


		public void Draw(RenderContext batch, Vector2 pos, Color color)
		{
			_image.Draw(batch, new Rectangle((int)pos.X, (int)pos.Y, Size.X, Size.Y), color);
		}
	}
}