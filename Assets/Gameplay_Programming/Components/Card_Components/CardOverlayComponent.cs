using TMPro;
using UnityEngine;

public class CardOverlayComponent : MonoBehaviour
{
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text typeText;
    [SerializeField] TMP_Text descriptionText;

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
        if (!_data) return;

        nameText.text = _data.cardName;
        typeText.text = _data.cardElement.ToString();

        if (_data is SoldierCardData _soldier)
        {
            healthText.text = _soldier.health.ToString();
            damageText.text = _soldier.damages.ToString();
        }
        else if (_data is SpellCardData _spell)
        {
            descriptionText.text = _spell.description;
        }
    }
}
