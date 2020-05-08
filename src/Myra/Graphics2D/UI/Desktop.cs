﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Myra.Graphics2D.UI.Styles;
using Myra.Utility;

#if !STRIDE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#else
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Input;
#endif

namespace Myra.Graphics2D.UI
{
	public struct MouseInfo
	{
		public Point Position;
		public bool IsLeftButtonDown;
		public bool IsMiddleButtonDown;
		public bool IsRightButtonDown;
		public float Wheel;
	}

	public static class Desktop
	{
		public const int DoubleClickIntervalInMs = 500;

		public static Func<Rectangle> DefaultBoundsFetcher = () =>
		{
			var size = MyraEnvironment.GraphicsDevice.ViewSize();
			return new Rectangle(0, 0, size.X, size.Y);
		};

		private static RenderContext _renderContext;

		private static bool _layoutDirty = true;
		private static bool _widgetsDirty = true;
		private static Widget _focusedKeyboardWidget, _focusedMouseWheelWidget;
		private static readonly List<Widget> _widgetsCopy = new List<Widget>();
		private static DateTime _lastTouchDown;
		private static DateTime? _lastKeyDown;
		private static int _keyDownCount = 0;
		private static MouseInfo _lastMouseInfo;
		private static IReadOnlyCollection<Keys> _downKeys, _lastDownKeys;
		private static Widget _previousKeyboardFocus;
		private static Widget _previousMouseWheelFocus;
#if !STRIDE
		private static TouchCollection _oldTouchState;
#endif
		private static Widget _scheduleMouseWheelFocus;
		private static bool _isTouchDown;
		private static Point _mousePosition, _touchPosition;
		private static bool _contextMenuShown = false;
		private static bool _keyboardFocusSet = false;
		private static bool _mouseWheelFocusSet = false;
#if MONOGAME
		public static bool HasExternalTextInput = false;
#endif

		/// <summary>
		/// Root Widget
		/// </summary>
		public static Widget Root
		{
			get
			{
				if (Widgets.Count == 0)
				{
					return null;
				}

				return Widgets[0];
			}

			set
			{
				if (Root == value)
				{
					return;
				}

				HideContextMenu();
				Widgets.Clear();

				if (value != null)
				{
					Widgets.Add(value);
				}
			}
		}

		public static IReadOnlyCollection<Keys> DownKeys
		{
			get
			{
				return _downKeys;
			}
		}

		public static Point MousePosition
		{
			get
			{
				return _mousePosition;
			}

			private set
			{
				if (value == _mousePosition)
				{
					return;
				}

				_mousePosition = value;
				MouseMoved.Invoke();

				ChildrenCopy.ProcessMouseMovement();

				if (IsTouchDown)
				{
					TouchPosition = MousePosition;
				}
			}
		}

		public static Point TouchPosition
		{
			get
			{
				return _touchPosition;
			}

			private set
			{
				if (value == _touchPosition)
				{
					return;
				}

				_touchPosition = value;
				TouchMoved.Invoke();

				ChildrenCopy.ProcessTouchMovement();
			}
		}

		public static HorizontalMenu MenuBar { get; set; }

		public static Func<MouseInfo> MouseInfoGetter
		{
			get; set;
		}

		public static Func<IReadOnlyCollection<Keys>> DownKeysGetter
		{
			get; set;
		}

		internal static List<Widget> ChildrenCopy
		{
			get
			{
				UpdateWidgetsCopy();
				return _widgetsCopy;
			}
		}

		public static ObservableCollection<Widget> Widgets { get; } = new ObservableCollection<Widget>();

		public static Func<Rectangle> BoundsFetcher = DefaultBoundsFetcher;

		internal static Rectangle InternalBounds { get; private set; }

		public static Widget ContextMenu { get; private set; }

		public static Widget FocusedKeyboardWidget
		{
			get { return _focusedKeyboardWidget; }

			set
			{
				if (value != null)
				{
					_keyboardFocusSet = true;
				}

				if (value == _focusedKeyboardWidget)
				{
					return;
				}

				var oldValue = _focusedKeyboardWidget;
				if (oldValue != null)
				{
					if (WidgetLosingKeyboardFocus != null)
					{
						var args = new CancellableEventArgs<Widget>(oldValue);
						WidgetLosingKeyboardFocus(null, args);
						if (oldValue.IsPlaced && args.Cancel)
						{
							return;
						}
					}
				}

				_focusedKeyboardWidget = value;
				if (oldValue != null)
				{
					oldValue.OnLostKeyboardFocus();
				}

				if (_focusedKeyboardWidget != null)
				{
					_focusedKeyboardWidget.OnGotKeyboardFocus();
					WidgetGotKeyboardFocus.Invoke(_focusedKeyboardWidget);
				}
			}
		}

		public static Widget FocusedMouseWheelWidget
		{
			get
			{
				return _focusedMouseWheelWidget;
			}

			set
			{
				if (value != null)
				{
					_mouseWheelFocusSet = true;
				}

				_focusedMouseWheelWidget = value;
			}
		}

		private static RenderContext RenderContext
		{
			get
			{
				EnsureRenderContext();

				return _renderContext;
			}
		}

		/// <summary>
		/// Parameters passed to SpriteBatch.Begin
		/// </summary>
		public static SpriteBatchBeginParams SpriteBatchBeginParams
		{
			get
			{
				return RenderContext.SpriteBatchBeginParams;
			}

			set
			{
				RenderContext.SpriteBatchBeginParams = value;
			}
		}

		public static float Opacity { get; set; }

		public static bool IsMouseOverGUI
		{
			get
			{
				return IsPointOverGUI(MousePosition);
			}
		}

		public static bool IsTouchOverGUI
		{
			get
			{
				return IsPointOverGUI(TouchPosition);
			}
		}

		internal static bool IsShiftDown
		{
			get
			{
				return _downKeys.Contains(Keys.LeftShift) || _downKeys.Contains(Keys.RightShift);
			}
		}

		internal static bool IsControlDown
		{
			get
			{
#if !STRIDE
				return _downKeys.Contains(Keys.LeftControl) || _downKeys.Contains(Keys.RightControl);
#else
				return _downKeys.Contains(Keys.LeftCtrl) || _downKeys.Contains(Keys.RightCtrl);
#endif
			}
		}

		internal static bool IsAltDown
		{
			get
			{
#if !STRIDE
				return _downKeys.Contains(Keys.LeftAlt) || _downKeys.Contains(Keys.RightAlt);
#else
				return _downKeys.Contains(Keys.LeftAlt) || _downKeys.Contains(Keys.RightAlt);
#endif
			}
		}

		public static bool IsTouchDown
		{
			get
			{
				return _isTouchDown;
			}

			set
			{
				if (value == _isTouchDown)
				{
					return;
				}

				_isTouchDown = value;
				if (_isTouchDown)
				{

					InputOnTouchDown();
					TouchDown.Invoke();
				}
				else
				{
					InputOnTouchUp();
					TouchUp.Invoke();
				}
			}
		}

		public static int RepeatKeyDownStartInMs { get; set; } = 500;

		public static int RepeatKeyDownInternalInMs { get; set; } = 50;

		public static bool HasModalWidget
		{
			get
			{
				for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
				{
					var w = ChildrenCopy[i];
					if (w.Visible && w.Enabled && w.IsModal)
					{
						return true;
					}
				}

				return false;
			}
		}

		private static bool IsMenuBarActive
		{
			get
			{
				return (MenuBar != null && (MenuBar.OpenMenuItem != null || IsAltDown));
			}
		}

		public static Point InlineSpriteSize = new Point(24, 24);
		public static Func<string, IImage> SpriteFunc = null;

        public static Action<Keys> KeyDownHandler;

		public static event EventHandler MouseMoved;

		public static event EventHandler TouchMoved;
		public static event EventHandler TouchDown;
		public static event EventHandler TouchUp;
		public static event EventHandler TouchDoubleClick;

		public static event EventHandler<GenericEventArgs<float>> MouseWheelChanged;

		public static event EventHandler<GenericEventArgs<Keys>> KeyUp;
		public static event EventHandler<GenericEventArgs<Keys>> KeyDown;
		public static event EventHandler<GenericEventArgs<char>> Char;

		public static event EventHandler<CancellableEventArgs<Widget>> ContextMenuClosing;
		public static event EventHandler<GenericEventArgs<Widget>> ContextMenuClosed;

		public static event EventHandler<CancellableEventArgs<Widget>> WidgetLosingKeyboardFocus;
		public static event EventHandler<GenericEventArgs<Widget>> WidgetGotKeyboardFocus;

		static Desktop()
		{
			Opacity = 1.0f;
			Widgets.CollectionChanged += WidgetsOnCollectionChanged;

			MouseInfoGetter = DefaultMouseInfoGetter;
			DownKeysGetter = DefaultDownKeysGetter;

			KeyDownHandler = OnKeyDown;

#if FNA
			TextInputEXT.TextInput += c =>
			{
				OnChar(c);
			};
#endif
		}

#if !STRIDE
		public static MouseInfo DefaultMouseInfoGetter()
		{
			var state = Mouse.GetState();

			var result = new MouseInfo
			{
				Position = new Point(state.X, state.Y),
				IsLeftButtonDown = state.LeftButton == ButtonState.Pressed,
				IsMiddleButtonDown = state.MiddleButton == ButtonState.Pressed,
				IsRightButtonDown = state.RightButton == ButtonState.Pressed,
				Wheel = state.ScrollWheelValue
			};

			return result;
		}

		public static IReadOnlyCollection<Keys> DefaultDownKeysGetter()
		{
			return Keyboard.GetState().GetPressedKeys();
		}
#else
		public static MouseInfo DefaultMouseInfoGetter()
		{
			var input = MyraEnvironment.Game.Input;

			var v = input.AbsoluteMousePosition;

			var result = new MouseInfo
			{
				Position = new Point((int)v.X, (int)v.Y),
				IsLeftButtonDown = input.IsMouseButtonDown(MouseButton.Left),
				IsMiddleButtonDown = input.IsMouseButtonDown(MouseButton.Middle),
				IsRightButtonDown = input.IsMouseButtonDown(MouseButton.Right),
				Wheel = input.MouseWheelDelta
			};

			return result;
		}

		public static IReadOnlyCollection<Keys> DefaultDownKeysGetter()
		{
			var input = MyraEnvironment.Game.Input;

			return input.Keyboard.DownKeys;
		}
#endif

		public static Widget GetChild(int index)
		{
			return ChildrenCopy[index];
		}

		private static void HandleDoubleClick()
		{
			if ((DateTime.Now - _lastTouchDown).TotalMilliseconds < DoubleClickIntervalInMs)
			{
				TouchDoubleClick.Invoke();

				ChildrenCopy.ProcessTouchDoubleClick();

				_lastTouchDown = DateTime.MinValue;
			}
			else
			{
				_lastTouchDown = DateTime.Now;
			}
		}

		private static void ContextMenuOnTouchDown()
		{
			if (ContextMenu == null || ContextMenu.Bounds.Contains(TouchPosition))
			{
				return;
			}

			var ev = ContextMenuClosing;
			if (ev != null)
			{
				var args = new CancellableEventArgs<Widget>(ContextMenu);
				ev(null, args);

				if (args.Cancel)
				{
					return;
				}
			}

			HideContextMenu();
		}

		private static void InputOnTouchDown()
		{
			_contextMenuShown = false;
			_keyboardFocusSet = false;
			_mouseWheelFocusSet = false;

			ChildrenCopy.ProcessTouchDown();

			if (!_keyboardFocusSet && FocusedKeyboardWidget != null)
			{
				// Nullify keyboard focus
				FocusedKeyboardWidget = null;
			}

			if (!_mouseWheelFocusSet && FocusedMouseWheelWidget != null && FocusedMouseWheelWidget.MouseWheelFocusCanBeNull)
			{
				// Nullify mouse wheel focus
				FocusedMouseWheelWidget = null;
			}

			if (!_contextMenuShown)
			{
				ContextMenuOnTouchDown();
			}
		}

		private static void InputOnTouchUp()
		{
			ChildrenCopy.ProcessTouchUp();
		}

		public static void ShowContextMenu(Widget menu, Point position)
		{
			HideContextMenu();

			ContextMenu = menu;
			if (ContextMenu == null)
			{
				return;
			}

			ContextMenu.HorizontalAlignment = HorizontalAlignment.Left;
			ContextMenu.VerticalAlignment = VerticalAlignment.Top;

			var measure = ContextMenu.Measure(InternalBounds.Size());

			if (position.X + measure.X > InternalBounds.Right)
			{
				position.X = InternalBounds.Right - measure.X;
			}

			if (position.Y + measure.Y > InternalBounds.Bottom)
			{
				position.Y = InternalBounds.Bottom - measure.Y;
			}

			ContextMenu.Left = position.X;
			ContextMenu.Top = position.Y;

			ContextMenu.Visible = true;

			Widgets.Add(ContextMenu);

			if (ContextMenu.AcceptsKeyboardFocus)
			{
				_previousKeyboardFocus = FocusedKeyboardWidget;
				FocusedKeyboardWidget = ContextMenu;
			}

			_scheduleMouseWheelFocus = ContextMenu;

			_contextMenuShown = true;
		}

		public static void HideContextMenu()
		{
			if (ContextMenu == null)
			{
				return;
			}

			Widgets.Remove(ContextMenu);
			ContextMenu.Visible = false;

			ContextMenuClosed.Invoke(ContextMenu);
			ContextMenu = null;

			if (_previousKeyboardFocus != null)
			{
				FocusedKeyboardWidget = _previousKeyboardFocus;
				_previousKeyboardFocus = null;
			}

			if (_previousMouseWheelFocus != null)
			{
				FocusedMouseWheelWidget = _previousMouseWheelFocus;
				_previousMouseWheelFocus = null;
			}
		}

		private static void WidgetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (Widget w in args.NewItems)
				{
					w.IsPlaced = true;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (Widget w in args.OldItems)
				{
					w.IsPlaced = false;
				}
			}
			else if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				foreach (Widget w in ChildrenCopy)
				{
					w.IsPlaced = false;
				}
			}

			InvalidateLayout();
			_widgetsDirty = true;
		}

		private static void EnsureRenderContext()
		{
			if (_renderContext == null)
			{
				var spriteBatch = new SpriteBatch(MyraEnvironment.GraphicsDevice);
				_renderContext = new RenderContext
				{
					Batch = spriteBatch
				};
			}
		}

		public static void RenderVisual()
		{
			EnsureRenderContext();

			var oldScissorRectangle = CrossEngineStuff.GetScissor();

			_renderContext.Begin();

			CrossEngineStuff.SetScissor(InternalBounds);
			_renderContext.View = InternalBounds;
			_renderContext.Opacity = Opacity;

			if (Stylesheet.Current.DesktopStyle != null &&
				Stylesheet.Current.DesktopStyle.Background != null)
			{
				_renderContext.Draw(Stylesheet.Current.DesktopStyle.Background, InternalBounds);
			}

			foreach (var widget in ChildrenCopy)
			{
				if (widget.Visible)
				{
					widget.Render(_renderContext);
				}
			}

			_renderContext.End();

			CrossEngineStuff.SetScissor(oldScissorRectangle);
		}

		public static void Render()
		{
			UpdateInput();
			UpdateLayout();
			RenderVisual();
		}

		static public void InvalidateLayout()
		{
			_layoutDirty = true;
		}

		public static void UpdateLayout()
		{
			var newBounds = BoundsFetcher();
			if (InternalBounds != newBounds)
			{
				InvalidateLayout();
			}

			InternalBounds = newBounds;

			if (InternalBounds.IsEmpty)
			{
				return;
			}

			if (!_layoutDirty)
			{
				return;
			}

			foreach (var i in ChildrenCopy)
			{
				if (i.Visible)
				{
					i.Layout(InternalBounds);
				}
			}

			// Rest processing
			MenuBar = null;
			var active = true;
			for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
			{
				var w = ChildrenCopy[i];
				if (!w.Visible)
				{
					continue;
				}

				UIUtils.ProcessWidgets(w, widget =>
				{
					widget.Active = active;

					if (MenuBar == null && widget is HorizontalMenu)
					{
						// Found MenuBar
						MenuBar = (HorizontalMenu)widget;
					}

					if (FocusedMouseWheelWidget == null && widget is ScrollViewer && widget.AcceptsMouseWheelFocus && active)
					{
						// If focused mouse wheel widget unset, then set first that accepts such focus
						FocusedMouseWheelWidget = widget;
					}

					// Continue
					return true;
				});

				// Everything after first modal widget is not active
				if (w.IsModal)
				{
					active = false;
				}
			}

			UpdateRecursiveLayout(ChildrenCopy);

			ChildrenCopy.ProcessMouseMovement();

			_layoutDirty = false;
		}

		static private void UpdateRecursiveLayout(IEnumerable<Widget> widgets)
		{
			foreach (var i in widgets)
			{
				if (!i.Layout2d.Nullable)
				{
					ExpressionParser.Parse(i, ChildrenCopy);
				}

				var c = i as Container;
				if (c != null)
				{
					UpdateRecursiveLayout(c.ChildrenCopy);
				}
			}
		}

		private static Widget GetWidgetBy(Widget root, Func<Widget, bool> filter)
		{
			if (filter(root))
			{
				return root;
			}

			var asContainer = root as Container;
			if (asContainer == null)
			{
				return null;
			}

			for (var i = 0; i < asContainer.ChildrenCount; ++i)
			{
				var w = asContainer.GetChild(i);
				var result = GetWidgetBy(w, filter);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public static Widget GetWidgetBy(Func<Widget, bool> filter)
		{
			foreach (var w in ChildrenCopy)
			{
				var result = GetWidgetBy(w, filter);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		static public Widget GetWidgetByID(string ID)
		{
			return GetWidgetBy(w => w.Id == ID);
		}

		public static int CalculateTotalWidgets(bool visibleOnly)
		{
			var result = 0;
			foreach (var w in Widgets)
			{
				if (visibleOnly && !w.Visible)
				{
					continue;
				}

				++result;

				var asContainer = w as Container;
				if (asContainer != null)
				{
					result += asContainer.CalculateTotalChildCount(visibleOnly);
				}
			}

			return result;
		}

		private static Widget GetTopWidget()
		{
			for (var i = ChildrenCopy.Count - 1; i >= 0; --i)
			{
				var w = ChildrenCopy[i];
				if (w.Visible && w.Enabled && w.Active)
				{
					return w;
				}
			}

			return null;
		}

		public static void HandleButton(bool isDown, bool wasDown, MouseButtons buttons)
		{
			if (isDown && !wasDown)
			{
				TouchPosition = MousePosition;
				IsTouchDown = true;
				HandleDoubleClick();
			}
			else if (!isDown && wasDown)
			{
				IsTouchDown = false;
			}
		}

#if !STRIDE
		public static void UpdateTouch()
		{
			var touchState = TouchPanel.GetState();
			if (!touchState.IsConnected)
			{
				return;
			}

			if (touchState.Count > 0)
			{
				var pos = touchState[0].Position;
				TouchPosition = new Point((int)pos.X, (int)pos.Y);
			}

			if (touchState.Count > 0 && _oldTouchState.Count == 0)
			{
				// Down
				IsTouchDown = true;
				HandleDoubleClick();
			}
			else if (touchState.Count == 0 && _oldTouchState.Count > 0)
			{
				// Up
				IsTouchDown = false;
			}

			_oldTouchState = touchState;
		}
#endif

		public static void UpdateMouseInput()
		{
			if (_scheduleMouseWheelFocus != null)
			{
				if (_scheduleMouseWheelFocus.AcceptsMouseWheelFocus)
				{
					_previousMouseWheelFocus = FocusedMouseWheelWidget;
					FocusedMouseWheelWidget = _scheduleMouseWheelFocus;
				}

				_scheduleMouseWheelFocus = null;
			}

			if (MouseInfoGetter == null)
			{
				return;
			}

			var mouseInfo = MouseInfoGetter();
			var mousePosition = mouseInfo.Position;

			if (SpriteBatchBeginParams.TransformMatrix != null)
			{
				// Apply transform
				var t = Vector2.Transform(
					new Vector2(mousePosition.X, mousePosition.Y),
					SpriteBatchBeginParams.InverseTransform);

				mousePosition = new Point((int)t.X, (int)t.Y);
			}

			MousePosition = mousePosition;

			HandleButton(mouseInfo.IsLeftButtonDown, _lastMouseInfo.IsLeftButtonDown, MouseButtons.Left);
			HandleButton(mouseInfo.IsMiddleButtonDown, _lastMouseInfo.IsMiddleButtonDown, MouseButtons.Middle);
			HandleButton(mouseInfo.IsRightButtonDown, _lastMouseInfo.IsRightButtonDown, MouseButtons.Right);
#if STRIDE
				var handleWheel = mouseInfo.Wheel != 0;
#else
			var handleWheel = mouseInfo.Wheel != _lastMouseInfo.Wheel;
#endif

			if (handleWheel)
			{
				var delta = mouseInfo.Wheel;
#if !STRIDE
				delta -= _lastMouseInfo.Wheel;
#endif
				MouseWheelChanged.Invoke(delta);

				if (FocusedMouseWheelWidget != null)
				{
					FocusedMouseWheelWidget.OnMouseWheel(delta);
				}
			}

			_lastMouseInfo = mouseInfo;
		}

		public static void UpdateKeyboardInput()
		{
			if (DownKeysGetter == null)
			{
				return;
			}

			_downKeys = DownKeysGetter();

			if (_downKeys != null && _lastDownKeys != null)
			{
				var now = DateTime.Now;
				foreach (var key in _downKeys)
				{
					if (!_lastDownKeys.Contains(key))
					{
						KeyDownHandler?.Invoke(key);

						_lastKeyDown = now;
						_keyDownCount = 0;
					}
				}

				foreach (var key in _lastDownKeys)
				{
					if (!_downKeys.Contains(key))
					{
						// Key had been released
						KeyUp.Invoke(key);
						if (_focusedKeyboardWidget != null && _focusedKeyboardWidget.Active)
						{
							_focusedKeyboardWidget.OnKeyUp(key);
						}

						_lastKeyDown = null;
						_keyDownCount = 0;
					}
					else if (_lastKeyDown != null &&
					  ((_keyDownCount == 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownStartInMs) ||
					  (_keyDownCount > 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownInternalInMs)))
					{
						KeyDownHandler?.Invoke(key);

						_lastKeyDown = now;
						++_keyDownCount;
					}
				}
			}

			_lastDownKeys = _downKeys.ToArray();
		}

		public static void UpdateInput()
		{
			UpdateMouseInput();
			UpdateKeyboardInput();

#if !STRIDE
			try
			{
				UpdateTouch();
			}
			catch (Exception)
			{
			}
#endif
		}

		public static void OnKeyDown(Keys key)
		{
			KeyDown.Invoke(key);

			if (IsMenuBarActive)
			{
				MenuBar.OnKeyDown(key);
			}
			else
			{
				// Small workaround: if key is escape  active widget is window
				// Send it there
				var topWidget = GetTopWidget();
				var asWindow = topWidget as Window;
				if (asWindow != null)
				{
					if (key == Keys.Escape)
					{
						asWindow.OnKeyDown(key);
					}

					var asDialog = asWindow as Dialog;
					if (asDialog != null && key == Keys.Enter)
					{
						// Dialog also always receives Enter (Ok)
						asWindow.OnKeyDown(key);
					}
				}

				if (_focusedKeyboardWidget != null && _focusedKeyboardWidget.Active)
				{
					_focusedKeyboardWidget.OnKeyDown(key);

#if STRIDE
					var ch = key.ToChar(_downKeys.Contains(Keys.LeftShift) ||
										_downKeys.Contains(Keys.RightShift));
					if (ch != null)
					{
						_focusedKeyboardWidget.OnChar(ch.Value);
					}
#endif
				}
			}

			if (key == Keys.Escape && ContextMenu != null)
			{
				HideContextMenu();
			}

#if MONOGAME
			if (!HasExternalTextInput && !IsControlDown && !IsAltDown)
			{
				var c = key.ToChar(IsShiftDown);
				if (c != null)
				{
					OnChar(c.Value);
				}
			}
#endif
		}

		public static void OnChar(char c)
		{
			if (IsMenuBarActive)
			{
				// Don't accept chars if menubar is open
				return;
			}

			if (_focusedKeyboardWidget != null && _focusedKeyboardWidget.Active)
			{
				_focusedKeyboardWidget.OnChar(c);
			}

			Char.Invoke(c);
		}

		private static void UpdateWidgetsCopy()
		{
			if (!_widgetsDirty)
			{
				return;
			}

			_widgetsCopy.Clear();
			_widgetsCopy.AddRange(Widgets);

			_widgetsDirty = false;
		}

		private static bool InternalIsPointOverGUI(Point p, Widget w)
		{
			if (!w.Visible || !w.BorderBounds.Contains(p))
			{
				return false;
			}

			if (!w.FallsThrough(p))
			{
				return true;
			}

			// If widget fell through, then it is Container for sure
			var asContainer = (Container)w;

			// Or if any child is solid
			foreach (var ch in asContainer.ChildrenCopy)
			{
				if (InternalIsPointOverGUI(p, ch))
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsPointOverGUI(Point p)
		{
			foreach (var widget in ChildrenCopy)
			{
				if (InternalIsPointOverGUI(p, widget))
				{
					return true;
				}
			}

			return false;
		}
	}
}