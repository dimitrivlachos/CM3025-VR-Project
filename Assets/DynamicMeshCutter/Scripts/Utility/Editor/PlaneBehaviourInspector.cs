using UnityEngine;
using UnityEditor;

namespace DynamicMeshCutter
{
    [CustomEditor(typeof(PlaneBehaviour))]
    public class PlaneBehaviourInspector : Editor
    {
        PlaneBehaviour _pb;

        public void OnEnable()
        {
            _pb = (PlaneBehaviour)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Cut"))
            {
                _pb.Cut();
            }
        }

        void OnSceneGUI()
        {
            DrawPlane(_pb.transform.forward, _pb.transform.position);
        }
        public void DrawPlane(Vector3 normal, Vector3 position)
        {
            Vector3 v;

            if (normal.normalized != Vector3.forward)
                v = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
            else
                v = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

            v *= _pb.DebugPlaneLength;

            var corner0 = position + v;
            var corner2 = position - v;
            var q = Quaternion.AngleAxis(90, normal);
            v = q * v;
            var corner1 = position + v;
            var corner3 = position - v;

            Debug.DrawLine(corner0, corner2, Color.green);
            Debug.DrawLine(corner1, corner3, Color.green);
            Debug.DrawLine(corner0, corner1, Color.green);
            Debug.DrawLine(corner1, corner2, Color.green);
            Debug.DrawLine(corner2, corner3, Color.green);
            Debug.DrawLine(corner3, corner0, Color.green);
            Debug.DrawRay(position, normal, Color.red);
        }
    }
}