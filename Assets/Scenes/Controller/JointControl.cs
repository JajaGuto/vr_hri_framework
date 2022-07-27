using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Unity.Robotics.UrdfImporter;

public class JointControl : MonoBehaviour
{
    Unity.Robotics.UrdfImporter.Control.Controller controller;


    
    ROSConnection ros;
    public string reachTopicName = "targets_reached";

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
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TargetsReachedMsg>(reachTopicName);
        direction = 0;
        reached = true;
        controller = (Unity.Robotics.UrdfImporter.Control.Controller)this.GetComponentInParent(typeof(Unity.Robotics.UrdfImporter.Control.Controller));
        joint = this.GetComponent<ArticulationBody>();
        controller.UpdateControlType(this);
        
        speed = controller.speed;
        torque = controller.torque;
        acceleration = controller.acceleration;
        
        
        if(joint.name == "up_eyelids_link" || joint.name == "down_eyelids_link" || joint.name == "jaw_link"){
            speed *=10;
            acceleration *=10;
        }
        if(joint.name == "right_eyebrow_prism_link" || joint.name == "right_eyebrow_link" ||
            joint.name == "right_eye_hor_link" || joint.name == "right_eye_link" ||
            joint.name == "left_eye_link" || joint.name == "left_eyebrow_prism_link" ||
            joint.name == "left_eyebrow_link" || joint.name == "left_eye_hor_link"){
                speed *=4;
                acceleration *=6;
        }
        if(joint.name == "neck_link" || joint.name == "head_link"){
            speed = controller.speed;
            acceleration = controller.acceleration;
        }
        // if(joint.name == "right_eyebrow_prism_link" || joint.name == "left_eyebrow_prism_link"){
        //     speed /=100;
        //     acceleration /=5000;
        // }
        target = joint.xDrive.target;
    }

    void FixedUpdate(){

        // speed = controller.speed;
        // torque = controller.torque;
        // acceleration = controller.acceleration;


        if (joint.jointType != ArticulationJointType.FixedJoint)
        {
            if (controltype == Unity.Robotics.UrdfImporter.Control.ControlType.PositionControl)
            {
                ArticulationDrive currentDrive = joint.xDrive;


                // if(target > currentDrive.upperLimit){
                //     target = currentDrive.upperLimit;
                // }
                // if(target < currentDrive.lowerLimit){
                //     target = currentDrive.lowerLimit;
                // }

                if (joint.jointType == ArticulationJointType.RevoluteJoint)
                {
                                

                            //if final target is close of drive position,then stop 
                            if(Math.Abs(target - currentDrive.target) < 1 && reached == false){

                                target = currentDrive.target;
                                direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.None;
                                reached = true;
                                //ros pub
                                TargetsReachedMsg msg = new TargetsReachedMsg(
                                    reached,
                                    joint.name
                                );
                                // Finally send the message to server_endpoint.py running in ROS
                                ros.Publish(reachTopicName, msg);
                            }
                            

                            //adjusting the direction which the target 
                            if(target > currentDrive.target && Math.Abs(target - currentDrive.target) > 1){
                                reached = false;
                                direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Positive;
                            }
                            if(target < currentDrive.target && Math.Abs(target - currentDrive.target) > 1){
                                direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Negative;
                                reached = false;
                            }

                            float newTargetDelta = (int)direction * Time.fixedDeltaTime * speed * (20+Math.Abs(target - currentDrive.target))/30;


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
                            //if final target is close of drive position,then stop 
                            if(Math.Abs(target - currentDrive.target) < 0.004 && reached == false){
                                target = currentDrive.target;
                                direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.None;
                                reached = true;
                                //ros pub
                                TargetsReachedMsg msg = new TargetsReachedMsg(
                                    reached,
                                    joint.name
                                );
                                // Finally send the message to server_endpoint.py running in ROS
                                ros.Publish(reachTopicName, msg);
                            }
                            

                            //adjusting the direction which the target 
                            if(target > currentDrive.target && Math.Abs(target - currentDrive.target) > 0.0001){
                                reached = false;
                                direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Positive;
                            }
                            if(target < currentDrive.target && Math.Abs(target - currentDrive.target) > 0.0001){
                                direction = Unity.Robotics.UrdfImporter.Control.RotationDirection.Negative;
                                reached = false;
                            }

                    float newTargetDelta = ((int)direction * Time.fixedDeltaTime * speed) /300;

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
