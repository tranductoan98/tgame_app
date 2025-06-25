using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class MapSceneManager : MonoBehaviour
{
    public static MapSceneManager Instance { get; private set; }
    public GameObject tilePrefab;
    public GameObject PlayerRoot;
    public GameObject Player;
    public Transform mapContainer;
    private MapSceneUI ui;
    public int mapId = 1;
    public float tileSize = 1;
    public int mapWidth = 0;
    public int mapHeight = 0;
    private Dictionary<int, Texture2D> atlasCache = new();
    private Dictionary<Vector2Int, MapTileData> tileDataMap = new();
    private Dictionary<int, GameObject> otherPlayers = new();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(MapSceneAPI.FetchMapInfo(mapId, OnMapInfoLoaded, Debug.LogError));
    }

    void OnMapInfoLoaded(MapInfo info)
    {
        int playerid = PlayerPrefs.GetInt("playerid");

        mapWidth = info.width;
        mapHeight = info.height;

        StartCoroutine(PlayerAPI.getPositionByPlayerId(playerid, positionData =>
        {
            if (positionData == null) return;

            Player.transform.position = new Vector3(positionData.x, positionData.y, 0);
            Player.transform.SetParent(PlayerRoot.transform, false);

            StartCoroutine(PlayerRenderUtil.LoadAndRenderPlayer(this, Player.transform, playerid, renderParts =>
            {
                var controller = Player.GetComponent<PlayerController>();
                var renderManager = Player.GetComponent<PlayerRenderManager>();
                if (renderManager == null)
                    renderManager = Player.AddComponent<PlayerRenderManager>();

                renderManager.Setup(renderParts, () => controller != null && controller.isMoving);
            }));

            CameraManager camCtrl = Camera.main.GetComponent<CameraManager>();
            if (camCtrl != null)
            {
                camCtrl.target = Player.transform;
                camCtrl.tileSize = tileSize;
                camCtrl.InitMapSize(info.width, info.height);
            }

            StartCoroutine(MapSceneAPI.FetchMapTiles(mapId, RenderTiles, Debug.LogError));

            StartCoroutine(PlayerAPI.GetPlayersInMap(mapId, players =>
            {
                foreach (var kvp in otherPlayers)
                {
                    Destroy(kvp.Value);
                }
                otherPlayers.Clear();

                foreach (var otherPlayer in players)
                {
                    if (otherPlayer.playerId == playerid)
                        continue;

                    GameObject otherPlayerGO = new GameObject($"Player_{otherPlayer.playerId}");
                    otherPlayerGO.transform.position = new Vector3(otherPlayer.x, otherPlayer.y, 0);
                    otherPlayerGO.transform.SetParent(PlayerRoot.transform, false);

                    var remoteCtrl = otherPlayerGO.AddComponent<RemotePlayerController>();

                    StartCoroutine(PlayerRenderUtil.LoadAndRenderPlayer(this, otherPlayerGO.transform, otherPlayer.playerId, renderParts =>
                    {
                        var renderManager = otherPlayerGO.AddComponent<PlayerRenderManager>();
                        renderManager.Setup(renderParts, () => false);
                    }));

                    otherPlayers[otherPlayer.playerId] = otherPlayerGO;
                }
            }, Debug.LogError));
        }, Debug.LogError));
    }
    void RenderTiles(List<MapTileData> tiles)
    {
        tileDataMap.Clear();
        foreach (MapTileData tile in tiles)
        {
            Vector2Int pos = new Vector2Int(tile.renderX, tile.renderY);
            tileDataMap[pos] = tile;

            Sprite sprite = LoadTileSprite(tile);
            GameObject obj = Instantiate(tilePrefab, mapContainer);
            obj.name = $"Tile_{tile.renderX}_{tile.renderY}";
            obj.transform.position = new Vector3(tile.renderX * tileSize, tile.renderY * tileSize, 0);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = tile.zorder;
            obj.transform.localScale = Vector3.one;
        }
    }
    Sprite LoadTileSprite(MapTileData tile)
    {
        if (!atlasCache.TryGetValue(tile.imageId, out Texture2D atlas))
        {
            atlas = Resources.Load<Texture2D>($"maps/{tile.imageId}");
            atlasCache[tile.imageId] = atlas;
        }
        Rect rect = new Rect(tile.x, tile.y, tile.w, tile.h);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(atlas, rect, pivot, tile.w);
    }
    public bool CanMoveToPosition(Vector2 worldPosition)
    {
        int tileX = Mathf.FloorToInt(worldPosition.x / tileSize);
        int tileY = Mathf.FloorToInt(worldPosition.y / tileSize);
        Vector2Int tilePos = new Vector2Int(tileX, tileY);

        if (tileX < 0 || tileX >= mapWidth || tileY < 0 || tileY >= mapHeight)
            return false;

        if (tileDataMap.TryGetValue(tilePos, out MapTileData tile))
        {
            return tile.isWalkable;
        }

        return false;
    }
    public void OnSocketMessageReceived(string json)
    {
        var baseMsg = JsonConvert.DeserializeObject<BaseSocketMessage>(json);
        if (baseMsg == null || string.IsNullOrEmpty(baseMsg.type)) return;

        switch (baseMsg.type)
        {
            case "update_move":
                HandleUpdateMove(json);
                break;

            case "player_online":
                HandlePlayerOnline(json);
                break;

            case "player_offline":
                HandlePlayerOffline(json);
                break;
        }
    }

    void HandlePlayerOnline(string json)
    {
        var otherPlayer = JsonConvert.DeserializeObject<PlayerOnlineMessage>(json);
        if (otherPlayer == null) return;

        if (otherPlayers.ContainsKey(otherPlayer.playerId)) return;

        GameObject otherPlayerGO = new GameObject($"Player_{otherPlayer.playerId}");
        otherPlayerGO.transform.position = new Vector3(otherPlayer.x, otherPlayer.y, 0);
        otherPlayerGO.transform.SetParent(PlayerRoot.transform, false);

        otherPlayerGO.AddComponent<RemotePlayerController>();

        StartCoroutine(PlayerRenderUtil.LoadAndRenderPlayer(this, otherPlayerGO.transform, otherPlayer.playerId, renderParts =>
        {
            var renderManager = otherPlayerGO.AddComponent<PlayerRenderManager>();
            renderManager.Setup(renderParts, null);
        }));

        otherPlayers[otherPlayer.playerId] = otherPlayerGO;
    }


    public void HandleUpdateMove(string json)
    {
        var otherPlayer = JsonConvert.DeserializeObject<UpdateMoveMessage>(json);
        if (otherPlayer == null) return;

        if (otherPlayers.TryGetValue(otherPlayer.playerId, out GameObject playerGO))
        {
            var remote = playerGO.GetComponent<RemotePlayerController>();

            if (remote != null)
            {
                remote.SetDirection(otherPlayer.direction);
                remote.SetTargetPosition(new Vector3(otherPlayer.x, otherPlayer.y, 0));
            }
        }
    }

    private void HandlePlayerOffline(string json)
    {
        var message = JsonConvert.DeserializeObject<PlayerOfflineMessage>(json);
        if (message == null) return;

        int playerId = message.playerId;

        if (otherPlayers.TryGetValue(playerId, out GameObject playerGO))
        {
            Destroy(playerGO);
            otherPlayers.Remove(playerId);

            Debug.Log($"👋 Player {playerId} đã offline và bị xóa khỏi bản đồ.");
        }
        else
        {
            Debug.LogWarning($"⚠️ Nhận player_offline cho playerId {playerId} nhưng không tìm thấy trong danh sách.");
        }
    }
    
    public void HandleLogout()
    {
        StartCoroutine(PlayerAPI.LogoutPlayer(
        onSuccess: () =>
        {
            Debug.Log("Đăng xuất thành công");
            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.Disconnect();
                Destroy(WebSocketManager.Instance.gameObject);
                WebSocketManager.Instance = null;
                Debug.Log("🧹 Đã huỷ WebSocketManager");
            }

            SceneManager.LoadScene("LoginScene");
        },
        onError: (err) =>
        {
            Debug.LogError("Lỗi đăng xuất: " + err);
        }
        ));
    }
}