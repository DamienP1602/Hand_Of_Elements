using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CardFadeComponent : MonoBehaviour
{
    [Serializable]
    public enum FadeStatus
    {
        FadeIn,
        FadeOut,
        None
    }

    public CanvasGroup Group { get; private set; }
    [SerializeField] FadeStatus fadeStatus = FadeStatus.None;
    [SerializeField] float fadeSpeed = 1.0f;

    public void SetFade(FadeStatus _fade)
    {
        fadeStatus = _fade;
    }

    private void Awake()
    {
        Group = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (fadeStatus != FadeStatus.None)
        {
            float _value = Time.deltaTime * (fadeStatus == FadeStatus.FadeIn ? 1.0f : -1.0f) * fadeSpeed;
            Group.alpha += _value;

            if (fadeStatus == FadeStatus.FadeIn && Group.alpha >= 1.0f)
                fadeStatus = FadeStatus.None;

            if (fadeStatus == FadeStatus.FadeOut && Group.alpha <= 0.0f)
                fadeStatus = FadeStatus.None;
        }
    }

}
