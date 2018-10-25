using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Linear {

	public static Vector3 CrossProduct(Vector3 v1, Vector3 v2){
		return new Vector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
	}

	public static float DotProduct(Vector3 v1, Vector3 v2){
		return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
	}

	public static Vector3 Rodrigues(Vector3 v, Vector3 k, float theta, Vector3 cross = default(Vector3), float dot = default(float)){
		if(cross == default(Vector3)){
			cross = CrossProduct(k, v);
		}

		if(dot == default(float)){
			dot = DotProduct(k, v);
		}

		return v * Mathf.Cos(Mathf.Deg2Rad * theta) + cross * Mathf.Sin(Mathf.Deg2Rad * theta) + k * dot * (1 - Mathf.Cos(Mathf.Deg2Rad * theta));
	}
}
