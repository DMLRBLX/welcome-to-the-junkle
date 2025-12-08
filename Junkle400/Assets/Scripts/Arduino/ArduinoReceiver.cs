using UnityEngine;
using System.IO.Ports;

public class ArduinoReceiver : MonoBehaviour
{
    [SerializeField] string comPort = "AUTO"; // Set to "AUTO" or leave empty to auto-detect

    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private HerdingController herdingController;
    [SerializeField] private bool forwardToCamera = true;
    [SerializeField] private float potMax = 1023f; // raw max from Arduino
    [SerializeField] Vector2 mThreshold = new Vector2(0.02f, 0.02f);
    [SerializeField] Vector2 cThreshold = new Vector2(0.02f, 0.02f);

    [SerializeField] private bool flipMoveXY = false;
    [SerializeField] private bool flipCameraXY = false;

    [SerializeField] private bool invertMoveX = false;
    [SerializeField] private bool invertMoveY = false;
    [SerializeField] private bool invertCameraX = false;
    [SerializeField] private bool invertCameraY = false;

    private Vector2 tempVector;
    SerialPort sp;

    void Awake()
    {
        // Delay creating SerialPort until we know the COM port
    }

    // Start is called before the first frame update
    void Start()
    {
        // Auto-detect COM port if requested
        if (string.IsNullOrEmpty(comPort) || comPort.Trim().ToUpperInvariant() == "AUTO")
        {
            comPort = FindArduinoPort();
        }

        if (string.IsNullOrEmpty(comPort))
        {
            Debug.LogWarning("ArduinoReceiver: No COM port found for Arduino.");
            return;
        }

        sp = new SerialPort(comPort, 115200, Parity.None, 8, StopBits.One);

        try
        {
            sp.Open();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ArduinoReceiver: Failed to open port {comPort}: {e.Message}");
            return;
        }
        /*
            Set the read timeout low so unity doesn't freeze,
            and catch the exception below in update that unity will throw
            when the port isn't open and unity tries to check it
        */
        sp.ReadTimeout = 1;
    }

    private string FindArduinoPort()
    {
        string[] ports = SerialPort.GetPortNames();
        foreach (var port in ports)
        {
            SerialPort testPort = null;
            try
            {
                testPort = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
                testPort.ReadTimeout = 200;
                testPort.Open();

                // Try to read a line that matches our expected Arduino payload
                // Format: mX;mY;jump;cX;cY;clean_herd
                string line = testPort.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var parts = line.Split(';');
                    if (parts.Length >= 6)
                    {
                        // Looks like our Arduino stream; select this port
                        testPort.Close();
                        return port;
                    }
                }
            }
            catch
            {
                // Ignore and try next port
            }
            finally
            {
                if (testPort != null)
                {
                    try { if (testPort.IsOpen) testPort.Close(); } catch {}
                    testPort.Dispose();
                }
            }
        }
        return null;
    }

    // Press/hold timing for clean vs herd
    private bool cleanHerdHeld = false;
    private float cleanHerdPressTime = 0f;
    private const float herdThresholdSeconds = 1.5f;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (sp.IsOpen)
        {
            try
            {
                if (sp.BytesToRead > 0)
                {
                    string input_data = sp.ReadLine();
                    GetInputData(input_data);
                }
                // Immediately clear out any leftover data after processing
                sp.DiscardInBuffer();
            }
            catch (System.Exception)
            {

            }
        }

        // Behavior:
        // - Short press (< threshold): clean on release
        // - Long hold (>= threshold): herd immediately when threshold reached
        if (clean_herd)
        {
            if (!cleanHerdHeld)
            {
                cleanHerdHeld = true;
                cleanHerdPressTime = Time.time;
            }

            float elapsed = Time.time - cleanHerdPressTime;
            if (elapsed >= herdThresholdSeconds)
            {
                // Threshold reached while still held: herd immediately
                herdingController?.TriggerHerding(playerController);
                // Prevent repeated triggers by advancing press time
                cleanHerdPressTime = Time.time; // reset window
            }
        }
        else
        {
            if (cleanHerdHeld)
            {
                float heldDuration = Time.time - cleanHerdPressTime;
                if (heldDuration < herdThresholdSeconds)
                {
                    // Short click: clean on release
                    playerController.HandleCleanup();
                }
            }
            cleanHerdHeld = false;
        }
    }

    bool clean_herd;
    private void GetInputData(string input)
    {
        string[] input_info = input.Split(";");
        //print(input);

        float mX = float.Parse(input_info[(flipMoveXY ? 1 : 0)]);
        float mY = float.Parse(input_info[(flipMoveXY ? 0 : 1)]);
        bool jump = int.Parse(input_info[2]) == 0 ? true : false;
        float cX = float.Parse(input_info[(flipCameraXY ? 4 : 3)]);
        float cY = float.Parse(input_info[(flipCameraXY ? 3 : 4)]);
        clean_herd = int.Parse(input_info[5]) == 0 ? true : false;

        mX = Mathf.Abs(mX) >= mThreshold.x ? mX : 0f;
        mY = Mathf.Abs(mY) >= mThreshold.y ? mY : 0f;
        cX = Mathf.Abs(cX) >= cThreshold.x ? cX : 0f;
        cY = Mathf.Abs(cY) >= cThreshold.y ? cY : 0f;

        if (invertMoveX) mX = -mX;
        if (invertMoveY) mY = -mY;
        if (invertCameraX) cX = -cX;
        if (invertCameraY) cY = -cY;

        // print(playerController.moveInput);
        playerController.moveInput = new Vector3(mX, mY, 0f);
        if (forwardToCamera && cameraController != null)
        {
            cameraController.SetArduinoLook(new Vector2(cX, cY));
        }
        if (jump) playerController.Jump();
    }
}
