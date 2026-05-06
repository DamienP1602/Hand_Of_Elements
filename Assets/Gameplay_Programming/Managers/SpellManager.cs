using System;
using UnityEngine;

public class SpellManager : Singleton<SpellManager>
{
    [SerializeField] CardEffectData currentEffect;

    #region Server Functions

    /// <summary>
    /// Server Function
    /// </summary>
    public void LaunchEffect(int _cardID, PlayerEnum _ownerType)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayer(_ownerType);
        HandCardComponent _card = _player.HandComponent.GetSelectedCard();
        currentEffect = _card.Data.effect;

        if (currentEffect.selectionMode == CardEffectData.CardEffectSelectionMode.SingleTarget)
        {
            _player.InteractComponent.SetSelectCard(true);
            return;
        }
        else
        {

        }
    }

    /// <summary>
    /// Server Function
    /// </summary>
    public void ComputeEffect(int _selectedCardID, PlayerEnum _ownerType)
    {
        BoardSlotComponent _card = GameManager.Instance.Board.GetCardFromID(_ownerType, _selectedCardID);

    }

    #endregion
}
