using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardOverlayComponent : MonoBehaviour
{
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text typeText;
    [SerializeField] TMP_Text descriptionText;

    BaseCardData data;

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
        data = _data;

        nameText.text = _data.cardName;
        typeText.text = _data.cardElement.ToString();

        if (_data is SoldierCardData _soldier)
        {
            healthText.text = _soldier.healthAmount.ToString();
            damageText.text = _soldier.attackAmount.ToString();
        }
        else if (_data is SpellCardData _spell)
        {
            descriptionText.text = _spell.description;
        }
    }

    public void UpdateHealth(int _healthAmount)
    {
        healthText.text = _healthAmount.ToString();

        if (data is SoldierCardData _soldier)
        {
            if (_healthAmount < _soldier.healthAmount)
                healthText.color = Color.red;
            else if (_healthAmount > _soldier.healthAmount)
                healthText.color = Color.green;
            else
                healthText.color = Color.white;
        }
    }
}
