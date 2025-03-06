# Window Manager

**CAUTION:** <u>If you want to launch sample code from this repository, install Extentject and DOTween plugins
first!</u>

The **Window Manager** is the simple implementation of the game window manager. It supports necessary and sufficient
functionality for managing windows and their interactions, such as:

* **Uniqueness** - when a window is displayed exclusively, while no other windows can be opened;
* **Overlap** – when opening a window deactivates other open windows;
* **Layers** - the groups of windows that can be configured to appear on top of each other.

## How to install
Select one of the following methods:

1. From Unity package.<br/>Select latest release from the https://github.com/vcow/lib-window-manager/releases and download __window-manager.unitypackage__ from Assets section.

2. From Git URL.<br/>Go to __Package Manager__, press __+__ in the top left of window and select __Install package from git URL__. Enter the URL below:
```
https://github.com/vcow/lib-window-manager.git#upm
```
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;or
```
https://github.com/vcow/lib-window-manager.git#4.1.1
```
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if you want to install exactly 4.1.1 version.

3. From OpenUPM.<br/>Go to __Edit -> Project Settings -> Package Manager__ and add next scoked registry:
* __Name__: package.openupm.com
* __URL__: https://package.openupm.com
* __Scope(s)__: com.vcow.window-manager

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Press __Save__, then go to __Package Manager__ and install __Scene Select Tool__ from the __My Registries -> package.openupm.com__ section.

4. Add to the ```manifest.json```.<br/>Open ```mainfest.json``` and add next string to the ```dependencies``` section:
```
{
  "dependencies": {
    "com.vcow.window-manager": "https://github.com/vcow/lib-window-manager.git#upm",
    ...
  }
}
```
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;or
```
{
  "dependencies": {
    "com.vcow.window-manager": "https://github.com/vcow/lib-window-manager.git#4.1.1",
    ...
  }
}
```
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;if you want to install exactly 4.1.1 version.

## How to use WindowManager

After installing the Window Manager, you need to specify Manager settings - the instance of WindowManagerSettings. These
settings provide the list of **windows groups**, the sorting order from which window canvas is starts (+100 for each
group), and a set of **windows prefabs libraries**.<br/>
You can create this settings in the ```MonoBehaviour``` or ```ScriptableObject``` to have feasibility to setup these
properties:

```csharp
public class WindowManagerSettingsProvider : ScriptableObject
{
    [SerializeField] private string[] _groupHierarchy;
    [SerializeField] private int _startCanvasSortingOrder;
    [SerializeField] private WindowsPrefabLibrary[] _windowLibraries;
    
    public WindowManagerSettings WindowManagerSettingsFactory() => new WindowManagerSettings
    {
        GroupHierarchy = _groupHierarchy,
        WindowLibraries = _windowLibraries,
        StartCanvasSortingOrder = _startCanvasSortingOrder
    };
}
```

#### Window groups

You can set up the level at which the window will appear. Send to the Window Manager a list of group names. Windows from
groups at the end of the list will be displayed on top of windows from groups at the beginning of the list.

#### Windows prefab library

It is the instance of ```WindowsPrefabLibrary``` ```ScriptableObject``` which contains the list of the ```Windows```
prefabs. You can create multiple windows prefabs libraries to group your windows into different sets. All libraries must
be added to the **WindowLibraries** list of the **WindowManagerSettings**.

### Create Window Manager

In its simplest form, you can use the Window Manager as a singleton, writing, for example, a script like this:

```csharp
public class WindowManager : MonoBehaviour
{
    private static UIWindowManager.WindowManager _windowManager;
    
    [SerializeField] private WindowManagerSettingsProvider _settingsProvider;
    
    public static IWindowManager Instance => _windowManager;
    
    private void Start()
    {
        Assert.IsNull(_windowManager, "WindowManager singleton must be created once.");
        _windowManager = new UIWindowManager.WindowManager(_settingsProvider.WindowManagerSettingsFactory());
        DontDestroyOnLoad(gameObject);
    }
    
    private void OnDestroy()
    {
        _windowManager.Dispose();
        _windowManager = null;
    }
}
```

Then you can access the Window Manager with code like this:

```csharp
    ...
    WindowManager.Instance.ShowWindow("SomeWindow");
    ...
```

## Window

After you create and adjust your **WindowManager** create the window.<br/>
All of the windows must be inherited from the ```Window<TDerived, TResult>``` generic. For modal windows there is a
simplified generic ```PopupWindowBase<TResult>```. If your window doesn't return any result, it doesn't matter what type
of result you specify.<br/>
The implementation of all window logic, including its activation and deactivation, is the responsibility of the
programmer.<br/>

The simplest window code looks like this:

```csharp
public class SimpleWindow : Window<SimpleWindow, DialogButtonType>
{
    public const string ID = nameof(SimpleWindow);
    
    public override void Activate(bool immediately = false)
    {
        State = WindowState.Active;
    }
    
    public override void Deactivate(bool immediately = false)
    {
        State = WindowState.Inactive;
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

Create the window prefab and add his reference to your **windows prefab library**.<br/>
After that you can call your window from code by accessing the WindowManager singleton:

```csharp
...
    public void OnOpenWindow()
    {
        WindowManager.Instance.ShowWindow(SimpleWindow.ID);
    }
    ...
```

See more details in <a href="https://raw.githack.com/vcow/lib-window-manager/master/docs/html/namespaces.html">
documentation</a>.

## Advanced use

### Window instance creation hook

If you want to access the window instance directly after it created to make additional initializations, you can set **instantiate window hook** as the second argument of the ```WindowManager``` constructor:

```csharp
    private void Start()
    {
        ...
        _windowManager = new UIWindowManager.WindowManager(_settingsProvider.WindowManagerSettingsFactory(), InstantiateWindowHook);
        ...
    }

    private void InstantiateWindowHook(IWindow window)
    {
        ...
    }
```

### Using with Zenject

Implement ```WindowManagerSettingsProvider``` as ```ScriptableObjectInstaller```:
```csharp
[CreateAssetMenu(fileName = "WindowManagerSettingsProvider", menuName = "Window Manager/Window Manager Settings Provider")]
public class WindowManagerSettingsProvider : ScriptableObjectInstaller<WindowManagerSettingsProvider>
{
    [FormerlySerializedAs("GroupHierarchy")] [SerializeField] private string[] _groupHierarchy;
    [FormerlySerializedAs("StartCanvasSortingOrder")] [SerializeField] private int _startCanvasSortingOrder;
    [FormerlySerializedAs("_windowProviders")] [SerializeField] private WindowsPrefabLibrary[] _windowLibraries;

    public override void InstallBindings()
    {
        Container.Bind<WindowManagerSettings>().FromMethod(WindowManagerSettingsFactory).AsTransient();
    }

    private WindowManagerSettings WindowManagerSettingsFactory() => new WindowManagerSettings
    {
        GroupHierarchy = _groupHierarchy,
        WindowLibraries = _windowLibraries,
        StartCanvasSortingOrder = _startCanvasSortingOrder
    };
}
```
Add the instance of the ```WindowManagerSettingsProvider``` ScriptableObject to the **ScriptableObjectInstallers** list of the **ProjectContext**. Override the ```InstallBindings()``` method as shown above. After that you will have ```WindowManagerSettings``` binding.

Bind ```IWindowManager``` to the ```UIWindowManager.WindowManager``` like this:
```csharp
public class GameInstaller : MonoInstaller<GameInstaller>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<UIWindowManager.WindowManager>().AsSingle()
            .WithArguments((UIWindowManager.WindowManager.InstantiateWindowHook)InstantiateWindowHook);
    }

    private void InstantiateWindowHook(IWindow window)
    {
        var instance = (Window)window;
        Container.InjectGameObject(instance.gameObject);
    }
}
```
The method ```InstantiateWindowHook()``` will provide dependency injection into the each window instance.<br/>
The ```UIWindowManager.WindowManager.Dispose()``` will be called automatically by binding via the method ```BindInterfacesTo<>()```.