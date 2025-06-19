using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform target;        
    public float tileSize = 1f;     
    public float followSpeed = 5f;  
    public float minSize = 1f;      
    public float maxSize = 3f;      
    private float mapWidthUnits;
    private float mapHeightUnits;
    private float camHalfWidth;
    private float camHalfHeight;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        cam.orthographic = true;
    }

    public void InitMapSize(int mapWidthTiles, int mapHeightTiles)
    {
        mapWidthUnits = mapWidthTiles * tileSize;
        mapHeightUnits = mapHeightTiles * tileSize;
        AdjustCameraSize();
        UpdateCameraHalfSize();
    }

    void UpdateCameraHalfSize()
    {
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    void AdjustCameraSize()
    {
        float aspect = cam.aspect;

        float sizeByWidth = (mapWidthUnits / aspect) / 2f;
        float sizeByHeight = mapHeightUnits / 2f;

        float camSize = Mathf.Max(sizeByWidth, sizeByHeight);
        cam.orthographicSize = Mathf.Clamp(camSize, minSize, maxSize);

        UpdateCameraHalfSize();
    }

    void LateUpdate()
    {
        if (cam == null || target == null) return;

        Vector3 pos = target.position;

        if (mapWidthUnits <= camHalfWidth * 2 || mapHeightUnits <= camHalfHeight * 2)
        {
            transform.position = new Vector3(mapWidthUnits / 2f, mapHeightUnits / 2f, -10f);
            return;
        }

        float clampedX = Mathf.Clamp(pos.x, camHalfWidth, mapWidthUnits - camHalfWidth);
        float clampedY = Mathf.Clamp(pos.y, camHalfHeight, mapHeightUnits - camHalfHeight);

        Vector3 targetPos = new Vector3(clampedX, clampedY, -10f);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
    }
}