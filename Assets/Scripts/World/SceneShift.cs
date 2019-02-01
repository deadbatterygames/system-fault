using UnityEngine;

//
// SceneShift.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Periodically move scene objects in order to center object at the origin
//

public class SceneShift : MonoBehaviour {

    // Public
    public float threshold = 1000f;

    // Private
    Transform[] objects;

    ParticleSystem[] particles;
    ParticleSystem.Particle[] parts = null;

    TrailRenderer[] trails;
    LineRenderer[] lines;

    void FixedUpdate() {
        Vector3 currentPos = transform.position;
        
        if (currentPos.magnitude > threshold) {
            objects = FindObjectsOfType<Transform>();

            // Objects
            foreach (Transform thing in objects) {
                if (thing.parent == null && thing.name != "GameManager" && thing.name != "HUD") {
                    thing.position -= currentPos;
                }
            }

            // Particles
            particles = FindObjectsOfType<ParticleSystem>();
            foreach (ParticleSystem system in particles) {
                if (system.main.simulationSpace != ParticleSystemSimulationSpace.World) continue;

                int numParts = system.main.maxParticles;
                if (numParts <= 0) continue;

                bool wasPaused = system.isPaused;
                bool wasPlaying = system.isPlaying;
                if (!wasPaused) system.Pause();

                if (parts == null || parts.Length < numParts) {
                    parts = new ParticleSystem.Particle[numParts];
                }

                int num = system.GetParticles(parts);
                for (int i = 0; i < num; i++) {
                    parts[i].position -= currentPos;
                }

                system.SetParticles(parts, num);

                if (wasPlaying) system.Play();
            }

            // Trails
            trails = FindObjectsOfType<TrailRenderer>();
            for (int i = 0; i < trails.Length; i++) {
                Vector3[] trailPositions = new Vector3[trails[i].positionCount];
                trails[i].GetPositions(trailPositions);

                for (int j = 0; j < trailPositions.Length; j++) trailPositions[j] -= currentPos;

                trails[i].SetPositions(trailPositions);
            }

            // Lines
            lines = FindObjectsOfType<LineRenderer>();
            for (int i = 0; i < lines.Length; i++) {
                Vector3[] linePositions = new Vector3[lines[i].positionCount];
                lines[i].GetPositions(linePositions);

                for (int j = 0; j < linePositions.Length; j++) linePositions[j] -= currentPos;

                lines[i].SetPositions(linePositions);
            }

            Debug.Log("SceneShift: Scene shifted");
        }
    }
}
