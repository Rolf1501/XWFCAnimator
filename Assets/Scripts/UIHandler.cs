using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private Slider wSlider;
    [SerializeField] private TMP_InputField wInput;
    [SerializeField] private Slider hSlider;
    [SerializeField] private TMP_InputField hInput;
    [SerializeField] private Slider dSlider;
    [SerializeField] private TMP_InputField dInput;

    [SerializeField] private Button updateExtent;

    [SerializeField] private TMP_InputField stepSize;
    [SerializeField] private TMP_InputField delay;
    [SerializeField] private Button collapseOnceButton;
    [SerializeField] private Button runButton;
    [SerializeField] private Button resetButton;

    private void Start()
    {
        InitGridValues();
        AddExtentListeners(wSlider, wInput);
        AddExtentListeners(hSlider, hInput);
        AddExtentListeners(dSlider, dInput);
        AddExtentUpdate(updateExtent);
        stepSize.text = XWFCAnimator.Instance.stepSize.ToString("0.0");
        AddCollapseListeners();

    }

    private void AddCollapseListeners()
    {
        collapseOnceButton.onClick.AddListener(delegate { XWFCAnimator.Instance.CollapseAndDrawOnce(); });
        runButton.onClick.AddListener(delegate
        {
            var textComponent = runButton.GetComponentInChildren<TMP_Text>();
            textComponent.text = XWFCAnimator.Instance.ToggleCollapseMode() ? "Pause" : "Run";
        });
        stepSize.onValueChanged.AddListener(delegate
        {
            try { XWFCAnimator.Instance.UpdateStepSize(float.Parse(stepSize.text)); }
            catch {}
        });
        delay.onValueChanged.AddListener(delegate
        {
            try { XWFCAnimator.Instance.UpdateDelay(float.Parse(delay.text)); }
            catch {}
        });
        resetButton.onClick.AddListener(delegate { XWFCAnimator.Instance.Reset(); });
    }

    private void InitGridValues()
    {
        UpdateSliderValue(wSlider, XWFCAnimator.Instance.extent.x);
        UpdateSliderValue(hSlider, XWFCAnimator.Instance.extent.y);
        UpdateSliderValue(dSlider, XWFCAnimator.Instance.extent.z);
        UpdateInputValue(wInput, XWFCAnimator.Instance.extent.x);
        UpdateInputValue(hInput, XWFCAnimator.Instance.extent.y);
        UpdateInputValue(dInput, XWFCAnimator.Instance.extent.z);
    }

    private void AddExtentUpdate(Button button)
    {
        button.onClick.AddListener(UpdateExtent);
    }

    private void AddExtentListeners(Slider slider, TMP_InputField field)
    {
        slider.onValueChanged.AddListener(delegate { UpdateInputValue(field, slider.value); });
        field.onValueChanged.AddListener(delegate { if(field.text != "") UpdateSliderValue(slider, float.Parse(field.text)); });
    }

    private void UpdateSliderValue(Slider slider, float value)
    {
        if (value > slider.maxValue)
        {
            value = slider.maxValue;
        }
        slider.value = value;
    }

    private void UpdateInputValue(TMP_InputField field, float value)
    {
        
        field.text = value.ToString("0");
    }

    private void UpdateExtent()
    {
        Debug.Log("Tried updating width.");
        XWFCAnimator.Instance.UpdateExtent(new Vector3(wSlider.value, hSlider.value, dSlider.value));
    }

    private void Update()
    {
        if (XWFCAnimator.Instance.IsDone() && runButton.enabled)
        {
            runButton.enabled = false;
            runButton.interactable = false;
            runButton.GetComponentInChildren<TMP_Text>().text = "";
        }
        else if (!XWFCAnimator.Instance.IsDone() && !runButton.enabled)
        {
            runButton.enabled = true;
            runButton.interactable = true;
            runButton.GetComponentInChildren<TMP_Text>().text = "Run";
        }
    }
}