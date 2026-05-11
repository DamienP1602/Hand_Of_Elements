
using UnityEngine;

public class HandCardComponent : CardComponent
{
    void Start()
    {
        isHovered.OnValueChanged += (_old, _new) => OnHovered(_new);
    }

    void Update()
    {

    }

    void OnHovered(bool _value)
    {
        transform.localScale = Vector3.one * (_value ? 1.1f : 1.0f);
    }
}
