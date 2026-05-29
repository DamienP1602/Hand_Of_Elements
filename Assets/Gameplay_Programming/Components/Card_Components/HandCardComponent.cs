
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
        transform.localScale = Vector3.one * (_value ? 2.0f : 1.0f);
        Vector3 _offset = Vector3.forward * 2.0f;
        transform.localPosition += _offset * (_value ? 1.0f : -1.0f);
    }
}
