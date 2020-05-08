﻿using System;
using System.ComponentModel;
using System.Xml.Serialization;

#if !XENKO
using Microsoft.Xna.Framework;
#else
using Xenko.Core.Mathematics;
#endif

namespace Myra.Graphics2D.UI
{
	public class SingleItemContainer<T> : Container where T : Widget
	{
		private T _internalChild;

		[Browsable(false)]
		[XmlIgnore]
		protected internal virtual T InternalChild
		{
			get { return _internalChild; }
			set
			{
				if (_internalChild != null)
				{
					_internalChild.Parent = null;
					_internalChild.IsPlaced = false;

					_internalChild = null;
				}

				_internalChild = value;

				if (_internalChild != null)
				{
					_internalChild.Parent = this;
					_internalChild.IsPlaced = IsPlaced;
				}

				InvalidateChildren();
			}
		}

		public override int ChildrenCount
		{
			get
			{
				return InternalChild != null ? 1 : 0;
			}
		}

		public override Widget GetChild(int index)
		{
			if (index < 0 ||
				InternalChild == null ||
				index >= 1)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			return InternalChild;
		}

		public override void Arrange()
		{
			base.Arrange();

			if (InternalChild == null)
			{
				return;
			}

			InternalChild.Layout(ActualBounds);
		}

		protected override Point InternalMeasure(Point availableSize)
		{
			var result = Point.Zero;

			if (InternalChild != null)
			{
				availableSize.X -= (Padding.Left + Padding.Right);
				availableSize.Y -= (Padding.Top + Padding.Bottom);
				result = InternalChild.Measure(availableSize);
			}

			return result;
		}

		public override void RemoveChild(Widget widget)
		{
			if (widget != InternalChild)
			{
				return;
			}

			InternalChild = null;
		}
	}
}