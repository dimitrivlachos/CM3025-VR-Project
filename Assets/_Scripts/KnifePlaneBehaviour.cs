/*
 * 
 * Code modified by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 * 
 */

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DynamicMeshCutter
{
    public class KnifePlaneBehaviour : CutterBehaviour
    {
        public float DebugPlaneLength = 2;
        public void Cut()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (!root.activeInHierarchy)
                    continue;
                var targets = root.GetComponentsInChildren<MeshTarget>();
                foreach (var target in targets)
                {
                    Cut(target, transform.position, transform.forward, null, OnCreated);
                }
            }
        }

        void OnCreated(Info info, MeshCreationData cData)
        {
            MeshCreation.TranslateCreatedObjects(info, cData.CreatedObjects, cData.CreatedTargets, Separation);
            
            // Add scripts to game object
            GameObject[] gameObjects = cData.CreatedObjects;

            foreach (var gameObject in gameObjects) {
                gameObject.AddComponent<OffsetInteractable>();
                gameObject.AddComponent<FoodItem>();
                gameObject.GetComponent<FoodItem>().hasBeenCut = true;
                gameObject.layer = 6;
            }
        }
    }
}
