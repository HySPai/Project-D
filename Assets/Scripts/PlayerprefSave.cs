using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerprefSave 
{
    public static float Music
    {
        get => PlayerPrefs.GetFloat("Music", 1f);
        set => PlayerPrefs.SetFloat("Music", value);
    }
    public static float Sound
    {
        get => PlayerPrefs.GetFloat("Sound", 1f);
        set => PlayerPrefs.SetFloat("Sound", value);
    }
    public static float Haptic
    {
        get => PlayerPrefs.GetFloat("Haptic", 1f);
        set => PlayerPrefs.SetFloat("Haptic", value);
    }
    public static int Level
    {
        get => PlayerPrefs.GetInt("Level", 1);
        set => PlayerPrefs.SetInt("Level", value);
    }
    public static int FirstPlay
    {
        get => PlayerPrefs.GetInt("FirstPlay", 1);
        set => PlayerPrefs.SetInt("FirstPlay", value);
    }
    public static int FirstCrate
    {
        get => PlayerPrefs.GetInt("FirstCrate", 1);
        set => PlayerPrefs.SetInt("FirstCrate", value);
    }
    public static int FirstArrow
    {
        get => PlayerPrefs.GetInt("FirstArrow", 1);
        set => PlayerPrefs.SetInt("FirstArrow", value);
    }
    public static int FirstGate
    {
        get => PlayerPrefs.GetInt("FirstGate", 1);
        set => PlayerPrefs.SetInt("FirstGate", value);
    }
}
