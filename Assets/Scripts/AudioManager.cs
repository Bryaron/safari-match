using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
    }

    public AudioClip moveSFX;
    public AudioClip missSFX;
    public AudioClip matchSFX;
    public AudioClip gameOverSFX;

    public AudioSource SFXSource;

    private void Start() {
        GameManager.instance.OnPointsUpdated.AddListener(PointsUpdated);
        GameManager.instance.OnGameStateUpdated.AddListener(GamesStateUpdated);
    }

    private void OnDestroy() {
        GameManager.instance.OnPointsUpdated.RemoveListener(PointsUpdated);
        GameManager.instance.OnGameStateUpdated.RemoveListener(GamesStateUpdated);
    }

    private void GamesStateUpdated(GameManager.GameState newState) {
        if (newState == GameManager.GameState.GameOver) {
            SFXSource.PlayOneShot(gameOverSFX);
        }

        if (newState == GameManager.GameState.InGame) {
            SFXSource.PlayOneShot(matchSFX);
        }
    }

    private void PointsUpdated() {
        SFXSource.PlayOneShot(matchSFX);
    }

    public void Move() {
        SFXSource.PlayOneShot(moveSFX);
    }

    public void Miss() {
        SFXSource.PlayOneShot(missSFX);
    }
}
