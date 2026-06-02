using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardOverlayComponent : MonoBehaviour
{
    [Header("Base Parameters")]
    [SerializeField] Canvas canva;
    [SerializeField] Image background;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text description;

    BaseCardData data;

    [Header("Scale Parameters")]
    [SerializeField] float scaleTarget = 1.0f;
    [SerializeField] float scaleSpeed = 1.0f;
    [SerializeField] bool needToChangeScale = false;
    [SerializeField] float currentScaleTime = 0.0f;

    void Start()
    {

    }

    void Update()
    {
        if (needToChangeScale)
            UpdateScale();
    }

    #region Update

    void UpdateScale()
    {
        currentScaleTime += Time.deltaTime * scaleSpeed;
        float _f = Mathf.Lerp(transform.localScale.x, scaleTarget, currentScaleTime);
        transform.localScale = Vector3.one * _f;

        if (currentScaleTime >= 1.0f)
            needToChangeScale = false;
    }

    #endregion

    #region Setters

    public void SetData(BaseCardData _data, bool _forceInit = false)
    {
        if (!_data) return;
        data = _data;

        if (_data is SoldierCardData _soldier)
        {
            damageText.text = _soldier.attackAmount.ToString();
            healthText.text = _soldier.healthAmount.ToString();
        }
        if (_data is SpellCardData _spell)
        {
            damageText.gameObject.SetActive(false);
            healthText.gameObject.SetActive(false);
        }

        if (data.hasEffect)
            description.text = data.description;

        if (GetComponent<HandCardComponent>() || _forceInit)
            nameText.text = _data.cardName;

        SetColorFromType();
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

    /// <summary>
    /// Temp
    /// </summary>
    public void SetColorFromType()
    {
        Color _color = Color.white;
        switch (data.cardElement)
        {
            case CardElement.Fire:
                _color = new Color(1.0f, 0.2f, 0.2f);
                break;
            case CardElement.Water:
                _color = new Color(0.5f, 0.5f, 1.0f);
                break;
            case CardElement.Earth:
                _color = new Color(0.5f, 1.0f, 0.5f);
                break;
            case CardElement.Air:
                _color = new Color(0.5f, 1.0f, 1.0f);
                break;
            default:
                break;
        }
        background.color = _color;
    }

    public void SetScaleTarget(float _value)
    {
        scaleTarget = _value;
        needToChangeScale = true;
        currentScaleTime = 0.0f;
    }

    #endregion
}
