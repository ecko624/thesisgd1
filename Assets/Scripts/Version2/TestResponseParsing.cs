using UnityEngine;

[System.Serializable]
public class ResponseData
{
    public string response;
    public bool fallback;
    public int intimacy_delta;
}

public class TestResponseParsing : MonoBehaviour
{
    [TextArea(3, 6)]
    public string sampleJson =
        "{\"response\":\"Do you have a friend here? I'm usually nice around people. Want to hang out later this after...\",\"fallback\":false,\"intimacy_delta\":-1}";

    void Start()
    {
        try
        {
            var parsed = JsonUtility.FromJson<ResponseData>(sampleJson);
            Debug.Log($"response: {parsed.response}");
            Debug.Log($"fallback: {parsed.fallback}");
            Debug.Log($"intimacy_delta: {parsed.intimacy_delta}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Parsing failed: " + ex.Message + " — raw: " + sampleJson);
        }
    }
}
