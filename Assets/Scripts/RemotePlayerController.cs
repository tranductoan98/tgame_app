using UnityEngine;

[RequireComponent(typeof(PlayerRenderManager))]
public class RemotePlayerController : MonoBehaviour
{
    private Vector3 targetPosition;
    private PlayerRenderManager renderManager;
    public float moveSpeed = 5f;
    private float stopThreshold = 0.01f;
    private bool isMoving = false;

    void Start()
    {
        renderManager = GetComponent<PlayerRenderManager>();
        targetPosition = transform.position;
        renderManager.isMovingFunc = () => isMoving;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > stopThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void SetDirection(string dir)
    {
        renderManager?.SetDirection(dir);
    }
}
