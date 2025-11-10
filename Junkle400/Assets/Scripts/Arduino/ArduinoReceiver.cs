using UnityEngine;
using System.IO.Ports;

public class ArduinoReceiver : MonoBehaviour
{
    [SerializeField] string comPort;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private bool forwardToCamera = true;
    [SerializeField] private float potMax = 1023f; // raw max from Arduino
    [SerializeField] Vector2 offset = new Vector2(0.02f, 0.02f);
    private Vector2 tempVector;
    SerialPort sp;

    void Awake()
    {
        sp = new SerialPort(comPort, 115200, Parity.None, 8, StopBits.One);
    }

    // Start is called before the first frame update
    void Start()
    {
        sp.Open();
        /*
            Set the read timeout low so unity doesn't freeze,
            and catch the exception below in update that unity will throw
            when the port isn't open and unity tries to check it
        */
        sp.ReadTimeout = 1;
    }

    bool checkedTime = true;
    float cleanTime;
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


        if (clean)
        {
            if (!checkedTime)
                cleanTime = Time.time;

            checkedTime = true;
            playerController.HandleCleanup();
        }
        else
        {
            if (checkedTime)
            {
                if (Time.time - cleanTime < 1.5f)
                {
                    playerController.HandleCleanup();
                }
                else
                {
                    // TODO: Herd!
                }
            }
                
            checkedTime = false;
        }
    }

    bool clean;
    private void GetInputData(string input)
    {
        string[] input_info = input.Split(";");
        //print(input);

        float mX = float.Parse(input_info[0]) + offset.x;
        float mY = float.Parse(input_info[1]) + offset.y;
        bool jump = int.Parse(input_info[2]) == 0 ? true : false;
        float cX = float.Parse(input_info[3]);
        float cY = float.Parse(input_info[4]);
        clean = int.Parse(input_info[5]) == 1 ? true : false;


        // print(playerController.moveInput);
        playerController.moveInput = new Vector3(mX, mY, 0f);
        if (forwardToCamera && cameraController != null)
        {
            cameraController.SetArduinoLook(new Vector2(cX, cY));
        }
        if (jump) playerController.Jump();
    }
}
