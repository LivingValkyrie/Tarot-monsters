using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Range(0, 23)]
    public int boardPosition;

    [Range(0, 23)]
    public int currentNode;

    public Transform[] path;
    public bool altPath = false;
    public Vector3 offset;

    public int attack;
    public int defense;
    public int agility;
    public int wisdom;

    public int arcanaCount = 0;
    
    void Start() {
        currentNode = 0;
        boardPosition = 0;
    }

    void Update() {
        if(boardPosition < 0) { boardPosition = 0; }
        if(currentNode < 0) { currentNode = 0; }

    }

}