[System.Flags]
public enum BoosterFunction
{
    None = 0,
    Jump = 1 << 0,
    Dash = 1 << 1,
    Glide = 1 << 2,
    Flight = 1 << 3
}