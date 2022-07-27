//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.UnityRoboticsDemo
{
    [Serializable]
    public class TargetsReachedMsg : Message
    {
        public const string k_RosMessageName = "unity_robotics_demo_msgs/TargetsReached";
        public override string RosMessageName => k_RosMessageName;

        public bool reached;
        public string joint;

        public TargetsReachedMsg()
        {
            this.reached = false;
            this.joint = "";
        }

        public TargetsReachedMsg(bool reached, string joint)
        {
            this.reached = reached;
            this.joint = joint;
        }

        public static TargetsReachedMsg Deserialize(MessageDeserializer deserializer) => new TargetsReachedMsg(deserializer);

        private TargetsReachedMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.reached);
            deserializer.Read(out this.joint);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.reached);
            serializer.Write(this.joint);
        }

        public override string ToString()
        {
            return "TargetsReachedMsg: " +
            "\nreached: " + reached.ToString() +
            "\njoint: " + joint.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
