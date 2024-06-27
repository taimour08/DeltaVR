using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class GetMethodBD2 : MonoBehaviour
{
    TMP_InputField outputArea;
    Button nextButton;
    Button prevButton;

    GlobalConfig config;
    int currentIndex = 0;

    void Start()
    {
        // Load configuration
        LoadConfig();

        // Find and get the UI components
        outputArea = GameObject.Find("TheOutput").GetComponent<TMP_InputField>();
        nextButton = GameObject.Find("NextButton").GetComponent<Button>();
        prevButton = GameObject.Find("PrevButton").GetComponent<Button>();

        // Log to ensure the input field and buttons are correctly assigned
        Debug.Log($"OutputArea: {outputArea != null}, NextButton: {nextButton != null}, PrevButton: {prevButton != null}");

        // Add listeners to buttons
        nextButton.onClick.AddListener(ShowNext);
        prevButton.onClick.AddListener(ShowPrev);

        // Start the coroutine to fetch data periodically
        StartCoroutine(FetchDataPeriodically());
    }


// Reads the configuration file and parses it into a GlobalConfig object.
    void LoadConfig()
    {
        string configFilePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        string json = File.ReadAllText(configFilePath);
        config = JsonUtility.FromJson<GlobalConfig>(json);
    }

    // Coroutine to fetch data periodically
    IEnumerator FetchDataPeriodically()
    {
        while (true)
        {
            Debug.Log("Fetching data...");
            yield return StartCoroutine(GetData_Coroutine());
            yield return new WaitForSeconds(config.pollingRates.fetchInterval);
        }
    }

    IEnumerator GetData_Coroutine()
    {
        outputArea.text = "Loading...";
        UnityWebRequest[] requests = new UnityWebRequest[]
        {
            UnityWebRequest.Get(config.urls.Energy),
            UnityWebRequest.Get(config.urls.CO2),
            UnityWebRequest.Get(config.urls.Temperature)
        };

        foreach (var request in requests)
        {
            request.SendWebRequest();
        }

        foreach (var request in requests)
        {
            while (!request.isDone)
            {
                yield return null;
            }
        }

        ProcessResponse(requests[0], config.infoLines[0]);
        ProcessResponse(requests[1], config.infoLines[1]);
        ProcessResponse(requests[2], config.infoLines[2]);
    }

    void ProcessResponse(UnityWebRequest request, string label)
    {
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"Error in request: {request.error}");
            outputArea.text = request.error;
        }
        else
        {
            string jsonAsText = request.downloadHandler.text;
            Debug.Log($"Response for {label}: {jsonAsText}");

            string pattern = @"(?<=\D|^)\d+\.\d+(?!.*\d+\.\d+)";
            Match match = Regex.Match(jsonAsText, pattern);

            if (match.Success)
            {
                double value = double.Parse(match.Value);
                value = (int)(double)value;

                Debug.Log(label + value);
                outputArea.text = label + ": " + value;
            }
            else
            {
                Debug.LogError("No numerical value found in the response.");
                outputArea.text = "No numerical value found.";
            }
        }
    }

    void ShowNext()
    {
        currentIndex = (currentIndex + 1) % config.infoLines.Length;
        DisplayCurrentInfo();
    }

    void ShowPrev()
    {
        currentIndex = (currentIndex - 1 + config.infoLines.Length) % config.infoLines.Length;
        DisplayCurrentInfo();
    }


// Switch? 
    void DisplayCurrentInfo()
    {
        switch (currentIndex)
        {
            case 0:
                StartCoroutine(GetData_Coroutine());
                break;
            case 1:
                StartCoroutine(GetData_Coroutine());
                break;
            case 2:
                StartCoroutine(GetData_Coroutine());
                break;
        }
    }
}

[Serializable]
public class PollingRates
{
    public int fetchInterval;
}

[Serializable]
public class Urls
{
    public string Energy;
    public string CO2;
    public string Temperature;
}

[Serializable]
public class GlobalConfig
{
    public PollingRates pollingRates;
    public string[] infoLines;
    public Urls urls;
}
