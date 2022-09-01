using System.Collections;
using Mirror;
using UnityEngine;


//by far not an ideal implementation as it's almost all-i-one behaviour for player,
//but good enough for prototyping 
public class PlayerControl : NetworkBehaviour, IDashTouchable
{

	[Header("Controls")]
	[SerializeField]
	private KeyCode forward = KeyCode.W;
	[SerializeField]
	private KeyCode left = KeyCode.A;
	[SerializeField]
	private KeyCode back = KeyCode.S;
	[SerializeField]
	private KeyCode right = KeyCode.D;
	[SerializeField]
	private KeyCode jump = KeyCode.Space;
	[SerializeField]
	private KeyCode dash = KeyCode.Mouse0;
	
	[SerializeField]
	private float movementSpeed = 5;

	[SerializeField]
	private float dashSpeedMultiplyer = 1;

	[SerializeField]
	private float jumpInitialSpeed = 5;

	[SerializeField]
	private float invincibilityDuration = 3;

	[Header("Other")]

	[SerializeField]
	private CharacterController characterController;

	[SerializeField]
	private Transform cameraHolder;

	[SerializeField]
	private Transform cameraPosition;

	[SerializeField]
	private Color invincibilityColor = Color.red;

	[SerializeField]
	private Color idleColor = Color.white;


	private Vector3 velocity;

	private bool jumpRequired;
	private bool dashRequired;

	private bool isDashing;


	/*as the usages of this property are rare, 
	 * i'm not caching the material, 
	 * so i don't have to explicitly 
	 * manage cached value on destroy*/
	private Color CurrentColor
	{
		get => GetComponentInChildren<MeshRenderer>().material.color;
		set
		{
			GetComponentInChildren<MeshRenderer>().material.color = value;
		}
	}

	[SyncVar(hook = nameof(SetIsInvincible))]
	private bool isInvinsible;
	public bool IsInvinsible 
	{ 
		get => isInvinsible;
		private set
		{
			CurrentColor = value ? invincibilityColor : idleColor;
			isInvinsible = value;
		} 
	}
	void SetIsInvincible(bool oldValue, bool newValue)
	{
		IsInvinsible = newValue;
	}

	private void Awake()
	{
		characterController.enabled = false;
	}

	public override void OnStartLocalPlayer()
	{
		Debug.Log("Initialising local player");
		characterController.enabled = true;
		SetCameraInPlace(Camera.main);
		CurrentColor = idleColor;
	}

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (!isLocalPlayer) return;
		// Vector2 cameraHolderEulerAngles = cameraHolder.eulerAngles;
		// cameraHolder.eulerAngles = cameraHolderEulerAngles;

		UpdateRotation();

		UpdateNeedForDash();

		UpdateNeedForMove();
	}

	void UpdateRotation()
	{
		transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X"), 0);
		//rotating whole player horizontally

		cameraHolder.rotation *= Quaternion.Euler(
			-Input.GetAxis("Mouse Y"),
			0,
			0);
		//so the camera rotating only vertically
	}

	void UpdateNeedForMove()
	{
		velocity =
			transform.TransformDirection(
				Vector3.ClampMagnitude(HorizontalMovement, 1) * movementSpeed)
			+ Vector3.up * velocity.y;
		if (Input.GetKeyDown(jump)) jumpRequired = true;
		//moving inself happens in fixed update
	}

	private Vector3 HorizontalMovement =>
		new Vector3(
			(Input.GetKey(left) ? -1 : 0)
			+ (Input.GetKey(right) ? 1 : 0),
			0,
			(Input.GetKey(back) ? -1 : 0)
			+ (Input.GetKey(forward) ? 1 : 0));

	void UpdateNeedForDash()
	{
		if (Input.GetKeyDown(dash))
		{
			dashRequired = true;
		}
		//dash itself happens in fixed update along with moving
	}

	void FixedUpdate()
	{
		if (!isLocalPlayer) return;
		float timeDelta = Time.fixedDeltaTime;
		float gravityY = Physics.gravity.y;
		velocity.y = characterController.isGrounded
			? (jumpRequired ? jumpInitialSpeed : gravityY * timeDelta)
			: velocity.y + gravityY * timeDelta;
		jumpRequired = false;


		if (dashRequired)
		{
			isDashing = true;
			Vector3 dashTravelVector =
				characterController.velocity * dashSpeedMultiplyer;
			characterController.Move(dashTravelVector);
			isDashing = false;
			dashRequired = false;
		}
		else
		{
			characterController.Move(velocity * timeDelta);
		}
	}

	private void OnValidate()
	{
		if (characterController == null) characterController =
				GetComponent<CharacterController>();
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (isDashing)
		{
			//we have to check this to not pass the wrong
			//object through network accidentally 
			if(hit.gameObject.GetComponent<IDashTouchable>() != null)
				TouchOnDash(hit.gameObject);
		}
	}

	[Command]
	void TouchOnDash(GameObject dashTouchable)
	{
		dashTouchable
			.GetComponent<IDashTouchable>()
			.OnTouchedDuringDash(gameObject);
	}

	private void SetCameraInPlace(Camera mainCamera)
	{
		Transform transform1 = mainCamera.transform;
		transform1.SetParent(cameraPosition);
		transform1.localPosition = Vector3.zero;
		transform1.localEulerAngles = Vector3.zero;
	}

	public void OnTouchedDuringDash(GameObject gameObject)
	{
		//this function along with IDashTouchable could be
		//moved from PlayerControl to separate component,
		//but i decided to leave it there to keep things simple as it's only a prototype
		//and refactoring of such game would require much deeper restructuring
		if (!IsInvinsible)
		{
			gameObject.GetComponent<PlayerScore>().AddPoint();
			StartCoroutine(InvincibilityRoutine());
		}
	}

	private IEnumerator InvincibilityRoutine()
	{
		IsInvinsible = true;
		yield return new WaitForSeconds(invincibilityDuration);
		IsInvinsible = false;
	}

}