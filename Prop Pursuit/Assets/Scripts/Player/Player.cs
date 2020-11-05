using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : Photon.MonoBehaviour
{
    public bool IsHunter = false;
    public bool HunterCanMove = false; // here
    public PhysicMaterial physMat;
    private int health = 100;

    private float speed = 7f;
    public float jumpSpeed = 8f;
    public float mouseSpeed = 30f;
    public LayerMask groundLayers;

    private Rigidbody rb;
    private Transform trans;
    private Camera myCam;
    private AudioSource noise;
    private Quaternion originCamRotation;
    private float MouseX = 0f;
    private float MouseY = 0f;

    private bool movePlayer;
    private bool jump;
    private bool hasAlerted = false;

    private Animator anim;
    public Transform spine;
    public Animator gunAnim;

    public ParticleSystem gunParticle;

    private void Awake()
    {
        if (!photonView.isMine)
        {
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }
        else
        {
            myCam = GetComponentInChildren<Camera>(); // disable all other players cameras
        }

        noise = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        trans = GetComponent<Transform>();

        // lock mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // increase packet send rate
        PhotonNetwork.sendRate = 30;
        PhotonNetwork.sendRateOnSerialize = 30;

        if (IsHunter)
        {
            anim = GetComponent<Animator>();
            if (photonView.isMine)
            {
                Vector3 color = new Vector3(PlayerDataManager.Instance.hunterColour.r, PlayerDataManager.Instance.hunterColour.g, PlayerDataManager.Instance.hunterColour.b);
                ChangeHunterColorTo(color);  // initialise player color to profile setting
            }
        }
        else
        {
            if (photonView.isMine)
            {
                Vector3 color = new Vector3(PlayerDataManager.Instance.propColour.r, PlayerDataManager.Instance.propColour.g, PlayerDataManager.Instance.propColour.b);
                ChangePropColorTo(color);  // initialise player color to profile setting
            }
        }

    }

    private void MouseLook()
    {
        //Gets mouse left/right movement
        MouseX += Input.GetAxis("Mouse X");
        //Gets mouse up/down movement
        MouseY += Input.GetAxis("Mouse Y");
        //Makes sure the x angle of the camera stays within a certain range
        MouseY = Mathf.Clamp(MouseY, -40, 40);
        //Updates both rotations with the right amounts and on the right axis of rotation relevant for each
        trans.rotation = Quaternion.Euler(transform.rotation.x, MouseX, transform.rotation.z);
    }

    private void Movement()
    {
        //Player movement
        movePlayer = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded())
                jump = true;
        }
    }

    private bool IsGrounded()
    {
        // check player is near the ground
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y - 0.5f, col.bounds.center.z), col.radius * 0.9f, groundLayers);
    }

    private void Update()
    {
        if (photonView.isMine)
        {
            if (GameManager.Instance.acceptInputs && !GameManager.Instance.GameOver) // not in a menu and not gameover
                MyInputs();

            if (GameManager.Instance.GameOver && movePlayer)
                movePlayer = false;
        }
    }

    private void MyInputs()
    {
        if (IsHunter && GameManager.Instance.SpawnTimerForHunter <= 0)
        {
            Movement();
            MouseLook();
            UpdateAim(MouseY);

            // shooting control
            if (Input.GetMouseButtonDown(0))
            {
                Ray camRay = myCam.ScreenPointToRay(Input.mousePosition);
                Shoot(camRay.origin, camRay.direction);
                UpdateGunAnimator(true);
                MakeNoise();
            }
            if (Input.GetMouseButtonUp(0))
            {
                UpdateGunAnimator(false);
            }
        }
        else if(!IsHunter)
        {
            Movement();
            MouseLook();
            myCam.transform.rotation = Quaternion.Euler(-MouseY, MouseX, 0);

            //Shooting control
            if (Input.GetMouseButtonDown(0))
            {
                Ray camRay = myCam.ScreenPointToRay(Input.mousePosition);
                //Does a raycast locally then updates result
                ChooseObject(camRay.origin, camRay.direction);
                photonView.RPC("ChooseObject", PhotonTargets.OthersBuffered, camRay.origin, camRay.direction);
                //trans.GetChild(0).transform.position = GetComponent<Collider>().bounds.center - new Vector3(0f, GetComponent<Collider>().bounds.size.y / 2, 0f);
            }

            // play sound
            if (GameManager.Instance.SpawnTimerForHunter <= 0 && !GameManager.Instance.GameOver)
            {
                if((int)GameManager.Instance.RoundTime % 15 == 0 && !hasAlerted)
                {
                    MakeNoise();
                    hasAlerted = true;
                }              
                else
                {
                    hasAlerted = false;
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {       
        if (photonView.isMine)
        {
            PhysicsMovement();
        }
    }

    void PhysicsMovement()
    {
        // local inputs
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
        // world translations
        Vector3 worldMoveDir = trans.TransformDirection(moveDir.normalized);
        // current speed
        Vector3 slow = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //Hunter logic
        if (IsHunter)
        {
            //Player movement
            if (movePlayer)
            {
                UpdateAnimator(true, 0f);
                rb.AddForce((worldMoveDir * speed) - slow, ForceMode.VelocityChange);
            }
            else if(!movePlayer)
            {
                UpdateAnimator(false, 0f);
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
            if (jump)
            {
                rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
                jump = false;
            }
        }
        //Hider logic
        else if(!IsHunter)
        {
            //Player movement
            if (movePlayer)
            {
                rb.AddForce((worldMoveDir * speed) - slow, ForceMode.VelocityChange);
            }
            else if (!movePlayer)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
            if (jump)
            {
                rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
                jump = false;
            }
        }
    }

    [PunRPC] void MakeNoise()
    {
        noise.Play();
        if (photonView.isMine)
        {
            photonView.RPC("MakeNoise", PhotonTargets.OthersBuffered);
        }
    }

    void Shoot(Vector3 camPos, Vector3 camDirection)
    {
        RaycastHit hit;
        if (Physics.Raycast(camPos, camDirection, out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                hit.transform.gameObject.GetComponent<Player>().TakeDamage(25); // deal damage to prop
            }
            else
            {
                TakeDamage(2); // hunter takes 5 damage for missing
            }
        }
    }

    void TakeDamage(int damage)
    {
        photonView.RPC("ReduceHealth", PhotonTargets.All, damage); // rpc is run on all clients including me
    }

    [PunRPC] void ReduceHealth(int damage)
    {
        health = health - damage;

        if (health <= 0)
        {
            health = 0;
            Destroy(this.gameObject);
        }
        if (photonView.isMine) // if my health was reduced change my text
        {
            Ingame_UI_Manager.instance.SetHealthText(health);
            if (health <= 0)
            {
                GameManager.Instance.PlayerDied(PhotonNetwork.player.ID); // let gamemanger know player died (for win conditions)
            }
        }
    }

    [PunRPC] void UpdateAim(float YAim)
    {
        spine.localRotation = Quaternion.Euler(0, 0, -YAim);
        if (photonView.isMine)
        {
            photonView.RPC("UpdateAim", PhotonTargets.OthersBuffered, YAim);
        }
    }

    [PunRPC]void UpdateAnimator(bool isRunning, float direction)
    {
        if (IsHunter)
        {
            anim.SetFloat("Velocity", direction);
            anim.SetBool("IsRunning", isRunning);
        }

        if(photonView.isMine)
        {
            photonView.RPC("UpdateAnimator", PhotonTargets.OthersBuffered, isRunning, direction);
        }
    }

    [PunRPC]
    void UpdateGunAnimator(bool isShooting)
    {
        gunAnim.SetBool("Shoot", isShooting);
        if (isShooting)
        {
            gunParticle.Play();
        }

        if (photonView.isMine)
        {
            photonView.RPC("UpdateGunAnimator", PhotonTargets.OthersBuffered, isShooting);
        }
    }

    [PunRPC] void ChangePropColorTo(Vector3 color)
    {
        GetComponent<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);

        if (photonView.isMine)
        {
            photonView.RPC("ChangePropColorTo", PhotonTargets.OthersBuffered, color);
        }
    }

    [PunRPC]
    void ChangeHunterColorTo(Vector3 color)
    {
        GetComponentInChildren<Renderer>().material.color = new Color(color.x, color.y, color.z, 1f);

        if (photonView.isMine)
        {
            photonView.RPC("ChangeHunterColorTo", PhotonTargets.OthersBuffered, color);
        }
    }

    [PunRPC] void ChooseObject(Vector3 camPos, Vector3 camDirection)
    {
        RaycastHit hit;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (Physics.Raycast(camPos, camDirection, out hit, Mathf.Infinity, layerMask))
        {
            GameObject otherGameObject = hit.transform.gameObject;
            if (otherGameObject.tag != "Player" && otherGameObject.tag != "NoClone")
            {
                trans.localScale = otherGameObject.transform.localScale;
                GetComponent<MeshFilter>().mesh = otherGameObject.GetComponent<MeshFilter>().mesh;
                GetComponent<Renderer>().material = otherGameObject.GetComponent<Renderer>().material;
                Destroy(GetComponent<CapsuleCollider>());
                Destroy(GetComponent<Collider>());
                gameObject.AddComponent(otherGameObject.GetComponent<Collider>().GetType());
                gameObject.AddComponent(typeof(CapsuleCollider));
                StartCoroutine("ColliderSetup"); // done in next frame                
                trans.position = new Vector3(trans.position.x, trans.position.y + (otherGameObject.GetComponent<Collider>().bounds.size.y / 2 + 0.1f), trans.position.z);
            }
        }
    }

    IEnumerator ColliderSetup()
    {
        yield return 0; // wait 1 frame
        GetComponent<Collider>().material = physMat;
        GetComponent<CapsuleCollider>().radius = 0.1f;
        GetComponent<CapsuleCollider>().height += 0.05f;
    }
}

