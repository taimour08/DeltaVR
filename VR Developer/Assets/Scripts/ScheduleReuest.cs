using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;  // Import TextMeshPro namespace

public class ScheduleRequest : MonoBehaviour
{
    // Reference to the TMP InputField component where the response will be displayed
    public TMP_InputField responseInputField;

    void Start()
    {
        // Start the coroutine to make the POST request
        StartCoroutine(PostRequest("https://ois2.ut.ee/api/timetable/room", "NAR18OH", "1020", "2024-05-02"));
    }

    IEnumerator PostRequest(string url, string building, string room, string date)
    {
        // Create a JSON object with the necessary data
        string jsonData = $"{{\"building\": \"{building}\", \"room\": \"{room}\", \"date\": \"{date}\"}}";

        // Convert the JSON string into a byte array
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Create the request
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            // Display error in the TMP InputField
            responseInputField.text = "Error: " + request.error;
        }
        else
        {
            // Process the response
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("This is the Response: " + jsonResponse);

            // Display the response in the TMP InputField
            responseInputField.text = jsonResponse;
        }
    }
}
