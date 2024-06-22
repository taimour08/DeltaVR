using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class GetMethodWeathe : MonoBehaviour
{
    TMP_InputField outputArea;

    // Start is a Unity method called when the script starts running
    void Start()
    {
        // Find and get the UI InputField component named "OutputArea"
        outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        // Find the UI Button component named "GetButton" and add a listener for the click event to call the GetData method
        GameObject.Find("GetButton").GetComponent<Button>().onClick.AddListener(GetData);
    }

    // GetData method triggered when the button is clicked
    void GetData() => StartCoroutine(GetData_Coroutine());

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine()
    {
        // Display "Loading..." in the UI text field
        outputArea.text = "Loading...";

        // Define the URI for the HTTP GET request
        string uri = "https://api.openweathermap.org/data/2.5/weather?q=Tartu&appid=fc2dd765b55ad13fd78e622ee10ebf97";
        //string uri = "https://datareader:notthatsecret777@172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22KogEN%22%20WHERE%20time%20%3E%3D%20%272024-01-01%27%20AND%20time%20%3C%3D%20%272024-01-02%27";

        // Set plain text HTTP connection option
       // UnityWebRequest.allowPlainHttp = UnityWebRequest.PlainHttp.Launch;

        // Create a UnityWebRequest for the specified URI
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            // Send the request and wait for a response
            yield return request.SendWebRequest();

            // Check if there's a network or HTTP error
            if (request.isNetworkError || request.isHttpError)
                // Display the error message in the UI text field
                outputArea.text = request.error;
            else
                // Display the downloaded text in the UI text field
                outputArea.text = request.downloadHandler.text;
        }
    }
}
