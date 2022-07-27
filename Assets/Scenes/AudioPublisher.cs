using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

public class AudioPublisher : MonoBehaviour
{
    AudioClip myAudioClip; 
    bool recording = false;
    
    ROSConnection ros;
    public string topicName = "audio";

     void Start() {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<AudioMsg>(topicName);
        float[] data = new float[] {0.0F};
        AudioMsg audio = new AudioMsg(data);
        ros.Publish(topicName, audio);
    }
     void Update () {
         
        if (OVRInput.Get(OVRInput.Button.One) && recording == false)
        {
            StartCoroutine(GetMic ());
            
        }
        // if (!OVRInput.Get(OVRInput.Button.One) && recording == true){
        //     recording = false;
        //     Debug.Log("audio pub");
            
        //     // byte[] mp3_data = EncodeMP3.SaveMp3(myAudioClip, $"{Application.dataPath}/Backgrounds/mp3File", 192);
        //     // EncodeMP3.(myAudioClip, $"{Application.dataPath}/Backgrounds/mp3File", 128);
        //     // byte[] bytes = WavToMp3.ConvertWavToMp3(myAudioClip, 128);

        //     // Debug.Log("audio bytess");
        //     // DumpToConsole(mp3_data);
        //     // Debug.Log(mp3_data);

        //     float[] data = SavWaveModified.GenerateWavBytes(myAudioClip); 
        //     AudioMsg audio = new AudioMsg(data);
            
        //     // Finally send the message to server_endpoint.py running in ROS
        //     ros.Publish(topicName, audio);
        //     DumpToConsole(data);
        //     DumpToConsole(audio);
        //     Debug.Log(data.Length);
        // }
     }     
        
         IEnumerator GetMic ()
    {
            Debug.Log("audio pub");
            recording = true;
            myAudioClip = Microphone.Start ( Microphone.devices[0].ToString(), false, 3, 44100 );

            Debug.Log(Microphone.devices.Length);
            yield return new WaitForSeconds(3);


            recording = false;
            Debug.Log("audio pub");
            
            // byte[] mp3_data = EncodeMP3.SaveMp3(myAudioClip, $"{Application.dataPath}/Backgrounds/mp3File", 192);
            // EncodeMP3.(myAudioClip, $"{Application.dataPath}/Backgrounds/mp3File", 128);
            // byte[] bytes = WavToMp3.ConvertWavToMp3(myAudioClip, 128);

            // Debug.Log("audio bytess");
            // DumpToConsole(mp3_data);
            // Debug.Log(mp3_data);

            float[] data = SavWaveModified.GenerateWavBytes(myAudioClip); 
            AudioMsg audio = new AudioMsg(data);
            
            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, audio);
            DumpToConsole(data);
            DumpToConsole(audio);
            Debug.Log(data.Length);
        }
    public static void DumpToConsole(object obj)
        {
            var output = JsonUtility.ToJson(obj, true);
            Debug.Log(output);
        }
 }