using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGameOver : MonoBehaviour {

    public int displayedPoints;
    public TextMeshProUGUI pointsUI;

    private void Start() {
        GameManager.instance.OnGameStateUpdated.AddListener(GameStateUpdated);
    }

    private void OnDestroy() {
        GameManager.instance.OnGameStateUpdated.RemoveListener(GameStateUpdated);
    }

    private void GameStateUpdated(GameManager.GameState newState) {
        if (newState == GameManager.GameState.GameOver) {
            displayedPoints = 0;
            StartCoroutine(DisplayPointsCoroutine());
        }
    }
    
    IEnumerator DisplayPointsCoroutine() {
        while(displayedPoints < GameManager.instance.Points) {
            displayedPoints++;
            pointsUI.text = displayedPoints.ToString();
            //Esperando un frame
            yield return new WaitForFixedUpdate();
        }

        displayedPoints = GameManager.instance.Points;
        pointsUI.text = displayedPoints.ToString();

        yield return null;
    }

    public void PlayAgainBtnClicked() {
        GameManager.instance.RestartGame();
    }

    public void ExitGameBtnClicked() {
        GameManager.instance.ExitGame();

    }
}
