// This script attaches the tabbed menu logic to the game.

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using XWFC;
using Button = UnityEngine.UIElements.Button;

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
    private VisualElement _adjGrid;
    private AdjacencyGridController _adjacencyGridController;
    private Button _updateAdjacencyButton;
    private Button _updateTilesetButton;

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
        Debug.Log("Tried updating width.");
        XWFCAnimator.Instance.UpdateExtent(new Vector3(_wSlider.value, _hSlider.value, _dSlider.value));
    }

    private void InitAdjacencyGrid()
    {
        _adjGrid = _root.Q<VisualElement>(_adjacencyGridName);
        _adjacencyGridController = new AdjacencyGridController(XWFCAnimator.Instance.GetTiles().Keys.ToList(), XWFCAnimator.Instance.GetTileAdjacencyConstraints(), XWFCAnimator.Instance.GetOffsets());
        var grids = _adjacencyGridController.Grids;
        var dropDown = new DropdownField();
        _adjGrid.Add(dropDown);

        var directionNames = new Bidict<string, Vector3>();
        directionNames.AddPair("North", Vector3.forward);
        directionNames.AddPair("East", Vector3.right);
        directionNames.AddPair("South", Vector3.back);
        directionNames.AddPair("West", Vector3.left);
        directionNames.AddPair("Up", Vector3.up);
        directionNames.AddPair("Down", Vector3.down);


        var offsets = XWFCAnimator.Instance.GetOffsets();
        foreach (var offset in offsets)
        {
            var directionName = directionNames.GetKey(offset);
            dropDown.choices.Add(directionName);
            var gridContainer = new VisualElement();
            gridContainer.name = directionName;
            gridContainer.AddToClassList("hidden");
            gridContainer.Add(grids[offset]);
            _adjGrid.Add(gridContainer);
        }

        var hiddenClass = "hidden";
        var selectedClass = "selected";
        
        var defaultDirection = Vector3.right;
        var defaultDirectionName = directionNames.GetKey(defaultDirection);
        var defaultChoice =  _adjGrid.Q<VisualElement>(defaultDirectionName);
        
        dropDown.value = defaultDirectionName;
        SwitchClass(defaultChoice, hiddenClass, selectedClass);
        dropDown.RegisterValueChangedCallback(delegate
        {
            var element = _adjGrid.Q<VisualElement>(className: selectedClass);
            SwitchClass(element, selectedClass, hiddenClass);

            var showGrid = _adjGrid.Q<VisualElement>(dropDown.value);
            SwitchClass(showGrid, hiddenClass, selectedClass);
            
            Debug.Log($"VALUE CHANGED TO {dropDown.value}");
        });
    }
    
    private static void SwitchClass(VisualElement element, string classRemove, string classAdd)
    {
        element.RemoveFromClassList(classRemove);
        element.AddToClassList(classAdd);
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