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
    [SerializeField] GameObject _infoPanelPrefab;
    [SerializeField] GameObject _infoPanelParentObj;
    [SerializeField] TMP_InputField _leftWheelVelocityInputField;
    [SerializeField] TMP_InputField _rightWheelVelocityInputField;
    CubeManager _cubeManager;
    Vector2 _previousPos;
    bool _roundTrip;
    int _rightWheelVelocity;
    int _leftWheelVelocity;
    Cube _targetCube;
    List<ToioInfoPanel> _infoPanels = new List<ToioInfoPanel>();

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
            var infoPanelObj = Instantiate(_infoPanelPrefab);
            infoPanelObj.transform.parent = _infoPanelParentObj.transform;
            infoPanelObj.transform.position = Vector3.zero;
            infoPanelObj.transform.rotation = Quaternion.identity;
            var infoPanel = infoPanelObj.GetComponent<ToioInfoPanel>();
            var cubeNum = _cubeManager.cubes.IndexOf(cube);
            infoPanel._buttonText.text = cubeNum.ToString();
            infoPanel._button.onClick.AddListener(delegate {SetTargetCube(cube);});
            _infoPanels.Add(infoPanel);

            cube
                .ObserveEveryValueChanged(x => x.pos)
                .Subscribe(_ => OnUpdatePose(cube));
            cube.idMissedCallback.AddListener("EventScene", OnMissedPose);
        }

        SetTargetCube(_cubeManager.cubes.First());
    }

    void SetTargetCube(Cube cube) {
        _targetCube = cube;
        foreach (var infoPanel in _infoPanels)
        {
            infoPanel.ChangeBackgroundImageColor(infoPanel._backgroundImageDefaultColor);
        }
        var selectedInfoPanel = _infoPanels[_cubeManager.cubes.IndexOf(cube)];
        selectedInfoPanel.ChangeBackgroundImageColor(selectedInfoPanel._backgroundImageSelectedColor);
    }

    void OnUpdatePose(Cube cube) {
        if (Vector2.Distance(cube.pos, _previousPos) > 2f) {
            _previousPos = cube.pos;
            _infoPanels[_cubeManager.cubes.IndexOf(cube)].SetPositionTexts((int)cube.pos.x, (int)cube.pos.y);
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

    public void MoveLRCommand(Cube cube, int left, int right) {
        if (_cubeManager.IsControllable(cube))
        {
            cube?.Move(left, right, durationMs:100);
        }
    }

    public void MoveLRCommandInputFiled() {
        var cube = _targetCube;
        if (_cubeManager.IsControllable(cube))
        {
            var left = int.Parse(_leftWheelVelocityInputField.text);
            var right = int.Parse(_rightWheelVelocityInputField.text);
            cube?.Move(left, right, durationMs:0);
        }
    }

    public void MoveLRCommandAll(int left, int right) {
        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                cube?.Move(left, right, durationMs:100);
            }
        }
    }

    public void Forward(Cube cube) { cube?.Move(20, 20, durationMs:0); }
    public void Backward(Cube cube) { cube?.Move(-20, -20, durationMs:0); }
    public void TurnRight(Cube cube) { cube?.Move(30, 0, durationMs:0); }
    public void TurnLeft(Cube cube) { cube?.Move(0, 30, durationMs:0); }
    public void Spin(Cube cube) { cube?.Move(20, -20, durationMs:0); }
    public void Stop(Cube cube) { cube?.Move(0, 0, durationMs:0, order:Cube.ORDER_TYPE.Strong); }

    void Update()
    {
        if (!_cubeManager.syncCubes.Any()) return;

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A)) {
            if (Input.GetKey(KeyCode.W)) {
                _rightWheelVelocity = 40;
                _leftWheelVelocity = 40;
            }
            if (Input.GetKey(KeyCode.A)) {
                _rightWheelVelocity = 10;
                _leftWheelVelocity = 0;
            }
            if (Input.GetKey(KeyCode.S)) {
                _rightWheelVelocity = -40;
                _leftWheelVelocity = -40;
            }
            if (Input.GetKey(KeyCode.D)) {
                _rightWheelVelocity = 0;
                _leftWheelVelocity = 10;
            }

            MoveLRCommand(_targetCube, _leftWheelVelocity, _rightWheelVelocity);
        }

        foreach(var cube in _cubeManager.syncCubes)
        {
            if (_cubeManager.IsControllable(cube))
            {
                if (_roundTrip) RoundTrip(cube);
            }
        }
    }

    public void SpinCube() {
        Spin(_targetCube);
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
