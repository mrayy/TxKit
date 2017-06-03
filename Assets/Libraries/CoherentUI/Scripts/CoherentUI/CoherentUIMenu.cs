using UnityEngine;
using System.Collections.Generic;

using Coherent.UI.Binding;

namespace Coherent.UI
{
	/// <summary>
	/// Handler for button related events
	/// </summary>
	[System.Serializable]
	public class ButtonHandler
	{
		/// <summary>
		/// Target GameObject or MonoBehavior for the event. None if using the
		/// current GameObject.
		/// </summary>
		public Object Target;

		/// <summary>
		/// Method (or message) name to be called for the event
		/// </summary>
		public string Method;

		/// <summary>
		/// Whether Method specifies a method or a message to be called
		/// </summary>
		public bool IsMessage = true;

		/// <summary>
		/// Initializes the Handler delegate in the Start method.
		/// If you want to set a different delegate, you can set it from
		/// another component in the Awake method.
		/// </summary>
		/// <param name="owner">the menu that owns our button</param>
		public void Initialize(CoherentUIMenu owner)
		{
			if (Handler != null)
			{
				return;
			}
			if (IsMessage)
			{
				var TargetObject = (Target != null)? Target : owner.gameObject;
				var TargetBehaviour = TargetObject as GameObject;
				if (TargetBehaviour == null)
				{
					var error = string.Format("Target {0} is not a GameObject",
							Target.GetType().Name);
					Debug.LogError(error);
				}
				Handler = () =>
				{
					TargetBehaviour.SendMessage(Method);
				};
			}
			else
			{
				var TargetType = Target.GetType();
				var MethodInfo = TargetType.GetMethod(Method,
					System.Type.EmptyTypes);
				if (MethodInfo == null)
				{
					var error = string.Format(
						"Could not find method {0} in type {1}", Method,
						TargetType.Name);
					Debug.LogError(error);
				}
				Handler = () =>
				{
					MethodInfo.Invoke(Target, new object[0]);
				};
			}
		}

		/// <summary>
		/// Invokes the handler
		/// </summary>
		public void Invoke()
		{
			Handler();
		}

		/// <summary>
		/// Delegate signature for the event handler
		/// </summary>
		public delegate void Invoker();


		/// <summary>
		/// The delegate that is executed when the event of this handler is
		/// triggered
		/// </summary>
		public Invoker Handler;
	}

	/// <summary>
	/// Describes a button in a CoherentUIMenu instance
	/// </summary>
	[System.Serializable]
	[CoherentType(PropertyBindingFlags.Explicit)]
	public class Button
	{
		/// <summary>
		/// The label of the button
		/// </summary>
		[CoherentProperty]
		public string Label;

		/// <summary>
		/// Where the button is enabled or disabled.
		/// Currently this property cannot be changed
		/// </summary>
		[CoherentProperty]
		public bool Disabled;

		/// <summary>
		/// Specifies handler for the click event of the button
		/// </summary>
		public ButtonHandler Click;

		/// <summary>
		/// Initialize this button
		/// </summary>
		/// <param name="owner">the menu which contains this button</param>
		public void Initialize(CoherentUIMenu owner)
		{
			Click.Initialize(owner);
		}
	}
}

[CoherentType(PropertyBindingFlags.Explicit)]
public class CoherentUIMenu : MonoBehaviour
{
	[SerializeField]
	private CoherentUIView View;

	/// <summary>
	/// The unique id of this menu in its view
	/// </summary>
	[CoherentProperty("Id")]
	[SerializeField]
	public string MenuID;

	/// <summary>
	/// The id of the parent element for the menu.
	/// You can control the position of the menu by setting the position of
	/// its parent element.
	/// </summary>
	[CoherentProperty("Parent")]
	[SerializeField]
	public string ParentID;

	/// <summary>
	/// Where the menu is initialy visible or not.
	/// </summary>
	[CoherentProperty]
	[SerializeField]
	public bool Visible = true;

	/// <summary>
	/// The buttons of this menu
	/// </summary>
	[CoherentProperty]
	[SerializeField]
	public Coherent.UI.Button[] Buttons;

	private static HashSet<uint> m_MenuReady = new HashSet<uint>();

	private delegate void MenuReadyDelegate();

	private event MenuReadyDelegate MenuReady;

	private void Start()
	{
		if (View == null)
		{
			View = GetComponent<CoherentUIView>();
		}
		foreach (var button in Buttons)
		{
			button.Initialize(this);
		}
		if (View.IsReadyForBindings)
		{
			SetupBindings();
		}
		else
		{
			View.Listener.ReadyForBindings += OnReadyForBindings;
		}
		View.Listener.BindingsReleased += OnBindingsReleased;
	}

	private void CreateMenu()
	{
		View.View.TriggerEvent("cui.CreateMenu", MenuID, this);
	}

	private void SetupBindings()
	{
		var view = View.View;
		if (m_MenuReady.Contains(view.GetId()))
		{
			CreateMenu();
		}
		else
		{
			MenuReady += this.CreateMenu;
		}

		view.RegisterForEvent("cui.MenuReady", (System.Action)OnMenuReady);

		var onClick = (System.Action<string, string>)this.OnButtonClicked;
		view.RegisterForEvent("cui.MenuButtonClicked", onClick);
	}

	private void OnReadyForBindings(int frameId, string path, bool isMainFrame)
	{
		if (isMainFrame)
		{
			SetupBindings();
		}
	}

	private void OnMenuReady()
	{
		m_MenuReady.Add(View.View.GetId());
		if (MenuReady != null)
		{
			MenuReady();
		}
	}

	private void OnBindingsReleased(int frameId, string path, bool isMainFrame)
	{
		if (isMainFrame)
		{
			m_MenuReady.Remove(View.View.GetId());
		}
	}

	private void OnButtonClicked(string id, string label)
	{
		if (MenuID == id)
		{
			foreach (var button in Buttons)
			{
				if (button.Label == label)
				{
					button.Click.Invoke();
					return;
				}
			}
		}
	}

	/// <summary>
	/// Show this menu
	/// </summary>
	public void Show()
	{
		var view = View.View;
		if (view != null && m_MenuReady.Contains(View.View.GetId()))
		{
			View.View.TriggerEvent("cui.ShowMenu", MenuID);
			Visible = true;
		}
		else
		{
			MenuReady += this.Show;
		}
	}

	/// <summary>
	/// Hide this menu
	/// </summary>
	public void Hide()
	{
		var view = View.View;
		if (view != null && m_MenuReady.Contains(View.View.GetId()))
		{
			View.View.TriggerEvent("cui.HideMenu", MenuID);
			Visible = true;
		}
		else
		{
			MenuReady += this.Hide;
		}
	}
}
