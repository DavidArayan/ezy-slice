using UnityEngine;
using System.Collections.Generic;
using System;

namespace EzySlice {
    
    /**
     * Allows creating and maintaining Singleton Objects which are attached to MonoBehaviour/GameObjects.
     * Guarantees Singleton Access to these objects.
     * 
     * Using SingletonPool.Get<YourClass>() will either return a previous instance of the Object or Create
     * and attach a new Instance to a GameObject. As such, Get() method will never return null.
     */
    public sealed class SingletonPool {

        // since we are storing singleton objects, we only need to associate one ID per Instance
        // TO-DO look into hashing strings to store ID's as integers for faster access
        private static readonly Dictionary<string, MonoBehaviour> singletonPool = new Dictionary<string, MonoBehaviour>();

        /**
         * Construct a new instance of the requested object type and return. This function will never
         * return null. Use the optional Action as a callback which will be fired if the instance
         * has been constructed for the first time.
         * 
         * To generate a new instance for an object, the class must inherit from MonoBehaviour since a
         * GameObject will be generated.
         */
        public static T Get<T>(Action<T> firstInstanceCallback = null) where T : MonoBehaviour {
            // grab the specific ID for the requested object. NOTE this can be optimized.
            string id = typeof(T).ToString();

            // early exit for the object if it has been generated previously
            if (singletonPool.ContainsKey(id)) {
                if (singletonPool[id] != null) {
                    return (T)singletonPool[id];
                }
            }

            // this will get executed if the previous instance has been removed
            // or this is a new instance
            T prevInstance = GameObject.FindObjectOfType<T>();

            if (prevInstance == null) {
                GameObject newObject = new GameObject("Singleton_" + id);

                T newInstance = newObject.AddComponent<T>();

                if (singletonPool.ContainsKey(id)) {
                    singletonPool[id] = newInstance;
                }
                else {
                    singletonPool.Add(id, newInstance);
                }

                if (firstInstanceCallback != null) {
                    firstInstanceCallback.Invoke(newInstance);
                }

                return newInstance;
            }

            if (singletonPool.ContainsKey(id)) {
                singletonPool[id] = prevInstance;
            }
            else {
                singletonPool.Add(id, prevInstance);
            }

            // perform a callback (optional)
            if (firstInstanceCallback != null) {
                firstInstanceCallback.Invoke(prevInstance);
            }

            return prevInstance;
        }

        /**
         * Destroy a previous instance if it exists. This will destroy ALL instances of the
         * requested Object which are currently active in the scene. Does nothing if no
         * instance exists.
         */
        public static void Destroy<T>(bool destroyGameObject = false) where T : MonoBehaviour {
            T[] instances = GameObject.FindObjectsOfType<T>();

            if (instances != null) {
                if (destroyGameObject) {
                    for (int i = 0; i < instances.Length; i++) {
                        MonoBehaviour.Destroy(instances[i].gameObject);
                    }
                }
                else {
                    for (int i = 0; i < instances.Length; i++) {
                        MonoBehaviour.Destroy(instances[i]);
                    }
                }
            }
        }
    }
}
