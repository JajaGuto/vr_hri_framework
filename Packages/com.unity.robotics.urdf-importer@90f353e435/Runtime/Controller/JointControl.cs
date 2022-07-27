using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
 */using Unity.Robotics.UrdfImporter;

public class JointControl : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller controller;


    /* 
    ROSConnection ros;
    public string reachTopicName = "target_reached";
 */
    public Unity.Robotics.UrdfImporter.Control.RotationDirection direction;
    public Unity.Robotics.UrdfImporter.Control.ControlType controltype;
    public float speed ;
    public float torque ;
    public float acceleration;
    public float target;
    public bool reached;
    public ArticulationBody joint;
    
    //public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };


    void Start()
    {

/* 
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TargetsReachedMsg>(reachTopicName);
 */
        direction = 0;
        reached = true;
        controller = (Unity.Robotics.UrdfImporter.Control.Controller)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller));
        joint = this.GetComponent<ArticulationBody>();
        controller.UpdateControlType(this);
        speed = controller.speed;
        torque = controller.torque;
        acceleration = controller.acceleration;
        target = joint.xDrive.target;
    }

    void FixedUpdate(){

        speed = controller.speed;
        torque = controller.torque;
        acceleration = controller.acceleration;


        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                ArticulationDrive currentDrive = joint.xDrive;


                if(target > currentDrive.upperLimit){
                    target = currentDrive.upperLimit;
                }
                if(target < currentDrive.lowerLimit){
                    target = currentDrive.lowerLimit;
                }

                //if final target is close of drive position,then stop 
                if(Math.Abs(target - currentDrive.target) < 1 && reached == false){

                    Debug.Log("ta perto");
                    Debug.Log(target);
                    Debug.Log(currentDrive.target);

                    target = currentDrive.target;
                    direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.None;
                    reached = true;
                    //ros pub
                    /* 
                    TargetsReachedMsg msg = new TargetsReachedMsg(
                        reached,
                        joint.name
                    );

                    // Finally send the message to server_endpoint.py running in ROS
                    ros.Publish(topicName, cubePos);
 */
                }
                

                //adjusting the direction which the target 
                if(target > currentDrive.target){
                    reached = false;
                    direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Positive;
                }
                if(target < currentDrive.target){
                    direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Negative;
                    reached = false;
                }

                float newTargetDelta = (int)direction * Time.fixedDeltaTime * speed;

                if (joint.jointType == ArticulationJointType.RevoluteJoint)
                {
                    if (joint.twistLock == ArticulationDofLock.LimitedMotion)
                    {
                        if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
                        {
                            currentDrive.target = currentDrive.upperLimit;
                        }
                        else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
                        {
                            currentDrive.target = currentDrive.lowerLimit;
                        }
                        else
                        {
                            currentDrive.target += newTargetDelta;
                        }
                    }
                    else
                    {
                        currentDrive.target += newTargetDelta;
   
                    }
                }

                else if (joint.jointType == ArticulationJointType.PrismaticJoint)
                {
                    if (joint.linearLockX == ArticulationDofLock.LimitedMotion)
                    {
                        if (newTargetDelta + currentDrive.target > currentDrive.upperLimit)
                        {
                            currentDrive.target = currentDrive.upperLimit;
                        }
                        else if (newTargetDelta + currentDrive.target < currentDrive.lowerLimit)
                        {
                            currentDrive.target = currentDrive.lowerLimit;
                        }
                        else
                        {
                            currentDrive.target += newTargetDelta;
                        }
                    }
                    else
                    {
                        currentDrive.target += newTargetDelta;
   
                    }
                }

                joint.xDrive = currentDrive;

            }
        }
    }
    
    //public void SetTarget(int){

    //}
}
