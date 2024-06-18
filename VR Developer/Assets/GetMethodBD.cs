using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class GetMethodBD: MonoBehaviour
{
    TMP_InputField outputArea;
    TMP_InputField outputArea2;
    TMP_InputField outputArea3;

    // Start is a Unity method called when the script starts running
    void Start()
    {
        // Find and get the UI InputField component named "OutputArea"
        outputArea = GameObject.Find("EnergyOutput").GetComponent<TMP_InputField>();
        outputArea2 = GameObject.Find("CO2Output").GetComponent<TMP_InputField>();
        outputArea3 = GameObject.Find("TempOutput").GetComponent<TMP_InputField>();
        
        // Start the coroutine to fetch data periodically
        StartCoroutine(FetchDataPeriodically());
    }

    // Coroutine to fetch data periodically every 2 minutes
    IEnumerator FetchDataPeriodically()
    {
        while (true)
        {
            // Fetch data
            yield return StartCoroutine(GetData_Coroutine());
            
            // Wait for 2 minutes before fetching data again
            yield return new WaitForSeconds(120);
        }
    }

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine() // so the game doesn't freeze 
    {
        // Display "Loading..." in the UI text field
        outputArea.text = "Loading...";
        outputArea2.text = "Loading...";


        // Define the URI for the HTTP GET request
        string uri1 = "http://172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22KogEN%22%20ORDER%20BY%20time%20DESC%20LIMIT%201";
        string uri2 = "http://172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22KogEN%22%20ORDER%20BY%20time%20DESC%20LIMIT%201";
        string uri3 = "http://172.17.67.20:8086/query?db=delta&q=SELECT%20*%20FROM%20%22TSu%22%20ORDER%20BY%20time%20DESC%20LIMIT%201";


        UnityWebRequest[] requests = new UnityWebRequest[]
        {
            UnityWebRequest.Get(uri1),
            UnityWebRequest.Get(uri2),
            UnityWebRequest.Get(uri3)
        };

        // Create a UnityWebRequest for the specified URI
        using (UnityWebRequest request = UnityWebRequest.Get(uri1)) // build the request
        {
            // Send the request and wait for a response
            yield return request.SendWebRequest();  

            // Check if there's a network or HTTP error
            if (request.isNetworkError || request.isHttpError)
            {
                // Display the error message in the UI text field
                outputArea.text = request.error;
            }
            else
            {
                // Display the downloaded text in the UI text field
                string jsonAsText = request.downloadHandler.text;
                // Regular expression to match the last floating point number after all alphabets
                string pattern = @"(?<=\D|^)\d+\.\d+(?!.*\d+\.\d+)";
                
                // Match the pattern in the JSON text
                Match match = Regex.Match(jsonAsText, pattern);
                
                if (match.Success)
                {
                    // Parse the matched value as double
                    double value = double.Parse(match.Value);

                    // Display the value in the console and in the UI
                    Debug.Log("Total Energy: " + value);
                    outputArea.text = "Total Energy: " + value;
                    
                }
                else
                {
                    // If no match found, display an error message
                    Debug.LogError("No numerical value found in the response.");
                    outputArea.text = "No numerical value found.";
                }
            }
        }
    }
}
