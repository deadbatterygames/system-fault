using UnityEngine;

//
// Tree.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Knocks over trees when they are hit at a certain speed
//

public class TreeKnocker : MonoBehaviour {

    [SerializeField] GameObject brokenTree;
    [SerializeField] GameObject stump;

    static float speedTolerance = 50f;
    static float massTolerance = 50f;

    void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody.mass > massTolerance && collision.relativeVelocity.magnitude > speedTolerance && !GetComponent<Rigidbody>()) {
            Instantiate(stump, transform.position, transform.rotation).transform.localScale = transform.localScale;
            Rigidbody treeRB = Instantiate(brokenTree, transform.position, transform.rotation).GetComponent<Rigidbody>();
            treeRB.velocity = collision.relativeVelocity;
            treeRB.transform.localScale = transform.localScale;
            GameManager.instance.AddGravityBody(treeRB);

            Destroy(gameObject);
        }
    }
}
