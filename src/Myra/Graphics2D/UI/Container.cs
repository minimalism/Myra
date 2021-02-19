using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using Myra.Utility;

#if !STRIDE
using Microsoft.Xna.Framework;
#else
using Stride.Core.Mathematics;
#endif

namespace Myra.Graphics2D.UI
{
	public abstract class Container : Widget
	{
		private bool _childrenDirty = true;
		private readonly List<Widget> _childrenCopy = new List<Widget>();

		[XmlIgnore]
		[Browsable(false)]
		public abstract int ChildrenCount { get; }

		internal List<Widget> ChildrenCopy
		{
			get
			{
				// We return copy of our collection
				// To prevent exception when someone modifies the collection during the iteration
				UpdateWidgets();

				return _childrenCopy;
			}
		}

		public override bool Enabled
		{
			get { return base.Enabled; }

			set
			{
				if (base.Enabled == value)
				{
					return;
				}

				base.Enabled = value;

				foreach (var item in ChildrenCopy)
				{
					item.Enabled = value;
				}
			}
		}

		public override bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				if (base.Visible == value)
				{
					return;
				}

				base.Visible = value;

				foreach (var child in ChildrenCopy)
				{
					child.ParentVisible = value;
				}
			}
		}

		public override Desktop? Desktop 
		{
			get
			{
				return base.Desktop;
			}

			set
			{
				base.Desktop = value;

				foreach (var child in ChildrenCopy)
				{
					child.Desktop = value;
				}
			}
		}

		internal override bool ParentVisible {
			get
			{
				return base.ParentVisible;
			}

			set
			{
				if (base.ParentVisible != value)
				{
					base.ParentVisible = value;

					foreach (var child in ChildrenCopy)
					{
						child.ParentVisible = value;
					}
				}
			}
		}

		public abstract Widget GetChild(int index);

		private void UpdateWidgets()
		{
			if (!_childrenDirty)
			{
				return;
			}


			_childrenCopy.Clear();

			for (var i = 0; i < ChildrenCount; ++i)
			{
				_childrenCopy.Add(GetChild(i));
			}

			_childrenDirty = false;
		}

		protected void InvalidateChildren()
		{
			InvalidateMeasure();
			_childrenDirty = true;
		}

		public override void OnMouseEntered()
		{
			base.OnMouseEntered();

			ChildrenCopy.ProcessMouseMovement();
		}

		public override void OnMouseLeft()
		{
			base.OnMouseLeft();

			ChildrenCopy.ProcessMouseMovement();
		}

		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			ChildrenCopy.ProcessMouseMovement();
		}

		public override void OnTouchEntered()
		{
			base.OnTouchEntered();

			ChildrenCopy.ProcessTouchMovement();
		}

		public override void OnTouchLeft()
		{
			base.OnTouchLeft();

			ChildrenCopy.ProcessTouchMovement();
		}

		public override void OnTouchMoved()
		{
			base.OnTouchMoved();

			ChildrenCopy.ProcessTouchMovement();
		}

		public override void OnTouchDown()
		{
			base.OnTouchDown();

			ChildrenCopy.ProcessTouchDown();
		}

		public override void OnTouchUp()
		{
			base.OnTouchUp();

			ChildrenCopy.ProcessTouchUp();
		}

		public override void OnTouchDoubleClick()
		{
			base.OnTouchDoubleClick();

			ChildrenCopy.ProcessTouchDoubleClick();
		}

		public override void MoveChildren(Point delta)
		{
			base.MoveChildren(delta);

			foreach (var child in ChildrenCopy)
			{
				if (!child.Visible)
					continue;

				child.MoveChildren(delta);
			}
		}

		public override void InternalRender(RenderContext batch)
		{
			foreach (var child in ChildrenCopy)
			{
				if (!child.Visible)
					continue;

				child.Render(batch);
			}
		}

		public int CalculateTotalChildCount(bool visibleOnly)
		{
			var result = ChildrenCopy.Count;

			foreach (var child in ChildrenCopy)
			{
				if (visibleOnly && !child.Visible)
				{
					continue;
				}

				var asCont = child as Container;
				if (asCont != null)
				{
					result += asCont.CalculateTotalChildCount(visibleOnly);
				}
			}

			return result;
		}

		public abstract void RemoveChild(Widget widget);
	}
}