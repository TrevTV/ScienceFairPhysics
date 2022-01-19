using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnrealEngine.Framework;
using Debug = UnrealEngine.Framework.Debug;

namespace Game
{
	/// <summary>
	/// Indicates the main entry point for automatic loading by the plugin
	/// </summary>
	public class Main
	{
        public static int secondsBeforeStart = 1;
        public static int secondsBeforeKill = 30;

        public static Vector3 force = new Vector3(300, 0, 0); // Given triple the force than Unity
        public static float xAxisPositionOffset = 50; // In cm, unlike Unity in meters

        private static float timeCounter;
        private static bool hasInited;
        private static bool hasStarted;
        private static bool hasFinished;

        private static bool hasHitFirstPoint;
        private static Stopwatch movementTimer;
        private static StaticMeshComponent sphereActor; // No direct access to the transform afaik
        private static List<float> pendulumMovementTimings;
        private static GoingTo movingTowards;
        private static Vector3 refLocation;

        private enum GoingTo
        {
            Left,
            Right
        }

        public static void OnWorldPostBegin()
        {
            // Init variables
            pendulumMovementTimings = new List<float>();
            movementTimer = new Stopwatch();

            World.ForEachActor<Actor>(actor =>
            {
                // Find the sphere, this isn't a good method but it works
                if (actor.Name == "StaticMeshActor_2")
                    sphereActor = actor.GetComponent<StaticMeshComponent>();
            });

            hasInited = true;
        }

        private static void StartSimulation()
        {
            // Add force and init variables
            sphereActor.SetPhysicsLinearVelocity(force);
            movingTowards = GoingTo.Left;
            hasStarted = true;
        }

        private static void KillSimulation()
        {
            // Disable physics
            sphereActor.SetSimulatePhysics(false);

            // Write data to disk (hard-coded path because I don't think UnrealCLR has a method for getting the project path)
            File.AppendAllText(@"D:\ScienceFair\UnrealChaos\Content\Test2\test2_data.txt", $"{pendulumMovementTimings.Average()}\n");

            // Log
            Debug.AddOnScreenMessage(0, 5, Color.Green, $"Successfully wrote {pendulumMovementTimings.Count} timings to file!");
            hasFinished = true;

            // Reset?
            var actor = World.GetActorByTag<Actor>("T1Controller");
            var comp = actor.GetComponentByTag<SceneComponent>("T1ControllerBP");
            comp.Invoke($"ReloadScene");
        }

        public static void OnWorldDuringPhysicsTick(float deltaTime)
        {
            if (!hasInited) return;

            // Calculate seconds from init
            timeCounter += deltaTime;

            // A not-so-good replacement for Unity's Invoke method
            if (timeCounter >= secondsBeforeStart && !hasStarted)
            {
                StartSimulation();
                timeCounter = 0;
            }
            else if (pendulumMovementTimings.Count == 20 && !hasFinished)
                KillSimulation();

            if (!hasStarted) return;
            sphereActor.GetLocation(ref refLocation);

            if (refLocation.X >= xAxisPositionOffset && movingTowards == GoingTo.Left)
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
            else if (refLocation.X <= -xAxisPositionOffset && movingTowards == GoingTo.Right)
            {
                // Write data and restart timer
                movementTimer.Stop();
                pendulumMovementTimings.Add(movementTimer.ElapsedMilliseconds);
                movementTimer.Restart();
                movingTowards = GoingTo.Left;
            }
        }
    }
}