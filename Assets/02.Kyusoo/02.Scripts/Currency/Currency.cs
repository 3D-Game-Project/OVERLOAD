using UnityEngine;

public class Currency : MonoBehaviour
{
    public DropItem Data { get; private set; }

    public void Initialize(DropItem data)
    {
        Data = data;
    }
}
