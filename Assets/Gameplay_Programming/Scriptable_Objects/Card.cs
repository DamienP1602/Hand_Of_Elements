using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/Card")]
public class Card : ScriptableObject, INetworkSerializable
{
    public int CardID;
    public string CardName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref CardID);
        serializer.SerializeValue(ref CardName);
    }
}
