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
    public GameObject rainObject;
    public GameObject snowObject;
    public GameObject sunObject;

    TMP_InputField outputArea;
    string weatherUrl;

    void Start()
    {
        outputArea = GameObject.Find("OutputArea").GetComponent<TMP_InputField>();
        GameObject.Find("GetButton").GetComponent<Button>().onClick.AddListener(GetData);

        LoadConfig();
    }

    void LoadConfig()
    {
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

        OConfig config = JsonUtility.FromJson<OConfig>(jsonString);

        if (config == null || config.openWeather == null || string.IsNullOrEmpty(config.openWeather.url))
        {
            Debug.LogError("Failed to parse GlobalConfig.json file or missing openWeather URL.");
            return;
        }

        weatherUrl = config.openWeather.url;
    }

    void GetData() => StartCoroutine(GetData_Coroutine());

    IEnumerator GetData_Coroutine()
    {
        outputArea.text = "⚡ Loading...";

        if (string.IsNullOrEmpty(weatherUrl))
        {
            outputArea.text = "Weather URL not configured.";
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(weatherUrl))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                outputArea.text = request.error;
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                WeatherData weatherData = JsonUtility.FromJson<WeatherData>(jsonResponse);

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

                Vector3 position = new Vector3(-63.98f, 16f, -294f);

                if (weatherData.weather[0].description.ToLower().Contains("rain"))
                {
                    ShowWeatherObject(rainObject, position);
                }
                else if (weatherData.weather[0].description.ToLower().Contains("cloud"))
                {
                    ShowWeatherObject(snowObject, position);
                }
                else if (weatherData.weather[0].description.ToLower().Contains("sun"))
                {
                    ShowWeatherObject(sunObject, position);
                }
                else
                {
                    HideAllWeatherObjects();
                }
            }
        }
    }

    void ShowWeatherObject(GameObject weatherObject, Vector3 position)
    {
        HideAllWeatherObjects();
        weatherObject.SetActive(true);
        weatherObject.transform.position = position;
    }

    void HideAllWeatherObjects()
    {
        rainObject.SetActive(false);
        snowObject.SetActive(false);
        sunObject.SetActive(false);
    }
}
