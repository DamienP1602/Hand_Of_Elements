using System;
using TMPro;
using UnityEngine;

public class CardOverlayComponent : MonoBehaviour
{
    [Header("Base Parameters")]
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text nameText;

    [Header("Single Description")]
    [SerializeField] GameObject singleDescriptionObject;
    [SerializeField] TMP_Text singleDescription;

    [Header("Hidden Description")]
    [SerializeField] GameObject hiddenDescriptionObject;
    [SerializeField] TMP_Text hiddenDescription;

    [Header("Elementary Combo Description")]
    [SerializeField] GameObject comboDescriptionObject;
    [SerializeField] TMP_Text firstDescription;
    [SerializeField] TMP_Text comboDescription;

    BaseCardData data;

    void Start()
    {

    }

    void Update()
    {

    }

    #region Setters

    public void SetData(BaseCardData _data)
    {
        if (!_data) return;
        data = _data;

        if (_data is SoldierCardData _soldier)
        {
            damageText.text = _soldier.attackAmount.ToString();
            healthText.text = _soldier.healthAmount.ToString();
        }

        if (GetComponent<HandCardComponent>())
        {
            nameText.text = _data.cardName;
            SetText();
        }
    }

    #endregion

    #region Functions

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

    public void UpdateAttack(int _attackAmount)
    {
        damageText.text = _attackAmount.ToString();

        if (data is SoldierCardData _soldier)
        {
            if (_attackAmount < _soldier.healthAmount)
                healthText.color = Color.red;
            else if (_attackAmount > _soldier.healthAmount)
                healthText.color = Color.green;
            else
                healthText.color = Color.white;
        }
    }

    void SetText()
    {
        if (!data.hasEffect)
            return;

        if (data.hasElementaryCombo)
        {
            comboDescriptionObject.SetActive(true);
            firstDescription.text = data.description;
            comboDescription.text = data.elementaryComboDescription;
        }
        else if (data.isHiddenEffect)
        {
            hiddenDescriptionObject.SetActive(true);
            hiddenDescription.text = data.description;
        }
        else
        {
            singleDescriptionObject.SetActive(true);
            singleDescription.text = data.description;
        }
    }

    #endregion
}
