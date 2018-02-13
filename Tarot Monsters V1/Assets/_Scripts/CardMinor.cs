using UnityEngine;

public class CardMinor {

    public CardType cardType;

    public CardMinor() {
        //set type to random
        cardType = (CardType)Random.Range( 0, 4 );
    }

}

public enum CardType {

    Atk,
    Def,
    Agi,
    Wis

}