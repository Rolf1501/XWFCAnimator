// This script attaches the tabbed menu logic to the game.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

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

    private Button _updateExtent;

    private TextField _stepSize;
    private TextField _delay;
    private Button _collapseOnceButton;
    private Button _runButton;
    private Button _resetButton;

    private string _adjacencyGridName = "adjacencyGrid";
    private VisualElement _adjGrid;
    private AdjacencyGridController _adjacencyGridController;

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
        _updateExtent = _root.Q<Button>("updateExtentButton");
    }

    private void AddListeners()
    {
        _resetButton.clicked += delegate
        {
            Debug.Log("Clicked it!");
            XWFCAnimator.Instance.Reset();
        };
    }

    private void Start()
    {
        InitAdjacencyGrid();
        InitGridValues();
        AddExtentListeners(_wSlider, _wInput);
        AddExtentListeners(_hSlider, _hInput);
        AddExtentListeners(_dSlider, _dInput);
        AddExtentUpdate(_updateExtent);
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
        _adjacencyGridController = new AdjacencyGridController(new List<int> {1,2,3}, XWFCAnimator.Instance.GetTileAdjacencyConstraints(), XWFCAnimator.Instance.GetOffsets());
        var grids = _adjacencyGridController.Grids;
        _adjGrid.Add(grids[Vector3.left]);
    }

    private void UpdateToggleListeners()
    {
    }

    private void ClassSwitch(VisualElement element, string classRemove, string classAdd)
    {
        element.RemoveFromClassList(classRemove);
        element.AddToClassList(classAdd);
    }

    private void ToggleDisabled(VisualElement element)
    {
        element.SetEnabled(false);
        ClassSwitch(element, "enabled-button", "disabled-button");
    }

    private void ToggleEnabled(VisualElement element)
    {
        element.SetEnabled(true);
        ClassSwitch(element,  "disabled-button","enabled-button");
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