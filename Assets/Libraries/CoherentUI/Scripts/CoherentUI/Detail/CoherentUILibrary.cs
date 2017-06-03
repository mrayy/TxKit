#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif

#if UNITY_NACL || UNITY_WEBPLAYER
#define COHERENT_UNITY_UNSUPPORTED_PLATFORM
#endif

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
using Coherent.UI;
#elif UNITY_IPHONE || UNITY_ANDROID
using Coherent.UI.Mobile;
#endif

#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
namespace Coherent.UI
#elif UNITY_IPHONE || UNITY_ANDROID
namespace Coherent.UI.Mobile
#endif
{
	class CoherentUILibrary
	{
		private ViewContextFactory m_ContextFactory = null;
		private ILogHandler m_LogHandler;

		private List<ViewContext> m_Contexts = new List<ViewContext>();

		/// <summary>
		/// Gets the path to CoherentUI_Host for the current platform
		/// </summary>
		/// <returns>relative path to CoherentUI_Host</returns>
		public static string GetHostDirectory()
		{
#if UNITY_EDITOR
			return (Application.platform == RuntimePlatform.WindowsEditor)
				? "Libraries/CoherentUI/Binaries/CoherentUI_Host/windows"
					: "Libraries/CoherentUI/Binaries/CoherentUI_Host/macosx";
#elif UNITY_STANDALONE_WIN
			return "CoherentUI_Host/windows";
#elif UNITY_STANDALONE_LINUX
			return "CoherentUI_Host/linux";
#elif UNITY_STANDALONE_OSX
			return "Data/CoherentUI_Host/macosx";
#elif UNITY_IPHONE || UNITY_ANDROID
			return "";
#else
#warning Unsupported Unity platform
			throw new System.ApplicationException
				("Coherent Browser doesn't support the target platfrom");
#endif
		}

		private ViewContextFactory ContextFactory
		{
			get
			{
				if (m_ContextFactory == null)
				{
					var factorySettings = new FactorySettings()
					#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
					{
						HostDirectory = System.IO.Path.Combine(
							Application.dataPath,
							CoherentUILibrary.GetHostDirectory())
					}
					#endif
					;

					m_LogHandler = new UnityLogHandler();

					m_ContextFactory = CoherentUI_Native.InitializeCoherentUI(
						Versioning.SDKVersion,
						License.COHERENT_KEY,
						factorySettings,
						new RenderingParameters(),
						Severity.Info,
						m_LogHandler
						);
				}
				return m_ContextFactory;
			}
		}

		private static CoherentUILibrary Instance = new CoherentUILibrary();

		/// <summary>
		/// Creates a new ViewContext
		/// </summary>
		/// <param name="ctxSettings">Settings for the context</param>
		/// <param name="listener">Listener for events for the context</param>
		/// <param name="fileHandler">File handler for the context</param>
		/// <returns>the newly created ViewContext</returns>
		public static ViewContext CreateViewContext(ContextSettings ctxSettings,
													ContextListener listener,
													FileHandler fileHandler)
		{
			var context = Instance.ContextFactory.CreateViewContext(
				ctxSettings, listener, fileHandler);
			Instance.m_Contexts.Add(context);

			return context;
		}

		/// <summary>
		/// Destroys the given ViewContext
		/// </summary>
		/// <param name="context">The context to be destroyed</param>
		public static void DestroyViewContext(ViewContext context)
		{
			Instance.m_Contexts.Remove(context);
			context.Uninitialize();
			context.Dispose();

			if (Instance.m_Contexts.Count == 0)
			{
				Instance.ContextFactory.Destroy();
				Instance.ContextFactory.Dispose();
				Instance.m_LogHandler.Dispose();
				Instance.m_LogHandler = null;
				Instance.m_ContextFactory = null;
			}
		}
	}
}
