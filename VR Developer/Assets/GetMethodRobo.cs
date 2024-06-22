using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class GetMethodRobo : MonoBehaviour
{
    TMP_InputField outputArea;
    TMP_InputField outputArea2;
    TMP_InputField outputArea3;
    Image energyIcon;
    Image co2Icon;
    Image tempIcon;

    // Start is a Unity method called when the script starts running
    void Start()
    {
        Debug.Log("Start method called.");

        // Find and get the UI InputField component named "OutputArea"
        outputArea = GameObject.Find("Energy1").GetComponent<TMP_InputField>();
        outputArea2 = GameObject.Find("CO21").GetComponent<TMP_InputField>();
        outputArea3 = GameObject.Find("Temp1").GetComponent<TMP_InputField>();

        // Find and get the UI Image components for icons
        energyIcon = GameObject.Find("EnergyIcon").GetComponent<Image>();
        co2Icon = GameObject.Find("CO2Icon").GetComponent<Image>();
        tempIcon = GameObject.Find("TempIcon").GetComponent<Image>();

        // Log to ensure the input fields and icons are correctly assigned
        Debug.Log($"OutputArea: {outputArea != null}, OutputArea2: {outputArea2 != null}, OutputArea3: {outputArea3 != null}");
        Debug.Log($"EnergyIcon: {energyIcon != null}, CO2Icon: {co2Icon != null}, TempIcon: {tempIcon != null}");

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
            
            // Wait for 2 minutes before fetching data again
            yield return new WaitForSeconds(120);
        }
    }

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine()
    {
        // Display "Loading..." in the UI text fields
        outputArea.text = "Loading...";
        outputArea2.text = "Loading...";
        outputArea3.text = "Loading...";

        // Define the URIs for the HTTP GET requests
        string uri1 = "http://datareader:notthatsecret777@172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22KogEN%22%20WHERE%20%22host%22%20=%20%2713318%27%20ORDER%20BY%20time%20DESC%20LIMIT%201";
        string uri2 = "http://datareader:notthatsecret777@172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22DP%22%20WHERE%20%22host%22%20=%20%27110530534%27%20ORDER%20BY%20time%20DESC%20LIMIT%201";
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

        // Process responses for each request
        ProcessResponse(requests[0], outputArea, "Total Energy: ", energyIcon);
        ProcessResponse(requests[1], outputArea2, "Total CO2: ", co2Icon);
        ProcessResponse(requests[2], outputArea3, "Total Temperature: ", tempIcon);
    }

    // Method to process each response
    void ProcessResponse(UnityWebRequest request, TMP_InputField outputArea, string label, Image icon)
    {
        if (request.isNetworkError || request.isHttpError)
        {
            // Display the error message in the UI text field
            Debug.LogError($"Error in request: {request.error}");
            outputArea.text = request.error;
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
                // Parse the matched value as double
                double value = double.Parse(match.Value);

                // Display the value in the console and in the UI
                Debug.Log(label + value);
                outputArea.text = label + value;
                icon.gameObject.SetActive(true);
            }
            else
            {
                // If no match found, display an error message
                Debug.LogError("No numerical value found in the response.");
                outputArea.text = "No numerical value found.";
                icon.gameObject.SetActive(false);
            }
        }
    }
}
