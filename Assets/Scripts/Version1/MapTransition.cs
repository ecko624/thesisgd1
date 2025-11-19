using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapTransition : MonoBehaviour
{
    [Header("Map Transition Settings")]
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;

    [Header("Quest Requirement")]
    [Tooltip("Leave empty if no quest is required to use this teleporter.")]
    [SerializeField] string requiredQuestId = "";

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

        // Notify the persistent camera (if present) so it can persist this boundary across scenes.
        // Use the boundary GameObject name so the PersistentCamera can re-find it after scene loads.
        if (PersistentCamera.Instance != null && mapBoundary != null)
        {
            PersistentCamera.Instance.SetConfinerBoundsByName(mapBoundary.gameObject.name);
        }


        // Check if we have a quest-specific cutscene and if that quest is active
        if (!string.IsNullOrEmpty(cutsceneSceneName) && !string.IsNullOrEmpty(requiredQuestId) && QuestController.Instance.IsQuestActive(requiredQuestId))
        {
            // If we just returned from this cutscene, skip re-triggering immediately.
            // This transient flag is set when leaving and cleared here to prevent a loop.
            if (PlayerPrefs.GetInt("JustLeftForCutscene", 0) == 1)
            {
                Debug.Log("⤴️ Returning from cutscene — skipping teleporter to avoid loop.");
                PlayerPrefs.DeleteKey("JustLeftForCutscene");
                PlayerPrefs.Save();
                UpdatePlayerPosition(collision.gameObject);
                return;
            }

            // Save target scene (Version1) and player position (after teleport)
            // After the cutscene finishes, the game will load the scene stored in this key.
            // We intentionally send the player to the 'Version1' scene instead of returning
            // to the originating scene.
            PlayerPrefs.SetString("NextSceneAfterCutscene", "Version1");
            PlayerPrefs.SetFloat("PlayerX", collision.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", collision.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", collision.transform.position.z);
            PlayerPrefs.Save();

            // Mark this cutscene as played so normal flow won't replay it (unless you
            // clear the key). Also set a transient flag so when the player returns
            // we can ignore the originating teleporter once to avoid immediate retrigger.
            PlayerPrefs.SetInt(cutsceneKey, 1);
            PlayerPrefs.SetInt("JustLeftForCutscene", 1);
            PlayerPrefs.Save();

            Debug.Log($"MapTransition: Quest '{requiredQuestId}' active - Starting cutscene '{cutsceneSceneName}'. NextSceneAfterCutscene set to 'Version1'. Player position saved ({collision.transform.position.x}, {collision.transform.position.y}, {collision.transform.position.z}).");

            // Load the cutscene scene
            SceneManager.LoadScene(cutsceneSceneName);
            return;
        }

        // If no quest is active or required, just do a normal teleport
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