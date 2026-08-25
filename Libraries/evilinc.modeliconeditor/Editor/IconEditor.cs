using Editor;
using Sandbox;
using System;
using System.Linq;

namespace GeneralGame.Editor;

/// <summary>
/// ���������� ��������� ��� �������� ������ ������� ��� ���������.
/// ��������� �������� ������, ��������, ����������� ������� ������ ������ � ��������� �����������.
/// </summary>
[Dock("Editor", "Model Icon Generator", "camera_alt")]
public class ModelIconGenerator : Widget
{
    // ����� ��� ����������
    private Scene _scene;
    private CameraComponent _camera;
    private SkinnedModelRenderer _modelRenderer;
    private AmbientLight _ambientLight;
    private DirectionalLight _sunLight;

    // ������� UI
    private SceneRenderingWidget _sceneWidget;
    private ModelDropWidget _modelDropWidget;
    private LineEdit _materialPathEdit;
    private LineEdit _widthEdit;
    private LineEdit _heightEdit;
    private LineEdit _outputPathEdit;
    private FloatSlider _cameraDistanceSlider;
    private FloatSlider _cameraFovSlider;
    private FloatSlider _modelYawSlider;
    private FloatSlider _modelPitchSlider;

    // �������� ������
    private float _modelYaw = 0f;
    private float _modelPitch = 0f;

    // ��������� ������
    private float _cameraYaw = 45f;
    private float _cameraPitch = 20f;
    private float _cameraDistance = 100f;
    private Vector3 _cameraTarget = Vector3.Zero;

    // ��������� ����
    private Vector2 _lastMousePos;
    private bool _isDragging = false;
    private bool _isPanning = false;

    // ������� �������
    private Model _currentModel;
    private Material _currentMaterial;
    private Color _backgroundColor = Color.Transparent;

    // ������
    private GameObject _prefabInstance;
    private PrefabDropWidget _prefabDropWidget;

    // ������
    private Widget _previewContainer;
    private Widget _previewWidget;
    private Label _previewSizeLabel;
    private int _previewWidth = 256;
    private int _previewHeight = 256;

    public ModelIconGenerator(Widget parent) : base(parent)
    {
        WindowTitle = "Model Icon Generator";
        MinimumSize = new Vector2(800, 600);
        SetSizeMode(SizeMode.CanGrow, SizeMode.CanGrow);

        CreateScene();
        CreateUI();
        UpdateCamera();
    }

    private void CreateScene()
    {
        _scene = Scene.CreateEditorScene();
        _scene.Name = "Model Icon Generator Scene";

        using (_scene.Push())
        {
            // ������
            {
                var go = new GameObject(true, "camera");
                _camera = go.AddComponent<CameraComponent>();
                _camera.BackgroundColor = _backgroundColor;
                _camera.FieldOfView = 50f;
                _camera.ZNear = 0.1f;
                _camera.ZFar = 10000f;
                _camera.Enabled = true;
            }

            // Ambient light
            {
                var go = new GameObject(true, "ambient");
                _ambientLight = go.AddComponent<AmbientLight>();
                _ambientLight.Color = Color.White * 0.3f;
                _ambientLight.Enabled = true;
            }

            // Directional light (sun)
            {
                var go = new GameObject(true, "sun");
                _sunLight = go.AddComponent<DirectionalLight>();
                _sunLight.Shadows = true;
                _sunLight.WorldRotation = new Angles(50, 45, 0);
                _sunLight.LightColor = Color.White * 0.8f;
                _sunLight.Enabled = true;
            }

            // Secondary light
            {
                var go = new GameObject(true, "fill_light");
                var light = go.AddComponent<DirectionalLight>();
                light.Shadows = false;
                light.WorldRotation = new Angles(30, -135, 0);
                light.LightColor = Color.Cyan * 0.2f;
                light.Enabled = true;
            }

            // Envmap
            {
                var go = new GameObject(true, "envmap");
                var envmap = go.AddComponent<EnvmapProbe>();
                envmap.Texture = Texture.Load("textures/cubemaps/default2.vtex");
                envmap.Bounds = BBox.FromPositionAndSize(Vector3.Zero, 100000);
            }

            // Model (������ ����������)
            {
                var go = new GameObject(true, "model");
                _modelRenderer = go.AddComponent<SkinnedModelRenderer>();
                _modelRenderer.Enabled = true;
            }
        }
    }

    private void CreateUI()
    {
        Layout = Layout.Column();
        Layout.Margin = 8;
        Layout.Spacing = 8;

        // ������� ������ � �����������
        var topPanel = Layout.Add(new Widget(this));
        topPanel.Layout = Layout.Row();
        topPanel.Layout.Spacing = 8;
        topPanel.FixedHeight = 32;

        // ����� ������ (drag-drop �� ����� ��� Asset Browser)
        topPanel.Layout.Add(new Label("Model:", this));
        _modelDropWidget = topPanel.Layout.Add(new ModelDropWidget(this));
        _modelDropWidget.FixedWidth = 200;
        _modelDropWidget.FixedHeight = 26;
        _modelDropWidget.OnModelChanged = (path) => LoadModel(path);

        // ����� ������� (drag-drop)
        topPanel.Layout.Add(new Label("Prefab:", this));
        _prefabDropWidget = topPanel.Layout.Add(new PrefabDropWidget(this));
        _prefabDropWidget.FixedWidth = 200;
        _prefabDropWidget.FixedHeight = 26;
        _prefabDropWidget.OnPrefabChanged = (path) => LoadPrefab(path);

        // ����� ���������
        topPanel.Layout.Add(new Label("Material:", this));
        _materialPathEdit = topPanel.Layout.Add(new LineEdit(this));
        _materialPathEdit.PlaceholderText = "materials/example.vmat";
        _materialPathEdit.FixedWidth = 200;
        _materialPathEdit.ReturnPressed += OnMaterialPathChanged;

        var materialBrowseBtn = topPanel.Layout.Add(new Button("...", this));
        materialBrowseBtn.FixedWidth = 30;
        materialBrowseBtn.Clicked += BrowseMaterial;

        topPanel.Layout.AddStretchCell();

        // ������ ������ ��������
        var settingsPanel = Layout.Add(new Widget(this));
        settingsPanel.Layout = Layout.Row();
        settingsPanel.Layout.Spacing = 8;
        settingsPanel.FixedHeight = 32;

        // ���������� (������ x ������)
        settingsPanel.Layout.Add(new Label("Size:", this));
        _widthEdit = settingsPanel.Layout.Add(new LineEdit(this));
        _widthEdit.Text = "256";
        _widthEdit.FixedWidth = 50;
        settingsPanel.Layout.Add(new Label("x", this));
        _heightEdit = settingsPanel.Layout.Add(new LineEdit(this));
        _heightEdit.Text = "256";
        _heightEdit.FixedWidth = 50;

        // FOV ������
        settingsPanel.Layout.Add(new Label("FOV:", this));
        _cameraFovSlider = settingsPanel.Layout.Add(new FloatSlider(this));
        _cameraFovSlider.Minimum = 10;
        _cameraFovSlider.Maximum = 120;
        _cameraFovSlider.Value = 50;
        _cameraFovSlider.FixedWidth = 80;
        _cameraFovSlider.OnValueEdited = () => { if (_camera.IsValid()) _camera.FieldOfView = _cameraFovSlider.Value; };

        // ��������� ������
        settingsPanel.Layout.Add(new Label("Distance:", this));
        _cameraDistanceSlider = settingsPanel.Layout.Add(new FloatSlider(this));
        _cameraDistanceSlider.Minimum = 10;
        _cameraDistanceSlider.Maximum = 1000;
        _cameraDistanceSlider.Value = 100;
        _cameraDistanceSlider.FixedWidth = 100;
        _cameraDistanceSlider.OnValueEdited = () => { _cameraDistance = _cameraDistanceSlider.Value; UpdateCamera(); };

        // �������� ������ (Yaw - �������������)
        settingsPanel.Layout.Add(new Label("Yaw:", this));
        _modelYawSlider = settingsPanel.Layout.Add(new FloatSlider(this));
        _modelYawSlider.Minimum = -180;
        _modelYawSlider.Maximum = 180;
        _modelYawSlider.Value = 0;
        _modelYawSlider.FixedWidth = 80;
        _modelYawSlider.OnValueEdited = () => { _modelYaw = _modelYawSlider.Value; UpdateModelRotation(); };

        // �������� ������ (Pitch - ������)
        settingsPanel.Layout.Add(new Label("Pitch:", this));
        _modelPitchSlider = settingsPanel.Layout.Add(new FloatSlider(this));
        _modelPitchSlider.Minimum = -90;
        _modelPitchSlider.Maximum = 90;
        _modelPitchSlider.Value = 0;
        _modelPitchSlider.FixedWidth = 80;
        _modelPitchSlider.OnValueEdited = () => { _modelPitch = _modelPitchSlider.Value; UpdateModelRotation(); };

        // ���� ����
        settingsPanel.Layout.Add(new Label("Background:", this));
        var bgColorBtn = settingsPanel.Layout.Add(new Button("Color", this));
        bgColorBtn.FixedWidth = 60;
        bgColorBtn.Clicked += ShowBackgroundColorPicker;

        var transparentBtn = settingsPanel.Layout.Add(new Button("Transparent", this));
        transparentBtn.FixedWidth = 80;
        transparentBtn.Clicked += () => SetBackgroundColor(Color.Transparent);

        settingsPanel.Layout.AddStretchCell();

        // ������ ��������
        var resetCameraBtn = settingsPanel.Layout.Add(new Button("Reset Camera", this));
        resetCameraBtn.FixedWidth = 100;
        resetCameraBtn.Clicked += ResetCamera;

        // ������ ������ - ���� ����������
        var savePanel = Layout.Add(new Widget(this));
        savePanel.Layout = Layout.Row();
        savePanel.Layout.Spacing = 8;
        savePanel.FixedHeight = 32;

        savePanel.Layout.Add(new Label("Output Path:", this));
        _outputPathEdit = savePanel.Layout.Add(new LineEdit(this));
        _outputPathEdit.PlaceholderText = "ui/icons/model_icon.png";
        _outputPathEdit.Text = "Assets/ui/icons/model_icon.png";

        savePanel.Layout.AddStretchCell();

        var saveBtn = savePanel.Layout.Add(new Button.Primary("Save Icon", this));
        saveBtn.FixedWidth = 100;
        saveBtn.Clicked += SaveIcon;

        var copyBtn = savePanel.Layout.Add(new Button("Copy to Clipboard", this));
        copyBtn.FixedWidth = 120;
        copyBtn.Clicked += CopyToClipboard;

        // �������� �������: ����� �����, ������ ������
        var mainArea = Layout.Add(new Widget(this));
        mainArea.Layout = Layout.Row();
        mainArea.Layout.Spacing = 8;
        mainArea.SetSizeMode(SizeMode.CanGrow, SizeMode.CanGrow);

        // ����� ���������� (�������� �� ���������� �����)
        _sceneWidget = mainArea.Layout.Add(new SceneRenderingWidget(this));
        _sceneWidget.SetSizeMode(SizeMode.CanGrow, SizeMode.CanGrow);
        _sceneWidget.Scene = _scene;
        _sceneWidget.MouseTracking = true;

        // ������ ������
        _previewContainer = mainArea.Layout.Add(new Widget(this));
        _previewContainer.Layout = Layout.Column();
        _previewContainer.Layout.Spacing = 4;
        _previewContainer.SetSizeMode(SizeMode.Default, SizeMode.CanGrow);

        _previewSizeLabel = _previewContainer.Layout.Add(new Label("Preview (256x256)", this));
        _previewSizeLabel.SetStyles("color: #aaa; font-size: 11px;");

        _previewWidget = _previewContainer.Layout.Add(new Widget(this));
        _previewWidget.FixedSize = new Vector2(_previewWidth, _previewHeight);
        _previewWidget.SetStyles("background-color: #1a1a1a; border: 1px solid #444;");
        _previewWidget.OnPaintOverride = () =>
        {
            RenderPreview();
            return true;
        };

        _previewContainer.Layout.AddStretchCell();

        // ����������
        var infoLabel = Layout.Add(new Label("LMB: Rotate | RMB: Pan | Scroll: Zoom | Drag model file to load", this));
        infoLabel.SetStyles("color: #888; font-size: 11px;");

        // �������� drag-drop
        AcceptDrops = true;
    }

    [EditorEvent.Frame]
    public void OnFrame()
    {
        if (!_scene.IsValid())
            return;

        _scene.EditorTick(RealTime.Now, RealTime.Delta);
        _sceneWidget.Scene = _scene;

        // ��������� ������ ������ ���� ���������
        UpdatePreviewSize();
        _previewWidget?.Update();
    }

    private void UpdatePreviewSize()
    {
        var newWidth = int.TryParse(_widthEdit.Text, out var w) ? Math.Clamp(w, 16, 1024) : 256;
        var newHeight = int.TryParse(_heightEdit.Text, out var h) ? Math.Clamp(h, 16, 1024) : 256;

        if (newWidth != _previewWidth || newHeight != _previewHeight)
        {
            _previewWidth = newWidth;
            _previewHeight = newHeight;
            _previewWidget.FixedSize = new Vector2(_previewWidth, _previewHeight);
            _previewSizeLabel.Text = $"Preview ({_previewWidth}x{_previewHeight})";
        }
    }

    private void RenderPreview()
    {
        if (!_camera.IsValid() || !_scene.IsValid())
            return;

        var pixmap = new Pixmap(_previewWidth, _previewHeight);
        _camera.RenderToPixmap(pixmap);

        Paint.Draw(_previewWidget.LocalRect, pixmap);
    }

    protected override void OnMousePress(MouseEvent e)
    {
        base.OnMousePress(e);

        if (!_sceneWidget.ScreenRect.IsInside(e.ScreenPosition))
            return;

        _lastMousePos = e.LocalPosition;

        if (e.LeftMouseButton)
        {
            _isDragging = true;
        }
        else if (e.RightMouseButton)
        {
            _isPanning = true;
        }
    }

    protected override void OnMouseReleased(MouseEvent e)
    {
        base.OnMouseReleased(e);
        _isDragging = false;
        _isPanning = false;
    }

    protected override void OnMouseMove(MouseEvent e)
    {
        base.OnMouseMove(e);

        if (!_sceneWidget.ScreenRect.IsInside(e.ScreenPosition) && !_isDragging && !_isPanning)
            return;

        var delta = e.LocalPosition - _lastMousePos;
        _lastMousePos = e.LocalPosition;

        if (_isDragging)
        {
            // �������� ������
            _cameraYaw += delta.x * 0.5f;
            _cameraPitch += delta.y * 0.5f;
            _cameraPitch = Math.Clamp(_cameraPitch, -89f, 89f);
            UpdateCamera();
        }
        else if (_isPanning)
        {
            // ���������������
            var right = _camera.WorldRotation.Right;
            var up = _camera.WorldRotation.Up;
            var panSpeed = _cameraDistance * 0.002f;
            _cameraTarget -= right * delta.x * panSpeed;
            _cameraTarget += up * delta.y * panSpeed;
            UpdateCamera();
        }
    }

    protected override void OnWheel(WheelEvent e)
    {
        base.OnWheel(e);

        // ���
        _cameraDistance *= 1f - (e.Delta * 0.001f);
        _cameraDistance = Math.Clamp(_cameraDistance, 10f, 1000f);
        _cameraDistanceSlider.Value = _cameraDistance;
        UpdateCamera();

        e.Accept();
    }

    public override void OnDragHover(DragEvent ev)
    {
        base.OnDragHover(ev);

        if (ev.Data?.HasFileOrFolder == true || ev.Data?.Object is Asset)
        {
            ev.Action = DropAction.Copy;
        }
    }

    public override void OnDragDrop(DragEvent ev)
    {
        base.OnDragDrop(ev);

        if (ev.Data?.Object is Asset asset)
        {
            HandleDroppedAsset(asset);
            ev.Action = DropAction.Copy;
        }
        else if (ev.Data?.HasFileOrFolder == true)
        {
            var droppedAsset = AssetSystem.FindByPath(ev.Data.FileOrFolder);
            if (droppedAsset != null)
            {
                HandleDroppedAsset(droppedAsset);
                ev.Action = DropAction.Copy;
            }
        }
    }

    private void HandleDroppedAsset(Asset asset)
    {
        if (asset.AssetType?.FileExtension == "vmdl")
        {
            LoadModel(asset.Path);
        }
        else if (asset.AssetType?.FileExtension == "prefab")
        {
            LoadPrefab(asset.Path);
        }
        else if (asset.AssetType?.FileExtension == "vmat")
        {
            _materialPathEdit.Text = asset.Path;
            LoadMaterial(asset.Path);
        }
    }

    private void UpdateCamera()
    {
        if (!_camera.IsValid())
            return;

        var yawRad = _cameraYaw * MathF.PI / 180f;
        var pitchRad = _cameraPitch * MathF.PI / 180f;

        var direction = new Vector3(
            MathF.Cos(pitchRad) * MathF.Sin(yawRad),
            MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            MathF.Sin(pitchRad)
        ).Normal;

        _camera.WorldPosition = _cameraTarget + direction * _cameraDistance;
        _camera.WorldRotation = Rotation.LookAt(-direction, Vector3.Up);
    }

    private void UpdateModelRotation()
    {
        using (_scene.Push())
        {
            var rotation = Rotation.From(_modelPitch, _modelYaw, 0);

            if (_modelRenderer.IsValid() && _modelRenderer.Enabled)
                _modelRenderer.WorldRotation = rotation;

            if (_prefabInstance.IsValid())
                _prefabInstance.WorldRotation = rotation;
        }
    }

    private void OnMaterialPathChanged()
    {
        LoadMaterial(_materialPathEdit.Text);
    }

    private void ClearPrefab()
    {
        if (_prefabInstance.IsValid())
        {
            using (_scene.Push())
            {
                _prefabInstance.Destroy();
            }
            _prefabInstance = null;
        }
    }

    private async void LoadModel(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            // ������ ������ — ����������
            ClearPrefab();

            _currentModel = await Model.LoadAsync(path);
            if (_currentModel == null)
            {
                Log.Warning($"Failed to load model: {path}");
                return;
            }

            using (_scene.Push())
            {
                _modelRenderer.Enabled = true;
                _modelRenderer.Model = _currentModel;
                _modelRenderer.WorldPosition = Vector3.Zero;
                _modelRenderer.WorldRotation = Rotation.From(_modelPitch, _modelYaw, 0);

                // ��������� �������� ���� ����
                if (_currentMaterial != null)
                {
                    _modelRenderer.MaterialOverride = _currentMaterial;
                }
            }

            Log.Info($"Loaded model: {path}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading model: {ex.Message}");
        }
    }

    private void LoadPrefab(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            var prefabFile = ResourceLibrary.Get<PrefabFile>(path);
            if (prefabFile == null)
            {
                Log.Warning($"Failed to load prefab: {path}");
                return;
            }

            // ������ ������ — ���������
            ClearPrefab();

            using (_scene.Push())
            {
                // ��������� SkinnedModelRenderer ��� ������
                _modelRenderer.Enabled = false;
                _currentModel = null;

                var prefabScene = SceneUtility.GetPrefabScene(prefabFile);
                _prefabInstance = prefabScene.Clone();
                _prefabInstance.WorldPosition = Vector3.Zero;
                _prefabInstance.WorldRotation = Rotation.From(_modelPitch, _modelYaw, 0);
            }

            Log.Info($"Loaded prefab: {path}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading prefab: {ex.Message}");
        }
    }

    private void LoadMaterial(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            _currentMaterial = null;
            if (_modelRenderer.IsValid())
                _modelRenderer.MaterialOverride = null;
            return;
        }

        try
        {
            _currentMaterial = Material.Load(path);
            if (_currentMaterial == null)
            {
                Log.Warning($"Failed to load material: {path}");
                return;
            }

            if (_modelRenderer.IsValid())
            {
                _modelRenderer.MaterialOverride = _currentMaterial;
            }

            Log.Info($"Loaded material: {path}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading material: {ex.Message}");
        }
    }

    private void BrowseMaterial()
    {
        var picker = AssetPicker.Create(null, AssetType.Material, new AssetPicker.PickerOptions()
        {
            EnableMultiselect = false
        });

        picker.Title = "Select Material";
        picker.OnAssetPicked = assets =>
        {
            var asset = assets?.FirstOrDefault();
            if (asset != null)
            {
                _materialPathEdit.Text = asset.Path;
                LoadMaterial(asset.Path);
            }
        };

        picker.Show();
    }

    private void ShowBackgroundColorPicker()
    {
        var picker = ColorPicker.OpenColorPopup(_backgroundColor, c =>
        {
            SetBackgroundColor(c);
        }, ScreenRect.BottomLeft);
    }

    private void SetBackgroundColor(Color color)
    {
        _backgroundColor = color;
        if (_camera.IsValid())
        {
            _camera.BackgroundColor = _backgroundColor;
        }
    }

    private void ResetCamera()
    {
        _cameraYaw = 45f;
        _cameraPitch = 20f;
        _cameraDistance = 100f;
        _cameraTarget = Vector3.Zero;
        _modelYaw = 0f;
        _modelPitch = 0f;
        _cameraDistanceSlider.Value = _cameraDistance;
        _modelYawSlider.Value = _modelYaw;
        _modelPitchSlider.Value = _modelPitch;
        UpdateCamera();
        UpdateModelRotation();
    }

    private void SaveIcon()
    {
        if (!_camera.IsValid())
        {
            Log.Warning("Camera not valid");
            return;
        }

        var width = int.TryParse(_widthEdit.Text, out var w) ? w : 256;
        var height = int.TryParse(_heightEdit.Text, out var h) ? h : 256;
        var outputPath = _outputPathEdit.Text;

        if (string.IsNullOrEmpty(outputPath))
        {
            Log.Warning("Output path is empty");
            return;
        }

        try
        {
            var bitmap = new Bitmap(width, height);
            _camera.RenderToBitmap(bitmap);

            var pngBytes = bitmap.ToPng();

            // ��������� � ����� �������
            var projectPath = Project.Current?.GetRootPath() ?? "";
            var fullPath = System.IO.Path.Combine(projectPath, outputPath);
            var directory = System.IO.Path.GetDirectoryName(fullPath);

            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            System.IO.File.WriteAllBytes(fullPath, pngBytes);

            Log.Info($"Icon saved to: {fullPath}");
            Log.Info($"Size: {width}x{height}, {pngBytes.Length} bytes");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to save icon: {ex.Message}");
        }
    }

    private void CopyToClipboard()
    {
        if (!_camera.IsValid())
            return;

        var width = int.TryParse(_widthEdit.Text, out var w) ? w : 256;
        var height = int.TryParse(_heightEdit.Text, out var h) ? h : 256;

        try
        {
            var pixmap = new Pixmap(width, height);
            _camera.RenderToPixmap(pixmap);

            // �������� ��� PNG ������
            var bitmap = new Bitmap(width, height);
            _camera.RenderToBitmap(bitmap);
            var pngData = bitmap.ToPng();
            var base64 = Convert.ToBase64String(pngData);

            EditorUtility.Clipboard.Copy(base64);

            Log.Info($"Icon data copied to clipboard (base64 PNG, {width}x{height})");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to copy to clipboard: {ex.Message}");
        }
    }

    public override void OnDestroyed()
    {
        base.OnDestroyed();

        ClearPrefab();
        _scene?.Destroy();
        _scene = null;
    }
}

/// <summary>
/// ������� ������ ��� �������� ������
/// </summary>
public class ModelHolder
{
    public Model TargetModel { get; set; }
}

/// <summary>
/// ������ ��� ������ ������ ����� ControlSheet (������������ drag-drop)
/// </summary>
public class ModelDropWidget : Widget
{
    public Action<string> OnModelChanged;

    private ModelHolder _holder;
    private ControlSheet _controlSheet;
    private SerializedObject _serializedObject;

    public string ModelPath => _holder?.TargetModel?.ResourcePath;

    public ModelDropWidget(Widget parent) : base(parent)
    {
        _holder = new ModelHolder();
        _serializedObject = EditorTypeLibrary.GetSerializedObject(_holder);

        Layout = Layout.Row();

        _controlSheet = new ControlSheet();
        _controlSheet.AddRow(_serializedObject.GetProperty(nameof(ModelHolder.TargetModel)));

        Layout.Add(_controlSheet);

        // ������������� �� ���������
        _serializedObject.OnPropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(SerializedProperty prop)
    {
        if (prop.Name == nameof(ModelHolder.TargetModel))
        {
            OnModelChanged?.Invoke(_holder.TargetModel?.ResourcePath);
        }
    }
}

public class PrefabHolder
{
    public PrefabFile TargetPrefab { get; set; }
}

public class PrefabDropWidget : Widget
{
    public Action<string> OnPrefabChanged;

    private PrefabHolder _holder;
    private ControlSheet _controlSheet;
    private SerializedObject _serializedObject;

    public PrefabDropWidget(Widget parent) : base(parent)
    {
        _holder = new PrefabHolder();
        _serializedObject = EditorTypeLibrary.GetSerializedObject(_holder);

        Layout = Layout.Row();

        _controlSheet = new ControlSheet();
        _controlSheet.AddRow(_serializedObject.GetProperty(nameof(PrefabHolder.TargetPrefab)));

        Layout.Add(_controlSheet);

        _serializedObject.OnPropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(SerializedProperty prop)
    {
        if (prop.Name == nameof(PrefabHolder.TargetPrefab))
        {
            OnPrefabChanged?.Invoke(_holder.TargetPrefab?.ResourcePath);
        }
    }
}
