using TMPro;
using UnityEngine;

public class NetworkDebugWidget : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;
    [SerializeField] TMP_Text infoText;

    #region Setters

    public void SetDebugText(string _text)
    {
        debugText.text = _text;
    }

    public void SetInfoText(string _text)
    {
        infoText.text = _text;
    }

    #endregion

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
