using System.Linq;
using TMPro;
using UnityEngine;
using System;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;


public class DebugConsole : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI debugAreaText = null;

    // [SerializeField]
    // private bool enableDebug = false;

    [SerializeField]
    private int maxLines = 10;
    
    
    ROSConnection ros;
    public string topic = "debug_topic";

    public string last_text = "";

    // Start is called before the first frame update
    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>(topic, DebugTopic);

        if (debugAreaText == null)
        {
            debugAreaText = GetComponent<TextMeshProUGUI>();
        }
        debugAreaText.text = "";
    }

    // Update is called once per frame
    // void Update()
    // {
        
    // }
    
    /// <summary>
    /// Receive text to debug display  
    /// </summary>
    /// <param name="msg">String with the text to debug display </param>
    void DebugTopic(StringMsg msg)
    {
        if (last_text != msg.data){
            last_text = msg.data;
            String debugText = msg.data.Remove(0,1);
            String type = msg.data.Substring(0, 1);
            if (type == "i"){
                LogInfo(debugText);
            } else if (type == "w"){
                LogWarning(debugText);
            } else if (type == "e"){
                LogError(debugText);
            } else {
                LogInfo(msg.data);
            }
        }
    }
    

    public void Clear() => debugAreaText.text = string.Empty;

    public void LogInfo(string message)
    {
        ClearLines();

        debugAreaText.text += $"<color=\"green\"> {message}</color>\n";
        // debugAreaText.text += $"<color=\"green\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    public void LogError(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"red\">{message}</color>\n";
    }

    public void LogWarning(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"yellow\"> {message}</color>\n";
    }

    private void ClearLines()
    {
        if (debugAreaText.text.Split('\n').Count() >= maxLines)
        {
            debugAreaText.text = string.Empty;
        }
    }
}
