using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using System.Linq;
using UniRx;
using TMPro;

public class ToioConnector : MonoBehaviour
{
    public ConnectType _connectType;
    public enum ConnectCountType {
        Single, 
        Multi
    }
    [SerializeField] ConnectCountType _connectCountType;
    [SerializeField] int _connectCount;
    CubeManager _cubeManager;
    Vector2 _previousPos;
    bool _roundTrip;

    void Start() {
        _cubeManager = new CubeManager(_connectType);
    }

    async public void Connect() {
        switch (_connectCountType)
        {
            case ConnectCountType.Single:
                await _cubeManager.SingleConnect();
                break;
            case ConnectCountType.Multi:
                if (_connectCount > 0) {
                    await _cubeManager.MultiConnect(_connectCount);
                } else {
                    Debug.LogError("1台以上の接続数を指定してください");
                }
                break;
            default:
                break;
        }

        foreach(var cube in _cubeManager.cubes)
        {
            cube
                .ObserveEveryValueChanged(x => x.pos)
                .Subscribe(_ => OnUpdatePose(cube));
            cube.idMissedCallback.AddListener("EventScene", OnMissedPose);
            
        }
    }

    void OnUpdatePose(Cube cube) {
        if (Vector2.Distance(cube.pos, _previousPos) > 2f) {
            Debug.Log("Update Pose");
            _previousPos = cube.pos;
        }
    }

    void OnMissedPose(Cube cube) {
        Debug.Log("Missed Pose");
    }

    public void Disconnect() {
        _cubeManager.DisconnectAll();
    }

    async public void Reconnect() {
        await _cubeManager.ReConnectAll();
    }

    public void ForwardAll() {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                Forward(cube);
            }
        }
    }

    public void BackwardAll() {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                Backward(cube);
            }
        }
    }

    public void TurnRightAll() {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                TurnRight(cube);
            }
        }
    }

    public void TurnLeftAll() {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                TurnLeft(cube);
            }
        }
    }

    public void StopAll() {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                Stop(cube);
            }
        }

        _roundTrip = false;
    }

    public void MoveLRCommand(int left, int right) {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                cube?.Move(left, right, durationMs:500);
            }
        }
    }

    public void Forward(Cube cube) { cube?.Move(40, 40, durationMs:0); }
    public void Backward(Cube cube) { cube?.Move(-40, -40, durationMs:0); }
    public void TurnRight(Cube cube) { cube?.Move(30, 0, durationMs:0); }
    public void TurnLeft(Cube cube) { cube?.Move(0, 30, durationMs:0); }
    public void Stop(Cube cube) { cube?.Move(0, 0, durationMs:0, order:Cube.ORDER_TYPE.Strong); }

    void Update()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A)) {
            Debug.Log("Key Pushed");
        }

        if (!_cubeManager.syncCubes.Any()) return;

        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                if (_roundTrip) RoundTrip(cube);
            }
        }
    }

    public void RoundTrip(Cube cube) {
        if (cube.pos.y < 190) {
            Backward(cube);
        } else if (320 < cube.pos.y) {
            Forward(cube);
        }
    }

    public void StartRoundTrip() {
        StartCoroutine(StartRoundTripCoroutine());
        _roundTrip = true;
    }

    IEnumerator StartRoundTripCoroutine() {
        var cubes = _cubeManager.syncCubes;
        var count = cubes.Count();
        while (count > 0) {
            var cube = cubes[cubes.Count()-count];
            Forward(cube);
            yield return new WaitForSeconds(0.5f);
            count --;
        }
    }
}
