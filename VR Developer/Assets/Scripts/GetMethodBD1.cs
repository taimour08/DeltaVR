using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

// Define the Config class to match the structure of the JSON file
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
public class Config
{
    public PollingRates pollingRates;
    public string[] infoLines;
    public Urls[] urlSets;
}

public class GetMethodBD1 : MonoBehaviour
{
    TMP_InputField theOutput;
    Button nextButton;
    Button prevButton;
    public GameObject Marker; // Reference to the Marker object
    public string outputName = "TheOutput";
    public string nextName = "Next";
    public string prevName = "Prev";


    string[] outputs;
    int currentIndex = 0;

    // Configuration fields
    Config config;
    Urls urls;
    string[] infoLines;
    int fetchInterval;

    // Field to specify which set of URLs this instance should use
    public int urlSetIndex = 0;

    // Start is a Unity method called when the script starts running
    public void Start()
    {
        Debug.Log("Start method called.");

        // Load the configuration
        LoadConfig();

        // Initialize the outputs array based on the number of infoLines
        outputs = new string[infoLines.Length];

        // Find and get the UI InputField component named "TheOutput"
        theOutput = GameObject.Find(outputName).GetComponent<TMP_InputField>();

        // Find the buttons and add listeners
        nextButton = GameObject.Find(nextName).GetComponent<Button>();
        prevButton = GameObject.Find(prevName).GetComponent<Button>();

        nextButton.onClick.AddListener(ShowNext);
        prevButton.onClick.AddListener(ShowPrev);

        // Log to ensure the input field and buttons are correctly assigned
        Debug.Log($"TheOutput: {theOutput != null}, NextButton: {nextButton != null}, PrevButton: {prevButton != null}");

        // Start the coroutine to fetch data periodically
        StartCoroutine(FetchDataPeriodically());

        // Update marker position initially
        UpdateMarkerPosition();
    }

    // this function takes data from the config, converts it into text, parses it and puts it into the relevant variables declared above
void LoadConfig()
{
    // Load the JSON file from the Assets folder
    string path = Path.Combine(Application.dataPath, "GlobalConfig.json");
    
    if (!File.Exists(path))
    {
        Debug.LogError($"GlobalConfig.json file not found at path: {path}");
        return;
    }

    string jsonString = File.ReadAllText(path);

    if (string.IsNullOrEmpty(jsonString))
    {
        Debug.LogError("GlobalConfig.json file is empty or could not be read.");
        return;
    }

    // Parse the JSON data   ** Hypothesis is that the problem lies here (enhance skills)
    config = JsonUtility.FromJson<Config>(jsonString);

    if (config == null)
    {
        Debug.LogError("Failed to parse GlobalConfig.json file.");
        return;
    }

    Debug.Log($"Parsed Config: {JsonUtility.ToJson(config)}");

    if (config.urlSets == null || config.urlSets.Length == 0)
    {
        Debug.LogError("No URL sets found in GlobalConfig.json file.");
        return;
    }

    if (urlSetIndex < 0 || urlSetIndex >= config.urlSets.Length)
    {
        Debug.LogError($"Invalid urlSetIndex: {urlSetIndex}. It should be between 0 and {config.urlSets.Length - 1}");
        return;
    }

    // Extract configuration values
    fetchInterval = config.pollingRates.fetchInterval;
    infoLines = config.infoLines;
    urls = config.urlSets[urlSetIndex];

    Debug.Log($"Config loaded successfully. Fetch Interval: {fetchInterval}, URL Set Index: {urlSetIndex}");
    Debug.Log($"URL Set: Energy = {urls.Energy}, CO2 = {urls.CO2}, Temperature = {urls.Temperature}");
}


    // Coroutine to fetch data periodically
    IEnumerator FetchDataPeriodically()
    {
        while (true)
        {
            Debug.Log("Fetching data...");
            // Fetch data for all configured URIs
            yield return StartCoroutine(GetData_Coroutine());

            // Show the first data item initially
            ShowCurrentOutput();

            // Wait for the configured interval before fetching data again
            yield return new WaitForSeconds(fetchInterval);
        }
    }

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine()
    {
        // Create an array of UnityWebRequests
        UnityWebRequest[] requests = new UnityWebRequest[]
        {
            UnityWebRequest.Get(urls.Energy),
            UnityWebRequest.Get(urls.CO2),
            UnityWebRequest.Get(urls.Temperature)
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
        currentIndex = (currentIndex + 1) % outputs.Length; // division by o.l so the value stays in bounds. 
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
