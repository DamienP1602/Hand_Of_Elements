using TMPro;
using UnityEngine;

public class CardOverlayComponent : MonoBehaviour
{
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text nameText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(BaseCardData _data)
    {
        nameText.text = _data.cardName;

        if (_data is SoldierCardData _soldier)
        {
            healthText.text = _soldier.health.ToString();
            damageText.text = _soldier.damages.ToString();
        }
    }
}
