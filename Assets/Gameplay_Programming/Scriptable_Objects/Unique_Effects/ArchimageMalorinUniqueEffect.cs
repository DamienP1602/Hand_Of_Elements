using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Archimage_Malo'in_Unique_Effect", menuName = "Scriptable Objects/Unique Effects/Archimage Malo'in")]
public class ArchimageMalorinUniqueEffect : UniqueEffectData
{
    public override string ChangeSpecificText(CardComponent _card, string _text)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayer(_card.OwnerTag);
        int _amount = _player.AmountOfOverload * 10;

        return _text.Replace("#", _amount.ToString());
    }

    public override void ExecuteEffect(CardComponent _card)
    {
        PlayerEntity _player = GameManager.Instance.GetPlayer(_card.OwnerTag);
        _card.Data.effect.amount = _player.AmountOfOverload * 10;
    }

    public override void EventLinkedWithUniqueEffect(CardComponent _card)
    {
        PlayerEntity _owner = GameManager.Instance.GetPlayer(_card.OwnerTag);
        _owner.overloadAmountChanged += (_value) => _card.OverlayComponent.SetData(_card.Data, true);
    }
}
