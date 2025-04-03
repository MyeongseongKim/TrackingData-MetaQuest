using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Oculus.Interaction;
using Oculus.Interaction.Input;

using Newtonsoft.Json;

using SerializableData;


public class TrackingDataManager : MonoBehaviour
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


    private SerializableTrackingData CaptureFrame()
    {
        var frame = new SerializableTrackingData();

        // timestamp
        frame.timestamp = Time.time;

        // head
        if (_cameraRig != null && _cameraRig.centerEyeAnchor != null)
        {
            var centerEyeData = new SerializablePose(
                _cameraRig.centerEyeAnchor.position, 
                _cameraRig.centerEyeAnchor.rotation
            );
            frame.head.Add("CenterEye", centerEyeData);
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


    private SerializableHandData ExtractHandData(Hand hand) 
    {
        var handData = new SerializableHandData();
        
        foreach (var pair in HAND_JOINTS) 
        {
            var name = pair.Key;
            var joints = pair.Value;

            List<SerializablePose> group = new List<SerializablePose>();
            for (int i = 0; i < joints.Length; i++) 
            {
                Pose pose = Pose.identity;
                if (hand.GetJointPose(joints[i], out pose))
                {
                    group.Add(new SerializablePose(pose));
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
