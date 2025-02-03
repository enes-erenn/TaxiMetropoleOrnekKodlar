using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MiniMapUtils
{
    RectTransform minimapRect, playerMarker;
    Matrix4x4 transformationMatrix;
    List<MarkerData> markers;
    GameObject player;
    ZoomData zoomData;
    Vector2 worldSize;
    Vector3 mapCenter;

    /// <summary>
    /// It prepares the minimap using the given inputs.
    /// <br/>
    /// 
    /// <br/>
    /// Player
    /// <br/>
    /// Player rect object in the minimap.
    /// <br/>
    /// 
    /// <br/>
    /// World Size
    /// <br/>
    /// World size is the width and length of the terrain.
    /// <br/>
    /// 
    /// <br/>
    /// Player
    /// <br/>
    /// Player object in the world.
    /// <br/>
    /// 
    /// <br/>
    /// Map Center
    /// <br/>
    /// Map center is the pivot point of the terrain. If you don't use terrain then create a transform and locate it to the left-bottom corner of the map.
    /// <br/>
    /// 
    /// </summary>
    /// <param name="_minimapRect"></param>
    /// <param name="_playerMarker"></param>
    /// <param name="_worldSize"></param>
    /// <param name="_player"></param>
    /// <param name="_mapCenter"></param>
    public MiniMapUtils(RectTransform _minimapRect, RectTransform _playerMarker, Vector2 _worldSize, GameObject _player, Vector3 _mapCenter, List<MarkerData> _markers)
    {
        minimapRect = _minimapRect;
        worldSize = _worldSize;
        playerMarker = _playerMarker;
        player = _player;
        mapCenter = _mapCenter;
        markers = _markers;
        zoomData = new();

        GetTransformationMatrix();
    }

    public void OnUpdate(float offset)
    {
        ZoomMap();
        MoveMap();
        MovePlayer();
        MoveMarkers(offset);
    }

    public void SwitchZoom()
    {
        zoomData.ZoomIn = !zoomData.ZoomIn;
    }

    /// <summary>
    /// Moves the player marker on the map according to the world position.
    /// </summary>
    void MovePlayer()
    {
        // Make marker scale constant
        float iconScale = 1 / minimapRect.transform.localScale.x;

        Vector3 playerPos = player.transform.position - mapCenter;
        Vector3 rotation = player.transform.rotation.eulerAngles;
        Vector2 position = WorldToMapPosition(playerPos);

        playerMarker.anchoredPosition = position;
        playerMarker.localScale = Vector3.one * iconScale;
        playerMarker.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
    }

    /// <summary>
    /// Moves and rotates the markers on the map.
    /// <br/>
    /// 
    /// <br/>
    /// Offset
    /// <br/>
    /// Distance from the minimap center.
    /// <br/>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    void MoveMarkers(float offset)
    {
        for (int m = 0; m < markers.Count; m++)
        {
            MarkerData marker = markers[m];

            // Make marker scale constant
            float iconScale = 1 / minimapRect.transform.localScale.x;

            Vector2 markerRectPos = WorldToMapPosition(marker.Position - mapCenter);

            Vector2 direction = markerRectPos - playerMarker.anchoredPosition;
            float distance = direction.magnitude;

            offset *= iconScale;

            // Is distance more than the minimap size?
            if (distance > offset)
            {
                direction.Normalize();
                markerRectPos = playerMarker.anchoredPosition + direction * offset;
            }

            if (marker.MarkerT == null)
                continue;

            Vector2 toPlayer = playerMarker.anchoredPosition - markerRectPos;
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

            marker.MarkerT.anchoredPosition = markerRectPos;
            marker.MarkerT.localRotation = Quaternion.Euler(0, 0, angle);
            marker.MarkerT.localScale = Vector3.one * iconScale;
        }
    }

    /// <summary>
    /// Moves the minimap in the opposite direction of the player move.
    /// </summary>
    void MoveMap()
    {
        float mapScale = minimapRect.transform.localScale.x;

        minimapRect.anchoredPosition = -playerMarker.anchoredPosition * mapScale;
    }

    /// <summary>
    /// Zooms the minimap within the min and max bounds.
    /// </summary>
    void ZoomMap()
    {
        float currentMapScale = minimapRect.localScale.x;

        float zoomAmount = (zoomData.ZoomIn ? zoomData.Speed : -zoomData.Speed) * currentMapScale * Time.deltaTime;
        float newScale = currentMapScale + zoomAmount;

        float clampedScale = Mathf.Clamp(newScale, zoomData.MinMax.x, zoomData.MinMax.y);

        minimapRect.localScale = Vector3.one * clampedScale;
    }

    /// <summary>
    /// Converts the worlds position to the rect position using the transformation matrix.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns>Rect position of the given world position</returns>
    Vector2 WorldToMapPosition(Vector3 worldPos)
    {
        Vector2 pos = new(worldPos.x, worldPos.z);
        return transformationMatrix.MultiplyPoint3x4(pos);
    }

    void GetTransformationMatrix()
    {
        Vector2 translation = -minimapRect.rect.size / 2; // Center of the map
        Vector2 scaleRatio = minimapRect.rect.size / worldSize;

        transformationMatrix = Matrix4x4.TRS(translation, Quaternion.identity, scaleRatio);

        //  {scaleRatio.x,   0,           0,   translation.x},
        //  {  0,        scaleRatio.y,    0,   translation.y},
        //  {  0,            0,           1,            0},
        //  {  0,            0,           0,            1}
    }

    [Serializable]
    public class ZoomData
    {
        public bool ZoomIn;
        public float Speed;
        public Vector2 MinMax;

        public ZoomData()
        {
            ZoomIn = true;
            Speed = 1f;
            MinMax = new(1, 2);
        }
    }
}
