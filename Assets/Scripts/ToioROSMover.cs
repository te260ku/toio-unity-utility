using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using ToioControlMsg = RosMessageTypes.ControlToioInterfaces.ToioControlMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

// キューブの制御
public class ToioROSMover : MonoBehaviour
{
    [SerializeField] string _topicName;
    [SerializeField] ToioConnector _toioConnector;
    ROSConnection _ros;

    // スタート時に呼ばれる
    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
        // topicNameに入力したトピック名をサブスクライブするように設定
        // ReceiveTwistMsg関数をコールバック関数として指定
        _ros.Subscribe<ToioControlMsg>(_topicName, OnReceivedToioControlMsg);
    }

    private void OnReceivedToioControlMsg(ToioControlMsg msg)
    {   
        _toioConnector.MoveLRCommand(msg.left, msg.right);
    }
}