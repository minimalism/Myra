using Myra.Graphics2D.UI.Styles;

namespace Myra.Graphics2D.UI
{
	public class HorizontalStackPanel : StackPanel
	{
		public HorizontalStackPanel(string styleName = Stylesheet.DefaultStyleName) : base(styleName) {}

		public override Orientation Orientation => Orientation.Horizontal;
	}
}
