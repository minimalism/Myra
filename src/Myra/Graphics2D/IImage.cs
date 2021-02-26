
using Microsoft.Xna.Framework.Graphics;
#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace Myra.Graphics2D
{
	public interface IImage: IBrush
	{
		Point Size { get; }
		
		// For TS compatibility...
		void Draw(SpriteBatch batch, Rectangle dest, Color color);
	}
}
