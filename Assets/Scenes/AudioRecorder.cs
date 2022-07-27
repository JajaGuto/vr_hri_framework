using UnityEngine;

 public class AudioRecorder : MonoBehaviour {
 
    AudioClip myAudioClip; 
    bool recording = false;
     void Start() {}
     void Update () {
         
        if (Input.GetKeyDown(KeyCode.Space) && recording == false)
        {
            recording = true;
            myAudioClip = Microphone.Start ( Microphone.devices[1].ToString(), false, 10, 44100 );

            Debug.Log(Microphone.devices.Length);
            foreach (var item in Microphone.devices)
            {
            Debug.Log(item.ToString());
            
            }
            Debug.Log("space down");
            Debug.Log(AudioSettings.outputSampleRate);
        }
        if (Input.GetKeyUp(KeyCode.Space) && recording == true){
            recording = false;

            SavWaveModified.Save("myfile2", myAudioClip); 
            Debug.Log("space up");

        }
     }
     
//     void OnGUI()
//     {
//          if (GUI.Button(new Rect(10,10,60,50),"Record"))
//      { 
//          myAudioClip = Microphone.Start ( null, false, 10, 44100 );
//      }
//      if (GUI.Button(new Rect(10,70,60,50),"Save"))
//      {
//         SavWav.Save("myfile", myAudioClip);
 
//  //        audio.Play();
//          }
//     }
 }