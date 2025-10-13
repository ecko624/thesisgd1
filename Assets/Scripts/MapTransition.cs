using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapTransition : MonoBehaviour
{
    [Header("Map Transition Settings")]
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;

    [Header("Cutscene Settings (optional)")]
    [Tooltip("Leave empty for normal teleporters. Fill only for cutscene teleporter.")]
    [SerializeField] string cutsceneSceneName = ""; // Example: Quest1Cutscene1.2
    [SerializeField] string cutsceneKey = "Quest1CutscenePlayed"; // Unique key for this cutscene

    private CinemachineConfiner confiner;

    private enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        confiner.m_BoundingShape2D = mapBoundary;

        // ✅ If this teleporter triggers a cutscene (only if name is filled)
        if (!string.IsNullOrEmpty(cutsceneSceneName))
        {
            // Check if cutscene already played
            if (PlayerPrefs.GetInt(cutsceneKey, 0) == 1)
            {
                Debug.Log("🎬 Cutscene already played, skipping...");
                UpdatePlayerPosition(collision.gameObject); // just teleport normally
                return;
            }

            // Save current scene name and player position (after teleport)
            PlayerPrefs.SetString("NextSceneAfterCutscene", SceneManager.GetActiveScene().name);
            PlayerPrefs.SetFloat("PlayerX", collision.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", collision.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", collision.transform.position.z);
            PlayerPrefs.Save();

            // Mark this cutscene as played
            PlayerPrefs.SetInt(cutsceneKey, 1);
            PlayerPrefs.Save();

            // Load the cutscene scene
            SceneManager.LoadScene(cutsceneSceneName);
            return;
        }

        // Normal teleport
        UpdatePlayerPosition(collision.gameObject);
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            if (teleportTargetPosition != null)
                player.transform.position = teleportTargetPosition.position;
            else
                Debug.LogWarning($"⚠️ No teleport target set for {gameObject.name}");
            return;
        }

        Vector3 additivePos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                additivePos.y += 2;
                break;
            case Direction.Down:
                additivePos.y -= 2;
                break;
            case Direction.Left:
                additivePos.x -= 2;
                break;
            case Direction.Right:
                additivePos.x += 2;
                break;
        }

        player.transform.position = additivePos;
    }
}