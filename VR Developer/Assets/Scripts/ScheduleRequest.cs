using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

[System.Serializable]
public class GlobalConfig
{
    public PollingRates pollingRates;
    public string[] infoLines;
  //  public UrlSet[] urlSets;
   // public OpenWeather openWeather;
    public Schedule schedule;

    public static GlobalConfig LoadConfig(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<GlobalConfig>(json);
        }
        else
        {
            Debug.LogError($"Config file not found: {filePath}");
            return null;
        }
    }
}

[System.Serializable]
public class Schedule
{
    public string room;
    public string date;
}

[System.Serializable]
public class Event
{
    public string uuid;
    public EventType event_type;
    public State state;
    public StudyWorkType study_work_type;
    public Lecturer[] lecturers;
    public EventTime time;  // Updated from Time to EventTime
    public Location location;
    public CourseTitle course_title;
    public string course_code;
}

[System.Serializable]
public class EventTime  // Renamed from Time to EventTime
{
    public string academic_weeks;
    public Weekday weekday;
    public string since_date;
    public string until_date;
    public string begin_time;
    public string end_time;
}

[System.Serializable]
public class EventType
{
    public string code;
    public string et;
    public string en;
}

[System.Serializable]
public class State
{
    public string code;
    public string et;
    public string en;
}

[System.Serializable]
public class StudyWorkType
{
    public string code;
    public string et;
    public string en;
}

[System.Serializable]
public class Lecturer
{
    public string person_uuid;
    public string person_name;
}

[System.Serializable]
public class Weekday
{
    public string code;
    public string et;
    public string en;
}

[System.Serializable]
public class Location
{
    public string address;
}

[System.Serializable]
public class CourseTitle
{
    public string et;
    public string en;
}

[System.Serializable]
public class ScheduleResponse
{
    public Event[] events;
}

public class ScheduleRequest : MonoBehaviour
{
    // Reference to the TMP InputField component where the response will be displayed
    public TMP_InputField responseInputField;

    // Path to the config file
    string path = Path.Combine(Application.dataPath, "GlobalConfig.json");
    private GlobalConfig globalConfig;

    void Start()
    {
        // Load the configuration
        globalConfig = GlobalConfig.LoadConfig(path);

        if (globalConfig != null)
        {
            // Start the coroutine to make the POST request using the config values
            StartCoroutine(PostRequest(globalConfig.schedule.room, globalConfig.schedule.date));
        }
        else
        {
            Debug.LogError("Failed to load global configuration.");
        }
    }

    IEnumerator PostRequest(string room, string date)
    {
        // Create a JSON object with the necessary data
        string jsonData = $"{{\"building\": \"NAR18OH\", \"room\": \"{room}\", \"date\": \"{date}\"}}";

        // Convert the JSON string into a byte array
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Create the request
        UnityWebRequest request = new UnityWebRequest("https://ois2.ut.ee/api/timetable/room", "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            // Display error in the TMP InputField
            responseInputField.text = "Error: " + request.error;
        }
        else
        {
            // Process the response
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("This is the Response: " + jsonResponse);

            // Deserialize the JSON response
            ScheduleResponse schedule = JsonUtility.FromJson<ScheduleResponse>(jsonResponse);

            // Create a string to hold the formatted output
            string formattedOutput = $"Date: {date}\n\n";

            foreach (Event eventItem in schedule.events)
            {
                string courseName = eventItem.course_title.en ?? eventItem.course_title.et;
                string beginTime = eventItem.time.begin_time;
                string endTime = eventItem.time.end_time;
                string teacher = eventItem.lecturers != null && eventItem.lecturers.Length > 0
                    ? eventItem.lecturers[0].person_name
                    : "(No teacher assigned)";

                formattedOutput += $"Course Name: {courseName}\n";
                formattedOutput += $"Time: {beginTime} - {endTime}\n";
                formattedOutput += $"Teacher: {teacher}\n\n";
            }

            // Display the formatted output in the TMP InputField
            responseInputField.text = formattedOutput;
        }
    }
}
