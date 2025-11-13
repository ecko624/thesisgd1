using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentCamera : MonoBehaviour
{
    private static PersistentCamera instance;
    public static PersistentCamera Instance => instance;

    // Last requested confiner bounds. We remember the boundary by GameObject name so we can
    // re-find it after scene loads (storing direct PolygonCollider2D references can point to
    // objects that get destroyed when scenes change).
    private string pendingBoundaryName;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Listen for scene loads so we can apply pending confiner bounds
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(pendingBoundaryName))
        {
            var found = FindBoundaryByName(pendingBoundaryName);
            if (found != null)
            {
                ApplyBoundsToConfiner(found);
            }
            else
            {
                Debug.LogWarning($"PersistentCamera: Could not find boundary named '{pendingBoundaryName}' in scene '{scene.name}'.");
            }
            // keep pendingBoundaryName so it remains the desired bounds
        }
    }

    // Public method to request a confiner bounds change. Safe to call from any script.
    // Request bounds by collider (applies now and stores name for later re-lookup)
    public void SetConfinerBounds(PolygonCollider2D bounds)
    {
        if (bounds == null)
        {
            Debug.LogWarning("PersistentCamera: SetConfinerBounds called with null bounds.");
            return;
        }

        pendingBoundaryName = bounds.gameObject.name;
        ApplyBoundsToConfiner(bounds);
    }

    // Request bounds by name: PersistentCamera will look up a PolygonCollider2D with this GameObject name
    // in the active scene and apply it immediately if found, otherwise it will apply it after the next scene load.
    public void SetConfinerBoundsByName(string boundaryName)
    {
        if (string.IsNullOrEmpty(boundaryName))
        {
            Debug.LogWarning("PersistentCamera: SetConfinerBoundsByName called with empty name.");
            return;
        }

        pendingBoundaryName = boundaryName;
        var found = FindBoundaryByName(boundaryName);
        if (found != null)
        {
            ApplyBoundsToConfiner(found);
        }
        else
        {
            Debug.Log($"PersistentCamera: Boundary '{boundaryName}' not found in current scene. Will try again on next scene load.");
        }
    }

    private void ApplyBoundsToConfiner(PolygonCollider2D bounds)
    {
        // Try to find the CinemachineConfiner in the active scene
        var confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = bounds;
            Debug.Log($"PersistentCamera: Applied confiner bounds from '{bounds.gameObject.name}' to CinemachineConfiner.");
        }
        else
        {
            Debug.Log($"PersistentCamera: No CinemachineConfiner found in scene to apply bounds from '{bounds.gameObject.name}'. Will apply on next scene load.");
        }
    }

    private PolygonCollider2D FindBoundaryByName(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) return null;
        return go.GetComponent<PolygonCollider2D>();
    }
}
