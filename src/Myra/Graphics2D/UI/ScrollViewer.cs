﻿using System;
using System.ComponentModel;
using Myra.Graphics2D.UI.Styles;
using Myra.Utility;
using System.Xml.Serialization;
using Myra.Attributes;

#if !XENKO
using Microsoft.Xna.Framework;
#else
using Xenko.Core.Mathematics;
#endif

namespace Myra.Graphics2D.UI
{
	public class ScrollViewer : SingleItemContainer<Widget>, IContent
	{
		private Orientation _scrollbarOrientation;
		internal bool _horizontalScrollingOn, _verticalScrollingOn;
		private bool _showHorizontalScrollBar, _showVerticalScrollBar;
		internal Rectangle _horizontalScrollbarFrame, _horizontalScrollbarThumb;
		internal Rectangle _verticalScrollbarFrame, _verticalScrollbarThumb;
		private int? _startBoundsPos;
		private int _thumbMaximumX, _thumbMaximumY;

		[Browsable(false)]
		[XmlIgnore]
		internal Point ScrollMaximum
		{
			get
			{
				if (InternalChild == null)
				{
					return Point.Zero;
				}

				var bounds = ActualBounds;

				return new Point(InternalChild.Bounds.Width - bounds.Width + ((_verticalScrollingOn && ShowVerticalScrollBar) ? _verticalScrollbarThumb.Width : 0),
								 InternalChild.Bounds.Height - bounds.Height + ((_horizontalScrollingOn && ShowHorizontalScrollBar) ? _horizontalScrollbarThumb.Height : 0));
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public Point ScrollPosition
		{
			get
			{
				if (InternalChild == null)
				{
					return Point.Zero;
				}

				return new Point(-InternalChild.Left, -InternalChild.Top);
			}
			set
			{
				if (InternalChild == null)
				{
					return;
				}

				var maximum = ScrollMaximum;
				if (value.X < 0)
				{
					value.X = 0;
				}

				if (maximum.X >= 0 && value.X > maximum.X)
				{
					value.X = maximum.X;
				}

				if (value.Y < 0)
				{
					value.Y = 0;
				}

				if (maximum.Y >= 0 && value.Y > maximum.Y)
				{
					value.Y = maximum.Y;
				}

				InternalChild.Left = -value.X;
				InternalChild.Top = -value.Y;
			}
		}

		internal Point ThumbPosition
		{
			get
			{
				var sp = ScrollPosition;
				var m = ScrollMaximum;

				var result = Point.Zero;
				if (m.X > 0)
				{
					result.X = sp.X * _thumbMaximumX / m.X;
				}

				if (m.Y > 0)
				{
					result.Y = sp.Y * _thumbMaximumY / m.Y;
				}

				return result;
			}
		}

		[Category("Appearance")]
		public IImage HorizontalScrollBackground
		{
			get; set;
		}

		[Category("Appearance")]
		public IImage HorizontalScrollKnob
		{
			get; set;
		}

		[Category("Appearance")]
		public IImage VerticalScrollBackground
		{
			get; set;
		}

		[Category("Appearance")]
		public IImage VerticalScrollKnob
		{
			get; set;
		}

		[Browsable(false)]
		[Content]
		public Widget Content
		{
			get
			{
				return base.InternalChild;
			}
			set
			{
				base.InternalChild = value;
				ResetScroll();
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		[Obsolete("Use Content instead")]
		public Widget Child
		{
			get
			{
				return Content;
			}

			set
			{
				Content = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		public bool ShowHorizontalScrollBar
		{
			get
			{
				return _showHorizontalScrollBar;
			}

			set
			{
				if (value == _showHorizontalScrollBar)
				{
					return;
				}

				_showHorizontalScrollBar = value;
				InvalidateMeasure();
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		public bool ShowVerticalScrollBar
		{
			get
			{
				return _showVerticalScrollBar;
			}

			set
			{
				if (value == _showVerticalScrollBar)
				{
					return;
				}

				_showVerticalScrollBar = value;
				InvalidateMeasure();
			}
		}

		[DefaultValue(HorizontalAlignment.Stretch)]
		public override HorizontalAlignment HorizontalAlignment
		{
			get
			{
				return base.HorizontalAlignment;
			}
			set
			{
				base.HorizontalAlignment = value;
			}
		}

		[DefaultValue(VerticalAlignment.Stretch)]
		public override VerticalAlignment VerticalAlignment
		{
			get
			{
				return base.VerticalAlignment;
			}
			set
			{
				base.VerticalAlignment = value;
			}
		}

		[DefaultValue(true)]
		public override bool ClipToBounds
		{
			get
			{
				return base.ClipToBounds;
			}
			set
			{
				base.ClipToBounds = value;
			}
		}

		internal protected override bool AcceptsMouseWheelFocus
		{
			get { return _verticalScrollingOn; }
		}

		protected internal override bool MouseWheelFocusCanBeNull => false;

		public override bool IsPlaced
		{
			get
			{
				return base.IsPlaced;
			}

			set
			{
				if (IsPlaced)
				{
					Desktop.TouchMoved -= DesktopTouchMoved;
					Desktop.TouchUp -= DesktopTouchUp;
				}

				base.IsPlaced = value;

				if (IsPlaced)
				{
					Desktop.TouchMoved += DesktopTouchMoved;
					Desktop.TouchUp += DesktopTouchUp;
				}
			}
		}

		private int HorizontalScrollbarHeight
		{
			get
			{
				var result = 0;
				if (HorizontalScrollBackground != null)
				{
					result = HorizontalScrollBackground.Size.Y;
				}

				if (HorizontalScrollKnob != null && HorizontalScrollKnob.Size.Y > result)
				{
					result = HorizontalScrollKnob.Size.Y;
				}

				return result;
			}
		}

		private int VerticalScrollbarWidth
		{
			get
			{
				var result = 0;
				if (VerticalScrollBackground != null)
				{
					result = VerticalScrollBackground.Size.X;
				}

				if (VerticalScrollKnob != null && VerticalScrollKnob.Size.X > result)
				{
					result = VerticalScrollKnob.Size.X;
				}

				return result;
			}
		}

		public ScrollViewer(string styleName = Stylesheet.DefaultStyleName)
		{
			ClipToBounds = true;
			_horizontalScrollingOn = _verticalScrollingOn = false;

			ShowVerticalScrollBar = ShowHorizontalScrollBar = true;

			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			SetStyle(styleName);
		}

		private void MoveThumb(int delta)
		{
			var scrollPosition = ScrollPosition;

			var maximum = ScrollMaximum;
			if (_scrollbarOrientation == Orientation.Horizontal)
			{
				var newPos = delta + scrollPosition.X;
				if (newPos < 0)
				{
					newPos = 0;
				}

				if (newPos > maximum.X)
				{
					newPos = maximum.X;
				}

				scrollPosition.X = newPos;
			}
			else
			{
				var newPos = delta + scrollPosition.Y;

				if (newPos < 0)
				{
					newPos = 0;
				}

				if (newPos > maximum.Y)
				{
					newPos = maximum.Y;
				}

				scrollPosition.Y = newPos;
			}

			ScrollPosition = scrollPosition;
		}

		internal override void MoveChildren(Point delta)
		{
			base.MoveChildren(delta);

			if (_horizontalScrollingOn)
			{
				_horizontalScrollbarFrame.Offset(delta.X, delta.Y);
				_horizontalScrollbarThumb.Offset(delta.X, delta.Y);
			}

			if (_verticalScrollingOn)
			{
				_verticalScrollbarFrame.Offset(delta.X, delta.Y);
				_verticalScrollbarThumb.Offset(delta.X, delta.Y);
			}
		}

		public override void OnTouchUp()
		{
			base.OnTouchUp();

			_startBoundsPos = null;
		}

		public override void OnTouchDown()
		{
			base.OnTouchDown();

			var touchPosition = Desktop.TouchPosition;

			var r = _verticalScrollbarThumb;
			var thumbPosition = ThumbPosition;
			r.Y += thumbPosition.Y;
			if (ShowVerticalScrollBar && _verticalScrollingOn && r.Contains(touchPosition))
			{
				_startBoundsPos = touchPosition.Y;
				_scrollbarOrientation = Orientation.Vertical;
			}

			r = _horizontalScrollbarThumb;
			r.X += thumbPosition.X;
			if (ShowHorizontalScrollBar && _horizontalScrollingOn && r.Contains(touchPosition))
			{
				_startBoundsPos = touchPosition.X;
				_scrollbarOrientation = Orientation.Horizontal;
			}
		}

		public override void OnMouseWheel(float delta)
		{
			base.OnMouseWheel(delta);

			if (!_verticalScrollingOn)
			{
				return;
			}

			var step = 10 * ScrollMaximum.Y / _thumbMaximumY;
			if (delta < 0)
			{
				_scrollbarOrientation = Orientation.Vertical;
				MoveThumb(step);
			}
			else if (delta > 0)
			{
				_scrollbarOrientation = Orientation.Vertical;
				MoveThumb(-step);
			}
		}

		public override void InternalRender(RenderContext context)
		{
			if (InternalChild == null || !InternalChild.Visible)
			{
				return;
			}

			// Render child
			base.InternalRender(context);

			var thumbPosition = ThumbPosition;
			if (_horizontalScrollingOn && ShowHorizontalScrollBar)
			{
				if (HorizontalScrollBackground != null)
				{
					context.Draw(HorizontalScrollBackground, _horizontalScrollbarFrame);
				}

				var r = _horizontalScrollbarThumb;
				r.X += thumbPosition.X;
				context.Draw(HorizontalScrollKnob, r);
			}

			if (_verticalScrollingOn && ShowVerticalScrollBar)
			{
				if (VerticalScrollBackground != null)
				{
					context.Draw(VerticalScrollBackground, _verticalScrollbarFrame);
				}

				var r = _verticalScrollbarThumb;
				r.Y += thumbPosition.Y;
				context.Draw(VerticalScrollKnob, r);
			}
		}

		public void ApplyScrollViewerStyle(ScrollViewerStyle style)
		{
			HorizontalScrollBackground = style.HorizontalScrollBackground;
			HorizontalScrollKnob = style.HorizontalScrollKnob;
			VerticalScrollBackground = style.VerticalScrollBackground;
			VerticalScrollKnob = style.VerticalScrollKnob;

			ApplyWidgetStyle(style);
		}

		protected override Point InternalMeasure(Point availableSize)
		{
			if (InternalChild == null)
			{
				return Point.Zero;
			}

			var measureSize = InternalChild.Measure(availableSize);

			var horizontalScrollbarVisible = ShowHorizontalScrollBar && measureSize.X > availableSize.X;
			var verticalScrollbarVisible = ShowVerticalScrollBar && measureSize.Y > availableSize.Y;
			if (horizontalScrollbarVisible || verticalScrollbarVisible)
			{
				if (horizontalScrollbarVisible)
				{
					measureSize.Y += HorizontalScrollbarHeight;
				}

				if (verticalScrollbarVisible)
				{
					measureSize.X += VerticalScrollbarWidth;
				}
			}

			return measureSize;
		}

		public override void Arrange()
		{
			if (InternalChild == null)
			{
				return;
			}

			var bounds = ActualBounds;
			var availableSize = bounds.Size();
			var oldMeasureSize = InternalChild.Measure(availableSize);

			_horizontalScrollingOn = oldMeasureSize.X > bounds.Width;
			_verticalScrollingOn = oldMeasureSize.Y > bounds.Height;
			if (_horizontalScrollingOn || _verticalScrollingOn)
			{
				var vsWidth = VerticalScrollbarWidth;
				var hsHeight = HorizontalScrollbarHeight;

				if (_horizontalScrollingOn && ShowHorizontalScrollBar)
				{
					availableSize.Y -= hsHeight;

					if (availableSize.Y < 0)
					{
						availableSize.Y = 0;
					}
				}

				if (_verticalScrollingOn && ShowVerticalScrollBar)
				{
					availableSize.X -= vsWidth;

					if (availableSize.X < 0)
					{
						availableSize.X = 0;
					}
				}

				// Remeasure with scrollbars
				var measureSize = InternalChild.Measure(availableSize);

				var bw = bounds.Width - (_verticalScrollingOn && ShowVerticalScrollBar ? vsWidth : 0);

				_horizontalScrollbarFrame = new Rectangle(bounds.Left,
					bounds.Bottom - hsHeight,
					bw,
					hsHeight);

				var mw = measureSize.X;
				if (mw == 0)
				{
					mw = 1;
				}

				_horizontalScrollbarThumb = new Rectangle(bounds.Left,
					bounds.Bottom - hsHeight,
					Math.Max(HorizontalScrollKnob.Size.X, bw * bw / mw),
					HorizontalScrollKnob.Size.Y);

				var bh = bounds.Height - (_horizontalScrollingOn ? hsHeight : 0);

				_verticalScrollbarFrame = new Rectangle(
					bounds.Left + bounds.Width - vsWidth,
					bounds.Top,
					vsWidth,
					bh);

				var mh = measureSize.Y;
				if (mh == 0)
				{
					mh = 1;
				}

				_verticalScrollbarThumb = new Rectangle(
					bounds.Left + bounds.Width - vsWidth,
					bounds.Top,
					VerticalScrollKnob.Size.X,
					Math.Max(VerticalScrollKnob.Size.Y, bh * bh / mh));

				_thumbMaximumX = bw - _horizontalScrollbarThumb.Width;
				_thumbMaximumY = bh - _verticalScrollbarThumb.Height;

				if (_thumbMaximumX == 0)
				{
					_thumbMaximumX = 1;
				}

				if (_thumbMaximumY == 0)
				{
					_thumbMaximumY = 1;
				}

				if (_horizontalScrollingOn && ShowHorizontalScrollBar)
				{
					bounds.Width = measureSize.X;
				}
				else if (_horizontalScrollingOn)
				{
					bounds.Width = oldMeasureSize.X;
				}
				else
				{
					bounds.Width = availableSize.X;
				}

				if (_verticalScrollingOn && ShowVerticalScrollBar)
				{
					bounds.Height = measureSize.Y;
				}
				else if (_verticalScrollingOn)
				{
					bounds.Height = oldMeasureSize.Y;
				}
				else
				{
					bounds.Height = availableSize.Y;
				}
			}

			InternalChild.Layout(bounds);

			// Fit scroll position in new maximums
			var scrollPosition = ScrollPosition;
			ScrollPosition = scrollPosition;
		}

		public void ResetScroll()
		{
			ScrollPosition = Point.Zero;
		}

		private void DesktopTouchMoved(object sender, EventArgs args)
		{
			if (!_startBoundsPos.HasValue)
				return;

			var touchPosition = Desktop.TouchPosition;

			int delta;
			if (_scrollbarOrientation == Orientation.Horizontal)
			{
				delta = (touchPosition.X - _startBoundsPos.Value) * ScrollMaximum.X / _thumbMaximumX;
				_startBoundsPos = touchPosition.X;
			}
			else
			{
				delta = (touchPosition.Y - _startBoundsPos.Value) * ScrollMaximum.Y / _thumbMaximumY;
				_startBoundsPos = touchPosition.Y;
			}

			
			MoveThumb(delta);
		}

		private void DesktopTouchUp(object sender, EventArgs args)
		{
			_startBoundsPos = null;
		}

		protected override void InternalSetStyle(Stylesheet stylesheet, string name)
		{
			ApplyScrollViewerStyle(stylesheet.ScrollViewerStyles[name]);
		}
	}
}