using System;
using Unity.Robotics;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

namespace Unity.Robotics.UrdfImporter.Control
{
    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public enum ControlType { PositionControl };

    public class Controller : MonoBehaviour
    {
        
        ROSConnection ros;
        public string jointsTargetTopic = "joints_to_target";
        public string jointsDirectionTopic = "joints_direction";
        
        private ArticulationBody[] articulationChain;
        // Stores original colors of the part being highlighted
        private Color[] prevColor;
        private int previousIndex;

        // [InspectorReadOnly(hideInEditMode: true)]
        public string selectedJoint;
        [HideInInspector]
        public int selectedIndex;

        public Dictionary<string, int> indexOfJoint = new Dictionary<string, int>();

        public ControlType control = ControlType.PositionControl;
        public float stiffness;
        public float damping;
        public float forceLimit;
        public float speed = 5f; // Units: degree/s
        public float torque = 100f; // Units: Nm or N
        public float acceleration = 5f;// Units: m/s^2 / degree/s^2

        [Tooltip("Color to highlight the currently selected join")]
        public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);

        void Start()
        {

            // start the ROS connection
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<JointsToTargetMsg>(jointsTargetTopic, JointsToTarget);
            
            ros.Subscribe<JointsToTargetMsg>(jointsDirectionTopic, JointsDirection);

            previousIndex = selectedIndex = 1;
            // this.gameObject.AddComponent<FKRobot>();
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();
            int defDyanmicVal = 10;
            int indexValue = 0;
            string joints_name = "joints: ";
            string joints_index = "joints_index: ";
            foreach (ArticulationBody joint in articulationChain)
            {
                joint.gameObject.AddComponent<JointControl>();
                joint.jointFriction = defDyanmicVal;
                joint.angularDamping = defDyanmicVal;
                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.forceLimit = forceLimit;
                joint.xDrive = currentDrive;
                indexOfJoint.Add(joint.name, indexValue);
                indexValue ++;
            
                joints_name += ", '"+joint.name+"'";
                joints_index += indexValue.ToString()+", ";
            }
            
            DisplaySelectedJoint(selectedIndex);
        }

        /// <summary>
        /// Receive list of Joints and desired Final Targets to each joint 
        /// </summary>
        /// <param name="msg">JointsToTargetMsg with joints and respective desired final targets</param>
        void JointsToTarget(JointsToTargetMsg msg)
        {
            Debug.Log("JointsToTarget callback");
            for (int i = 0; i < msg.joints.Length; i++) 
                {
                    articulationChain[msg.joints[i]].GetComponent<JointControl>().target = msg.targets[i];                    
                }
        }
        
        /// <summary>
        /// Receive list of Joints and desired Targets related to actual joint Position
        /// For example target = -5 => Move to position = actual position - 5 
        /// </summary>
        /// <param name="msg">JointsToTargetMsg with joints and respective desired final targets</param>
        void JointsDirection(JointsToTargetMsg msg)
        {
            Debug.Log("JointsDirection callback");
            for (int i = 0; i < msg.joints.Length; i++) 
                {
                    articulationChain[msg.joints[i]].GetComponent<JointControl>().target = 
                        articulationChain[msg.joints[i]].GetComponent<JointControl>().joint.xDrive.target + msg.targets[i];                    
                }
        }

        void SetSelectedJointIndex(int index)
        {
            if (articulationChain.Length > 0) 
            {
                selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
            }
        }

        void Update()
        {
            bool SelectionInput1 = Input.GetKeyDown("right");
            bool SelectionInput2 = Input.GetKeyDown("left");

            SetSelectedJointIndex(selectedIndex); // to make sure it is in the valid range
            UpdateDirection(selectedIndex);

            if (SelectionInput2)
            {
                SetSelectedJointIndex(selectedIndex - 1);
                Highlight(selectedIndex);
            }
            else if (SelectionInput1)
            {
                SetSelectedJointIndex(selectedIndex + 1);
                Highlight(selectedIndex);
            }

            UpdateDirection(selectedIndex);
        }

        /// <summary>
        /// Highlights the color of the robot by changing the color of the part to a color set by the user in the inspector window
        /// </summary>
        /// <param name="selectedIndex">Index of the link selected in the Articulation Chain</param>
        private void Highlight(int selectedIndex)
        {
            if (selectedIndex == previousIndex || selectedIndex < 0 || selectedIndex >= articulationChain.Length) 
            {
                return;
            }



            DisplaySelectedJoint(selectedIndex);
            Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(0).GetComponentsInChildren<Renderer>();

        }

        void DisplaySelectedJoint(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= articulationChain.Length) 
            {
                return;
            }
            selectedJoint = articulationChain[selectedIndex].name + " (" + selectedIndex + ")";
        }

        /// <summary>
        /// Sets the direction of movement of the joint on every update
        /// </summary>
        /// <param name="jointIndex">Index of the link selected in the Articulation Chain</param>
        private void UpdateDirection(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= articulationChain.Length) 
            {
                return;
            }

            float moveDirection = Input.GetAxis("Vertical");
            JointControl current = articulationChain[jointIndex].GetComponent<JointControl>();

            JointControl up_eyelids_joint = articulationChain[indexOfJoint["up_eyelids_link"]].GetComponent<JointControl>();
            if (previousIndex != jointIndex)
            {
                JointControl previous = articulationChain[previousIndex].GetComponent<JointControl>();
                previous.direction = RotationDirection.None;
                previousIndex = jointIndex;
            }

            if (current.controltype != control) 
            {
                UpdateControlType(current);
            }

            if (moveDirection > 0)
            {
                
                current.target = current.joint.xDrive.target + 10;
                articulationChain[indexOfJoint["up_eyelids_link"]].GetComponent<JointControl>().target = 35;
                articulationChain[indexOfJoint["down_eyelids_link"]].GetComponent<JointControl>().target = 35;

                //up_eyelids_joint.target = 34;
                //up_eyelids_joint.joint.xDrive.target += 33.34;
                //up_eyelids_joint.joint.xDrive.target = 34;
            }
            else if (moveDirection < 0)
            {
                
                current.target = current.joint.xDrive.target - 10;
                articulationChain[indexOfJoint["up_eyelids_link"]].GetComponent<JointControl>().target = 0;
                articulationChain[indexOfJoint["down_eyelids_link"]].GetComponent<JointControl>().target = 0;

            }
            // else
            // {
            //     current.direction = RotationDirection.None;
            // }
        }


        public void UpdateControlType(JointControl joint)
        {
            joint.controltype = control;
            if (control == ControlType.PositionControl)
            {
                ArticulationDrive drive = joint.joint.xDrive;
                
                drive.stiffness = stiffness;
                drive.damping = damping;
                joint.joint.xDrive = drive;

            }
        }

        public void OnGUI()
        {
            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(Screen.width / 2 - 200, 10, 400, 20), "Press left/right arrow keys to select a robot joint.", centeredStyle);
            GUI.Label(new Rect(Screen.width / 2 - 200, 30, 400, 20), "Press up/down arrow keys to move " + selectedJoint + ".", centeredStyle);
        }
    }
}
