﻿using Myra.Graphics2D.UI;
using System.Collections.Generic;

#if !STRIDE
using Microsoft.Xna.Framework;
#else
using Stride.Core.Mathematics;
#endif

namespace Myra.Utility
{
	internal static class InputHelpers
	{
		private static bool CommonTouchCheck(this Widget w)
		{
			return w.Visible && w.Active && w.ContainsMouse;
		}

		public static bool FallsThrough(this Widget w, Point p)
		{
			// Only containers can fall through
			if (!(w is Grid ||
				w is StackPanel ||
				w is Panel ||
				w is SplitPane ||
				w is ScrollViewer))
			{
				return false;
			}

			// Real containers are solid only if backround is set
			if (w.IsSolidAt(p))
			{
				return false;
			}

			var asScrollViewer = w as ScrollViewer;
			if (asScrollViewer != null)
			{
				// Special case
				if (asScrollViewer._horizontalScrollingOn && asScrollViewer._horizontalScrollbarFrame.Contains(p) ||
					asScrollViewer._verticalScrollingOn && asScrollViewer._verticalScrollbarFrame.Contains(p))
				{
					return false;
				}
			}

			return true;
		}

		public static void ProcessTouchDown(this List<Widget> widgets)
		{
			for (var i = widgets.Count - 1; i >= 0; --i)
			{
				var w = widgets[i];

				if (w.CommonTouchCheck())
				{
					w.OnTouchDown();
					if (!w.FallsThrough(Desktop.TouchPosition))
					{
						break;
					}
				}

				if (w.IsModal)
				{
					break;
				}
			}
		}

		public static void ProcessTouchUp(this List<Widget> widgets)
		{
			for (var i = widgets.Count - 1; i >= 0; --i)
			{
				var w = widgets[i];

				if (w.IsTouchInside)
				{
					w.OnTouchUp();
				}
			}
		}

		public static void ProcessTouchDoubleClick(this List<Widget> widgets)
		{
			for (var i = widgets.Count - 1; i >= 0; --i)
			{
				var w = widgets[i];

				if (w.CommonTouchCheck())
				{
					w.OnTouchDoubleClick();
					if (!w.FallsThrough(Desktop.TouchPosition))
					{
						break;
					}
				}

				if (w.IsModal)
				{
					break;
				}
			}
		}

		public static void ProcessMouseMovement(this List<Widget> widgets)
		{
			bool mouseConsumed = false;
			for (var i = widgets.Count - 1; i >= 0; --i)
			{
				var w = widgets[i];

				var wasMouseOver = w.IsMouseInside;

				if (!mouseConsumed)
				{
					if (w.CommonTouchCheck())
					{
						if (!wasMouseOver)
						{
							w.OnMouseEntered();
						}
						else
						{
							w.OnMouseMoved();
						}
						mouseConsumed = true;
					}
					else if (wasMouseOver)
					{
						w.OnMouseLeft();
					}
					if (w.IsModal)
					{
						mouseConsumed = true;
					}
				}
				else if (wasMouseOver)
				{
					w.OnMouseLeft();
				}
			}
		}

		public static void ProcessTouchMovement(this List<Widget> widgets)
		{
			// First run: call on OnTouchLeft on all widgets if it is required
			for (var i = widgets.Count - 1; i >= 0; --i)
			{
				var w = widgets[i];
				if (!w.ContainsTouch && w.IsTouchInside)
				{
					w.OnTouchLeft();
				}
			}

			// Second run: OnTouchEnter/OnTouchMoved
			for (var i = widgets.Count - 1; i >= 0; --i)
			{
				var w = widgets[i];

				if (w.CommonTouchCheck())
				{
					var isTouchOver = w.ContainsTouch;
					var wasTouchOver = w.IsTouchInside;

					if (isTouchOver && !wasTouchOver)
					{
						w.OnTouchEntered();
					}

					if (isTouchOver && wasTouchOver)
					{
						w.OnTouchMoved();
					}

					if (!w.FallsThrough(Desktop.TouchPosition))
					{
						break;
					}
				}

				if (w.IsModal)
				{
					break;
				}
			}
		}
	}
}