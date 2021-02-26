using System;
using System.Collections.Generic;
using System.Text;
using Myra.Graphics2D.UI;
using FontStashSharp;
using Myra.Utility;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace Myra.Graphics2D.Text
{
	public class FormattedText
	{
		public const int NewLineWidth = 0;

		private SpriteFontBase _font;
		private string _text = string.Empty;
		private int _verticalSpacing;
		private VerticalAlignment _lineAlignment = VerticalAlignment.Center;
		private int? _width;
		private readonly List<TextLine> _lines = new List<TextLine>();
		private bool _calculateGlyphs, _supportsCommands;
		private Point _size;
		private bool _dirty = true;
		private StringBuilder _stringBuilder = new StringBuilder();
		private readonly Dictionary<int, Point> _measures = new Dictionary<int, Point>();

		public SpriteFontBase Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == _font)
				{
					return;
				}

				_font = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == _text)
				{
					return;
				}

				_text = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public int VerticalSpacing
		{
			get
			{
				return _verticalSpacing;
			}

			set
			{
				if (value == _verticalSpacing)
				{
					return;
				}

				_verticalSpacing = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public VerticalAlignment LineVerticalAlignment
		{
			get
			{
				return _lineAlignment;
			}
			set
			{
				_lineAlignment = value;
				InvalidateLayout();
			}
		}

		public int? Width
		{
			get
			{
				return _width;
			}
			set
			{
				if (value == _width)
				{
					return;
				}

				_width = value;
				InvalidateLayout();
			}
		}

		public List<TextLine> Lines
		{
			get
			{
				Update();
				return _lines;
			}
		}

		public Point Size
		{
			get
			{
				Update();
				return _size;
			}
		}

		public bool CalculateGlyphs
		{
			get
			{
				return _calculateGlyphs;
			}

			set
			{
				if (value == _calculateGlyphs)
				{
					return;
				}

				_calculateGlyphs = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public bool SupportsCommands
		{
			get
			{
				return _supportsCommands;
			}

			set
			{
				if (value == _supportsCommands)
				{
					return;
				}

				_supportsCommands = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		private const char ColorCommandChar = 'c';
		private const char SpriteCommandChar = 'i';

		internal ChunkInfo LayoutRow(int startIndex, int? width, bool parseCommands)
		{
			var r = new ChunkInfo
			{
				StartIndex = startIndex,
				LineEnd = true
			};

			if (string.IsNullOrEmpty(_text))
			{
				return r;
			}

			_stringBuilder.Clear();
			int? lastBreakPosition = null;
			Point? lastBreakMeasure = null;

			for (var i = r.StartIndex; i < _text.Length; ++i)
			{
				var c = _text[i];

				if (char.IsHighSurrogate(c))
				{
					_stringBuilder.Append(c);
					++r.CharsCount;
					continue;
				}

				if (SupportsCommands && c == '\\')
				{
					if (i < _text.Length - 2 && _text[i + 1] is char commandChar && _text[i + 2] == '[')
					{
						// Find end
						var startPos = i + 3;
						var j = _text.IndexOf(']', startPos);

						if (j != -1)
						{
							// Found
							if (i > r.StartIndex)
							{
								// Break right here, as next chunk is a command block
								if (commandChar == SpriteCommandChar)
								{
									// sprites have width, will this one fit on our line?
									r.LineEnd = (width != null && (r.X + UI.Desktop.InlineSpriteSize.X) > width.Value);
								}
								else
								{
									r.LineEnd = false;
								}
								return r;
							}

							if (parseCommands && commandChar == ColorCommandChar)
							{
								r.Color = ColorStorage.FromName(_text.Substring(startPos, j - startPos));
							}
							else if (commandChar == SpriteCommandChar)
							{
								// Break because this is a sprite chunk
								r.LineEnd = j == _text.Length - 1;
								r.X = UI.Desktop.InlineSpriteSize.X;
								r.Y = UI.Desktop.InlineSpriteSize.Y;
								r.IsSprite = true;
								r.CharsCount = j - r.StartIndex + 1;
								lastBreakPosition = j;
								lastBreakMeasure = UI.Desktop.InlineSpriteSize;
								return r;
							}

							r.StartIndex = j + 1;
							i = j;
							continue;
						}
					}
				}

				_stringBuilder.Append(c);

				Point sz;
				if (c != '\n')
				{
					var v = Font.MeasureString(_stringBuilder);
					sz = new Point((int)v.X, _font.FontSize);
				}
				else
				{
					sz = new Point(r.X + NewLineWidth, Math.Max(r.Y, _font.FontSize));

					// Break right here
					++r.CharsCount;
					r.X = sz.X;
					r.Y = sz.Y;
					break;
				}

				if (width != null && sz.X > width.Value)
				{
					if (lastBreakPosition != null)
					{
						r.CharsCount = lastBreakPosition.Value - r.StartIndex;
					}

					if (lastBreakMeasure != null)
					{
						r.X = lastBreakMeasure.Value.X;
						r.Y = lastBreakMeasure.Value.Y;
					}

					break;
				}

				if (char.IsWhiteSpace(c))
				{
					lastBreakPosition = i + 1;
					lastBreakMeasure = sz;
				}

				++r.CharsCount;
				r.X = sz.X;
				r.Y = sz.Y;
			}

			return r;
		}

		private static int GetMeasureKey(int? width)
		{
			return width != null ? width.Value : -1;
		}

		public Point Measure(int? width)
		{
			var key = GetMeasureKey(width);
			if (_measures.TryGetValue(key, out Point result))
			{
				return result;
			}
			
			result = Mathematics.PointZero;
			if (!string.IsNullOrEmpty(_text))
			{
				var i = 0;
				var y = 0;

				var remainingWidth = width;
				var lineWidth = 0;
				var lineHeight = 0;
				while (i < _text.Length)
				{
					var chunkInfo = LayoutRow(i, remainingWidth, false);
					if (lineHeight < chunkInfo.Y)
					{
						lineHeight = chunkInfo.Y;
					}
					if (i == chunkInfo.StartIndex && chunkInfo.CharsCount == 0 && remainingWidth == width)
					{
						break;
					}
					lineWidth += chunkInfo.X;
					i = chunkInfo.StartIndex + chunkInfo.CharsCount;

					if (remainingWidth.HasValue)
					{
						remainingWidth = remainingWidth.Value - chunkInfo.X;
					}

					if (chunkInfo.LineEnd)
					{
						if (lineWidth > result.X)
						{
							result.X = lineWidth;
						}

						lineWidth = 0;
						remainingWidth = width;

						y += lineHeight;
						y += _verticalSpacing;
						lineHeight = 0;
					}
				}

				result.Y = y;
			}

			if (result.Y == 0)
			{
				result.Y = _font.FontSize;
			}

			_measures[key] = result;

			return result;
		}

		private void Update()
		{
			if (!_dirty)
			{
				return;
			}

			_lines.Clear();

			if (string.IsNullOrEmpty(_text))
			{
				_dirty = false;
				return;
			}

			var i = 0;
			var line = new TextLine
			{
				TextStartIndex = i
			};

			var width = Width;
			while (i < _text.Length)
			{
				var c = LayoutRow(i, width, true);
				if (i == c.StartIndex && c.CharsCount == 0 && c.LineEnd && line.TextStartIndex == c.StartIndex)
				{
					break;
				}

				ITextChunk chunk;
				if (c.IsSprite)
				{
					// trim off the \\i{} tag and pass in only the sprite id
					chunk = new SpriteChunk(_text.Substring(c.StartIndex + 3, c.CharsCount - 4), new Point(c.X, c.Y))
					{
						Color = c.Color
					};
				}
				else
				{
					chunk = new TextChunk(_font, _text.Substring(c.StartIndex, c.CharsCount), new Point(c.X, c.Y), CalculateGlyphs)
					{
						TextStartIndex = i,
						Color = c.Color
					};
				}

				width -= chunk.Size.X;

				i = c.StartIndex + c.CharsCount;

				line.Chunks.Add(chunk);
				line.Count += chunk.Count;

				line.Size.X += chunk.Size.X;
				if (chunk.Size.Y > line.Size.Y)
				{
					line.Size.Y = chunk.Size.Y;
				}

				if (c.LineEnd)
				{
					// New line
					_lines.Add(line);

					line = new TextLine
					{
						TextStartIndex = i
					};

					width = Width;
				}
			}

			// Calculate size
			_size = Mathematics.PointZero;
			for (i = 0; i < _lines.Count; ++i)
			{
				line = _lines[i];

				line.LineIndex = i;
				line.Top = _size.Y;

				for (var j = 0; j < line.Chunks.Count; ++j)
				{
					var chunk = line.Chunks[j];
					chunk.LineIndex = line.LineIndex;
					chunk.ChunkIndex = j;
					if (LineVerticalAlignment == VerticalAlignment.Bottom)
					{
						chunk.Top = line.Top + line.Size.Y - chunk.Size.Y;
					}
					else if (LineVerticalAlignment == VerticalAlignment.Top)
					{
						chunk.Top = line.Top;
					}
					else
					{
						chunk.Top = line.Top + (line.Size.Y - chunk.Size.Y) / 2;
					}
				}

				if (line.Size.X > _size.X)
				{
					_size.X = line.Size.X;
				}

				_size.Y += line.Size.Y;

				if (i < _lines.Count - 1)
				{
					_size.Y += _verticalSpacing;
				}
			}

			var key = GetMeasureKey(Width);
			_measures[key] = _size;

			_dirty = false;
		}

		public TextLine GetLineByCursorPosition(int cursorPosition)
		{
			Update();

			if (_lines.Count == 0)
			{
				return null;
			}

			if (cursorPosition < 0)
			{
				return _lines[0];
			}

			for (var i = 0; i < _lines.Count; ++i)
			{
				var s = _lines[i];
				if (s.TextStartIndex <= cursorPosition && cursorPosition < s.TextStartIndex + s.Count)
				{
					return s;
				}
			}

			return _lines[_lines.Count - 1];
		}

		public TextLine GetLineByY(int y)
		{
			if (string.IsNullOrEmpty(_text) || y < 0)
			{
				return null;
			}

			Update();

			for (var i = 0; i < _lines.Count; ++i)
			{
				var s = _lines[i];

				if (s.Top <= y && y < s.Top + s.Size.Y)
				{
					return s;
				}
			}

			return null;
		}

		public GlyphInfo GetGlyphInfoByIndex(int charIndex)
		{
			var strings = Lines;

			foreach (var si in strings)
			{
				if (charIndex >= si.Count)
				{
					charIndex -= si.Count;
				}
				else
				{
					return si.GetGlyphInfoByIndex(charIndex);
				}
			}

			return null;
		}

		public void Draw(RenderContext context, TextAlign align, Rectangle bounds, Rectangle clip, Color textColor, bool useChunkColor)
		{
			var y = bounds.Y;
			foreach (var line in Lines)
			{
				if (y + line.Size.Y >= clip.Top && y <= clip.Bottom)
				{
					int x = bounds.X;

					switch (align)
					{
						case TextAlign.Center:
							x = bounds.X + (bounds.Width / 2) - (line.Size.X / 2);
							break;
						case TextAlign.Right:
							x = bounds.X + bounds.Width - line.Size.X;
							break;
					}

					textColor = line.Draw(context, new Vector2(x, y), textColor, useChunkColor);
				}
				else
				{
					foreach (var chunk in line.Chunks)
					{
						if (useChunkColor && chunk.Color != null)
						{
							textColor = chunk.Color.Value;
						}
					}
				}

				y += line.Size.Y;
				y += _verticalSpacing;
			}
		}

		private void InvalidateLayout()
		{
			_dirty = true;
		}

		private void InvalidateMeasures()
		{
			_measures.Clear();
		}
	}
}