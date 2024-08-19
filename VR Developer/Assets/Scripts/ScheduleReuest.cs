using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
//using Newtonsoft.Json.Linq; // For JSON parsing (optional, use if needed)


public class ScheduleRequest : MonoBehaviour
{
    void Start()
    {
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
        }
        else
        {
            // Process the response
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("This is the Response: " + jsonResponse);

            // Optionally, parse the JSON response
            // For example, if the response is a JSON object:
            // var data = JObject.Parse(jsonResponse);
            // string someValue = data["someKey"].ToString();

            // Further processing of the received data
        }
    }
}
