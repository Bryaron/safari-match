using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIScreen : MonoBehaviour {

    public RectTransform containerRect;
    public CanvasGroup containerCanvas;
    public Image background;
    public GameManager.GameState visibleState;
    public float transitionTime;

    private void Start() {
        GameManager.instance.OnGameStateUpdated.AddListener(GameStateUpdated);
        bool initialState = GameManager.instance.gameState == visibleState;
        background.enabled = initialState;
        containerRect.gameObject.SetActive(initialState);
    }

    private void GameStateUpdated(GameManager.GameState newState) {
        if(newState == visibleState) {
            ShowScreen();

        }
        else {
            HideScreen();
        }
    }

    private void HideScreen() {
        //background animation
        var bgColor = background.color;
        bgColor.a = 0;
        background.DOColor(bgColor, transitionTime * 0.5f);
        //container
        containerCanvas.alpha = 1;
        containerRect.anchoredPosition = Vector2.zero;
        containerCanvas.DOFade(0f, transitionTime * 0.5f);
        containerRect.DOAnchorPos(new Vector2(0, -100), transitionTime * 0.5f).onComplete = () =>{
            background.enabled = false;
            containerRect.gameObject.SetActive(false);
        };
    }

    private void ShowScreen() {
        //enable eleents
        background.enabled = true;
        containerRect.gameObject.SetActive(true);
        //background animation
        var bgColor = background.color;
        bgColor.a = 0;
        background.color = bgColor;
        bgColor.a = 1;
        background.DOColor(bgColor, transitionTime);
        //Container animation
        containerCanvas.alpha = 0;
        containerRect.anchoredPosition = new Vector2(0, 100);
        containerCanvas.DOFade(1f, transitionTime);
        containerRect.DOAnchorPos(Vector2.zero, transitionTime);
    }
}
