﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartGameButton : GameButton
{

    public WaveController waveController;

    public override void PerformAction()
    {
        AudioManager.Play("StartGame");
        waveController.PlayWave();
        gameObject.SetActive(false);
    }
}
