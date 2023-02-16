using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DynamicMeshCutter
{
    public class AsycWorker
    {
        CutterBehaviour _behaviour;
        public AsycWorker(CutterBehaviour behaviour)
        {
            _behaviour = behaviour;
            int threads = SystemInfo.processorCount;
            threads = Mathf.Max(0, threads - 1);
            Initialize(threads);
        }
        class CuttingTask
        {
            public WaitCallback Callback;
            public object Arg;
        }

        public OnCut OnCut; //callback for cutter
        private readonly AutoResetEvent _autoReset = new AutoResetEvent(false);
        private Queue<CuttingTask> _queue = new Queue<CuttingTask>();

        private Thread[] _pool;
        private MeshCutting _meshCut = new MeshCutting();
        private void Initialize(int threads)
        {
            _pool = new Thread[threads];
            for (int i = 0; i < threads; i++)
            {
                _pool[i] = new Thread(Thread);
                _pool[i].IsBackground = true;
                _pool[i].Name = "thread" + i;
                _pool[i].Start();
            }

            
        }

        private void Thread()
        {
            //while (true)
            while(_behaviour != null && _behaviour.CutterIsEnabled && !CutterBehaviour.ApplicationHasQuit)
            {
                if (_queue.Count == 0)
                {
                    _autoReset.WaitOne();
                }
                else
                {
                    try
                    {
                        CuttingTask task = _queue.Dequeue();
                        if (task == null)
                        {
                            continue;
                        }

                        task.Callback(task.Arg);
                    }
                    catch (InvalidOperationException e)
                    {
                        Debug.LogWarning(e);
                    }
                }
            }
            Debug.Log("Dynamic Mesh Cutter: Thread finished successfully!");
        }
        public void Enqeue(Info info)
        {
            CuttingTask cuttingTask = new CuttingTask
            {
                Callback = new WaitCallback(AsyncCutting),
                Arg = info,
            };

            _queue.Enqueue(cuttingTask);
            _autoReset.Set();
        }
        private void AsyncCutting(object args)
        {
            Info info = args as Info;
            if (info == null)
            {
                return;
            }

            try
            {
                VirtualMesh[] cutMeshes = _meshCut.Cut(ref info);
                info.CreatedMeshes = cutMeshes;
                if (cutMeshes == null)
                {
                    if (OnCut != null)
                    {
                        OnCut.Invoke(false, info);
                    }
                    return;
                }

                if (OnCut != null)
                {
                    OnCut.Invoke(true, info);
                }
            }
            catch (Exception e)
            {
                if (OnCut != null)
                {
                    OnCut.Invoke(false, info);
                }
                Debug.LogError("Async cutter failed." + e);
            }
        }

    }


}
