using UnityEngine;

[RequireComponent(typeof(CardFadeComponent))]
public class HandCardComponent : CardComponent
{
    public CardFadeComponent FadeComponent { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        FadeComponent = GetComponent<CardFadeComponent>();
    }

    void Start()
    {

    }

    void Update()
    {

    }

}
