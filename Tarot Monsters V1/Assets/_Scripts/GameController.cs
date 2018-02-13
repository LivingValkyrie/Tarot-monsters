using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum TurnPhase {

	Start,
	Draw,
	Roll,
	Move,
	Combat,
	End

}

public class GameController : MonoBehaviour {
	#region fields

	[Tooltip("the nodes of the map. place in order")]
	public Transform[] movementNodes;

	[Tooltip("the nodes of the alternate path. place in order")]
	public Transform[] movementNodesAlt;

	public TurnPhase phase; //public for testing

	//panel variables
	public GameObject rollPanel;
	public GameObject startPanel;
	public Text startPanelText;

	public GameObject combatPanel;
	public Text combatPanelText;

	public GameObject endPanel;
	public Text endPanelText;

	public GameObject splitPanel;

	public GameObject finalPrompt;
	public Text finalPromptText;

	public Text logText;

	public string[] monsterNames;

	//combat variables
	int nodeAtk;
	int nodeDef;
	CombatType combatType;

	//multiplayer
	public PlayerMovement[] players;
	int currentPlayer;
	public Camera[] cameras;

	//split path stuff
	bool canMove = true;

	//boss combat stuff
	bool[] bossCombat;

	//tarot card stuff
	public GameObject eventPanel;
	public List<CardMinor> minorArcanaDeck;

	//draw phase
	public GameObject drawPanel;
	public Text drawPanelText;

	//stat display
	public Text statDisplay;

	//singleton
	public GameController S;

	#endregion

	// Use this for initialization
	void Start() {
		if (S == null) {
			S = this;
		}

		phase = TurnPhase.Start;
		currentPlayer = 0;

		//initialize deck
		minorArcanaDeck = new List<CardMinor>(40);
		for (int i = 0; i < minorArcanaDeck.Capacity; i++) {
			minorArcanaDeck.Add(new CardMinor());
		}

		//hard coded to disable all but camera 1
		for (int i = 1; i < cameras.Length; i++) {
			cameras[i].enabled = false;
		}

		bossCombat = new bool[players.Length];
		for (int i = 1; i < bossCombat.Length; i++) {
			bossCombat[i] = false;
		}

		//disable all but player 1's particles
		for (int i = 1; i < players.Length; i++) {
			players[i].gameObject.GetComponent<ParticleSystem>().enableEmission = false;
			players[i].path = new Transform[movementNodes.Length];
			for (int j = 0; j < players[i].path.Length; j++) {
				players[i].path[j] = movementNodes[j];
			}
		}

		//set players path to basic path
		foreach (PlayerMovement t in players) {
			t.path = new Transform[movementNodes.Length];
			for (int j = 0; j < t.path.Length; j++) {
				t.path[j] = movementNodes[j];
			}
		}
	}

	// Update is called once per frame
	void Update() {
		statDisplay.text = players[currentPlayer].transform.name + "\n" + players[currentPlayer].attack + "\n" +
		                   players[currentPlayer].defense
		                   + "\n" + players[currentPlayer].agility + "\n" + players[currentPlayer].wisdom;

		switch (phase) {
			case TurnPhase.Start:
				StartTurn();
				break;
			case TurnPhase.Draw:
				if (players[currentPlayer].wisdom > players[currentPlayer].arcanaCount) {
					DrawPhase();
				} else {
					phase = TurnPhase.Roll;
				}

				break;
			case TurnPhase.Roll:
				Roll();
				break;
			case TurnPhase.Move:
				MoveForward(players[currentPlayer], players[currentPlayer].path);
				break;
			case TurnPhase.Combat:
				CombatPhase();
				break;
			case TurnPhase.End:
				EndTurn();
				break;
		}
	}

	void DrawPhase() {
		drawPanel.SetActive(true);

		drawPanelText.text = "Draw arcana for wisdom score!";
	}

	public void DrawButton() {
		drawPanel.SetActive(false);

		DrawArcana();
	}

	void DrawArcana() {
		gameObject.GetComponent<AudioSource>().Play();
		players[currentPlayer].arcanaCount++;
		if (minorArcanaDeck.Count > 0) {
			switch (minorArcanaDeck[0].cardType) {
				case CardType.Agi:
					players[currentPlayer].agility++;
					PrintToLog("you drew a minor arcana! " + minorArcanaDeck[0].cardType.ToString() + " increased");
					minorArcanaDeck.RemoveAt(0);
					break;
				case CardType.Wis:
					players[currentPlayer].wisdom++;
					PrintToLog("you drew a minor arcana! " + minorArcanaDeck[0].cardType.ToString() + " increased");
					minorArcanaDeck.RemoveAt(0);
					break;
				case CardType.Atk:
					players[currentPlayer].attack++;
					PrintToLog("you drew a minor arcana! " + minorArcanaDeck[0].cardType.ToString() + " increased");
					minorArcanaDeck.RemoveAt(0);
					break;
				case CardType.Def:
					players[currentPlayer].defense++;
					PrintToLog("you drew a minor arcana! " + minorArcanaDeck[0].cardType.ToString() + " increased");
					minorArcanaDeck.RemoveAt(0);
					break;
			}
		}
	}

	void EventSpace() {
		eventPanel.SetActive(true);
	}

	public void EventButton() {
		gameObject.GetComponent<AudioSource>().Play();
		eventPanel.SetActive(false);
		DrawArcana();
		EndTurn();
	}

	void EndPhase() {
		gameObject.GetComponent<AudioSource>().Play();

		//set active player to next player if they exist, if not set to first
		//set active camera to active players camera
		cameras[currentPlayer].enabled = false;

		if (currentPlayer + 1 < players.Length) {
			currentPlayer++;
		} else {
			currentPlayer = 0;
		}

		cameras[currentPlayer].enabled = true;

		phase = TurnPhase.Start;
	}

	enum CombatType {

		Attack,
		Evade,
		BossAttack,
		BossEvade

	}

	void CombatPhase() {
		combatPanel.SetActive(true);
	}

	void PrintToLog(string log) {
		logText.text = log;
	}

	void StartTurn() {
		startPanelText.text = players[currentPlayer].gameObject.name + "'s turn";
		players[currentPlayer].gameObject.GetComponent<ParticleSystem>().enableEmission = true;
		startPanel.SetActive(true);
	}

	public void StartButton() {
		gameObject.GetComponent<AudioSource>().Play();
		startPanel.SetActive(false);
		phase = TurnPhase.Draw;
	}

	void EndTurn() {
		phase = TurnPhase.End;
		endPanelText.text = players[currentPlayer].gameObject.name + " end turn";
		players[currentPlayer].gameObject.GetComponent<ParticleSystem>().enableEmission = false;
		endPanel.SetActive(true);
	}

	public void EndButton() {
		gameObject.GetComponent<AudioSource>().Play();
		endPanel.SetActive(false);
		EndPhase();
		phase = TurnPhase.Start;
	}

	void Roll() {
		rollPanel.SetActive(true);
	}

	public void RollButton() {
		gameObject.GetComponent<AudioSource>().Play();
		rollPanel.SetActive(false);

		//testing values
		players[currentPlayer].boardPosition += RollDice(1, DiceType.D6);

		phase = TurnPhase.Move;
	}

	int RollDice(int count, DiceType type) {
		int totalRoll = 0;
		int roll = 0;
		for (int i = 0; i < count; i++) {
			if (type == DiceType.D3) {
				roll = Random.Range(1, 4);
			} else if (type == DiceType.D6) {
				roll = Random.Range(1, 7);
			}

			totalRoll += roll;
		}

		PrintToLog("you rolled " + totalRoll);

		return totalRoll;
	}

	enum DiceType {

		D3,
		D6

	}

	void MoveForward(PlayerMovement currentPlayer, Transform[] currentPath) {
		//move player

		if (canMove) {
			Vector3 currPos = currentPlayer.transform.position;

			if (currentPlayer.boardPosition >= currentPath.Length) {
				currentPlayer.boardPosition = currentPath.Length - 1;
			}
			if (currentPlayer.currentNode >= currentPath.Length) {
				currentPlayer.currentNode = currentPath.Length - 1;
			}

			if (currentPlayer.currentNode < currentPlayer.boardPosition) {
				Vector3 newPos = Vector3.Lerp(currPos,
				                              currentPath[currentPlayer.currentNode + 1].position +
				                              currentPlayer.offset,
				                              5 * Time.deltaTime);
				newPos.y = currPos.y;
				currentPlayer.transform.position = newPos;

				//transform.Translate(newPos);

				Vector3 distanceBetweenNodes = currentPlayer.transform.position - currPos;

				if (distanceBetweenNodes.magnitude < .01) { //transform.position == GC.movementNodes[currentNode + 1].position
					currentPlayer.currentNode++;

					//read node type. 
					MovementNode node = currentPath[currentPlayer.currentNode].GetComponent<MovementNode>();
					switch (node.type) {
						case NodeType.Split:
							splitPanel.SetActive(true);
							canMove = false;

							//split path pop up
							break;
						case NodeType.Boss:
							BossNodeLogic();

							//boss pop up
							//set movement to this node, so they cannot move further
							break;
					}
				}
			} else {
				//pull in node info
				MovementNode node = currentPath[currentPlayer.currentNode].GetComponent<MovementNode>();
				NodeHandling(node);
			}
		}
	}

	public void AttackChoice() {
		combatType = bossCombat[currentPlayer] ? CombatType.BossAttack : CombatType.Attack;
		Combat();
	}

	public void EvadeChoice() {
		combatType = bossCombat[currentPlayer] ? CombatType.BossEvade : CombatType.Evade;
		Combat();
	}

	void Combat() {
		gameObject.GetComponent<AudioSource>().Play();

		switch (combatType) {
			case CombatType.Attack:

				int attackRoll = RollDice(players[currentPlayer].attack, DiceType.D3);

				if (attackRoll > nodeDef) {
					PrintToLog("you rolled " + attackRoll + " and won!");
					EndTurn();
				} else {
					PrintToLog("you rolled " + attackRoll + " and lost! and teleported back " +
					           (nodeDef - attackRoll) + " spaces!");
					Teleport(players[currentPlayer], players[currentPlayer].path, nodeDef - attackRoll);

					//move player back.
					EndTurn();
				}

				break;
			case CombatType.Evade:

				int evadeRoll = RollDice(players[currentPlayer].agility, DiceType.D3);

				if (evadeRoll > nodeAtk) {
					PrintToLog("you rolled " + evadeRoll + " and won!");
					EndTurn();
				} else {
					PrintToLog("you rolled " + evadeRoll + " and lost! and teleported back " + (nodeAtk - evadeRoll) +
					           " spaces!");
					Teleport(players[currentPlayer], players[currentPlayer].path, nodeAtk - evadeRoll);

					//move player back.
					EndTurn();
				}

				break;
			case CombatType.BossAttack:
				attackRoll = RollDice(players[currentPlayer].attack, DiceType.D3);

				if (attackRoll > nodeDef) {
					PrintToLog("you rolled " + attackRoll + " and won!");
					players[currentPlayer].path[players[currentPlayer].currentNode + 1].GetComponent<MovementNode>().
					                      bossHealth = 0;
					EndTurn();
				} else {
					PrintToLog("you rolled " + attackRoll +
					           " and lost! teleported back to the start of tier! (path has been reset.)");

					//teleport to first space
					Vector3 newPos = players[currentPlayer].path[0].position;
					newPos.y = players[currentPlayer].transform.position.y;
					players[currentPlayer].transform.position = newPos + players[currentPlayer].offset;
					players[currentPlayer].boardPosition = players[currentPlayer].currentNode = 0;

					//default path
					if (players[currentPlayer].altPath) {
						for (int j = 0; j < players[currentPlayer].path.Length; j++) {
							players[currentPlayer].path[j] = movementNodes[j];
						}
					}
					EndTurn();
				}
				break;

			case CombatType.BossEvade:
				evadeRoll = RollDice(players[currentPlayer].agility, DiceType.D3);

				if (evadeRoll > nodeAtk) {
					PrintToLog("you rolled " + evadeRoll + " and won!");
					players[currentPlayer].path[players[currentPlayer].currentNode + 1].GetComponent<MovementNode>().
					                      bossHealth--;
					EndTurn();
				} else {
					PrintToLog("you rolled " + evadeRoll +
					           " and lost! teleported back to the start of tier! (path has been reset.)");

					//teleport to first space
					Vector3 newPos = players[currentPlayer].path[0].position;
					newPos.y = players[currentPlayer].transform.position.y;
					players[currentPlayer].transform.position = newPos + players[currentPlayer].offset;
					players[currentPlayer].boardPosition = players[currentPlayer].currentNode = 0;

					//default path
					if (players[currentPlayer].altPath) {
						for (int j = 0; j < players[currentPlayer].path.Length; j++) {
							players[currentPlayer].path[j] = movementNodes[j];
						}
					}

					EndTurn();
				}
				break;
		}
		combatPanel.SetActive(false);
		bossCombat[currentPlayer] = false;
	}

	void BossNodeLogic() {
		if (players[currentPlayer].path[players[currentPlayer].currentNode].GetComponent<MovementNode>().bossHealth > 0) {
			//boss is alive
			//cap movement to currrent space
			players[currentPlayer].boardPosition = players[currentPlayer].currentNode;
			players[currentPlayer].currentNode--;

			bossCombat[currentPlayer] = true;

			MovementNode node =
				players[currentPlayer].path[players[currentPlayer].currentNode + 1].GetComponent<MovementNode>();

			nodeAtk = node.nodeAttack;
			nodeDef = node.nodeDefense;

			//combat logic
			combatPanelText.text = "Boss Encountered! \n" + "Attack: " + nodeAtk + " Defense: " + nodeDef;
			combatPanel.SetActive(true);
		}
	}

	public void SplitPathChange() {
		//disable modal
		splitPanel.SetActive(false);

		//if reg path change to alt, if alt change to normal
		if (!players[currentPlayer].altPath) {
			for (int j = 0; j < players[currentPlayer].path.Length; j++) {
				players[currentPlayer].path[j] = movementNodesAlt[j];
			}
		} else {
			for (int j = 0; j < players[currentPlayer].path.Length; j++) {
				players[currentPlayer].path[j] = movementNodes[j];
			}
		}
		players[currentPlayer].altPath = !players[currentPlayer].altPath;
		canMove = true;
	}

	public void SplitPathStay() {
		canMove = true;

		//disable modal
		splitPanel.SetActive(false);
	}

	void NodeHandling(MovementNode node) {
		gameObject.GetComponent<AudioSource>().Play();

		switch (node.type) {
			case NodeType.Split:
				PrintToLog("you landed on a split path node");
				phase = TurnPhase.End;

				break;

			case NodeType.Combat:
				PrintToLog("you landed on a combat node");
				phase = TurnPhase.Combat;
				nodeAtk = node.nodeAttack;
				nodeDef = node.nodeDefense;
				combatPanelText.text = monsterNames[Random.Range(0, monsterNames.Length)] + " Attacks!\n" +
				                       "Attack: " + nodeAtk + " Defense: " + nodeDef;
				break;

			case NodeType.Event:
				PrintToLog("you landed on an event node");
				EventSpace();

				//play event
				break;

			case NodeType.Teleport:
				PrintToLog("you landed on a teleport node");

				//change path
				SplitPathChange();

				//teleport to new path teleporter
				Vector3 newPos = players[currentPlayer].path[players[currentPlayer].boardPosition].position;
				newPos.y = players[currentPlayer].transform.position.y;
				players[currentPlayer].transform.position = newPos + players[currentPlayer].offset;
				EndTurn();
				break;

			case NodeType.Empty:
				PrintToLog("you landed on an empty node");
				EndTurn();
				break;

			case NodeType.Boss:
				PrintToLog("you landed on a boss node");
				break;

			case NodeType.FinalSpace:
				PrintToLog("you landed on the last space. you win!");
				FinalSpace();
				break;
		}
	}

	void FinalSpace() {
		finalPromptText.text = players[currentPlayer].transform.name + " Made it to the end first!";
		finalPrompt.SetActive(true);
	}

	void Teleport(PlayerMovement currentPlayer, Transform[] currentPath, int newSpace) {
		//pass in the player, the players path and players.currentNode - difference
		int spaceToMoveTo;
		if (currentPlayer.currentNode - newSpace < 0) {
			spaceToMoveTo = 0;
		} else {
			spaceToMoveTo = currentPlayer.currentNode - newSpace;
		}
		Vector3 newPos = currentPath[spaceToMoveTo].position;
		newPos.y = currentPlayer.transform.position.y;
		currentPlayer.transform.position = newPos + currentPlayer.offset;
		currentPlayer.currentNode = currentPlayer.boardPosition -= newSpace; //set board and current position
	}

}

/*


    void MoveBackward(PlayerMovement currentPlayer, Transform[] currentPath) {
        //move player
        Debug.Log( "teleport started" );
        Vector3 currPos = currentPlayer.transform.position;

        //shouldnt need but keep incase
        if(currentPlayer.boardPosition >= currentPath.Length) { currentPlayer.boardPosition = currentPath.Length - 1; }
        if(currentPlayer.currentNode >= currentPath.Length) { currentPlayer.currentNode = currentPath.Length - 1; }

        if(currentPlayer.currentNode > currentPlayer.boardPosition) { //must change boardposition before calling this method
            Debug.Log( "able to move backwards" );
            Vector3 newPos = Vector3.Lerp( currPos,
                                           currentPath[currentPlayer.currentNode].position,
                                           5 * Time.deltaTime );
            newPos.y = currPos.y;
            currentPlayer.transform.position = newPos;

            Vector3 distanceBetweenNodes = currentPlayer.transform.position - currPos;

            if(distanceBetweenNodes.magnitude < .01) { //transform.position == GC.movementNodes[currentNode + 1].position
                Debug.Log( "position test passed" );
                currentPlayer.currentNode--;
            }

        }

        Debug.Log( "move backward ended" );
    }



*/