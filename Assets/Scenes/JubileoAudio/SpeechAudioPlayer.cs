using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using UnityEngine.Assertions;


using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;


[RequireComponent(typeof(AudioSource))]
public class SpeechAudioPlayer : MonoBehaviour
{
    // Private
    // [SerializeField]
    private AudioSource _audioSource = null;
    private List<AudioClip> _randomAudioClipPool = new List<AudioClip>();
    private AudioClip _previousAudioClip = null;

    // Serialized
    [Tooltip("Audio clip arrays with a value greater than 1 will have randomized playback.")]
    [SerializeField]
    private AudioClip[] _audioClips;
    [Tooltip("Volume set here will override the volume set on the attached sound source component.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _volume = 0.7f;
    [Tooltip("Check the 'Use Random Range' bool to and adjust the min and max slider values for randomized volume level playback.")]
    [SerializeField]
    private MinMaxPair _volumeRandomization;
    [Tooltip("Pitch set here will override the volume set on the attached sound source component.")]
    [SerializeField]
    [Range(-3f,3f)]
    [Space(10)]
    private float _pitch = 1f;
    [Tooltip("Check the 'Use Random Range' bool to and adjust the min and max slider values for randomized volume level playback.")]
    [SerializeField]
    private MinMaxPair _pitchRandomization;
    [Tooltip("True by default. Set to false for sounds to bypass the spatializer plugin. Will override settings on attached audio source.")]
    [SerializeField]
    [Space(10)]
    private bool _spatialize = true;
    [Tooltip("False by default. Set to true to enable looping on this sound. Will override settings on attached audio source.")]
    [SerializeField]
    private bool _loop = false;
    [Tooltip("100% by default. Sets likelyhood sample will actually play when called")]
    [SerializeField]
    private float _chanceToPlay = 100;
    [Tooltip("If enabled, audio will play automatically when this gameobject is enabled")]
    [SerializeField]
    private bool _playOnStart = false;
    private bool waiting_to_play = false;
    private bool playing = false;

    

     public float updateStep = 0.05f;
     public int sampleDataLength = 1024;
 
     private float currentUpdateTime = 0f;
 
     private float clipLoudness;
     private float[] clipSampleData;

    
    ROSConnection ros;
    public string topicName = "joints_to_target";
    public string subTopicName = "text_to_speech";

    protected virtual void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<JointsToTargetMsg>(topicName);
        ros.Subscribe<StringMsg>(subTopicName, TextToSpeech);
        
         clipSampleData = new float[sampleDataLength];
 
        _audioSource = gameObject.GetComponent<AudioSource>();
        // Validate that we have audio to play
        Assert.IsTrue(_audioClips.Length > 0, "An AudioTrigger instance in the scene has no audio clips.");
        // Add all audio clips in the populated array into an audio clip list for randomization purposes
        for (int i = 0; i < _audioClips.Length; i++)
        {
            _randomAudioClipPool.Add(_audioClips[i]);
        }
        // Copy over values from the audio trigger to the audio source
        _audioSource.volume = _volume;
        _audioSource.pitch = _pitch;
        _audioSource.spatialize = _spatialize;
        _audioSource.loop = _loop;
        _audioSource.clip =  GetAudioByName("i_am");
        Random.InitState((int)Time.time);
        // Play audio on start if enabled
        
        // if(_playOnStart){
        //     StartCoroutine(PlayInitialAudio());
        // }
            
    }

     // Update is called once per frame
     void Update () {
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep && playing) {
        //  if (playing) {
             currentUpdateTime = 0f;
             _audioSource.clip.GetData(clipSampleData, _audioSource.timeSamples); 
             //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
             clipLoudness = 0f;
             foreach (var sample in clipSampleData) {
                 clipLoudness += Mathf.Abs(sample);
             }
             clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for


             
            float[] joints = new float[] {clipLoudness*240.0F};
            int[] targets = new int[] {23};
            JointsToTargetMsg jaw_move = new JointsToTargetMsg(targets,joints);
            // jaw_move.joints = ["jaw_link"]
            // jaw_move.targets = [clipLoudness*240.]
            ros.Publish(topicName, jaw_move);

         }
 
     }
    
    /// <summary>
    /// Receive list of Joints and desired Final Targets to each joint 
    /// </summary>
    /// <param name="msg">JointsToTargetMsg with joints and respective desired final targets</param>
    void TextToSpeech(StringMsg msg)
    {
        Debug.Log("TextToSpeech callback");

        var audios = msg.data.Split(' ');
        
        Debug.Log("TextToSpeech callback");
        
        StartCoroutine(PlayAudios(audios));

    }
    
    IEnumerator PlayAudios(string[] audiosName)
    {
        playing = true;
        for (int i = 0; i < audiosName.Length; i++) {
            _audioSource.clip =  GetAudioByName(audiosName[i]);
            // Play the audio
            _audioSource.Play();
            yield return new WaitForSeconds(_audioSource.clip.length);
        }
        playing = false;
    }

    // IEnumerator PlayInitialAudio()
    // {
    //     waiting_to_play = true;
    //     yield return new WaitForSeconds(4);
    //     // Check if volume randomization is set
    //     if (_volumeRandomization.UseRandomRange == true)
    //     {
    //         _audioSource.volume = Random.Range(_volumeRandomization.Min, _volumeRandomization.Max);
    //     }
    //     // Check if pitch randomization is set
    //     if (_pitchRandomization.UseRandomRange == true)
    //     {
    //         _audioSource.pitch = Random.Range(_pitchRandomization.Min, _pitchRandomization.Max);
    //     }
    //     // If the audio trigger has one clip, play it. Otherwise play a random without repeat clip
    //     _audioSource.clip =  GetAudioByName("you_look");
    //     // Play the audio
    //     _audioSource.Play();
    //     playing = true;
    //     // _audioSource.PlayOneShot(clipToPlay);
    //     yield return new WaitForSeconds(_audioSource.clip.length);
    //     // If the audio trigger has one clip, play it. Otherwise play a random without repeat clip
        
    //     _audioSource.clip = GetAudioByName("scared");
    //     // Play the audio
    //     _audioSource.Play();
    //     // _audioSource.Play(clipToPlay);
    //     yield return new WaitForSeconds(_audioSource.clip.length);
        
    //     playing = false;
    //     waiting_to_play = false;
    // }
    public AudioClip GetAudioByName(string name)
    {
        foreach (AudioClip clip in _audioClips)
        {
            if(clip.name == name)
            return clip;
        }
        return null;
    }

    // IEnumerator PlayAudios()
    // {
    //     // Check if volume randomization is set
    //     if (_volumeRandomization.UseRandomRange == true)
    //     {
    //         _audioSource.volume = Random.Range(_volumeRandomization.Min, _volumeRandomization.Max);
    //     }
    //     // Check if pitch randomization is set
    //     if (_pitchRandomization.UseRandomRange == true)
    //     {
    //         _audioSource.pitch = Random.Range(_pitchRandomization.Min, _pitchRandomization.Max);
    //     }
    //     // If the audio trigger has one clip, play it. Otherwise play a random without repeat clip
    //     AudioClip clipToPlay = _audioClips.Length == 1 ? _audioClips[0] : RandomClipWithoutRepeat();
    //     _audioSource.clip = clipToPlay;
    //     Debug.Log("clipToPlay.length");
    //     Debug.Log(clipToPlay.length);
    //     // Play the audio
    //     _audioSource.Play();
    //     yield return new WaitForSeconds(clipToPlay.length);
    // }

    /// <summary>
    /// Choose a random clip without repeating the last clip
    /// </summary>
    private AudioClip RandomClipWithoutRepeat()
    {
        int randomIndex = Random.Range(0, _randomAudioClipPool.Count);
        AudioClip randomClip = _randomAudioClipPool[randomIndex];
        _randomAudioClipPool.RemoveAt(randomIndex);
        if (_previousAudioClip != null) {
            _randomAudioClipPool.Add(_previousAudioClip);
        }
        _previousAudioClip = randomClip;
        return randomClip;
    }
}
[System.Serializable]
public struct MinMaxPair
{
    [SerializeField]
    private bool _useRandomRange;
    [SerializeField]
    private float _min;
    [SerializeField]
    private float _max;
    public bool UseRandomRange => _useRandomRange;
    public float Min => _min;
    public float Max => _max;
}
