using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinySauceSDKController : SingletonMonoBehaviour<TinySauceSDKController>
{
    public bool isInitialized = false;
    void Start()
    {
#if TINY_SDK
        TinySauce.SubscribeOnInitFinishedEvent(onInitFinished);
#endif
    }

    private void onInitFinished(bool arg1, bool arg2)
    {
        isInitialized = true;
    }
    public void OnWin()
    {
#if TINY_SDK
               TinySauce.OnGameFinished(true ,0, PlayerprefSave.Level);
#endif
    }
    public void OnLose()
    {
#if TINY_SDK
        TinySauce.OnGameFinished(false, 0, PlayerprefSave.Level);
#endif
    }
}
