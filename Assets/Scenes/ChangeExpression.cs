using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class ChangeExpression : MonoBehaviour
{
    
    bool changing = false;
    int active = 0;
    
    
    [SerializeField]
    private SkinnedMeshRenderer[] meshes;

    ROSConnection ros;
    public string topicName = "debug_topic";
    
    [SerializeField]
    private string[] expressions;

    // Start is called before the first frame update
    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);

        for (int i = 0; i < meshes.Length; i++)
        {
            if (i != active){
                meshes[i].enabled=false;
            }
        }
        meshes[active].enabled=true;
        
        StringMsg msg = new StringMsg("iAvatar expression: "+expressions[active]);
        ros.Publish(topicName, msg);

    }

    // Update is called once per frame
    void Update()
    {
        
        if (OVRInput.Get(OVRInput.Button.Three) && changing == false)
        {
            StartCoroutine(Change());
        }
    }
    
         IEnumerator Change()
    {
            changing = true;
            yield return new WaitForSeconds(0.3f);
            if (OVRInput.Get(OVRInput.Button.Three)==false){

                Debug.Log("change expression");
                active ++;
                if (active == meshes.Length){
                    active = 0;
                }

                for (int i = 0; i < meshes.Length; i++)
                {
                    if (i != active){
                        meshes[i].enabled=false;
                    }
                }
                meshes[active].enabled=true;

                StringMsg msg = new StringMsg("iAvatar expression: "+expressions[active]);
                ros.Publish(topicName, msg);
                yield return new WaitForSeconds(1);
            }

            changing = false;
        }
}
