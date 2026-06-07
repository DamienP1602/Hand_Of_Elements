using TMPro;
using UnityEngine;

public class PlayerPortraitComponent : MonoBehaviour
{
    [SerializeField] TMP_Text healthText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Functions
    public void SetHealthAmount(int _newAmount)
    {
        healthText.SetText(_newAmount.ToString());
    }
    #endregion
}
