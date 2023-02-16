using System.Collections.Generic;
using UnityEngine;

namespace DynamicMeshCutter
{

    public delegate void OnCut(bool success, Info info); //after cut, before mesh creation
    public delegate void OnCreated(Info info, MeshCreationData creationData); //after original mesh target has been destroyed. after new meshes have been created and instantiate
    public class Info
    {
        //basic info
        public MeshTarget MeshTarget;
        public VirtualPlane Plane;

        ////advanced info
        public Mesh TargetOriginalMesh;
        public VirtualMesh TargetVirtualMesh;
        public Matrix4x4[] Bindposes;
        public Info(MeshTarget target, VirtualPlane plane, OnCut onCut, OnCreated onCreated, object boxed)
        {
            this.MeshTarget = target;
            this.Plane = plane;
            OnCutCallback = onCut;
            OnCreatedCallback = onCreated;
            BoxedUserData = boxed;

            MeshCreation.GetMeshInfo(target, out TargetOriginalMesh, out Bindposes);
            TargetVirtualMesh = new VirtualMesh(TargetOriginalMesh);

            if (target.DynamicRagdoll != null) //dynamic ragdoll could be missing
            {
                TargetVirtualMesh.AssignRagdoll(target.DynamicRagdoll);
            }
        }
     
        //info created during cutting tasks
        public VirtualMesh[] CreatedMeshes;
        public int[] Sides;
        public int[] BT; //buttom (0) or top (1)
        public List<Vector3> LocalFaceCenters = new List<Vector3>();
        //callbacks
        public OnCut OnCutCallback;
        public OnCreated OnCreatedCallback;
        public object BoxedUserData;
        public List<Vector3> GetWorldFaceCenters()
        {
            var worldCenters = new List<Vector3>();
            for (int i = 0; i < LocalFaceCenters.Count; i++)
            {
                worldCenters.Add(MeshTarget.transform.TransformPoint(LocalFaceCenters[i]));
            }
            return worldCenters;
        }
    }

    public abstract class CutterBehaviour : MonoBehaviour
    {
        public float Separation = 0.02f;
        [Tooltip("Automatically destroy the original object that is cut, when cut")]
        public bool DestroyTargets = true;
        [Tooltip("Use multiple threads to cut. Drastically reduces lag. Recommend ON")]
        public bool UseAsync = true;
        [Tooltip("Cut objects whose vertices are LESS than this will NOT be created")]
        public int VertexCreationThreshold = 0;
        public Material DefaultMaterial;

        private bool _cutterIsEnabled;
        public bool CutterIsEnabled => _cutterIsEnabled;

        public static bool ApplicationHasQuit = true;

        private AsycWorker _asyncWorker; //does the async work
        public AsycWorker AsyncWorker
        {
            get
            {
                if (_asyncWorker == null)
                    InitializeWorker();
                return _asyncWorker;
            }
        }
        private List<Info> _successes = new List<Info>();
        private List<Info> _fails = new List<Info>();
        private Queue<Info> _qSuccesses = new Queue<Info>();
        private Queue<Info> _qFails = new Queue<Info>();

        private bool _isInitialized = false;

        void InitializeWorker()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            _asyncWorker = new AsycWorker(this);
            _asyncWorker.OnCut += OnCut;
        }

        void Terminate()
        {
            _asyncWorker = null;
        }

        private void OnApplicationQuit()
        {
            ApplicationHasQuit = true;
        }

        private void Awake()
        {
            ApplicationHasQuit = false;
        }
        private void OnEnable()
        {
            _cutterIsEnabled = true;
        }
        private void OnDisable()
        {
            _cutterIsEnabled = false;
            Terminate();    
        }
        protected virtual void Update()
        {
            if (_successes.Count != 0)
            {
                lock (_successes)
                {
                    for (int i = 0; i < _successes.Count; i++)
                    {
                        _qSuccesses.Enqueue(_successes[i]);
                    }
                    _successes.Clear();
                }
            }

            while (_qSuccesses.Count != 0)
            {
                Info info = _qSuccesses.Dequeue();
                info.OnCutCallback?.Invoke(true, info);
                CreateGameObjects(info);
            }

            if (_fails.Count != 0)
            {
                lock (_fails)
                {
                    for (int i = 0; i < _fails.Count; i++)
                    {
                        _qFails.Enqueue(_fails[i]);
                    }
                    _fails.Clear();
                }
            }

            while (_qFails.Count != 0)
            {
                Info info = _qFails.Dequeue();
                info.OnCutCallback?.Invoke(false, info);
            }
        }

        public void Cut(MeshTarget target, Vector3 worldPosition, Vector3 worldNormal, OnCut onCut = null, OnCreated onCreated = null, object boxedUserData = null)
        {
            if (!target.isActiveAndEnabled)
                return;

            Matrix4x4 worldToLocalMatrix = target.transform.worldToLocalMatrix;

            if (target.RequireLocal)
            {
                Matrix4x4 scalingMatrix = Matrix4x4.Scale(target.transform.lossyScale);
                worldToLocalMatrix = scalingMatrix * worldToLocalMatrix;
            }

            //Get Local Position
            Vector4 worldP = new Vector4(worldPosition.x, worldPosition.y, worldPosition.z, 1f);
            Vector4[] worldPColumn = new Vector4[4];
            Vector3 localP = worldToLocalMatrix * worldP;

            //Get Local Normal
            Vector3 worldN = new Vector4(worldNormal.x, worldNormal.y, worldNormal.z, 1f);
            Matrix4x4 worldToLocalMatrixNormal = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                var column = worldToLocalMatrix.GetColumn(i);
                if (i == 4)
                    column = new Vector4(0, 0, 0, 1f);
                worldToLocalMatrixNormal.SetColumn(i, column);
            }
            worldToLocalMatrixNormal = worldToLocalMatrixNormal.inverse.transpose;
            Vector3 localN = worldToLocalMatrixNormal * worldN;
            localN.Normalize();

            VirtualPlane plane = new VirtualPlane(localP, localN, worldPosition, worldNormal);
            Info info = new Info(target, plane, onCut, onCreated, boxedUserData);

        

            if (!UseAsync)
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                int amount = 0;

                MeshCutting meshcutting = new MeshCutting();
                VirtualMesh[] virtualMeshes = meshcutting.Cut(ref info);
                info.CreatedMeshes = virtualMeshes;
                if (virtualMeshes == null)
                    OnCut(false, info);
                else
                {
                    OnCut(true, info);
                    amount = virtualMeshes.Length;
                }

                watch.Stop();
                Debug.Log($"Synchronus cut creating {amount} meshes took {watch.ElapsedMilliseconds} ms. Success ? {virtualMeshes != null}");

            }
            else
            {
                AsyncWorker.Enqeue(info);
            }
        }

        protected virtual void CreateGameObjects(Info info)
        {
            MeshCreationData creationInfo = MeshCreation.CreateObjects(info, DefaultMaterial, VertexCreationThreshold);

            if (DestroyTargets)
            {
                if (info.MeshTarget)
                {
                    info.MeshTarget.transform.position = new Vector3(0, -10000, 0);
                    if (info.MeshTarget.GameobjectRoot != null)
                        Destroy(info.MeshTarget.GameobjectRoot, 0);
                    else
                        Destroy(info.MeshTarget.gameObject, 0);
                }
            }

            for(int i =0;i< creationInfo.CreatedObjects.Length; i++)
            {
                if(creationInfo.CreatedObjects[i] == null)
                {
                    Debug.Log("Dynamic Mesh Cutter: Cut supressed creation of object due to VertexCreationThreshold. Make sure you handle NullReferenceExceptions!");
                }
            }

            info.OnCreatedCallback?.Invoke(info, creationInfo);
        }

        private void OnCut(bool success, Info info)
        {
            if (success)
            {
                lock (_successes)
                {
                    _successes.Add(info);
                }
            }
            else
            {
                lock (_fails)
                {
                    _fails.Add(info);
                }
            }
        }
    }
}