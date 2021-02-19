﻿using System;
using System.Reflection;
using AssetManagementBase;
using AssetManagementBase.Utility;
using Myra.Assets;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Engine;
using Stride.Graphics;
#else
using Myra.Platform;
#endif

namespace Myra
{
	public static class MyraEnvironment
	{
		private static AssetManager _defaultAssetManager;
		private static bool _assetsLoadersUpdated = false;

		public static event EventHandler GameDisposed;

		public static int FontAtlasSize = 1024;

#if MONOGAME || FNA || STRIDE

		private static Game _game;

		public static Game Game
		{
			get
			{
				if (_game == null)
				{
					throw new Exception("MyraEnvironment.Game is null. Please, set it to the Game instance before using Myra.");
				}

				return _game;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (_game == value)
				{
					return;
				}

#if !STRIDE
				if (_game != null)
				{
					_game.Disposed -= GameOnDisposed;
				}
#endif

				_game = value;

				UpdateAssetManager();

#if !STRIDE
				if (_game != null)
				{
					_game.Disposed += GameOnDisposed;
				}
#endif
			}
		}

		public static GraphicsDevice GraphicsDevice
		{
			get => Game.GraphicsDevice;
		}
#else

		private static IMyraPlatform _platform;

		public static IMyraPlatform Platform
		{
			get
			{
				if (_platform == null)
				{
					throw new Exception("MyraEnvironment.Platform is null. Please, set it before using Myra.");
				}

				return _platform;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_platform = value;

				UpdateAssetManager();
			}
		}
#endif

		/// <summary>
		/// Default Assets Manager
		/// </summary>
		public static AssetManager DefaultAssetManager
		{
			get
			{
				if (_defaultAssetManager == null)
				{
					_defaultAssetManager = new AssetManager(new FileAssetResolver(PathUtils.ExecutingAssemblyDirectory));
				}

				return _defaultAssetManager;
			}
		}

		public static bool DrawWidgetsFrames { get; set; }
		public static bool DrawKeyboardFocusedWidgetFrame { get; set; }
		public static bool DrawMouseWheelFocusedWidgetFrame { get; set; }
		public static bool DrawTextGlyphsFrames { get; set; }
		public static bool DisableClipping { get; set; }

		private static void UpdateAssetManager()
		{
			if (_assetsLoadersUpdated)
			{
				return;
			}

#if MONOGAME || FNA
			AssetManager.SetAssetLoader(new SoundEffectLoader());
#endif
			AssetManager.SetAssetLoader(new Texture2DLoader());
			AssetManager.SetAssetLoader(new StaticSpriteFontLoader());
			AssetManager.SetAssetLoader(new FontSystemLoader());
			AssetManager.SetAssetLoader(new DynamicSpriteFontLoader());
			AssetManager.SetAssetLoader(new SpriteFontBaseLoader());

			_assetsLoadersUpdated = true;
		}

		private static void GameOnDisposed(object sender, EventArgs eventArgs)
		{
			DefaultAssets.Dispose();

			var ev = GameDisposed;
			if (ev != null)
			{
				ev(null, EventArgs.Empty);
			}
		}

		public static string Version
		{
			get
			{
				var assembly = typeof(MyraEnvironment).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		internal static string InternalClipboard;

		/// <summary>
		/// Applies an overall scaling factor to adjust the size and spacing of widgets on devices with high definition displays
		/// </summary>
		public static float? LayoutScale { get; set; }
	}
}