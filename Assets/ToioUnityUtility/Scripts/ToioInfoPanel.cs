using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ToioInfoPanel : MonoBehaviour
{
    public Button _button;
    public TextMeshProUGUI _buttonText;
    public Image _backgroundImage;
    public Color _backgroundImageDefaultColor;
    public Color _backgroundImageSelectedColor;
    public List<TextMeshProUGUI> _positionTexts = new List<TextMeshProUGUI>();

    public void ChangeBackgroundImageColor(Color color) {
        _backgroundImage.color = color;
    }

    public void SetPositionTexts(int x, int y) {
        _positionTexts[0].text = x.ToString();
        _positionTexts[1].text = y.ToString();
    }
}
