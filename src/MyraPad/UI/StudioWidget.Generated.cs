/* Generated by MyraPad at 31.07.2019 16:19:20 */
using Myra.Graphics2D.UI;

#if !XENKO
using Microsoft.Xna.Framework;
#else
using Xenko.Core.Mathematics;
#endif

namespace MyraPad.UI
{
	partial class StudioWidget
	{
		private void BuildUI()
		{
			_menuFileNew = new MenuItem();
			_menuFileNew.Id = "_menuFileNew";
			_menuFileNew.Text = "&New";

			_menuFileOpen = new MenuItem();
			_menuFileOpen.Id = "_menuFileOpen";
			_menuFileOpen.Text = "&Open";

			_menuFileReload = new MenuItem();
			_menuFileReload.Id = "_menuFileReload";
			_menuFileReload.Text = "&Reload";

			_menuFileSave = new MenuItem();
			_menuFileSave.Id = "_menuFileSave";
			_menuFileSave.Text = "&Save";

			_menuFileSaveAs = new MenuItem();
			_menuFileSaveAs.Id = "_menuFileSaveAs";
			_menuFileSaveAs.Text = "Save &As...";

			_menuFileExportToCS = new MenuItem();
			_menuFileExportToCS.Id = "_menuFileExportToCS";
			_menuFileExportToCS.Text = "&Export To C#...";

			var menuSeparator1 = new MenuSeparator();

			_menuFileLoadStylesheet = new MenuItem();
			_menuFileLoadStylesheet.Id = "_menuFileLoadStylesheet";
			_menuFileLoadStylesheet.Text = "&Load Stylesheet";

			_menuFileReloadStylesheet = new MenuItem();
			_menuFileReloadStylesheet.Id = "_menuFileReloadStylesheet";
			_menuFileReloadStylesheet.Text = "Rel&oad Stylesheet";

			_menuFileResetStylesheet = new MenuItem();
			_menuFileResetStylesheet.Id = "_menuFileResetStylesheet";
			_menuFileResetStylesheet.Text = "Rese&t Stylesheet";

			var menuSeparator2 = new MenuSeparator();

			_menuFileDebugOptions = new MenuItem();
			_menuFileDebugOptions.Id = "_menuFileDebugOptions";
			_menuFileDebugOptions.Text = "&UI Debug Options";

			var menuSeparator3 = new MenuSeparator();

			_menuFileQuit = new MenuItem();
			_menuFileQuit.Id = "_menuFileQuit";
			_menuFileQuit.Text = "&Quit";

			var menuItem1 = new MenuItem();
			menuItem1.Id = "";
			menuItem1.Text = "&File";
			menuItem1.Items.Add(_menuFileNew);
			menuItem1.Items.Add(_menuFileOpen);
			menuItem1.Items.Add(_menuFileReload);
			menuItem1.Items.Add(_menuFileSave);
			menuItem1.Items.Add(_menuFileSaveAs);
			menuItem1.Items.Add(_menuFileExportToCS);
			menuItem1.Items.Add(menuSeparator1);
			menuItem1.Items.Add(_menuFileLoadStylesheet);
			menuItem1.Items.Add(_menuFileReloadStylesheet);
			menuItem1.Items.Add(_menuFileResetStylesheet);
			menuItem1.Items.Add(menuSeparator2);
			menuItem1.Items.Add(_menuFileDebugOptions);
			menuItem1.Items.Add(menuSeparator3);
			menuItem1.Items.Add(_menuFileQuit);

			_menuEditFormatSource = new MenuItem();
			_menuEditFormatSource.Id = "_menuEditFormatSource";
			_menuEditFormatSource.Text = "&Format Source";

			var menuItem2 = new MenuItem();
			menuItem2.Id = "";
			menuItem2.Text = "&Edit";
			menuItem2.Items.Add(_menuEditFormatSource);

			_menuHelpAbout = new MenuItem();
			_menuHelpAbout.Id = "_menuHelpAbout";
			_menuHelpAbout.Text = "&About";

			var menuItem3 = new MenuItem();
			menuItem3.Id = "";
			menuItem3.Text = "&Help";
			menuItem3.Items.Add(_menuHelpAbout);

			var horizontalMenu1 = new HorizontalMenu();
			horizontalMenu1.Id = "";
			horizontalMenu1.Items.Add(menuItem1);
			horizontalMenu1.Items.Add(menuItem2);
			horizontalMenu1.Items.Add(menuItem3);

			_projectHolder = new Panel();
			_projectHolder.Id = "_projectHolder";

			_textSource = new TextField();
			_textSource.Text = "";
			_textSource.Multiline = true;
			_textSource.Wrap = true;
			_textSource.Id = "_textSource";
			_textSource.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Stretch;
			_textSource.GridRow = 2;

			var scrollPane1 = new ScrollPane();
			scrollPane1.GridRow = 2;
			scrollPane1.Content = _textSource;

			_leftSplitPane = new VerticalSplitPane();
			_leftSplitPane.Id = "_leftSplitPane";
			_leftSplitPane.Widgets.Add(_projectHolder);
			_leftSplitPane.Widgets.Add(scrollPane1);

			var horizontalSeparator1 = new HorizontalSeparator();
			horizontalSeparator1.GridRow = 1;

			_textStatus = new TextBlock();
			_textStatus.Text = "Reloading...";
			_textStatus.Id = "_textStatus";
			_textStatus.GridRow = 2;

			var grid1 = new Grid();
			grid1.RowsProportions.Add(new Grid.Proportion
			{
				Type = Myra.Graphics2D.UI.Grid.ProportionType.Fill,
			});
			grid1.RowsProportions.Add(new Grid.Proportion());
			grid1.RowsProportions.Add(new Grid.Proportion());
			grid1.Widgets.Add(_leftSplitPane);
			grid1.Widgets.Add(horizontalSeparator1);
			grid1.Widgets.Add(_textStatus);

			_propertyGridPane = new ScrollPane();
			_propertyGridPane.Content = null;
			_propertyGridPane.Id = "_propertyGridPane";

			var horizontalSeparator2 = new HorizontalSeparator();
			horizontalSeparator2.GridRow = 1;

			_textLocation = new TextBlock();
			_textLocation.Text = "Line: 1, Column: 2, Indent: 3";
			_textLocation.Id = "_textLocation";
			_textLocation.GridRow = 2;

			var grid2 = new Grid();
			grid2.RowsProportions.Add(new Grid.Proportion
			{
				Type = Myra.Graphics2D.UI.Grid.ProportionType.Fill,
			});
			grid2.RowsProportions.Add(new Grid.Proportion());
			grid2.RowsProportions.Add(new Grid.Proportion());
			grid2.GridColumn = 2;
			grid2.Widgets.Add(_propertyGridPane);
			grid2.Widgets.Add(horizontalSeparator2);
			grid2.Widgets.Add(_textLocation);

			_topSplitPane = new HorizontalSplitPane();
			_topSplitPane.Id = "_topSplitPane";
			_topSplitPane.GridRow = 1;
			_topSplitPane.Widgets.Add(grid1);
			_topSplitPane.Widgets.Add(grid2);

			
			RowsProportions.Add(new Grid.Proportion());
			RowsProportions.Add(new Grid.Proportion
			{
				Type = Myra.Graphics2D.UI.Grid.ProportionType.Fill,
			});
			Id = "Root";
			Widgets.Add(horizontalMenu1);
			Widgets.Add(_topSplitPane);
		}

		
		public MenuItem _menuFileNew;
		public MenuItem _menuFileOpen;
		public MenuItem _menuFileReload;
		public MenuItem _menuFileSave;
		public MenuItem _menuFileSaveAs;
		public MenuItem _menuFileExportToCS;
		public MenuItem _menuFileLoadStylesheet;
		public MenuItem _menuFileReloadStylesheet;
		public MenuItem _menuFileResetStylesheet;
		public MenuItem _menuFileDebugOptions;
		public MenuItem _menuFileQuit;
		public MenuItem _menuEditFormatSource;
		public MenuItem _menuHelpAbout;
		public Panel _projectHolder;
		public TextField _textSource;
		public VerticalSplitPane _leftSplitPane;
		public TextBlock _textStatus;
		public ScrollPane _propertyGridPane;
		public TextBlock _textLocation;
		public HorizontalSplitPane _topSplitPane;
	}
}