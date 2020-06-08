﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Castbar : MonoBehaviour
{
    [SerializeField]
    private float myFadeoutSpeed = 0.0f;

    [SerializeField]
    private Image myCastbarUI = null;
    [SerializeField]
    private Image mySpellIconUI = null;
    [SerializeField]
    private Text mySpellNameUI = null;
    [SerializeField]
    private Text myCastTimeUI = null;

    private Coroutine fadeCoroutine;
    private CanvasGroup myCanvasGroup;

    private void Awake()
    {
        myCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowCastbar()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        myCanvasGroup.alpha = 1.0f;
    }

    public void HideCastbar()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        myCanvasGroup.alpha = 0.0f;
    }

    public void FadeOutCastbar()
    {
        fadeCoroutine = StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        float timeLeft = myFadeoutSpeed;

        while (myCanvasGroup.alpha > 0.0f)
        {
            timeLeft -= Time.deltaTime;
            myCanvasGroup.alpha = timeLeft / myFadeoutSpeed;
            yield return null;
        }
    }

    public void SetCastbarFillAmount(float aValue)
    {
        myCastbarUI.fillAmount = aValue;
    }

    public void SetSpellName(string aName)
    {
        mySpellNameUI.text = aName;
    }

    public void SetCastbarColor(Color aColor)
    {
        myCastbarUI.color = aColor;
    }

    public void SetSpellIcon(Sprite aSprite)
    {
        mySpellIconUI.sprite = aSprite;
    }

    public void SetCastTimeText(string aString)
    {
        myCastTimeUI.text = aString;
    }
}
