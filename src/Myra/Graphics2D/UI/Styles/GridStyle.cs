namespace Myra.Graphics2D.UI.Styles
{
	public class GridStyle : WidgetStyle
	{
		public int RowSpacing { get; set; }
		public int ColumnSpacing { get; set; }
		public Proportion DefaultColumnProportion { get; set; }
		public Proportion DefaultRowProportion { get; set; }

		public GridStyle() { }

		public GridStyle(GridStyle gs) : base(gs)
		{
			this.RowSpacing = gs.RowSpacing;
			this.ColumnSpacing = gs.ColumnSpacing;
			this.DefaultColumnProportion = gs.DefaultColumnProportion;
			this.DefaultRowProportion = gs.DefaultRowProportion;
		}
	}
}