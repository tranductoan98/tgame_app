using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerInputActions inputActions;
    private SpriteRenderer sr;
    private MapSceneManager mapManager;
    private Vector2 lastSentPosition;
    private string lastSentDirection = "";
    public bool isMoving;
    private PlayerRenderManager renderManager;
    void Awake()
    {
        inputActions = new PlayerInputActions();
    }
    void OnEnable()
    {
        inputActions.Player.Enable();
    }
    void OnDisable()
    {
        inputActions.Player.Disable();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        mapManager = FindFirstObjectByType<MapSceneManager>();
        renderManager = GetComponent<PlayerRenderManager>();
    }
    void Update()
    {
        movement = inputActions.Player.Move.ReadValue<Vector2>();

        isMoving = movement != Vector2.zero;

        if (movement.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

         if (renderManager != null)
            renderManager.SetMovingState(isMoving);
    }
    void FixedUpdate()
    {
        if (movement != Vector2.zero && mapManager != null)
        {
            Vector2 directionVec = movement.normalized;
            Vector2 newPosition = rb.position + directionVec * moveSpeed * Time.fixedDeltaTime;

            if (mapManager.CanMoveToPosition(newPosition))
            {
                rb.MovePosition(newPosition);

                string direction = GetDirection(movement);
                Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(newPosition.x), Mathf.RoundToInt(newPosition.y));

                if (tilePos != Vector2Int.RoundToInt(lastSentPosition) || direction != lastSentDirection)
                {
                    MoveMessage msg = new MoveMessage
                    {
                        x = tilePos.x,
                        y = tilePos.y,
                        direction = direction
                    };

                    WebSocketManager.Instance.SendJson(msg);

                    lastSentPosition = tilePos;
                    lastSentDirection = direction;
                }
            }
        }
    }
    private string GetDirection(Vector2 move)
    {
        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            return move.x > 0 ? "RIGHT" : "LEFT";
        else
            return move.y > 0 ? "UP" : "DOWN";
    }
}