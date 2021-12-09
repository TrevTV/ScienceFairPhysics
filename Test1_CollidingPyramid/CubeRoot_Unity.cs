using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Trevor.ScienceFair.CollidingPyramid
{
    public class CubeRoot_Unity : MonoBehaviour
    {
        public float secondsBeforeStart = 2f;
        public float secondsBeforeKill = 5f;

        private Rigidbody[] cubeRbs;
        private List<SingleCube> cubeTrackers;

        private void Start()
        {
            // Setup every cube found under this object.
            cubeRbs = gameObject.GetComponentsInChildren<Rigidbody>();
            cubeTrackers = new List<SingleCube>();

            foreach (Rigidbody cube in cubeRbs)
            {
                if (cube.gameObject.activeInHierarchy)
                    cubeTrackers.Add(cube.gameObject.AddComponent<SingleCube>());
            }

            // Start wait for simulation start
            Invoke(nameof(BeginSimulation), secondsBeforeStart);
        }

        private void BeginSimulation()
        {
            // Tell each cube to enable physics
            foreach (SingleCube cube in cubeTrackers)
                cube.BeginPosTracking();

            // Start wait for simulation end
            Invoke(nameof(KillSimulation), secondsBeforeKill);
        }

        private void KillSimulation()
        {
            // Get all the distances from the cubes
            List<float> distances = new List<float>();
            foreach (SingleCube cube in cubeTrackers)
                distances.Add(cube.CalculateFinalDistance());

            // Log average
            Debug.Log($"Average distance from cube spawn, {distances.Average()}");
        }

        private class SingleCube : MonoBehaviour
        {
            private Vector3 basePosition;
            private Rigidbody body;

            public void BeginPosTracking()
            {
                // Enable physics and keep the original position
                body = GetComponent<Rigidbody>();
                basePosition = body.position;
                body.isKinematic = false;
            }

            public float CalculateFinalDistance()
            {
                // Find distance and disable physics
                float distance = Vector3.Distance(basePosition, body.position);
                body.isKinematic = true;

                return distance;
            }
        }
    }
}
