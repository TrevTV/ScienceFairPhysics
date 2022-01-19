using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BulletUnity;

namespace Trevor.ScienceFair.Stacking
{
    public class CubeStacker_UnityBullet : MonoBehaviour
    {
        public static int currentRun;
        public static string data;

        public Mesh cubeMesh;
        public float startingYPoint;
        public float unitsBetweenCubes;

        public int secondsBetweenResets;
        public int startCubeAmount;
        public int cubesToAddEachReset;

        public GameObject cubePrefab;

        [SerializeField] private int currentCubeAmount;
        private List<SingleCube> currentCubes;

        private void Start()
        {
            currentCubes = new List<SingleCube>();
            currentCubeAmount = startCubeAmount - cubesToAddEachReset;
            StartCoroutine(CoLoop());
        }

        private IEnumerator CoLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(secondsBetweenResets);
            while (true)
            {
                List<float> distances = new List<float>();
                foreach (SingleCube rb in currentCubes)
                {
                    distances.Add(rb.CalculateFinalDistance());
                    Destroy(rb.gameObject);
                }
                if (distances.Count > 0)
                {
                    //Debug.Log($"Average distance from cube spawn, {distances.Average()}");
                    data += $"{distances.Average()}\n";
                }

                if (currentCubeAmount == 20)
                {
                    Debug.Log("Complete!");
                    Debug.Log(data);
                    data = "";
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Test3");
                    break;
                }

                currentCubes.Clear();

                currentCubeAmount += cubesToAddEachReset;

                float currentYPoint = startingYPoint;
                float mass = 0.25f;
                for (int i = 0; i < currentCubeAmount; i++)
                {
                    GameObject go = GameObject.Instantiate(cubePrefab, new Vector3(0, currentYPoint, 0), Quaternion.identity);
                    currentYPoint += unitsBetweenCubes;
                    go.GetComponent<BRigidBody>().mass = mass;
                    go.GetComponent<MeshFilter>().sharedMesh = cubeMesh;
                    var sC = go.AddComponent<SingleCube>();
                    currentCubes.Add(sC);
                    sC.BeginPosTracking(this);
                    mass += 0.25f;
                }

                yield return wait;
            }
        }

        private class SingleCube : MonoBehaviour
        {
            private CubeStacker_Unity parent;
            private Vector3 basePosition;
            private Transform body;

            public void BeginPosTracking(CubeStacker_Unity Parent)
            {
                parent = Parent;
                // Enable physics and keep the original position
                body = transform;
                basePosition = body.position;
                basePosition.y -= parent.unitsBetweenCubes;
                //body.isKinematic = false;
            }

            public float CalculateFinalDistance()
            {
                // Find distance and disable physics
                float distance = Vector3.Distance(basePosition, body.position);
                //body.isKinematic = true;

                return distance;
            }
        }
    }
}