using System.IO;
using UnityEngine;

public class SaveController
{
    private string saveLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Define save Location
        saveLocation = Path.Combine(Application.persistentDataPath, "SaveData.jpon");
    }


    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {

        }













    }











































       






