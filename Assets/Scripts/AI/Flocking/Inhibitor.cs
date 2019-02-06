using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inhibitor {

	public enum Type {
		None,
		Water,
        StripNode
	}
	public GameObject go;
	public float radius;
	public Type type;
    public bool inStrip;
    public FlockingObstacle previous, next;
	public Inhibitor(GameObject go, float radius, Type type = Type.None){
		this.go = go;
		this.radius = radius;
        this.inStrip = false;
		this.type = type; 
	}

    public Inhibitor(GameObject go, float radius, FlockingObstacle previous, FlockingObstacle next) : this(go, radius){
        this.inStrip = true;
        this.type = Type.StripNode;
        this.previous = previous;
        this.next = next;
    }
	public Vector3 GetPosition(){
		return go.transform.position;
	}
    public Inhibitor GetPrevious(){
        return (previous != null) ? previous.inhibitor : null;
    }
    public Inhibitor GetNext(){
        return (next != null) ? next.inhibitor : null;
    }
}
