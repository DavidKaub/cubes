using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    [SerializeField]
    private GameObject mutliplayerWorlds;

    private PeerToPeerManager peerToPeerManager;

    private Vector3 lastPosition = new Vector3(0, 0, 0);

    // To setup object for this script:
    /* Create player object.
     * Child game camera to player.
     * Assign player to own layer. 
     * Remove player's layer from the camera's culling mask.
     * Add CharacterController component to player.
     * Add this script to player and set variables.
     */

    // Cam look variables.
    [SerializeField]
    private float rotSpeedX = 1f; // Mouse X sensitivity control, set in editor.
    [SerializeField]
    private float rotSpeedY = 1f; // Mouse Y sensitivity control, set in editor.

    [SerializeField]
    private float rotDamp; // Damping value for camera rotation.

    private float mY = 0f; // Mouse X.
    private float mX = 0f; // Mouse Y.

    // Player move variables.

    [SerializeField]
    private float walkSpeed; // Walk (normal movement) speed, set in editor.
    [SerializeField]
    private float runSpeed; // Run speed, set in editor.

    private float currentSpeed; // Stores current movement speed.

    [SerializeField]
    private KeyCode runKey; // Run key, set in editor.

    private CharacterController cc; // Reference to attached CharacterController.

    [SerializeField]
    private GameObject playerCamera; // Player cam, set in editor.

    private bool newPositionSet = false;

    private Vector3 newPosition;

    private Vector3 GetRandomPosition(int range)
    {
        float x = Random.Range(-range, range);
        float y = Random.Range(-range, range);
        float z = Random.Range(-range, range);
        //float x = Random.flo
        return new Vector3(x, y, z);
    }

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        peerToPeerManager = mutliplayerWorlds.GetComponent<PeerToPeerManager>();
        currentSpeed = walkSpeed;
        cc.transform.position = GetRandomPosition(50);
    }

    private void LateUpdate()
    {
        if (newPositionSet)
        {
            cc.transform.position = newPosition;
            newPositionSet = false;
            Debug.Log("set new position " + transform.position.ToString() + " done!");
            return;
        }
        // Get mouse axis.
        mX += Input.GetAxis("Mouse X") * rotSpeedX * (Time.deltaTime * rotDamp);
        mY += -Input.GetAxis("Mouse Y") * rotSpeedY * (Time.deltaTime * rotDamp);

        // Clamp Y so player can't 'flip'.
        mY = Mathf.Clamp(mY, -80, 80);

        // Adjust rotation of camera and player's body.
        // Rotate the camera on its X axis for up / down camera movement.
        playerCamera.transform.localEulerAngles = new Vector3(mY, 0f, 0f);
        // Rotate the player's body on its Y axis for left / right camera movement.
        transform.eulerAngles = new Vector3(0f, mX, 0f);

        // Get Hor and Ver input.
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        // Set speed to walk speed.
        currentSpeed = walkSpeed;
        // If player is pressing run key and moving forward, set speed to run speed.
        if (Input.GetKey(runKey) && Input.GetKey(KeyCode.W)) currentSpeed = runSpeed;

        // Get new move position based off input.
        Vector3 moveDir;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDir = (transform.right * hor) + (transform.forward * ver) + (-transform.up);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            moveDir = (transform.right * hor) + (transform.forward * ver) + (transform.up);
        }
        else
        {   // moveDir = moveDir = (transform.right * hor) + (transform.forward * ver);
            moveDir = (transform.right * hor) + (transform.forward * ver);

        }        
        
        // Move CharController. 
        // .Move will not apply gravity, use SimpleMove if you want gravity.
        cc.Move(moveDir * currentSpeed * Time.deltaTime);

        if (Vector3.Distance(cc.transform.position, lastPosition)> 0.1){
            lastPosition = cc.transform.position;
            peerToPeerManager.UpdateLocalPlayerPosition(cc.transform.position);
        }
    }
    
    public void moveTo(Vector3 positionToMoveTo)
    {
        newPosition = positionToMoveTo;
        newPositionSet = true;
    }
}