using System.IO;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System;

/// <summary>
///cxccxcx
/// </summary>

public class ImagePublisher : MonoBehaviour
{
    public Camera Camera;
    
    ROSConnection ros;
    //name of target topic to publish 
    public string topicName = "camera_image";

    // Publish the image every N seconds
    public float publishMessageFrequency = 0.1f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            SendImage();

            timeElapsed = 0;
        }
    }
 
    public void SendImage()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = Camera.targetTexture;
 
        Camera.Render();
 
        Texture2D image = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;
 
        // byte[] bytes = image.EncodeToPNG();
        // Destroy(image);

        // File.WriteAllBytes(Application.dataPath + "/Backgrounds/" + fileCounter + ".png", bytes);
        // fileCounter++;

        HeaderMsg header = new HeaderMsg();
        ImageMsg msg = ToImageMsg(image, header);
        
        // Encode the texture as an ImageMsg, and send to ROS
        // HeaderMsg header;
        // ImageMsg imageMsg = camText.ToImageMsg(header);
        Debug.Log("publishing image");
        ros.Publish(topicName, msg);
        // image.Dispose();
        Destroy(image);

            // Finally send the message to server_endpoint.py running in ROS
        // ros.Publish(topicName, image);
    }
        
    public static ImageMsg ToImageMsg(Texture2D tex, HeaderMsg header)
        {
            byte[] data = null;
            string encoding;
            int step;
            switch (tex.format)
            {
                case TextureFormat.RGB24:
                    data = new byte[tex.width * tex.height * 3];
                    tex.GetPixelData<byte>(0).CopyTo(data);
                    encoding = "rgb8";
                    step = 3 * tex.width;
                    ReverseInBlocks(data, tex.width * 3, tex.height);
                    break;
                case TextureFormat.RGBA32:
                    data = new byte[tex.width * tex.height * 4];
                    tex.GetPixelData<byte>(0).CopyTo(data);
                    encoding = "rgba8";
                    step = 4 * tex.width;
                    ReverseInBlocks(data, tex.width * 4, tex.height);
                    break;
                case TextureFormat.R8:
                    data = new byte[tex.width * tex.height];
                    tex.GetPixelData<byte>(0).CopyTo(data);
                    encoding = "8UC1";
                    step = 1 * tex.width;
                    ReverseInBlocks(data, tex.width, tex.height);
                    break;
                case TextureFormat.R16:
                    data = new byte[tex.width * tex.height * 2];
                    tex.GetPixelData<byte>(0).CopyTo(data);
                    encoding = "16UC1";
                    step = 2 * tex.width;
                    ReverseInBlocks(data, tex.width * 2, tex.height);
                    break;
                default:
                    Color32[] pixels = tex.GetPixels32();
                    data = new byte[pixels.Length * 4];
                    // although this is painfully slow, it does work... Surely there's a better way
                    int writeIdx = 0;
                    for (int Idx = 0; Idx < pixels.Length; ++Idx)
                    {
                        Color32 p = pixels[Idx];
                        data[writeIdx] = p.r;
                        data[writeIdx + 1] = p.g;
                        data[writeIdx + 2] = p.b;
                        data[writeIdx + 3] = p.a;
                        writeIdx += 4;
                    }
                    ReverseInBlocks(data, tex.width * 4, tex.height);
                    encoding = "rgba8";
                    step = 4 * tex.width;
                    break;
            }
            return new ImageMsg(header, height: (uint)tex.height, width: (uint)tex.width, encoding: encoding, is_bigendian: 0, step: (uint)step, data: data);
        }
        
        static byte[] s_ScratchSpace;
        static void ReverseInBlocks(byte[] array, int blockSize, int numBlocks)
        {
            if (blockSize * numBlocks > array.Length)
            {
                Debug.LogError($"Invalid ReverseInBlocks, array length is {array.Length}, should be at least {blockSize * numBlocks}");
                return;
            }

            if (s_ScratchSpace == null || s_ScratchSpace.Length < blockSize)
                s_ScratchSpace = new byte[blockSize];

            int startBlockIndex = 0;
            int endBlockIndex = ((int)numBlocks - 1) * blockSize;

            while (startBlockIndex < endBlockIndex)
            {
                Buffer.BlockCopy(array, startBlockIndex, s_ScratchSpace, 0, blockSize);
                Buffer.BlockCopy(array, endBlockIndex, array, startBlockIndex, blockSize);
                Buffer.BlockCopy(s_ScratchSpace, 0, array, endBlockIndex, blockSize);
                startBlockIndex += blockSize;
                endBlockIndex -= blockSize;
            }
        }

}