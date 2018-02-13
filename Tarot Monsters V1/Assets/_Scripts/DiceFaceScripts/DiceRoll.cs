using UnityEngine;
using System.Collections;

public class DiceRoll : MonoBehaviour {

    public int currentValue = 1;
    public LayerMask dieValueLayerMask = -1;
    bool _isMoving = true;

	Rigidbody rigidbody;

	void Start() {
		rigidbody = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void FixedUpdate() {

		if (rigidbody.IsSleeping()) {
			_isMoving = false;
		}

		if (!_isMoving) {

			RaycastHit hitInfo;

			if (Physics.Raycast(transform.position, Vector3.up, out hitInfo, Mathf.Infinity, dieValueLayerMask)) {

				currentValue = hitInfo.collider.GetComponent<DiceFaceValue>().faceValue;
				Debug.Log(this.ToString() + " " + currentValue);

				//send this roll to diceroller
			}
		}
	}
}

//tbh idk what im doing, im just winging it XD