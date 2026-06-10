using UnityEngine;
using UnityEngine.UI;

// используется раз в начале при диалоге выбора

public class SliderListener : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    [SerializeField] private Text _text;

    private void Start()
    {
        UpdateText(_slider.value);

        _slider.onValueChanged.AddListener(UpdateText);
    }

    private void UpdateText(float value)
    {
        int intValue = Mathf.RoundToInt(value);

        _text.text = intValue.ToString();

        GameSceneManager.Instance.ObjectsToGenerate(intValue);
    }

    private void OnDestroy()
    {
        if (_slider != null)
        {
            _slider.onValueChanged.RemoveListener(UpdateText);
        }
    }
}