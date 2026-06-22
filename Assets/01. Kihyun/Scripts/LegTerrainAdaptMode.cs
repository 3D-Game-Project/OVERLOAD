public enum LegTerrainAdaptMode
{
    None,           // 지형 시각 보정 없음
    BodyTiltOnly,   // 몸체만 지형에 맞춰 기울임
    FootIK,         // 발 IK까지 사용
    Suspension      // 바퀴/서스펜션 방식
}