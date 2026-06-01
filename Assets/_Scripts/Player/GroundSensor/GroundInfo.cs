using UnityEngine;

// 땅 상태 판정용
// 땅이 감지 유무, 거리, 충돌 지점, 각도
// 공중 높이 조절이나 지형에 따른 위치 조정등에 필요함
public struct GroundInfo
{
    public bool HasGround;
    public float Distance;
    public Vector3 Point;
    public Vector3 Normal;
}