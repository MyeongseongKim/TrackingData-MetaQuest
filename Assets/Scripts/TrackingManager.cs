using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Oculus.Interaction;
using Oculus.Interaction.Input;

using Newtonsoft.Json;


#region Data Classes

[Serializable]
public struct SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
}

[Serializable]
public struct SerializableQuaternion
{
    public float x, y, z, w;
    public SerializableQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }
}

[Serializable]
public class PoseData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;

    public PoseData(Vector3 pos, Quaternion rot)
    {
        position = new SerializableVector3(pos);
        rotation = new SerializableQuaternion(rot);
    }
    public PoseData(Pose pose)
    {
        position = new SerializableVector3(pose.position);
        rotation = new SerializableQuaternion(pose.rotation);
    }
}

[Serializable]
public class HandData 
{
    public Dictionary<string, List<PoseData>> groups = new();
}

[Serializable]
public class TrackingFrame 
{
    public float timestamp;
    public Dictionary<string, PoseData> head = new();
    public Dictionary<string, HandData> hands = new();
}

#endregion


public class TrackingManager : MonoBehaviour
{
    public static readonly Dictionary<string, HandJointId[]> HAND_JOINTS = new Dictionary<string, HandJointId[]> {
        { "Wrist", new[] {HandJointId.HandWristRoot} },  
        { "Palm", new[] {HandJointId.HandPalm} }, 
        { "Thumb", new[] {HandJointId.HandThumb1, HandJointId.HandThumb2, HandJointId.HandThumb3, HandJointId.HandThumbTip} }, 
        { "Index", new[] {HandJointId.HandIndex1, HandJointId.HandIndex2, HandJointId.HandIndex3, HandJointId.HandIndexTip} }, 
        { "Middle", new[] {HandJointId.HandMiddle1, HandJointId.HandMiddle2, HandJointId.HandMiddle3, HandJointId.HandMiddleTip} }, 
        { "Ring", new[] {HandJointId.HandRing1, HandJointId.HandRing2, HandJointId.HandRing3, HandJointId.HandRingTip} }, 
        { "Pinky", new[] {HandJointId.HandPinky1, HandJointId.HandPinky2, HandJointId.HandPinky3, HandJointId.HandPinkyTip} }  
    };


    [SerializeField]
    private OVRCameraRig _cameraRig;

    [SerializeField]
    private Hand _leftHand;

    [SerializeField]
    private Hand _rightHand;


    void Start()
    {

    }


    void Update()
    {
        var frame = CaptureFrame();
        // string json = JsonUtility.ToJson(frame);
        string json = JsonConvert.SerializeObject(frame, Formatting.Indented);
        Debug.Log(json);
    }


    private TrackingFrame CaptureFrame()
    {
        var frame = new TrackingFrame();

        // timestamp
        frame.timestamp = Time.time;

        // head
        if (_cameraRig != null && _cameraRig.centerEyeAnchor != null)
        {
            var centerEyePoseData = new PoseData(
                _cameraRig.centerEyeAnchor.position, 
                _cameraRig.centerEyeAnchor.rotation
            );
            frame.head.Add("CenterEye", centerEyePoseData);
        }
        else 
        {
            frame.head.Add("CenterEye", null);
        }

        // left hand
        if (_leftHand != null && _leftHand.IsHighConfidence) 
        {
            var leftHandData = ExtractHandData(_leftHand);
            frame.hands.Add("LeftHand", leftHandData);
        }
        else 
        {
            frame.hands.Add("LeftHand", null);
        }

        // right hand
        if (_rightHand != null && _rightHand.IsHighConfidence) 
        {
            var rightHandData = ExtractHandData(_rightHand);
            frame.hands.Add("RightHand", rightHandData);
        }
        else 
        {
            frame.hands.Add("RightHand", null);
        }

        return frame;
    }


    private HandData ExtractHandData(Hand hand) 
    {
        var handData = new HandData();
        
        foreach (var pair in HAND_JOINTS) 
        {
            var name = pair.Key;
            var joints = pair.Value;

            List<PoseData> group = new List<PoseData>();
            for (int i = 0; i < joints.Length; i++) 
            {
                Pose pose = Pose.identity;
                if (hand.GetJointPose(joints[i], out pose))
                {
                    group.Add(new PoseData(pose));
                }
                else
                {
                    group.Add(null);
                }
            }

            handData.groups.Add(name, group);
        }

        return handData;
    }
}
