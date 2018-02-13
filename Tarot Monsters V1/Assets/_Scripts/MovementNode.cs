using UnityEngine;

public class MovementNode : MonoBehaviour {

    public NodeType type;

    public int nodeAttack;
    public int nodeDefense;

    public int bossHealth;

}

public enum NodeType {

    Empty,
    Combat,
    Teleport,
    Split,
    Event,
    Boss,
    FinalSpace

}