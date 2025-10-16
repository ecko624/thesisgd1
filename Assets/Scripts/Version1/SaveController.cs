using System.Collections;
using TMPro;
using UnityEngine;
using System.IO;
using Cinemachine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    public TMP_Text saveMessage; // drag your TextMeshPro UI element here in the Inspector

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name
        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));

        // ✅ Show message and start fading
        if (saveMessage != null)
        {
            saveMessage.text = "Game Saved!";
            saveMessage.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay(1f)); // fade after 1 second
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade out effect (optional)
        float fadeTime = 1f;
        float t = 0;
        Color originalColor = saveMessage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            saveMessage.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, t / fadeTime));
            yield return null;
        }

        saveMessage.gameObject.SetActive(false);
        saveMessage.color = originalColor; // reset alpha for next time
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
            FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D =
                GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();
        }
        else
        {
            SaveGame();
        }
    }
}
