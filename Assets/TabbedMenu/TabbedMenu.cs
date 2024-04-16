// This script attaches the tabbed menu logic to the game.
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using XWFC;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;
using Toggle = UnityEngine.UIElements.Toggle;

/*
 * Tab handling obtained from: https://docs.unity3d.com/Manual/UIE-create-tabbed-menu-for-runtime.html
 * Accessed on 2024-03-28
 */
public class TabbedMenu : MonoBehaviour
{
    private TabbedMenuController _controller;
    private VisualElement _root;
    
    private SliderInt _wSlider;
    private TextField _wInput;
    private SliderInt _hSlider;
    private TextField _hInput;
    private SliderInt _dSlider;
    private TextField _dInput;

    private Button _updateExtentButton;

    private TextField _stepSize;
    private TextField _delay;
    private Button _collapseOnceButton;
    private Button _runButton;
    private Button _resetButton;

    private string _adjacencyGridName = "adjacencyGrid";
    private string _adjacencyToggleContainer = "adjacencyToggleContainer";
    private VisualElement _adjGrid;
    private AdjacencyGridController _adjacencyGridController;
    private Button _updateAdjacencyButton;

    private string _tilesetListName = "tilesetListContainer";
    private Button _updateTilesetButton;
    private HashSet<int> _activeTiles = new();

    private const string HiddenClassName = "hidden";
    private const string SelectedClassName = "selected";
    private readonly Vector3 _defaultDirection = Vector3.right;
    

    private void OnEnable()
    {
        UIDocument menu = GetComponent<UIDocument>();
        _root = menu.rootVisualElement;

        _controller = new TabbedMenuController(_root);

        _controller.RegisterTabCallbacks();
        
        Bind();
        AddListeners();
    }

    private void Bind()
    {
        _resetButton = _root.Q<Button>("resetButton");
        _runButton = _root.Q<Button>("runButton");
        _collapseOnceButton = _root.Q<Button>("collapseOnceButton");
        _wSlider = _root.Q<SliderInt>("widthSlider");
        _hSlider = _root.Q<SliderInt>("heightSlider");
        _dSlider = _root.Q<SliderInt>("depthSlider");
        _wInput = _root.Q<TextField>("widthInput");
        _hInput = _root.Q<TextField>("heightInput");
        _dInput = _root.Q<TextField>("depthInput");
        _delay = _root.Q<TextField>("delayInput");
        _stepSize = _root.Q<TextField>("stepSizeInput");
        _updateExtentButton = _root.Q<Button>("updateExtentButton");
        _updateAdjacencyButton = _root.Q<Button>("updateAdjacencyButton");
        _updateTilesetButton = _root.Q<Button>("updateTilesetButton");
    }

    private void AddListeners()
    {
        _resetButton.clicked += delegate
        {
            Debug.Log("Reset!");
            XWFCAnimator.Instance.Reset();
        };

        _updateAdjacencyButton.clicked += delegate
        {
            Debug.Log("Updated Adjacency Constraints!");
            XWFCAnimator.Instance.UpdateAdjacencyConstraints(_adjacencyGridController.ToAdjacencySet());
        };
    }

    private void Start()
    {
        InitAdjacencyGrid();
        InitGridValues();
        InitTilesetList();
        AddExtentListeners(_wSlider, _wInput);
        AddExtentListeners(_hSlider, _hInput);
        AddExtentListeners(_dSlider, _dInput);
        AddExtentUpdate(_updateExtentButton);
        _stepSize.value = XWFCAnimator.Instance.stepSize.ToString("0");
        _delay.value = XWFCAnimator.Instance.delay.ToString("0.0");
        AddCollapseListeners();

    }

    private void AddCollapseListeners()
    {
        _collapseOnceButton.clicked += delegate { XWFCAnimator.Instance.CollapseAndDrawOnce(); };
        _runButton.clicked += delegate
        {
            _runButton.text = XWFCAnimator.Instance.ToggleCollapseMode() ? "Pause" : "Run";
        };
        _stepSize.RegisterValueChangedCallback(delegate
        {
            try { XWFCAnimator.Instance.UpdateStepSize(float.Parse(_stepSize.text)); }
            catch {}
        });
        _delay.RegisterValueChangedCallback(delegate
        {
            try { XWFCAnimator.Instance.UpdateDelay(float.Parse(_delay.text)); }
            catch {}
        });
        _resetButton.clicked += delegate { Debug.Log("RESET!!"); XWFCAnimator.Instance.Reset(); };
    }

    private void InitGridValues()
    {
        UpdateSliderValue(_wSlider, XWFCAnimator.Instance.extent.x);
        UpdateSliderValue(_hSlider, XWFCAnimator.Instance.extent.y);
        UpdateSliderValue(_dSlider, XWFCAnimator.Instance.extent.z);
        UpdateInputValue(_wInput, XWFCAnimator.Instance.extent.x);
        UpdateInputValue(_hInput, XWFCAnimator.Instance.extent.y);
        UpdateInputValue(_dInput, XWFCAnimator.Instance.extent.z);
    }

    private void AddExtentUpdate(Button button)
    {
        button.clicked += UpdateExtent;
    }

    private void AddExtentListeners(SliderInt slider, TextField field)
    {
        slider.RegisterValueChangedCallback(delegate { UpdateInputValue(field, slider.value); });
        field.RegisterValueChangedCallback(delegate { if(field.value != "") UpdateSliderValue(slider, float.Parse(field.value)); });
    }

    private void UpdateSliderValue(SliderInt slider, float value)
    {
        var intValue = (int) value;
        if (intValue > slider.highValue) intValue = slider.highValue;
        slider.value = intValue;
    }

    private void UpdateInputValue(TextField field, float value)
    {
        field.value = value.ToString("0");
    }

    private void UpdateExtent()
    {
        XWFCAnimator.Instance.UpdateExtent(new Vector3Int(_wSlider.value, _hSlider.value, _dSlider.value));
    }

    private Bidict<string, Vector3> GetOffsetNamesMapping()
    {
        var directionNames = new Bidict<string, Vector3>();
        directionNames.AddPair("North", Vector3.forward);
        directionNames.AddPair("East", Vector3.right);
        directionNames.AddPair("South", Vector3.back);
        directionNames.AddPair("West", Vector3.left);
        directionNames.AddPair("Up", Vector3.up);
        directionNames.AddPair("Down", Vector3.down);
        return directionNames;
    }

    private void InitAdjacencyDropDown()
    {
        var dropDown = new DropdownField();
        _adjGrid.Add(dropDown);

        var directionNames = GetOffsetNamesMapping();
        
        var offsets = XWFCAnimator.Instance.GetOffsets();
        foreach (var offset in offsets)
        {
            var directionName = directionNames.GetKey(offset);
            dropDown.choices.Add(directionName);
        }
        
        dropDown.value = directionNames.GetKey(_defaultDirection);
        
        dropDown.RegisterValueChangedCallback(delegate
        {
            var element = _adjGrid.Q<VisualElement>(className: SelectedClassName);
            SwitchClass(element, SelectedClassName, HiddenClassName);

            var showGrid = _adjGrid.Q<VisualElement>(dropDown.value);
            SwitchClass(showGrid, HiddenClassName, SelectedClassName);
            
            Debug.Log($"VALUE CHANGED TO {dropDown.value}");
        });
    }

    private void InitAdjacencyToggles(List<int> tiles, HashSetAdjacency hashSetAdjacency, Vector3[] offsets)
    {
        var toggles = _adjGrid.Q<VisualElement>(_adjacencyToggleContainer);
        if (toggles != null)
        {
            toggles.Clear();
        }
        else
        {
            toggles = new VisualElement();
            toggles.name = _adjacencyToggleContainer;
        }

        _adjacencyGridController = new AdjacencyGridController(tiles, hashSetAdjacency, offsets);
        // _adjacencyGridController = new AdjacencyGridController(XWFCAnimator.Instance.GetTiles().Keys.ToList(), XWFCAnimator.Instance.GetTileAdjacencyConstraints(), XWFCAnimator.Instance.GetOffsets());
        var directionNames = GetOffsetNamesMapping();
        var grids = _adjacencyGridController.Grids;

        foreach (var offset in offsets)
        {
            var directionName = directionNames.GetKey(offset);
            var gridContainer = new VisualElement();
            gridContainer.name = directionName;
            gridContainer.AddToClassList("hidden");
            gridContainer.Add(grids[offset]);
            toggles.Add(gridContainer);
        }
        _adjGrid.Add(toggles);
        var defaultChoice =  _adjGrid.Q<VisualElement>(directionNames.GetKey(_defaultDirection));
        SwitchClass(defaultChoice, HiddenClassName, SelectedClassName);
    }

    private void InitAdjacencyGrid()
    {
        _adjGrid = _root.Q<VisualElement>(_adjacencyGridName);
        InitAdjacencyDropDown();
        InitAdjacencyToggles(XWFCAnimator.Instance.GetTiles().Keys.ToList(), XWFCAnimator.Instance.GetTileAdjacencyConstraints(), XWFCAnimator.Instance.GetOffsets());
    }
    
    public static void SwitchClass(VisualElement element, string classRemove, string classAdd)
    {
        element.RemoveFromClassList(classRemove);
        element.AddToClassList(classAdd);
    }

    private void InitTilesetList()
    {
        var tiles = XWFCAnimator.Instance.CompleteTerminalSet;
        var tilesetListContainer = _root.Q<VisualElement>(_tilesetListName);
        tilesetListContainer.AddToClassList("tile-entry-container");
        
        foreach (var tileId in tiles.Keys)
        {
            var value = XWFCAnimator.Instance.TileSet.Keys.Contains(tileId);
            var entry = TileEntry(tileId, value);
            tilesetListContainer.Add(entry);
            if (value) _activeTiles.Add(tileId);
        }
        /*
         * Upon changing tile set:
         * Update tileset used in xwfc.
         * Update visual elements of adjacency. 
         */
        _updateTilesetButton.clicked += delegate
        {
            var tileDict = _activeTiles.ToDictionary(tileId => tileId, tileId => tiles[tileId]);
            Debug.Log("Tried updating tileset...");
            InitAdjacencyToggles(tileDict.Keys.ToList(), new HashSetAdjacency(), OffsetFactory.GetOffsets());
            _adjacencyGridController.Populate(true);
            XWFCAnimator.Instance.UpdateTerminals(TileSet.FromDict(tileDict));
            XWFCAnimator.Instance.UpdateAdjacencyConstraints(_adjacencyGridController.ToAdjacencySet());
        };
    }

    private VisualElement TileEntry(int tileId, bool toggleValue)
    {
        var entry = new VisualElement();
        entry.name = $"tilesetEntry{tileId}";
        entry.AddToClassList("tile-entry");
        var toggle = new Toggle();
        toggle.RegisterValueChangedCallback(delegate { TileToggle(tileId); });
        toggle.name = $"tilesetEntryToggle{tileId}";
        toggle.value = toggleValue;
        entry.Add(toggle);
        var text = new Label();
        text.text = $"{tileId}";
        entry.Add(text);
        var img = new Image();
        var tt = new TileTexture(XWFCAnimator.Instance.drawnTilePositions[tileId], 5, new Vector2(45,45));
        
        img.image = tt.RenderTexture;
        entry.Add(img);
        
        return entry;
    }

    private void TileToggle(int tileId)
    {
        if (_activeTiles.Contains(tileId))
        {
            _activeTiles.Remove(tileId);
            return;
        }

        _activeTiles.Add(tileId);
    }

    private static void ToggleDisabled(VisualElement element)
    {
        element.SetEnabled(false);
        SwitchClass(element, "enabled-button", "disabled-button");
    }

    private static void ToggleEnabled(VisualElement element)
    {
        element.SetEnabled(true);
        SwitchClass(element,  "disabled-button","enabled-button");
    }

    private void Update()
    {
        if (XWFCAnimator.Instance.IsDone() && _runButton.enabledInHierarchy)
        {
            ToggleDisabled(_runButton);
            _runButton.text = "All done";
            
            ToggleDisabled(_collapseOnceButton);
        }
        else if (!XWFCAnimator.Instance.IsDone() && !_runButton.enabledInHierarchy)
        {
            ToggleEnabled(_runButton);
            _runButton.text = "Run";
            
            ToggleEnabled(_collapseOnceButton);
        }
    }
}