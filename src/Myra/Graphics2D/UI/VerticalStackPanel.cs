using Myra.Graphics2D.UI.Styles;

namespace Myra.Graphics2D.UI
{
	public class VerticalStackPanel : StackPanel
	{
		public VerticalStackPanel(string styleName = Stylesheet.DefaultStyleName) : base(styleName) {} 
		
		public override Orientation Orientation => Orientation.Vertical;
	}
}
