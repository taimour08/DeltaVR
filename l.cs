using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json.Linq;

public class GetMethodBD1 : MonoBehaviour
{
    TMP_InputField theOutput;
    Button nextButton;
    Button prevButton;
    public GameObject Marker; // Reference to the Marker object

    string[] outputs = new string[3];
    int currentIndex = 0;

    // Configuration variables
    float fetchInterval;
    List<string> infoLines;
    Dictionary<string, string> urls;

    // Start is a Unity method called when the script starts running
    void Start()
    {
        Debug.Log("Start method called.");

        // Load configuration from the JSON file
        LoadConfig();

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

        // Update marker position initially
        UpdateMarkerPosition();
    }

    // Method to load configuration from JSON file
    void LoadConfig()
    {
        string configPath = Path.Combine(Application.dataPath, "globalConfig.json");
        string configJson = File.ReadAllText(configPath);
       // JObject config = JObject.Parse(configJson);

        fetchInterval = (float)config["pollingRates"]["fetchInterval"];
        infoLines = config["infoLines"].Select(il => il.ToString()).ToList();
        urls = config["urls"].ToObject<Dictionary<string, string>>();
    }

    // Coroutine to fetch data periodically
    IEnumerator FetchDataPeriodically()
    {
        while (true)
        {
            Debug.Log("Fetching data...");
            // Fetch data for all URIs
            yield return StartCoroutine(GetData_Coroutine());

            // Show the first data item initially
            ShowCurrentOutput();

            // Wait for the defined interval before fetching data again
            yield return new WaitForSeconds(fetchInterval);
        }
    }

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine()
    {
        // Create an array of UnityWebRequests based on URLs from the configuration
        UnityWebRequest[] requests = new UnityWebRequest[urls.Count];
        int i = 0;
        foreach (var url in urls.Values)
        {
            requests[i++] = UnityWebRequest.Get(url);
        }

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

        // Process responses for each request and output them based on info lines
        for (int i = 0; i < requests.Length; i++)
        {
            ProcessResponse(requests[i], i, infoLines[i] + ": ");
        }
    }

    // Method to process each response
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
        UpdateMarkerPosition(); // Update marker position whenever the output changes
    }

    // Show the next output
    void ShowNext()
    {
        currentIndex = (currentIndex + 1) % outputs.Length; // division by 0.l so the value stays in bounds. 
        ShowCurrentOutput();
    }

    // Show the previous output
    void ShowPrev()
    {
        currentIndex = (currentIndex - 1 + outputs.Length) % outputs.Length;
        ShowCurrentOutput();
    }

    // Update the marker position based on the current index
    void UpdateMarkerPosition()
    {
        Vector3 newPosition;
        switch (currentIndex)
        {
            case 2:
                newPosition = new Vector3(-59.65f, 16.35f, -286.2f);
                break;
            case 1:
                newPosition = new Vector3(-60.02f, 16.35f, -286.2f);
                break;
            case 0:
                newPosition = new Vector3(-60.42f, 16.35f, -286.2f);
                break;
            default:
                newPosition = Marker.transform.position;
                break;
        }

        Marker.transform.position = newPosition;
    }
}
