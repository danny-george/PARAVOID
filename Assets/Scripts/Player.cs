using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using DataManagement;
using ParavoidUI;

public class Player : MonoBehaviour 
{
    /** Notes For this Script:
    * Note to change location of file managment storage!!!!!
    * When that is completed than check to see if the files are running properly by load changes
    * And then you should be set hopefully......
    */

    #region PlayerData

    public Vector3 currentPosition;
    public string currentScene;
    public string dateAndTime;
    public string playerFileName;

    #endregion

    #region UniversalData

    public List<string> files = new List<string>();

    #endregion

    public void Awake() 
    {
        gameObject.name = "Player";
        currentScene = gameObject.scene.name;
        dateAndTime = System.DateTime.Now.ToString();

        Debug.Log("Awake: " + (PlayerCarryOverData.playerDupe != null? 
            PlayerCarryOverData.playerDupe.playerFileName : "Null"));

        if (PlayerCarryOverData.playerDupe != null) {
            Debug.Log("It Ran");
            PastePlayerData(PlayerCarryOverData.playerDupe);
        } 

        //Go ahead and make sure that when it loads the player, this object will carry over as a copy with a name change and the other will be fond to set the information to....
    }

    public void Update() 
    {
        currentPosition = transform.position; //Intended to track the player position

        if (currentScene != gameObject.scene.name 
            && gameObject.scene.name != "DontDestroyOnLoad") 
        {
            currentScene = gameObject.scene.name;
        }

    }

    #region Player Save System Methods

    public void SavePlayer(string slotName)
    {
        dateAndTime = System.DateTime.Now.ToString();
        playerFileName = slotName;

        SaveSystem.SerializePlayerData(this, slotName);

        if(!files.Contains(slotName))
        {
            files.Add(slotName);

            SaveGameFiles();
        }
    }

    public void LoadPlayer(string slotName)
    {
        PlayerData data = SaveSystem.DeserializePlayerData(slotName);

        //Load Data
        currentScene = data.currentScene;
        Debug.Log(currentScene);

        //Load File Data
        dateAndTime = data.dateAndTime;
        playerFileName = data.playerFileName;

        //Load specific objects, entities, and player positions and states
        transform.position = new Vector3(data.currentPosition[0], data.currentPosition[1], data.currentPosition[2]);

        //Load the scene
        PlayerCarryOverData.UpdatePlayerData(this);
        Debug.Log("Loader: " + (PlayerCarryOverData.playerDupe != null? 
            PlayerCarryOverData.playerDupe.playerFileName : "Null"));
        SceneLoader.LoadScene(currentScene);
    }

    public void PastePlayerData(Player player)
    {
        PlayerData data = SaveSystem.DeserializePlayerData(player.playerFileName);

        //Load Data
        currentScene = data.currentScene;

        //Load File Data
        dateAndTime = data.dateAndTime;
        playerFileName = data.playerFileName;

        //Load specific objects, entities, and player positions and states
        transform.position = new Vector3(data.currentPosition[0], data.currentPosition[1], data.currentPosition[2]);
    }

    public void ErasePlayer(string slotName)
    {
        SaveSystem.SerializePlayerData(null, slotName);
    }

    public void DeletePlayer(string slotName)
    {
        SaveSystem.DeleteFile(slotName);
        files.Remove(slotName);
        //Destroy(saveContent.transform.Find("SaveSlot (" + slotName + ")").gameObject); --Make a call to SlotManager

        SaveGameFiles();
    }

    #endregion

    #region GameFiles

    public void SaveGameFiles()
    {
        SaveSystem.SerializeGameFiles(null, this);
    }

    public void LoadGameFiles()
    {   
        UniversalData data = SaveSystem.DeserializeGameFiles();

        if (data == null)
        {
            Debug.Log("Adding Universal Data");
            SaveGameFiles();
            data = SaveSystem.DeserializeGameFiles();
        }

        foreach (string file in data.files)
        {
            if (SaveSystem.doesFileExist(file))
                files.Add(file);
            else
            {
                SaveSystem.SerializePlayerData(null, file);
                files.Add(file);
            }
                
        }
    }

    #endregion
}
