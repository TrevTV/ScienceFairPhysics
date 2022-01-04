using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Trevor.ScienceFair.Pendulum
{
    public class AddPendulumForce_UnityPhysX : MonoBehaviour
    {
        public float secondsBeforeStart = 5f;
        public float secondsBeforeKill = 60f;
        
        public Joint joint;
        public Vector3 force = new Vector3(100, 0, 0);
        public float xAxisPositionOffset = 0.7f;

        private bool started;
        private bool hasHitFirstPoint;
        private Stopwatch movementTimer;
        private Transform cachedTransform;
        private List<float> pendulumMovementTimings;
        private GoingTo movingTowards;

        private enum GoingTo
        {
            Left,
            Right
        }
        
        private void Start()
        {
            // Setup variable instances
            cachedTransform = joint.connectedBody.transform;
            movementTimer = new Stopwatch();
            pendulumMovementTimings = new List<float>();
            
            // Begin wait
            Invoke(nameof(GiveForce), secondsBeforeStart);
        }

        private void LateUpdate()
        {
            if (!started) return;
            
            // If hit furthest left point
            if (cachedTransform.position.x >= xAxisPositionOffset && movingTowards == GoingTo.Left)
            {
                // Setup timer if it's the first hit
                if (!hasHitFirstPoint)
                {
                    hasHitFirstPoint = true;
                    movingTowards = GoingTo.Right;
                    movementTimer.Start();
                    return;
                }
                    
                // Write data and restart timer
                movementTimer.Stop();
                pendulumMovementTimings.Add(movementTimer.ElapsedMilliseconds);
                movementTimer.Restart();
                movingTowards = GoingTo.Right;
            }
            // If hit furthest right point
            else if (cachedTransform.position.x <= -xAxisPositionOffset && movingTowards == GoingTo.Right)
            {
                // Write data and restart timer
                movementTimer.Stop();
                pendulumMovementTimings.Add(movementTimer.ElapsedMilliseconds);
                movementTimer.Restart();
                movingTowards = GoingTo.Left;
            }
        }

        public void GiveForce()
        {
            // Apply force and setup variables
            joint.connectedBody.velocity = force;
            movingTowards = GoingTo.Left;
            started = true;
            
            // Begin wait for KillCount
            Invoke(nameof(KillCount), secondsBeforeKill);
        }

        public void KillCount() => StartCoroutine(CoKillCount());

        private IEnumerator CoKillCount()
        {
            // Wait until we have an even number of timings
            while ((pendulumMovementTimings.Count - 1) % 2 == 0)
                yield return null;
            
            // Stop timer and physics
            joint.connectedBody.isKinematic = true;
            movementTimer.Stop();
            
            // Setup data for export
            string finalData = "";
            foreach (float val in pendulumMovementTimings)
                finalData += $"{val}\n";

            // Write data to disk
            File.WriteAllText(Application.dataPath + "\\Test2\\test2_data.txt", finalData);
            
            // Log
            Debug.Log($"Successfully wrote {pendulumMovementTimings.Count} timings to file!");
        }
    }
}