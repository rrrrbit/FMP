using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
public class SaveSystem : MonoBehaviour
{
    public void SavePrefs()
    {
        PlayerPrefs.SetInt("Volume", 50);
        PlayerPrefs.Save();

    }

    public void LoadPrefs()
    {
        int volume = PlayerPrefs.GetInt("Volume", 0);
    }




}
