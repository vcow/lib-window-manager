# Window Manager
**CAUTION:** <u>If you want to launch sample code from this repository, install Extentject and DOTween plugins first!</u>

The **Window Manager** is the basis of a simple window manager in the game. It supports necessary and sufficient functionality for managing windows and their interactions, such as:
* **Uniqueness** - when a window is displayed exclusively, while no other windows can be opened;
* **Overlap** – when opening a window deactivates other open windows;
* **Layers** - the groups of windows that can be configured to appear on top of each other.

### ScreenLocker extension
This tool is a simple manager of lock screens that are used to block user input and hide the transition process when the game is, for example, switching between scenes or waiting for some asynchronous process.

## Installation
You can download and install <code>window-manager.unitypackage</code> from this repository or add the Window Manager base from **Github** as a dependency.

### Github
Go to the <code>manifest.json</code> and in the section <code>dependencies</code> add next dependencies:
```
  "dependencies": {
    "vcow.base.activatable": "https://github.com/vcow/lib-logicality.git?path=/Assets/Scripts/Base/Activatable#3.0.1",
    "vcow.base.window-manager": "https://github.com/vcow/lib-window-manager.git?path=/Assets/Scripts/Base/WindowManager#3.0.1",
    "vcow.helpers.touch-helper": "https://github.com/vcow/lib-touch-helper.git?path=/Assets/Scripts/Helpers/TouchHelper#2.0.1",
    ...
  }
```

## How to use WindowManager
After installing the Window Manager base, you need to inherit your own WindowManager component from it and create a GameObject with this component. This GameObject will be your WindowManager, which should be implemented as a singleton.<br/>
It's doesn't matter how you implemented your singleton. In the sample project from this repository  that was, for example, dependency injection.
```csharp
Container.Bind<IWindowManager>().FromComponentInNewPrefabResource(@"WindowManager").AsSingle();
```
You can write your own singleton component like that:
```csharp
using Base.WindowManager;

public sealed class WindowManager : WindowManagerBase
{
	protected override int StartCanvasSortingOrder => 100;

	public static IWindowManager Instance { get; private set; }

	protected override void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);

		base.Awake();
	}
}
```
In your WindowManager's inspector, you can see two lists: **GroupHierarchy** and **WindowProviders**.<br/>
**GroupHierarchy** is a list of window group names. The windows in the group from the top entry will appear below the windows from the group from the bottom entry.

#### WindowProvider
WindowProvider is a list of references to window prefabs. If your game has many windows, you can split them into several WindowProviders for ease of use.<br/>
To create WindowProvider inherit a new class from the <code>Base.WindowManager.WindowProviderBase</code>:
```csharp
[CreateAssetMenu(fileName = "CommonWindowProvider", menuName = "Window Manager/Common Window Provider")]
public class CommonWindowProvider : WindowProviderBase
{
    [SerializeField] private Window[] _windows;

    public override IReadOnlyList<Window> Windows => _windows;
}
```
<u>Add all of your WindowProviders to the **WindowProviders** list of your WindowManager.</u>

After you create and adjust your WindowManager create the window.<br/>
All of the windows must be inherited from the <code>Window&lt;TDerived, TResult></code> generic. For modal windows there is a simplified generic <code>PopupWindowBase&lt;TResult></code>. If your window doesn't return any result, it doesn't matter what type of result you specify.<br/>
The implementation of all window logic, including its activation and deactivation, is the responsibility of the programmer.<br/>

The simplest window code looks like this:
```csharp
using Base.Activatable;
using Base.WindowManager;
using Base.WindowManager.Template;

public class SimpleWindow : Window<SimpleWindow, DialogButtonType>
{
	public const string ID = nameof(SimpleWindow);

	public override void Activate(bool immediately = false)
	{
		ActivatableState = ActivatableState.Active;
	}

	public override void Deactivate(bool immediately = false)
	{
		ActivatableState = ActivatableState.Inactive;
	}

	public override void SetArgs(object[] args)
	{
	}

	public void OnClose()
	{
		Close();
	}

	public override string WindowId => ID;
}
```
Create the window prefab and add his reference to your WindowProvider.<br/>
After that you can call your window from code by accessing the WindowManager singleton:
```csharp
...
	public void OnOpenWindow()
	{
		WindowManager.Instance.ShowWindow(SimpleWindow.ID);
	}
...
```
See more details in <a href="https://raw.githack.com/vcow/lib-window-manager/master/docs/html/namespaces.html">documentation</a>.

## How to use ScreenLockerManager
ScreenLockerManagerBase is a base class from which you need to derive your own implementation. In the simplest case, this is a class with a constructor that receives a list of screen blockers.<br/>
Next, you should figure out how to pass the list of blocker prefabs to the Manager. In the example code in this repository you can see how this is implemented using dependency injection. Or you can use your own singleton, like this:
```csharp
using System.Collections.Generic;
using Base.WindowManager.Extensions.ScreenLockerExtension;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScreenLockerManager : MonoBehaviour
{
	private class ScreenLockerManagerImpl : ScreenLockerManagerBase
	{
		public ScreenLockerManagerImpl(IEnumerable<ScreenLockerBase> screenLockers) : base(screenLockers)
		{
		}
	}

	[SerializeField] private List<ScreenLockerBase> _lockers;
	
	public static IScreenLockerManager Instance { get; private set; }

	private void Awake()
	{
		Instance = new ScreenLockerManagerImpl(_lockers);
		DontDestroyOnLoad(gameObject);
	}
}
```
In both cases you get the list of lockers in your Inspector where you can add the locker prefab references.

Implement the screen locker components. Screen locker must be derived from the <code>ScreenLocker&lt;TDerived></code> generic. The simplest code of screen locker looks like this:
```csharp
using Base.Activatable;
using Base.WindowManager.Extensions.ScreenLockerExtension;

public class BusyScreenLocker : ScreenLocker<BusyScreenLocker>
{
	public override void Activate(bool immediately = false)
	{
		ActivatableState = ActivatableState.Active;
	}

	public override void Deactivate(bool immediately = false)
	{
		ActivatableState = ActivatableState.Inactive;
	}

	public override bool Force()
	{
		return true;
	}

	public override LockerType LockerType => LockerType.BusyWait;
}
```
Create screen locker prefab with that component and add their reference to the lockers list in your ScreenLockerManager.<br/>
Now you can lock and unlock the screen with your locker by calling <code>Lock()</code> and <code>Unlock()</code> methods of the ScreenLockerManager:
```csharp
...
	public void OnLockScreen()
	{
		ScreenLockerManager.Instance.Lock(LockerType.BusyWait, () => StartCoroutine(UnlockScreenRoutine()));
	}

	private IEnumerator UnlockScreenRoutine()
	{
		yield return new WaitForSeconds(3);
		ScreenLockerManager.Instance.Unlock(type => Debug.LogFormat("{0} unlocked", type));
	}
...
```
See more details in <a href="https://raw.githack.com/vcow/lib-window-manager/master/docs/html/namespaces.html">documentation</a>.

## Scene Select Tool (bonus)
A convenient utility for quickly switching between scenes in the Unity Editor. Look at **Tools -> Scene Select Tool**.
