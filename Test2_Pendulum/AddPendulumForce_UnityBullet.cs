using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using BulletUnity;
using Debug = UnityEngine.Debug;
using System.Linq;

namespace Trevor.ScienceFair.Pendulum 
{
    public class AddPendulumForce_UnityBullet : MonoBehaviour
    {
        public static int currentRun = 0;
        public static int toRun = 20;

        public float secondsBeforeStart = 5f;
        public float secondsBeforeKill = 60f;

        public BTypedConstraint joint;
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
            cachedTransform = joint.otherRigidBody.transform;
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
            joint.otherRigidBody.velocity = force;
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
            joint.otherRigidBody.enabled = false;
            movementTimer.Stop();
            started = false;

            // Write data to disk
            File.AppendAllText(Application.dataPath + "\\Test2\\test2_data.txt", $"{pendulumMovementTimings.Average()}\n");

            // Log
            Debug.Log($"Successfully wrote {pendulumMovementTimings.Count} timings to file!");

            currentRun++;
            if (currentRun != toRun)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Test2");
            else
                Application.Quit();
        }
    }
}