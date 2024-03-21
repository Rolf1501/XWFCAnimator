using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public Slider wSlider;
    public TMP_InputField wInput;
    public Slider hSlider;
    public TMP_InputField hInput;
    public Slider dSlider;
    public TMP_InputField dInput;

    private void Start()
    {
        wSlider.onValueChanged.AddListener(delegate
        {
            UpdateInputValue(wInput, wSlider.value); UpdateWidth();
        });
        wInput.onValueChanged.AddListener(delegate
        {
            UpdateSliderValue(wSlider, float.Parse(wInput.text)); UpdateWidth();
        });
    }

    private void UpdateSliderValue(Slider slider, float value)
    {
        slider.value = value;
    }

    private void UpdateInputValue(TMP_InputField field, float value)
    {
        field.text = value.ToString("0");
    }

    private void UpdateWidth()
    {
        Debug.Log("Tried updating width?");
        XWFCAnimator.Instance.UpdateExtent();
    }
}