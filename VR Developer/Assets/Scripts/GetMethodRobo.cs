using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;


/*    MAIN IDEA
    - Uses a string array. process function to store the final string in indices. 
    - Use the three functions to change the value of the index that is currently show on the screen
*/

public class GetMethodRobo: MonoBehaviour
{
    TMP_InputField theOutput;
    Button nextButton;
    Button prevButton;

    string[] outputs = new string[3];
    int currentIndex = 0;

    // Start is a Unity method called when the script starts running
    void Start()
    {
        Debug.Log("Start method called.");

        // Find and get the UI InputField component named "TheOutput"
        theOutput = GameObject.Find("TheOutput").GetComponent<TMP_InputField>();

        // Find the buttons and add listeners
        nextButton = GameObject.Find("Next").GetComponent<Button>();
        prevButton = GameObject.Find("Prev").GetComponent<Button>();

        nextButton.onClick.AddListener(ShowNext);
        prevButton.onClick.AddListener(ShowPrev);

        // Log to ensure the input field and buttons are correctly assigned
        Debug.Log($"TheOutput: {theOutput != null}, NextButton: {nextButton != null}, PrevButton: {prevButton != null}");

        // Start the coroutine to fetch data periodically
        StartCoroutine(FetchDataPeriodically());
    }

    // Coroutine to fetch data periodically every 2 minutes
    IEnumerator FetchDataPeriodically()
    {
        while (true)
        {
            Debug.Log("Fetching data...");
            // Fetch data for all three URIs
            yield return StartCoroutine(GetData_Coroutine());

            // Show the first data item initially
            ShowCurrentOutput();

            // Wait for 2 minutes before fetching data again
            yield return new WaitForSeconds(120);
        }
    }

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine()
    {
        // Define the URIs for the HTTP GET requests
        // put host names for iot lab devices
        string uri1 = "http://datareader:notthatsecret777@172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22KogEN%22%20WHERE%20%22host%22%20=%20%2713318%27%20ORDER%20BY%20time%20DESC%20LIMIT%201";
        string uri2 = "http://datareader:notthatsecret777@172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22DP%22%20WHERE%20%22host%22%20=%20%27110530530%27%20ORDER%20BY%20time%20DESC%20LIMIT%201";
        string uri3 = "http://datareader:notthatsecret777@172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22TSu%22%20ORDER%20BY%20time%20DESC%20LIMIT%201";

        // Create an array of UnityWebRequests
        UnityWebRequest[] requests = new UnityWebRequest[]
        {
            UnityWebRequest.Get(uri1),
            UnityWebRequest.Get(uri2),
            UnityWebRequest.Get(uri3)
        };

        // Send all requests and wait for responses
        yield return StartCoroutine(SendRequests(requests));
    }

    // Coroutine to send multiple requests and handle responses
    IEnumerator SendRequests(UnityWebRequest[] requests)
    {
        // Send all requests in parallel
        foreach (var request in requests)
        {
            request.SendWebRequest();
        }

        // Wait for all requests to complete
        foreach (var request in requests)
        {
            while (!request.isDone)
            {
                yield return null;
            }
        }

        // Process responses for each request. Put output strings in each of the index of string list 
        ProcessResponse(requests[0], 0, "Total Energy: ");
        ProcessResponse(requests[1], 1, "Total CO2: ");
        ProcessResponse(requests[2], 2, "Temperature: ");
    }

    // Method to process each response - (Get the output of the request)
    void ProcessResponse(UnityWebRequest request, int index, string label)
    {
        if (request.isNetworkError || request.isHttpError)
        {
            // Display the error message in the UI text field
            Debug.LogError($"Error in request: {request.error}");
            outputs[index] = $"{request.error}";
        }
        else
        {
            // Display the downloaded text in the UI text field
            string jsonAsText = request.downloadHandler.text;
            Debug.Log($"Response for {label}: {jsonAsText}"); 

            // Regular expression to match the last floating point number after all alphabets
            string pattern = @"(?<=\D|^)\d+\.\d+(?!.*\d+\.\d+)";
                
            // Match the pattern in the JSON text
            Match match = Regex.Match(jsonAsText, pattern);
                
            if (match.Success)
            {
                // Parse (Convert) the matched value as double
                double value = double.Parse(match.Value);
                int intValue = (int)value;

                // Display the value in the console and in the UI
                Debug.Log(label + intValue);
                outputs[index] = $"{label}{intValue}";
            }
            else
            {
                // If no match found, display an error message
                Debug.LogError("No numerical value found in the response.");
                outputs[index] = "No numerical value found.";
            }
        }
    }

  

    // Show the current output based on the currentIndex
    void ShowCurrentOutput()
    {
        theOutput.text = outputs[currentIndex];
    }

    // Show the next output
    void ShowNext()
    {
        currentIndex = (currentIndex + 1) % outputs.Length; // division by o.l so the value stays in bounds. 
        ShowCurrentOutput();
    }

    // Show the previous output
    void ShowPrev()
    {
        currentIndex = (currentIndex - 1 + outputs.Length) % outputs.Length;
        ShowCurrentOutput();
    }
}
