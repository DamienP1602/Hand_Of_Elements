using TMPro;
using UnityEngine;

public class DebugWidget : MonoBehaviour
{
    [SerializeField] bool ShowText = true;
    [SerializeField] TMP_Text text;

    public void SetDebugText(string _text)
    {
        if (ShowText)
            text.text = _text + "\n";
    }

    public void AddDebugText(string _text)
    {
        if (ShowText)
            text.text += _text + "\n";
    }
}
