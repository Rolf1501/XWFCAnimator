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

    [SerializeField] private TMP_InputField iterationsPerSecond;
    [SerializeField] private Button collapseOnceButton;
    [SerializeField] private Button runButton;

    private void Start()
    {
        InitGridValues();
        AddExtentListeners(wSlider, wInput);
        AddExtentListeners(hSlider, hInput);
        AddExtentListeners(dSlider, dInput);
        AddExtentUpdate(updateExtent);
        AddCollapseListeners();

    }

    private void AddCollapseListeners()
    {
        collapseOnceButton.onClick.AddListener(delegate { XWFCAnimator.Instance.CollapseAndDrawOnce(); });
        runButton.onClick.AddListener(delegate { XWFCAnimator.Instance.ToggleCollapseMode(); });
        iterationsPerSecond.onValueChanged.AddListener(delegate { XWFCAnimator.Instance.UpdateIterationsPerSecond(float.Parse(iterationsPerSecond.text)); });
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
        field.onValueChanged.AddListener(delegate { UpdateSliderValue(slider, float.Parse(field.text)); });
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
}