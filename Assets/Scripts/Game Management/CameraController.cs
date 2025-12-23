using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    readonly float CameraLinearSpeed = 30f;
    readonly float CameraRotationalSpeed = 60f;
    readonly float CameraZoomSpeed = 10f;
    readonly float MinFOV = 5f;
    readonly float MaxFOV = 100f;
    public Camera camera;
    Vector3 nextStep = Vector3.zero;
    Vector3 angularVelocity = new Vector3(0f, 1f, 0f);
    Quaternion deltaRotation;
    Rigidbody myBody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myBody = GetComponent<Rigidbody>();
        angularVelocity *= CameraRotationalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        nextStep = Vector3.zero;
        if (Input.GetKey("right"))
        {
            // Moves camera right
            nextStep += transform.right * Time.deltaTime * CameraLinearSpeed;
        }
        else if (Input.GetKey("left"))
        {
            // Moves camera left
            nextStep += -transform.right * Time.deltaTime * CameraLinearSpeed;
        }

        if (Input.GetKey("up"))
        {
            // Moves camera forward
            nextStep += transform.forward * Time.deltaTime * CameraLinearSpeed;
        }
        else if (Input.GetKey("down"))
        {
            // Moves camera backwards
            nextStep += -transform.forward * Time.deltaTime * CameraLinearSpeed;
        }

        if (Input.GetKey("w"))
        {
            // Moves camera up
            nextStep += transform.up * Time.deltaTime * CameraLinearSpeed;
        }
        else if (Input.GetKey("s"))
        {
            // Moves camera down
            nextStep += -transform.up * Time.deltaTime * CameraLinearSpeed;
        }

        //Actually Moves the camera
        myBody.MovePosition(transform.position + nextStep);

        if (Input.GetKey("d"))
        {
            deltaRotation = Quaternion.Euler(angularVelocity * Time.deltaTime);
            myBody.MoveRotation(myBody.rotation * deltaRotation);
        }
        else if (Input.GetKey("a"))
        {
            deltaRotation = Quaternion.Euler(-angularVelocity * Time.deltaTime);
            myBody.MoveRotation(myBody.rotation * deltaRotation);
        }

        if (Input.GetKey("z"))
        {
            // Zooms in
            if (camera.fieldOfView >= MinFOV)
            {
                // Only Zooms in if the FOV is wider than the minFOV
                camera.fieldOfView += -CameraZoomSpeed * Time.deltaTime;
            }
            else
            {
                camera.fieldOfView = MinFOV;
            }
        }
        else if (Input.GetKey("x"))
        {
            // Zooms out
            if (camera.fieldOfView <= MaxFOV)
            {
                // Only Zooms out if the FOV is narrower than the maxFOV
                camera.fieldOfView += CameraZoomSpeed * Time.deltaTime;
            }
            else
            {
                camera.fieldOfView = MaxFOV;
            }
        }
        // Makes sure that the position of the rig and camera is the same
        camera.transform.position = transform.position;
    }
}
