using TMPro;
using UnityEngine;

public class NetworkDebugWidget : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;
    [SerializeField] TMP_Text infoText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDebugText(string _text)
    {
        debugText.text = _text;
    }

    public void SetInfoText(string _text)
    {
        infoText.text = _text;
    }
}
