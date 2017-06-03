#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif

#if UNITY_NACL || UNITY_WEBPLAYER
#define COHERENT_UNITY_UNSUPPORTED_PLATFORM
#endif

#if UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
#define COHERENT_SIMULATE_MOBILE_IN_EDITOR
#endif

#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3
#define COHERENT_UNITY_PRE_4_5
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
using Coherent.UI;
using CoherentUI = Coherent.UI;
#elif UNITY_IPHONE || UNITY_ANDROID
using Coherent.UI.Mobile;
using CoherentUI = Coherent.UI.Mobile;
#endif

/// <summary>
/// Component controlling the CoherentUI System
/// </summary>
[AddComponentMenu("Coherent Browser/UI System")]
public class CoherentUISystem : MonoBehaviour
{

	private static CoherentUISystem m_Instance = null;
	public static CoherentUISystem DefaultContext
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = CoherentUISystem.Create();
				if (m_Instance == null)
				{
					throw new System.ApplicationException("Unable to create "
						+ "Coherent UI System");
				}
			}
			return m_Instance;
		}
	}

	public const byte COHERENT_PREFIX = 177;
	public enum CoherentRenderingEventType
	{
		DrawView = 1,
		WakeRenderer = 2

	};

	public enum CoherentRenderingFlags
	{
		None = 0,
		FlipY = 1,
		CorrectGamma = 2
	};
	public enum CoherentFilteringModes
	{
		PointFiltering = 1,
		LinearFiltering = 2
	};
	private ViewContext m_UISystem;
	private SystemListener m_SystemListener;
	private FileHandler m_FileHandler;

	/// <summary>
	/// Creates the FileHandler instance for the system. Change to allow usage of custom FileHandler
	/// </summary>
	public static System.Func<FileHandler> FileHandlerFactoryFunc = () =>
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		// Android's default file handler is implemented in the Coherent library for better performance.
		// If you need custom reading, check the Android custom file handler sample.
		return null;
#else
		return new UnityFileHandler();
#endif
	};

	/// <summary>
	/// Creates the SystemListener instance for the system. Change to allow usage of custom EventListener
	/// <remarks>custom OnSystemReady override must call SystemListener.OnSystemReady</remarks>
	/// <para>Action to be given to SystemListener constructor</para>
	/// </summary>
	public static System.Func<System.Action, SystemListener> SystemListenerFactoryFunc;

	#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
	private Vector2 m_LastMousePosition = new Vector2(-1, -1);
	private MouseEventData m_MouseMoveEvent = new MouseEventData() { MouseModifiers = new EventMouseModifiersState(), Modifiers = new EventModifiersState() };
	#endif

	#if !UNITY_EDITOR && (UNITY_ANDROID)
	private string m_TouchScreenKeyboardText = SystemListener.GetTouchScreenKbdInitialText();

	private TouchScreenKeyboard m_TouchScreenKeyboard;

	private bool m_ExpectKbdShow = false;
	private bool m_ExpectedKbdClosed = false;

	public TouchScreenKeyboard TouchscreenKeyboard
	{
		get
		{
			return m_TouchScreenKeyboard;
		}

		set
		{
			if (m_ExpectedKbdClosed)
			{
				m_ExpectedKbdClosed = false;

				// assert value == null
				if (value != null)
				{
					Debug.LogWarning("Setting touch-screen object to non-null when keyboard was closed!");
				}

				m_TouchScreenKeyboard = null;
				m_TouchScreenKeyboardText = SystemListener.GetTouchScreenKbdInitialText();
				return;
			}

			if (m_ExpectKbdShow)
			{
				m_ExpectKbdShow = false;

				// Send signal for closing
				foreach (CoherentUIView view in m_Views)
				{
					if (view != null && view.Listener != null && view.Listener.View != null)
					{
						view.Listener.View.DispatchKeyEventInternal(101, 0);
					}
				}

				m_ExpectedKbdClosed = true;
			}

			if (value == null && m_TouchScreenKeyboard != null && TouchScreenKeyboard.visible)
			{
				foreach (CoherentUIView view in m_Views)
				{
					if (view != null && view.Listener != null && view.Listener.View != null)
					{
						view.Listener.View.DispatchKeyEventInternal(100, 0);
					}
				}

				m_ExpectKbdShow = true;
			}
			if (value == null)
			{
				m_TouchScreenKeyboardText = SystemListener.GetTouchScreenKbdInitialText();
			}

			m_TouchScreenKeyboard = value;
		}
	}
	#endif

	/// <summary>
	/// Indicates whether one of the views in the system is keeping input focus.
	/// </summary>
	private bool m_SystemHasFocusedView = false;

	/// <summary>
	/// Determines whether the Coherent UI System component is currently in its Update() method
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is updating; otherwise, <c>false</c>.
	/// </returns>
	public bool IsUpdating { get; private set; }

	public delegate void OnUISystemDestroyingDelegate();
	public event OnUISystemDestroyingDelegate UISystemDestroying;

	public delegate void SystemReadyEventHandler();

	private SystemReadyEventHandler SystemReadyHandlers;

	public event SystemReadyEventHandler SystemReady
	{
		add {
			if (!IsReady())
			{
				SystemReadyHandlers += value;
			}
			else
			{
				m_ReadyHandlers.Add(value);
			}
		}
		remove {
			SystemReadyHandlers -= value;
		}
	}

	private List<SystemReadyEventHandler> m_ReadyHandlers = new List<SystemReadyEventHandler>();

	private List<CoherentUIView> m_Views = new List<CoherentUIView>();

	internal void AddView(CoherentUIView view)
	{
		m_Views.Add(view);
	}

	internal bool RemoveView(CoherentUIView view)
	{
		return m_Views.Remove(view);
	}

	public List<CoherentUIView> UIViews
	{
		get
		{
			return m_Views;
		}
	}

	public static CoherentUISystem Create()
	{
		if (GameObject.Find("CoherentUISystem") != null)
		{
			Debug.LogWarning("Unable to create CoherentUISystem because a GameObject with the same name already exists!");
			return null;
		}

		var go = new GameObject("CoherentUISystem");
		CoherentUISystem system = go.AddComponent<CoherentUISystem>();
		if (system != null && Debug.isDebugBuild)
		{
			system.DebuggerPort = 9999;
		}
		return system;
	}

	/// <summary>
	/// enable proxy support for loading web pages
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_EnableProxy = false;
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Proxy",
							Tooltip="Enables proxy support",
							IsStatic=true)]
	public bool EnableProxy
	{
		get {
			return m_EnableProxy;
		}
		set {
			m_EnableProxy = value;
		}
	}

	/// <summary>
	/// allow cookies
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_AllowCookies = true;
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Cookies",
							Tooltip="Enables support for cookies",
							IsStatic=true)]
	public bool AllowCookies
	{
		get {
			return m_AllowCookies;
		}
		set {
			m_AllowCookies = value;
		}
	}

	/// <summary>
	/// URL for storing persistent cookies
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_CookiesResource = "cookies.dat";
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Cookies file",
							Tooltip="The file where cookies will be saved",
							IsStatic=true)]
	public string CookiesResource
	{
		get {
			return m_CookiesResource;
		}
		set {
			m_CookiesResource = value;
		}
	}

	/// <summary>
	/// path for browser cache
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_CachePath = "cui_cache";
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Cache path",
							Tooltip="The folder where the navigation cache will be saved",
							IsStatic=true)]
	public string CachePath
	{
		get {
			return m_CachePath;
		}
		set {
			m_CachePath = value;
		}
	}

	/// <summary>
	/// path for HTML5 localStorage
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_HTML5LocalStoragePath = "cui_app_cache";
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Local storage path",
							Tooltip="The directory where the HTML5 local storage will be saved",
							IsStatic=true)]
	public string HTML5LocalStoragePath
	{
		get {
			return m_HTML5LocalStoragePath;
		}
		set {
			m_HTML5LocalStoragePath = value;
		}
	}

	/// <summary>
	/// disable fullscreen for plugins like Flash and Silverlight
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_ForceDisablePluginFullscreen = true;
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Disable fullscreen plugins",
							Tooltip="All plugins will have their fullscreen support disabled",
							IsStatic=true)]
	public bool ForceDisablePluginFullscreen
	{
		get {
			return m_ForceDisablePluginFullscreen;
		}
		set {
			m_ForceDisablePluginFullscreen = value;
		}
	}

	/// <summary>
	/// disable web security like cross-domain checks
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_DisableWebSecurity = false;
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Disable web security",
							Tooltip="Allows loading making HTTP requests from coui://",
							IsStatic=true)]
	public bool DisableWebSecurity
	{
		get {
			return m_DisableWebSecurity;
		}
		set {
			m_DisableWebSecurity = value;
		}
	}

	/// <summary>
	/// port for debugging Views, -1 to disable
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private int m_DebuggerPort = 9999;
	[CoherentExposePropertyStandalone(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Debugger port",
							Tooltip="The port where the system will listen for the debugger",
							IsStatic=true)]
	public int DebuggerPort
	{
		get {
			return m_DebuggerPort;
		}
		set {
			m_DebuggerPort = value;
		}
	}

	/// <summary>
	/// The main camera. Used for obtaining mouse position over the HUD and raycasting in the world.
	/// </summary>
	public Camera m_MainCamera = null;

	[HideInInspector]
	public bool DeviceSupportsSharedTextures = false;

	[HideInInspector]
	[SerializeField]
	private bool m_UseURLCache = true;
	/// <summary>
	/// Sets if the system should use the URL Cache. NOTE: This should almost always be enabled.
	/// Disable it if you already set the URL cache yourself for the app
	/// </summary>
	/// <value>
	/// If to set the cache
	/// </value>
	[CoherentExposePropertyiOS(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "URL cache",
							Tooltip="Use the URL cache of the device",
							IsStatic=true)]
	public bool UseURLCache
	{
		get {
			return m_UseURLCache;
		}
		set {
			m_UseURLCache = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private int m_MemoryCacheSize = 4*1024*1024;
	/// <summary>
	/// Sets the in-memory size of the URL cache
	/// </summary>
	/// <value>
	/// The maximum size of the in-memory cache
	/// </value>
	[CoherentExposePropertyiOS(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Memory cache size",
							Tooltip="The maximum size of the in-memory cache",
							IsStatic=true)]
	public int MemoryCacheSize
	{
		get {
			return m_MemoryCacheSize;
		}
		set {
			m_MemoryCacheSize = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private int m_DiskCacheSize = 32*1024*1024;
	/// <summary>
	/// Sets the on-disk size of the URL cache
	/// </summary>
	/// <value>
	/// The maximum size of the on-disk cache
	/// </value>
	[CoherentExposePropertyiOS(Category = CoherentExposePropertyInfo.FoldoutType.General,
							PrettyName = "Disk cache size",
							Tooltip="The maximum size of the disk cache",
							IsStatic=true)]
	public int DiskCacheSize
	{
		get {
			return m_DiskCacheSize;
		}
		set {
			m_DiskCacheSize = value;
		}
	}

	private void StartActivator()
	{
		var activation = new System.Diagnostics.Process();
		string hostDir = Path.Combine(Application.dataPath,
			CoherentUILibrary.GetHostDirectory());
		activation.StartInfo.Arguments = string.Format("--unity3d --host \"{0}\"", hostDir);
		string activator = null;
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsPlayer:
			activator = Path.Combine(Application.dataPath, "Activator.exe");
			break;
		case RuntimePlatform.OSXPlayer:
			activator = Path.Combine(Application.dataPath,
				"Libraries/Activator.app/Contents/MacOS/Activator");
			break;
		case RuntimePlatform.WindowsEditor:
			activator = Path.Combine(Application.dataPath,
				"Libraries/CoherentUI/Activator/Activator.exe");
			break;
		case RuntimePlatform.OSXEditor:
			activator = Path.Combine(Application.dataPath,
				"Libraries/CoherentUI/Activator/Activator.app/Contents/MacOS/Activator");
			break;
		default:
			// set the path for Linux Player
			activator = Path.Combine(Application.dataPath, "Activator");
			break;
		}
		activation.StartInfo.FileName = activator;

		int activationCode = 0;
		if (File.Exists(activation.StartInfo.FileName) || Directory.Exists(activation.StartInfo.FileName))
		{
			activation.Start();
			activation.WaitForExit();
			activationCode = activation.ExitCode;
		}

		if (activationCode != 0)
		{
			Debug.LogError("Could not activate Coherent Browser when running built game. Please contact support@coherent-labs.com and attach the \"{0}/Coherent_Browser.log\" file");
		}
	}

	// Use this for initialization
	void Start () {
		#if COHERENT_UI_EVALUATION_UNITY3D
			StartActivator();
		#endif

		if (SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11")
				|| SystemInfo.operatingSystem.Contains("Mac"))
		{
			DeviceSupportsSharedTextures = true;
		}

		if (m_UISystem == null)
		{
			m_SystemListener = (SystemListenerFactoryFunc != null)? SystemListenerFactoryFunc(this.OnSystemReady) : new SystemListener(this.OnSystemReady);
			if (FileHandlerFactoryFunc != null)
			{
				m_FileHandler = FileHandlerFactoryFunc();
			}
			#if !UNITY_ANDROID || UNITY_EDITOR

			if (m_FileHandler == null)
			{
				Debug.LogWarning("Unable to create file handler using factory function! Falling back to default handler.");
				m_FileHandler = new UnityFileHandler();
			}
			#endif

			#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE || COHERENT_UNITY_UNSUPPORTED_PLATFORM
			ContextSettings settings = new ContextSettings()
			{
				EnableProxy = this.EnableProxy,
				AllowCookies = this.AllowCookies,
				CookiesResource = "file:///" + Application.persistentDataPath + '/' + this.CookiesResource,
				CachePath = Path.Combine(Application.temporaryCachePath, this.CachePath),
				HTML5LocalStoragePath = Path.Combine(Application.temporaryCachePath, this.HTML5LocalStoragePath),
				ForceDisablePluginFullscreen = this.ForceDisablePluginFullscreen,
				DisableWebSecurity = this.DisableWebSecurity,
				DebuggerPort = this.DebuggerPort,
			};
			#elif UNITY_IPHONE || UNITY_ANDROID
			ContextSettings settings = new ContextSettings() {
				iOS_UseURLCache = m_UseURLCache,
				iOS_URLMemoryCacheSize = (uint)m_MemoryCacheSize,
				iOS_URLDiskCacheSize = (uint)m_DiskCacheSize,
			};
			int sdkVersion = Coherent.UI.Mobile.Versioning.SDKVersion;
			#endif

			if (string.IsNullOrEmpty(Coherent.UI.License.COHERENT_KEY))
			{
				throw new System.ApplicationException("You must supply a license key to start Coherent Browser! Follow the instructions in the manual for editing the License.cs file.");
			}

			m_UISystem = CoherentUILibrary.CreateViewContext(
				settings, m_SystemListener, m_FileHandler);
			if (m_UISystem == null)
			{
				throw new System.ApplicationException("Creating a ViewContext"
					+ " failed!");
			}
			Debug.Log ("Coherent Browser system initialized..");
			#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
			CoherentUIViewRenderer.WakeRenderer(
				(byte)m_UISystem.GetContextId());
			#endif
		}
		m_StartTime = Time.realtimeSinceStartup;

		DontDestroyOnLoad(this.gameObject);
	}

	private void OnSystemReady()
	{
		if (SystemReadyHandlers != null)
		{
			SystemReadyHandlers();
		}
	}

	/// <summary>
	/// Determines whether this instance is ready.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is ready; otherwise, <c>false</c>.
	/// </returns>
	public bool IsReady() {
		return m_UISystem != null && m_SystemListener.IsReady;
	}

	/// <summary>
	/// Determines whether there is an focused Click-to-focus view
	/// </summary>
	/// <value>
	/// <c>true</c> if there is an focused Click-to-focus view; otherwise, <c>false</c>.
	/// </value>
	public bool HasFocusedView {
		get { return m_SystemHasFocusedView; }
	}

	public delegate void OnViewFocusedDelegate(bool focused);

	/// <summary>
	/// Occurs when a Click-to-focus view gains or loses focus
	/// </summary>
	public event OnViewFocusedDelegate OnViewFocused;

	private void SetViewFocused(bool focused)
	{
		m_SystemHasFocusedView = focused;
		if (OnViewFocused != null)
		{
			OnViewFocused(focused);
		}
	}

	private void TrackInputFocus() {
		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		if (m_MainCamera == null)
		{
			m_MainCamera = Camera.main;
			if (m_MainCamera == null)
			{
				return;
			}
		}

		bool isClick = Input.GetMouseButtonDown(0);
		if (!m_SystemHasFocusedView && !isClick)
		{
			// Do nothing if the left mouse button isn't clicked
			// (and there is no focused view; if there is, we need to track the mouse)
			return;
		}

		CoherentUIView cameraView = m_MainCamera.gameObject.GetComponent<CoherentUIView>();
		if (cameraView && cameraView.ClickToFocus)
		{
			var view = cameraView.View;
			if (view != null)
			{
				var normX = (Input.mousePosition.x / cameraView.Width);
				var normY = (1 - Input.mousePosition.y / cameraView.Height);

				normX = normX *
				cameraView.WidthToCamWidthRatio(m_MainCamera.pixelWidth);

				normY = 1 - ((1 - normY) *
				cameraView.HeightToCamHeightRatio(m_MainCamera.pixelHeight));

				if (normX >= 0 && normX <= 1 && normY >= 0 && normY <= 1)
				{
					view.IssueMouseOnUIQuery(normX, normY);
					view.FetchMouseOnUIQuery();
					if (view.IsMouseOnView())
					{
						if (isClick)
						{
							// Reset input processing for all views
							foreach (var item in m_Views)
							{
								item.ReceivesInput = false;
							}
							// Set input to the clicked view
							cameraView.ReceivesInput = true;
							SetViewFocused(true);
						}

						return;
					}
				}
			}
		}

		// Activate input processing for the view below the mouse cursor
		RaycastHit hitInfo;
		if (Physics.Raycast(m_MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
		{
			CoherentUIView viewComponent = hitInfo.collider.gameObject.GetComponent(typeof(CoherentUIView)) as CoherentUIView;

			if (viewComponent != null && viewComponent.ClickToFocus)
			{
				if (isClick)
				{
					// Reset input processing for all views
					foreach (var item in m_Views)
					{
						item.ReceivesInput = false;
					}
					// Set input to the clicked view
					viewComponent.ReceivesInput = true;
					SetViewFocused(true);
				}

				viewComponent.SetMousePosition(
					(int)(hitInfo.textureCoord.x * viewComponent.Width),
					(int)(hitInfo.textureCoord.y * viewComponent.Height));

				return;
			}
		}

		// If neither the HUD nor an object was clicked, clear the focus
		if (m_SystemHasFocusedView && isClick)
		{
			// Reset input processing for all views
			foreach (var item in m_Views)
			{
				item.ReceivesInput = false;
			}
			SetViewFocused(false);
		}
		#endif
	}

	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
		if(UnityEditor.EditorApplication.isCompiling)
		{
			OnAssemblyReload();
		}
#endif

		if (m_UISystem != null)
		{
			IsUpdating = true;

			m_UISystem.Update();
			if (m_ReadyHandlers.Count > 0)
			{
				foreach (var handler in m_ReadyHandlers)
				{
					handler();
				}
				m_ReadyHandlers.Clear();
			}

			TrackInputFocus();

#if UNITY_ANDROID && !UNITY_EDITOR
			foreach (CoherentUIView view in UIViews)
			{
				if (view == null || view.Listener == null || view.Listener.View == null)
				{
					continue;
				}

				if (m_TouchScreenKeyboard != null)
				{
					int lengthDiff = m_TouchScreenKeyboard.text.Length - m_TouchScreenKeyboardText.Length;
					if (lengthDiff != 0)
					{
						if (lengthDiff < 0)
						{
							for (int i = 0; i < -lengthDiff; ++i)
							{
								view.Listener.View.DispatchKeyEventInternal(1, 0x08); // Backspace
							}
						}
						else
						{
							for (int i = m_TouchScreenKeyboardText.Length; i < m_TouchScreenKeyboard.text.Length; ++i)
							{
								view.Listener.View.DispatchKeyEventInternal(1, (int)m_TouchScreenKeyboard.text[i]);
							}
						}

						m_TouchScreenKeyboardText = m_TouchScreenKeyboard.text;
					}
				}
			}
#endif
			IsUpdating = false;
		}
	}

	#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
	void MouseMovedViewUpdate(CoherentUIView view)
	{

		if (view.ReceivesInput && view != null && view.View != null)
		{
			if (view.MouseX != -1 && view.MouseY != -1)
			{
				m_MouseMoveEvent.X = view.MouseX;
				m_MouseMoveEvent.Y = view.MouseY;
			}
			else
			{
				CalculateScaledMouseCoordinates(ref m_MouseMoveEvent, view,
					true);
			}

			view.View.MouseEvent(m_MouseMoveEvent);
		}
	}
	#endif

	void LateUpdate() {
		#if UNITY_ANDROID || COHERENT_SIMULATE_MOBILE_IN_EDITOR || COHERENT_SIMULATE_MOBILE_IN_PLAYER
		CoherentUI.InputManager.PrepareForNextFrame();
		#elif UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		// Check if the mouse moved
		Vector2 mousePosition = Input.mousePosition;
		if (mousePosition != m_LastMousePosition)
		{
			if (m_MouseMoveEvent != null && m_Views != null)
			{
				Coherent.UI.InputManager.GenerateMouseMoveEvent(
					ref m_MouseMoveEvent);

				//Cache the initial mouse X and Y
				int mouseX = m_MouseMoveEvent.X;
				int mouseY = m_MouseMoveEvent.Y;
				foreach (var item in m_Views)
				{
					CoherentUIView view = item;

					MouseMovedViewUpdate(view);

					//Since we are using a single mouse event for all
					//of the views and the MouseMovedViewUpdate
					//mutates the event's X and Y per view, we have to
					//reset the X and Y for the next view

					m_MouseMoveEvent.X = mouseX;
					m_MouseMoveEvent.Y = mouseY;
				}
			}

			m_LastMousePosition = mousePosition;
		}
		#endif
	}

	void OnApplicationQuit() {
		if (m_UISystem != null)
		{
			if(UISystemDestroying != null) {
				UISystemDestroying();
			}
			CoherentUILibrary.DestroyViewContext(m_UISystem);
			m_UISystem = null;
			m_SystemListener.Dispose();
		}
	}

	public void OnAssemblyReload()
	{
		if(m_UISystem != null)
		{
			for(int i = m_Views.Count - 1; i >= 0; --i)
			{
				m_Views[i].DestroyView();
			}

			Debug.LogWarning("Assembly reload detected. UI System will shut down.");
			OnApplicationQuit();
		}
	}

	private bool IsPointInsideAnyView(int x, int y)
	{
		if (m_Views == null)
		{
			return false;
		}

		for (int i = 0; i < m_Views.Count; ++i)
		{
			var view = m_Views[i];

			if (view.InputState ==
				CoherentUIView.CoherentViewInputState.TakeNone)
			{
				continue;
			}

			if (x >= view.XPos && x <= view.XPos + view.Width &&
				y >= view.YPos && y <= view.YPos + view.Height)
			{
				return true;
			}
		}

		return false;
	}

	public virtual void OnGUI()
	{
		if (m_Views == null)
		{
			return;
		}

		#if UNITY_ANDROID || COHERENT_SIMULATE_MOBILE_IN_EDITOR || COHERENT_SIMULATE_MOBILE_IN_PLAYER
		if (Event.current.isMouse && !IsPointInsideAnyView(
				(int)Event.current.mousePosition.x,
				(int)Event.current.mousePosition.y))
		{
			var evt = Event.current;
			int x = (int)evt.mousePosition.x;
			int y = (int)evt.mousePosition.y;

			switch (evt.type)
			{
			case EventType.MouseDown:
				CoherentUI.InputManager.ProcessTouchEvent(
					(int)TouchPhase.Began, evt.button, x, y);
				break;
			case EventType.MouseUp:
				CoherentUI.InputManager.ProcessTouchEvent(
					(int)TouchPhase.Ended, evt.button, x, y);
				break;
			case EventType.MouseDrag:
				CoherentUI.InputManager.ProcessTouchEvent(
					(int)TouchPhase.Moved, evt.button, x, y);
				break;
			}
		}
		#endif

		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		MouseEventData mouseEventData = null;
		KeyEventData keyEventData = null;
		KeyEventData keyEventDataChar = null;

		switch (Event.current.type)
		{
		case EventType.MouseDown:
			{
				mouseEventData = Coherent.UI.InputManager.ProcessMouseEvent(Event.current);
				mouseEventData.Type = MouseEventData.EventType.MouseDown;
			}
			break;
		case EventType.MouseUp:
			{
				mouseEventData = Coherent.UI.InputManager.ProcessMouseEvent(Event.current);
				mouseEventData.Type = MouseEventData.EventType.MouseUp;
			}
			break;
		case EventType.ScrollWheel:
			{
				if (Event.current.delta.SqrMagnitude() > 0)
				{
					mouseEventData = Coherent.UI.InputManager.ProcessMouseEvent(Event.current);
					mouseEventData.Type = MouseEventData.EventType.MouseWheel;
				}
			}
			break;
		case EventType.KeyDown:
			if (Event.current.keyCode != KeyCode.None)
			{
				keyEventData = Coherent.UI.InputManager.ProcessKeyEvent(Event.current);
				keyEventData.Type = KeyEventData.EventType.KeyDown;

				if (keyEventData.KeyCode == 0)
				{
					keyEventData = null;
				}
			}
			if (Event.current.character != 0)
			{
				keyEventDataChar = Coherent.UI.InputManager.ProcessCharEvent(Event.current);

				if(keyEventDataChar.KeyCode == 10)
				{
					keyEventDataChar.KeyCode = 13;
				}

				// Enter key character is not reported correctly by
				// Unity3D 4.5 for Linux, so it has to be changed in this case
				#if (!COHERENT_UNITY_PRE_4_5 && UNITY_STANDALONE_LINUX)
					if(keyEventData != null && keyEventData.KeyCode == 13)
					{
						keyEventDataChar.KeyCode = 13;
					}
				#endif
			}
			break;
		case EventType.KeyUp:
			{
				keyEventData = Coherent.UI.InputManager.ProcessKeyEvent(Event.current);
				keyEventData.Type = KeyEventData.EventType.KeyUp;

				if (keyEventData.KeyCode == 0)
				{
					keyEventData = null;
				}
			}
			break;
		}

		foreach (var item in m_Views) {
			var view = item.View;
			#if COHERENT_SIMULATE_MOBILE_IN_EDITOR || COHERENT_SIMULATE_MOBILE_IN_PLAYER
			bool forwardInput = (item.InputState !=
				CoherentUIView.CoherentViewInputState.TakeNone);
			#else
			bool forwardInput = item.ReceivesInput;
			#endif
			if (forwardInput && view != null)
			{
				if (mouseEventData != null)
				{
					if (item.MouseX != -1 && item.MouseY != -1)
					{
						mouseEventData.X = item.MouseX;
						mouseEventData.Y = item.MouseY;
					}

					//Check if there is a camera attached to the view's parent
					//Views attached on surfaces do not have such camera.
					var isOnSurface = (item.gameObject.GetComponent<Camera>() == null);

					if (!isOnSurface)
					{
						CalculateScaledMouseCoordinates(ref mouseEventData,
						item,
						false);
					}

					view.MouseEvent(mouseEventData);

					//Note: The Event.current.Use() marks the event as used,
					//and makes the other GUI elements to ignore it, but does
					//not destroy the event immediately
					Event.current.Use();
				}
				if (keyEventData != null)
				{
					view.KeyEvent(keyEventData);
					Event.current.Use();
				}
				if (keyEventDataChar != null)
				{
					view.KeyEvent(keyEventDataChar);
					Event.current.Use();
				}
			}
		}
		#endif
	}

	#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
	private void CalculateScaledMouseCoordinates(ref MouseEventData data,
		CoherentUIView view,
		bool invertY)
	{
		float camWidth;
		float camHeight;

		var isOnSurface = (view.gameObject.GetComponent<Camera>() == null);

		if (!isOnSurface)
		{
			Camera cameraComponent = view.gameObject.GetComponent<Camera>();
			camWidth = cameraComponent.pixelWidth;
			camHeight = cameraComponent.pixelHeight;
		}
		else
		{
			var surfaceCameraObj =
				view.gameObject.transform.Find
					("CoherentRenderingCamera" + view.View.GetId());
			if (surfaceCameraObj != null && surfaceCameraObj.GetComponent<Camera>() != null)
			{
				Camera surfaceCameraComponent = surfaceCameraObj.GetComponent<Camera>();
				camWidth = surfaceCameraComponent.pixelWidth;
				camHeight = surfaceCameraComponent.pixelHeight;
			}
			else
			{
				return;
			}
		}

		float factorX = view.WidthToCamWidthRatio(camWidth);
		float factorY = view.HeightToCamHeightRatio(camHeight);

		float y = (invertY)? (camHeight - data.Y) : data.Y;

		data.X = (int)(data.X * factorX);
		data.Y = (int)(y * factorY);
	}
	#endif

	/// <summary>
	/// Gets the user interface system.
	/// </summary>
	/// <value>
	/// The user interface system.
	/// </value>
	public ViewContext UISystem
	{
		get
		{
			return m_UISystem;
		}
	}

	public float StartTime
	{
		get
		{
			return m_StartTime;
		}
	}
	private float m_StartTime;
}
