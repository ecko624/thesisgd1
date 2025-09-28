using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    // Start is called before the first frame update
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        LoadGame();
    }

    // Update is called once per frame
    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name
        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }
    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Player GameObject with tag 'Player' not found.");
            }
            else
            {
                player.transform.position = saveData.playerPosition;
            }

            var confiner = FindObjectOfType<CinemachineConfiner>();
            if (confiner == null)
            {
                Debug.LogWarning("CinemachineConfiner not found in scene.");
            }
            else
            {
                var boundaryObj = GameObject.Find(saveData.mapBoundary);
                if (boundaryObj == null)
                {
                    Debug.LogWarning($"Map boundary GameObject '{saveData.mapBoundary}' not found.");
                }
                else
                {
                    var poly = boundaryObj.GetComponent<PolygonCollider2D>();
                    if (poly == null)
                    {
                        Debug.LogWarning($"PolygonCollider2D not found on '{saveData.mapBoundary}'.");
                    }
                    else
                    {
                        confiner.m_BoundingShape2D = poly;
                    }
                }
            }
        }
        else
        {
            SaveGame();
        }
    }

}
