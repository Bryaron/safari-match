using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIPoints : MonoBehaviour {
    int displayedPoints = 0;
    public TextMeshProUGUI pointsLabel;

    private void Start() {
        GameManager.instance.OnPointsUpdated.AddListener(UpdatePoints);
        GameManager.instance.OnGameStateUpdated.AddListener(GameStateUpdated);
    }

    private void OnDestroy() {
        GameManager.instance.OnPointsUpdated.RemoveListener(UpdatePoints);
        GameManager.instance.OnGameStateUpdated.RemoveListener(GameStateUpdated);
    }

    private void GameStateUpdated(GameManager.GameState newState) {
        if(newState == GameManager.GameState.GameOver) {
            displayedPoints = 0;
            pointsLabel.text = displayedPoints.ToString();
        }
    }

    private void UpdatePoints() {
        StartCoroutine(UpdatePointsCoroutine());
    }

    IEnumerator UpdatePointsCoroutine() {
        while(displayedPoints < GameManager.instance.Points) {
            displayedPoints++;
            pointsLabel.text = displayedPoints.ToString();
            yield return new WaitForSeconds(0.1f);
        }
        yield return null;
    }
}
