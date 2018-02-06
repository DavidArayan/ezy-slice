using UnityEngine;
using System.Threading;
using System;
using System.Reflection;

namespace EzySlice {
    /**
     * Simple management class for managing threaded jobs. Made in a generic manner so it
     * can be used in many projects properly. Managing a pool of threads is far better for
     * performance than spawning and destroying threads each and every time.
     */
    public sealed class ThreadPool {
        public const int DEFAULT_POOL_SIZE = 128;

        /**
         * A Basic Internal use class to quickly queue jobs for execution via
         * simple delegates
         */
        internal class BasicJob : ThreadJob {
            // we don't need to override these values once constructed
            private readonly Action run_action;
            private readonly Action end_action;
            private readonly bool runEndOnMain;

            public BasicJob(Action run, Action end, bool runEndOnMain) {
                this.run_action = run;
                this.end_action = end;
                this.runEndOnMain = runEndOnMain;
            }

            public override bool InvokeCompleteOnMain() {
                return runEndOnMain;
            }

            protected override void Complete() {
                if (end_action != null) {
                    end_action();
                }
            }

            protected override void Run() {
                if (run_action != null) {
                    run_action();
                }
            }
        }

        // the primary instance of the Thread Pooler
        private static ThreadPool _INSTANCE;

        public static ThreadPool Instance {
            get {
                if (_INSTANCE == null) {
                    _INSTANCE = new ThreadPool(DEFAULT_POOL_SIZE);
                }

                return _INSTANCE;
            }
        }

        private readonly Thread[] threadPool;
        private readonly ThreadJob[] taskQueue;

        private readonly AutoResetEvent putNotification;
        private readonly AutoResetEvent getNotification;

        private readonly Semaphore latch;

        private int putPointer;
        private int getPointer;
        private int currentTasks;

        /**
         * Return the number of threads which are active in the pool. The more threads
         * means the more jobs can be executed concurrently
         */
        public int ThreadCount {
            get {
               return this.threadPool.Length;
            }
        }

        /**
         * Returns the number of jobs which can exist in the queue at any one time 
         */
        public int QueueCount {
            get {
                return this.taskQueue.Length;
            }
        }

        /**
         * Initialize this pool with a set queue size. 
         * The pool will only have as many threads as there are processors
         * available for optimal execution
         */
        private ThreadPool(int queueSize) {
            // for an initialize for the main thread callback
            ThreadMainQueue queue = ThreadMainQueue.Instance;

            this.threadPool = new Thread[SystemInfo.processorCount];
            this.taskQueue = new ThreadJob[queueSize];

            this.putPointer = 0;
            this.getPointer = 0;
            this.currentTasks = 0;

            this.putNotification = new AutoResetEvent(false);
            this.getNotification = new AutoResetEvent(false);

            // for single threaded applications, we will only manage
            // a single thread of execution which simplifies things
            if (threadPool.Length > 1) {
                // used as a countdown to manage the execution queues
                latch = new Semaphore(0, queueSize);

                for (int i = 0; i < threadPool.Length; i++) {
                    threadPool[i] = new Thread(MultiThreadCallback);
                    threadPool[i].Start();
                }
            }
            else {
                for (int i = 0; i < threadPool.Length; i++) {
                    threadPool[i] = new Thread(SingleThreadCallback);
                    threadPool[i].Start();
                }
            }
        }

        /**
         * This function will be executed by the multi-threaded version
         * of the executor. All threads execute the same job
         */
        private void MultiThreadCallback() {
            // this is an infinite loop which should never exit
            while (true) {
                // to avoid performance issues, we will countdown on our
                // semaphore to ensure we actually have a job to execute
                // otherwise this thread will yield for other processes to run
                latch.WaitOne();

                int currentPointer;
                int nextPointer;

                // grab a new task to execute
                do {
                    currentPointer = getPointer;
                    nextPointer = currentPointer + 1;

                    if (nextPointer == taskQueue.Length) {
                        nextPointer = 0;
                    }
                }
                while (Interlocked.CompareExchange(ref getPointer, nextPointer, currentPointer) != currentPointer);

                ThreadJob task = taskQueue[currentPointer];

                if (Interlocked.Decrement(ref currentTasks) == taskQueue.Length - 1) {
                    getNotification.Set();
                }

                // Grab the private method signature via reflection and invoke at runtime
                Type taskType = task.GetType();
                MethodInfo run = taskType.GetMethod(ThreadJob.RUN_FNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                run.Invoke(task, null);

                // invoke the final stuff on the main thread
                if (task.InvokeCompleteOnMain()) {
                    ThreadMainQueue.Instance.Enqueue(task);
                }
                else {
                    // otherwise we oinvoke right here since the user does not care
                    // if on main or background thread
                    MethodInfo end = taskType.GetMethod(ThreadJob.END_FNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    end.Invoke(task, null); 
                }
            }
        }

        /**
         * This is the callback for single threaded applications (ie systems with only one core)
         * Will probably no longer be required, but good to have just in case
         */
        private void SingleThreadCallback() {
            while (true) {
                while (currentTasks == 0) {
                    putNotification.WaitOne();
                }

                ThreadJob task = taskQueue[getPointer++];

                if (getPointer == taskQueue.Length) {
                    getPointer = 0;
                }

                if (Interlocked.Decrement(ref currentTasks) == taskQueue.Length - 1) {
                    getNotification.Set();
                }

                // Grab the private method signature via reflection and invoke at runtime
                Type taskType = task.GetType();
                MethodInfo run = taskType.GetMethod(ThreadJob.RUN_FNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                run.Invoke(task, null);

                // invoke the final stuff on the main thread
                if (task.InvokeCompleteOnMain()) {
                    ThreadMainQueue.Instance.Enqueue(task);
                }
                else {
                    // otherwise we oinvoke right here since the user does not care
                    // if on main or background thread
                    MethodInfo end = taskType.GetMethod(ThreadJob.END_FNC, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    end.Invoke(task, null);
                }
            }
        }

        /**
         * Add a new job to be executed by the framework
         */
        public void Enqueue(ThreadJob task) {
            // ensure the queue pool has not reached maximum. If it has, freeze the
            // current thread until a spot becomes available
            while (currentTasks == taskQueue.Length) {
                getNotification.WaitOne();
            }

            taskQueue[putPointer] = task;

            ++putPointer;

            if (putPointer == taskQueue.Length) {
                putPointer = 0;
            }

            if (threadPool.Length == 1) {
                if (Interlocked.Increment(ref currentTasks) == 1) {
                    putNotification.Set();
                }
            }
            else {
                Interlocked.Increment(ref currentTasks);
                latch.Release();
            }
        }

        /**
         * Uses Delegates to enqueue a new job into the framework. This is an easier alternative
         * than extending the ThreadJob class for every job. This function will save the delegates which
         * will be called accordingly.
         * 
         * Just like the standard Enqueue function, this call will block if the queue is full.
         */
        public void Enqueue(Action run, Action finished = null, bool runCompleteOnMainThread = true) {
            // don't need to run anything if the run action is null
            if (run == null) {
                // just call finished, unless that's null too!
                if (finished != null) {
                    finished();
                }

                return;
            }

            // generate a new job with the provided delegates and enqueue like any other job
            Enqueue(new BasicJob(run, finished, runCompleteOnMainThread));
        }
    }
}