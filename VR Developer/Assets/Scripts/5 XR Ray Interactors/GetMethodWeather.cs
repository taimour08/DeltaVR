using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public class OpenWeather
{
    public string url;
}

[System.Serializable]
public class OConfig
{
    public OpenWeather openWeather;
}

[System.Serializable]
public class Coord
{
    public float lon;
    public float lat;
}

[System.Serializable]
public class Weather
{
    public int id;
    public string main;
    public string description;
    public string icon;
}

[System.Serializable]
public class Main
{
    public float temp;
    public float feels_like;
    public float temp_min;
    public float temp_max;
    public int pressure;
    public int humidity;
    public int sea_level;
    public int grnd_level;
}

[System.Serializable]
public class Wind
{
    public float speed;
    public int deg;
}

[System.Serializable]
public class Clouds
{
    public int all;
}

// same way of catching JSON in C#
// above are the same as the urls in pf
// this is the same as Config in previous files

[System.Serializable]
public class WeatherData
{
    public Coord coord;
    public Weather[] weather;
    public string @base;
    public Main main;
    public int visibility;
    public Wind wind;
    public Clouds clouds;
    public string name;
    public int cod;
}


public class GetMethodWeather : MonoBehaviour
{
    TMP_InputField outputArea;
    string weatherUrl;

    // Start is a Unity method called when the script starts running
    void Start()
    {
        // Find and get the UI InputField component named "OutputArea"
        outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        // Find the UI Button component named "GetButton" and add a listener for the click event to call the GetData method
        GameObject.Find("GetButton").GetComponent<Button>().onClick.AddListener(GetData);

        // Load the configuration file to get the weather URL
        LoadConfig();
    }

    void LoadConfig()
    {
        // Path to the JSON configuration file
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

        Debug.Log($"JSON String: {jsonString}");

        // Parse the JSON data
        OConfig config = JsonUtility.FromJson<OConfig>(jsonString);

        if (config == null || config.openWeather == null || string.IsNullOrEmpty(config.openWeather.url))
        {
            Debug.LogError("Failed to parse GlobalConfig.json file or missing openWeather URL.");
            return;
        }

        // Set the weather URL
        weatherUrl = config.openWeather.url;
    }

    // GetData method triggered when the button is clicked
    void GetData() => StartCoroutine(GetData_Coroutine());

    // Coroutine for handling data retrieval asynchronously
    IEnumerator GetData_Coroutine()
{
    // Display "Loading..." in the UI text field with a lightning bolt icon
    outputArea.text = "⚡ Loading...";

    if (string.IsNullOrEmpty(weatherUrl))
    {
        outputArea.text = "Weather URL not configured.";
        yield break;
    }

    // Create a UnityWebRequest for the specified URI
    using (UnityWebRequest request = UnityWebRequest.Get(weatherUrl))
    {
        // Send the request and wait for a response
        yield return request.SendWebRequest();

        // Check if there's a network or HTTP error
        if (request.isNetworkError || request.isHttpError)
        {
            // Display the error message in the UI text field with a lightning bolt icon
            outputArea.text = request.error;
        }
        else
        {
            // Parse the JSON response
            string jsonResponse = request.downloadHandler.text;
            WeatherData weatherData = JsonUtility.FromJson<WeatherData>(jsonResponse);

            // Format and display the weather information
            string weatherInfo = $"Weather: {weatherData.weather[0].description}\n" +
                                 $"Temp: {weatherData.main.temp} K\n" +
                                 $"Temp feels like: {weatherData.main.feels_like} K\n" +
                                 $"Temp min: {weatherData.main.temp_min} K\n" +
                                 $"Temp max: {weatherData.main.temp_max} K\n" +
                                 $"Pressure: {weatherData.main.pressure} hPa\n" +
                                 $"Humidity: {weatherData.main.humidity}%\n" +
                                 $"Sea level: {weatherData.main.sea_level} hPa\n" +
                                 $"Ground level: {weatherData.main.grnd_level} hPa\n" +
                                 $"Visibility: {weatherData.visibility} m\n" +
                                 $"Wind speed: {weatherData.wind.speed} m/s\n" +
                                 $"Wind direction: {weatherData.wind.deg}°";

            outputArea.text = weatherInfo;
        }
    }
}

}
