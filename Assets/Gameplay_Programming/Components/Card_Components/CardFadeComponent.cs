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

    public CanvasGroup Group { get; private set; }
    [Header("Parameters")]
    [SerializeField] FadeStatus fadeStatus = FadeStatus.None;
    [SerializeField] float fadeSpeed = 1.0f;
    [SerializeField] bool callServerRpcAfterFade;
    Action actionToTrigger;

    #region Setters

    public void SetFade(FadeStatus _fade, Action _action = null, bool _callServerRpc = false)
    {
        fadeStatus = _fade;
        actionToTrigger = null;

        if (_action != null)
        {
            actionToTrigger += actionToTrigger;
            callServerRpcAfterFade = _callServerRpc;
        }
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
            if (callServerRpcAfterFade)
                Action_ServerRpc();
            else
                actionToTrigger?.Invoke();
        }
    }

    void FadeOutUpdate()
    {
        Group.alpha -= Time.deltaTime * fadeSpeed;

        if (Group.alpha <= 0.0f)
        {
            fadeStatus = FadeStatus.None;
            if (callServerRpcAfterFade)
                Action_ServerRpc();
            else
                actionToTrigger?.Invoke();
        }
    }

    #endregion

    #region ServerRpc

    [ServerRpc]
    void Action_ServerRpc()
    {
        actionToTrigger?.Invoke();
    }

    #endregion

}
