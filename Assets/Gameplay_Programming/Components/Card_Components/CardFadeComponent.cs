using System;
using Unity.Netcode;
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

    public event Action OnFadeFinish;
    public CanvasGroup Group { get; private set; }
    [Header("Parameters")]
    [SerializeField] FadeStatus fadeStatus = FadeStatus.None;
    [SerializeField] float fadeSpeed = 1.0f;
    [SerializeField] bool callServerRpcAfterFade;

    #region Setters

    public void SetFade(FadeStatus _fade)
    {
        fadeStatus = _fade;
    }

    #endregion

    private void Awake()
    {
        Group = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (fadeStatus == FadeStatus.FadeIn)
            FadeInUpdate();

        if (fadeStatus == FadeStatus.FadeOut)
            FadeOutUpdate();
    }

    #region Update

    void FadeInUpdate()
    {
        Group.alpha += Time.deltaTime * fadeSpeed;

        if (Group.alpha >= 1.0f)
        {
            fadeStatus = FadeStatus.None;
            OnFadeFinish?.Invoke();
        }
    }

    void FadeOutUpdate()
    {
        Group.alpha -= Time.deltaTime * fadeSpeed;

        if (Group.alpha <= 0.0f)
        {
            fadeStatus = FadeStatus.None;
            OnFadeFinish?.Invoke();
        }
    }

    #endregion

}
