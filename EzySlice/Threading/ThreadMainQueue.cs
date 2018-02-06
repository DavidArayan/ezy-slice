using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace EzySlice {

    /**
     * Handles the callbacks from threads which require the finish() method to be
     * Called on the Unity main thread. This can be useful if updating UI objects
     * or performing operations which require main thread access.
     */
    public class ThreadMainQueue : MonoBehaviour {
        // our main queue of jobs which require a callback
        private readonly Queue<ThreadJob> finishedJobs = new Queue<ThreadJob>();

        // The locker is used to ensure we don't run into race conditions when working
        // with the queue
        private readonly object locker = new object();

        // static getter instance for this object
        private static ThreadMainQueue _INSTANCE;

        /**
         * We use a singleton to represent our object which is attached to an
         * Instantiated GameObject. SingletonPool ensures that this object does
         * not get created multiple times in the scene.
         */
        public static ThreadMainQueue Instance {
            get {
                if (_INSTANCE == null) {
                    _INSTANCE = SingletonPool.Get<ThreadMainQueue>();
                }

                return _INSTANCE;
            }
        }

        /**
         * Since this object is attached to a GameObject, the Update() method gets called
         * automatically by Unity. This allows the thread callback to be performed on the main
         * thread.
         * 
         * We will only dequeue a single job per loop, so we don't overwhelm the system and cause lag spikes
         */
        void Update() {
            ThreadJob job = null;

            // ensure we don't run into race conditions when dequeue from the queue
            lock (locker) {
                if (finishedJobs.Count > 0) {
                    job = finishedJobs.Dequeue();
                }
            }

            if (job != null) {
                // Grab the private method signature via reflection and invoke. The reason the method
                // is private is so users do not invoke it by accident!
                Type taskType = job.GetType();
                MethodInfo run = taskType.GetMethod(ThreadJob.END_FNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                run.Invoke(job, null);
            }
        }

        /**
         * This function will be called by the Threading Framework to queue
         * a completed job for completion.
         */
        public void Enqueue(ThreadJob job) {
            // avoid adding a null job
            if (job == null) {
                return;
            }

            lock (locker) {
                finishedJobs.Enqueue(job);
            }
        }
    }
}