public enum LegYawControlMode
{
    CameraThresholdFollow,  // 카메라와 각도가 커지면 다리 회전하기 위함 (Spider, Humanoid)
    MoveDirectionFollow,      // 이동방향과 차체 방향이 동일함 (Buggy, Tank, Tracks)
    None
}

public enum LegMoveControlMode
{
    Omnidirectional,    // 전 방향으로 이동 가능 (전후좌우)
    ForwardOnly         // 차제 기준으로 전후만, 좌우는 회전으로 처리
}