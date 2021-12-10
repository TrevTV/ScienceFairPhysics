using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnrealEngine.Framework;

namespace Game
{
	/// <summary>
	/// Indicates the main entry point for automatic loading by the plugin
	/// </summary>
	public class Main
	{
        public static int secondsBeforeStart = 2;
        public static int secondsBeforeKill = 5;

        private static Dictionary<StaticMeshComponent, Vector3> components;

        private static float timeCounter;
        private static bool hasInited;
        private static bool hasStarted;
        private static bool hasFinished;

        public static void OnWorldPostBegin()
        {
            components = new Dictionary<StaticMeshComponent, Vector3>();
            World.ForEachActor<Actor>(actor =>
            {
                // Find all cubes, this isn't a good method but it works
                if (actor.Name.StartsWith("StaticMeshActor") && !actor.Name.EndsWith("_0"))
                {
                    // Disable their physics and cache the component itself as well as the inital position
                    var staticMesh = actor.GetComponent<StaticMeshComponent>();
                    staticMesh.SetSimulatePhysics(false);
                    Vector3 location = new();
                    staticMesh.GetLocation(ref location);
                    components.Add(staticMesh, location);
                }
            });

            hasInited = true;
        }

        private static void StartSimulation()
        {
            // Enable physics for all cached actors
            Debug.AddOnScreenMessage(-1, 5, Color.Red, "Enabling physics simulation!");
            foreach (StaticMeshComponent c in components.Keys)
                c.SetSimulatePhysics(true);
            hasStarted = true;
        }

        private static void KillSimulation()
        {
            List<float> distances = new List<float>();

            foreach (var pair in components)
            {
                // Disable physics for all actors and compare the distance from the inital position
                StaticMeshComponent c = pair.Key;
                c.SetSimulatePhysics(false);
                Vector3 location = new();
                c.GetLocation(ref location);
                distances.Add(Vector3.Distance(location, pair.Value));
            }

            // Print the averaged result
            Debug.AddOnScreenMessage(-1, 5, Color.Red, $"Average distance from cube spawn, {distances.Average()}");
            hasFinished = true;
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
            else if (timeCounter >= secondsBeforeKill && !hasFinished)
                KillSimulation();
        }
    }
}