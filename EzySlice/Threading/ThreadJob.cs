using UnityEngine;
using System.Collections;

namespace EzySlice {

    /**
     * A simple Threaded Job interface with a run and finished methods which
     * are invoked by the threading framework. Functions are protected so outside
     * objects cannot invoke them
     */
    public abstract class ThreadJob {
        // these need to match the Method signatures to be implemented
        public const string RUN_FNC = "Run";
        public const string END_FNC = "Complete";

        /** 
         * This function will be invoked in a seperate thread by the 
         * framework. Perform the job in here
         */
        protected abstract void Run();

        /**
         * This function will be invoked once the Run() method completed.
         * Depending on user preferences, it can be invoked in the main thread
         * or a seperate thread
         */
        protected abstract void Complete();

        /**
         * Checks weather the job should be invoked on the main thread or a background
         * thread. Invoke on main thread if ui changes are required
         */
        public abstract bool InvokeCompleteOnMain();
    }
}