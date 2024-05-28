using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class GetMethod : MonoBehaviour
{
    public GameObject paintObject; // Reference to the Paint_06 object

    // Start is a Unity method called when the script starts running
    void Start()
    {
        GameObject.Find("ChangeButton").GetComponent<Button>().onClick.AddListener(MoveObject);
    }

    // Method to move the Paint_06 object
    void MoveObject()
    {
        // Check if the paintObject reference is set
        if (paintObject != null)
        {
            // Move the paintObject -3 units in the z direction
            paintObject.transform.Translate(0f, 0f, -2f);
        }
        else
        {
            Debug.LogError("Paint object reference is not set!");
        }
    }
}
