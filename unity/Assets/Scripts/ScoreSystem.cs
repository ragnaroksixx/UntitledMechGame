﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    public TMP_Text scoretext, highScore;
    public int score = 0;
    public static ScoreSystem instance;
    private void Awake()
    {
        instance = this;
        UpdateText();
    }

    public void NewRound()
    {
        score = 0;
        UpdateText();
    }

    public void AddToScore(int x)
    {
        score += x;
        if (score > PlayerPrefs.GetInt("highscore", 0))
            PlayerPrefs.SetInt("highscore", score);
        UpdateText();
    }

    private void UpdateText()
    {
        scoretext.text = score.ToString();
        highScore.text = PlayerPrefs.GetInt("highscore", 0).ToString();
    }
}

