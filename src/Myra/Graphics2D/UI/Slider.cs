﻿using System;
using System.ComponentModel;
using Myra.Graphics2D.UI.Styles;
using Myra.Utility;
using System.Xml.Serialization;

#if !XENKO
using Microsoft.Xna.Framework;
#else
using Xenko.Core.Mathematics;
#endif

namespace Myra.Graphics2D.UI
{
	public abstract class Slider : SingleItemContainer<ImageButton>
	{
		private float _value;

		[Browsable(false)]
		[XmlIgnore]
		public abstract Orientation Orientation
		{
			get;
		}

		[Category("Behavior")]
		[DefaultValue(0.0f)]
		public float Minimum
		{
			get; set;
		}

		[Category("Behavior")]
		[DefaultValue(100.0f)]
		public float Maximum
		{
			get; set;
		}

		[Category("Behavior")]
		[DefaultValue(0.0f)]
		public float Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value > Maximum)
				{
					//could throw error instead?
					value = Maximum;
				}

				if (value < Minimum)
				{
					//could throw error instead?
					value = Minimum;
				}

				if (_value == value)
				{
					return;
				}

				var oldValue = _value;
				_value = value;

				SyncHintWithValue();

				ValueChanged?.Invoke(this, new ValueChangedEventArgs<float>(oldValue, value));
			}
		}

		private int Hint
		{
			get
			{
				return Orientation == Orientation.Horizontal ? InternalChild.Left : InternalChild.Top;
			}

			set
			{
				if (Hint == value)
				{
					return;
				}

				if (Orientation == Orientation.Horizontal)
				{
					InternalChild.Left = value;
				}
				else
				{
					InternalChild.Top = value;
				}
			}
		}

		private int MaxHint
		{
			get
			{
				return Orientation == Orientation.Horizontal
					? Bounds.Width - InternalChild.Bounds.Width
					: Bounds.Height - InternalChild.Bounds.Height;
			}
		}

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
				}

				base.IsPlaced = value;

				if (IsPlaced)
				{
					Desktop.TouchMoved += DesktopTouchMoved;
				}
			}
		}

		/// <summary>
		/// Fires when the value had been changed
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<float>> ValueChanged;

		/// <summary>
		/// Fires only when the value had been changed by user(doesnt fire if it had been assigned through code)
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<float>> ValueChangedByUser;

		protected Slider(string styleName)
		{
			InternalChild = new ImageButton(null)
			{
				ReleaseOnTouchLeft = false
			};

			SetStyle(styleName);

			Maximum = 100;
		}

		private int GetHint()
		{
			return Orientation == Orientation.Horizontal ? Desktop.TouchPosition.X - ActualBounds.X - InternalChild.ActualBounds.Width / 2 :
				Desktop.TouchPosition.Y - ActualBounds.Y - InternalChild.ActualBounds.Height / 2;
		}

		public void ApplySliderStyle(SliderStyle style)
		{
			ApplyWidgetStyle(style);

			InternalChild.ApplyImageButtonStyle(style.KnobStyle);
		}

		private void SyncHintWithValue()
		{
			Hint = (int)(MaxHint * (_value / Maximum));
		}

		public override void Arrange()
		{
			base.Arrange();

			SyncHintWithValue();
		}

		public override void OnTouchDown()
		{
			base.OnTouchDown();

			UpdateHint();
			InternalChild.IsPressed = true;
		}

		private void UpdateHint()
		{
			var hint = GetHint();
			if (hint < 0)
			{
				hint = 0;
			}

			if (hint > MaxHint)
			{
				hint = MaxHint;
			}

			var oldValue = _value;
			var valueChanged = false;
			// Sync Value with Hint
			if (MaxHint != 0)
			{
				var d = Maximum - Minimum;

				var newValue = Minimum + hint * d / MaxHint;
				if (_value != newValue)
				{
					_value = newValue;
					valueChanged = true;
				}
			}

			Hint = hint;

			if (valueChanged)
			{
				var ev = ValueChanged;
				ev?.Invoke(this, new ValueChangedEventArgs<float>(oldValue, _value));

				ev = ValueChangedByUser;
				ev?.Invoke(this, new ValueChangedEventArgs<float>(oldValue, _value));
			}
		}

		private void DesktopTouchMoved(object sender, EventArgs args)
		{
			if (!InternalChild.IsPressed)
			{
				return;
			}

			UpdateHint();
		}
	}
}