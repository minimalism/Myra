using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Myra.Graphics2D.Text {
    public interface ITextChunk {
        void Draw(SpriteBatch batch, Point pos, Color color, float opacity = 1);
        Point Size { get; }
        Color? Color { get; }
        int LineIndex { get; set; }
        int ChunkIndex { get; set; }
        int Top { get; set; }
        int Count { get; }
    }
}
