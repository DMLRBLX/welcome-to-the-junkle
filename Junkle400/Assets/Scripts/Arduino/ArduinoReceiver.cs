using UnityEngine;
using System.IO.Ports;

public class ArduinoReceiver : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One);

    [SerializeField] private PlayerController playerController;
    [SerializeField] Vector2 offset = new Vector2(0.02f, 0.02f);
    private Vector2 tempVector;
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
    }

    private void GetInputData(string input)
    {
        string[] input_info = input.Split(";");
        //print(input);

        float x = float.Parse(input_info[0]) + offset.x;
        float y = float.Parse(input_info[1]) + offset.y;
        bool jump = int.Parse(input_info[2]) == 0 ? true : false;

        print(playerController.moveInput);
        playerController.moveInput = new Vector3(x, y, 0f);
        if (jump) playerController.Jump();
    }
}
