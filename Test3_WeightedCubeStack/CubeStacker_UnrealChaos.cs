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
        public static float startingYPoint = 50; // cm
        public static float unitsBetweenCubes = 150; // cm

        public static int secondsBetweenResets = 15;
        public static int startCubeAmount = 2;
        public static int cubesToAddEachReset = 2;

        private static int currentCubeAmount;
        private static Dictionary<StaticMeshComponent, Vector3> components;

        private static float timeCounter;
        private static bool hasInited;

        public static int repetitionAmount = 20;

        public static void OnWorldPostBegin()
        {
            components = new Dictionary<StaticMeshComponent, Vector3>();
            currentCubeAmount = startCubeAmount - cubesToAddEachReset;
        }

        public static void Loop()
        {
            List<float> distances = new List<float>();

            foreach (var pair in components)
            {
                // Disable physics for all actors and compare the distance from the inital position
                StaticMeshComponent c = pair.Key;
                c.SetSimulatePhysics(false);
                Vector3 location = new();
                c.GetLocation(ref location);
                distances.Add(Vector3.Distance(location, pair.Value) / 100);
                c.GetActor<Actor>().Destroy();
            }

            if (distances.Count > 0)
            {
                File.AppendAllText(@"D:\ScienceFair\UnrealChaos\Content\Test3\test3_repeated_data.txt", $"{distances.Average()}\n");
            }

            if (currentCubeAmount == 20)
            {
                Debug.AddOnScreenMessage(-1, 3, Color.Green, "Completed cycle!");
                File.AppendAllText(@"D:\ScienceFair\UnrealChaos\Content\Test3\test3_repeated_data.txt", $"\n\n");

                var actor = World.GetActorByTag<Actor>("T1Controller");
                var comp = actor.GetComponentByTag<SceneComponent>("T1ControllerBP");
                comp.Invoke($"ReloadScene");
            }

            components.Clear();
            currentCubeAmount += cubesToAddEachReset;

            float currentYPoint = startingYPoint;
            float mass = 0.25f;
            for (int i = 0; i < currentCubeAmount; i++)
            {
                StaticMeshComponent staticMeshComponent = new(new Actor(), setAsRoot: true);
                staticMeshComponent.SetStaticMesh(StaticMesh.Cube);
                staticMeshComponent.SetMaterial(0, Material.Load("/Game/Test2/Red"));
                staticMeshComponent.SetWorldLocation(new(0, 0, currentYPoint));
                staticMeshComponent.SetRelativeRotation(Maths.CreateFromYawPitchRoll(0.0f, 0.0f, 0.0f));
                staticMeshComponent.SetMass(mass);
                staticMeshComponent.SetSimulatePhysics(true);

                components.Add(staticMeshComponent, new(0, 0, currentYPoint - unitsBetweenCubes));

                currentYPoint += unitsBetweenCubes;
                mass += 0.25f;
            }
        }

        public static void OnWorldDuringPhysicsTick(float deltaTime)
        {
            // Calculate seconds from init
            timeCounter += deltaTime;

            // A not-so-good replacement for Unity's IEnumerators
            if (timeCounter >= secondsBetweenResets || !hasInited)
            {
                Loop();
                timeCounter = 0;
                hasInited = true;
            }
        }
    }
}