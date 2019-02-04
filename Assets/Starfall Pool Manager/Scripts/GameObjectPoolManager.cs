/*
MIT License

Copyright (c) 2019 Starfall Production

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// compile with: -doc:GameObjectPoolManager.xml

///<summary>
/// <c>GameObjectPoolManager</c> manages gameobjects pooled in
/// respective pool manager.
/// <para/> Attach this script into gameobject to make it into Pool Manager
///</summary>

public class GameObjectPoolManager : MonoBehaviour
{
    ///<summary>Name of individual <c>GameObjectPoolManager</c>.
    ///</summary>
    public string PoolName;

    ///<summary>Singleton to easy access all available <c>GameObjectPoolManager</c>.
    ///<example>
    ///This sample show how to use <c>poolManager</c> singleton.
    ///<code>
    ///void Create(){
    ///     var poolManager = GameObjectPoolManager.poolManager["PoolName"];
    ///}
    ///</code>
    ///</example>
    ///</summary>

    // Dictionary of available poolManager that can be accessed using PoolName
    public static Dictionary<string,GameObjectPoolManager> poolManager;
    

    // these 3 lists are a pair
    // and the order in these 3 lists are important
    public List<GameObject> gameObjectsPrefab;
    public List<int> gameObjectsPrefabCount;
    public List<string> gameObjectsPrefabKey;

    public Dictionary<string, GameObject> gameObjectsPrefabPools;
    public Dictionary<string, List<GameObject>> gameObjectsPools;
    private Dictionary<GameObject, Coroutine> destroyRequests;
    
    void Awake()
    {
        if(poolManager == null){
            poolManager = new Dictionary<string, GameObjectPoolManager>();
        }
        poolManager.Add(this.PoolName, this);
        this.gameObjectsPrefabPools = new Dictionary<string, GameObject>();
        this.gameObjectsPools = new Dictionary<string, List<GameObject>>();
        this.destroyRequests = new Dictionary<GameObject, Coroutine>();

        InitGameobjectPool();
    }

    // Destroy all game object and prefab when poolmanager destroyed
    // delete poolManager from dictionary
    // delete dictionary if empty
    void OnDestroy(){
        DestroyAll();
        poolManager.Remove(this.PoolName);
        if(poolManager.Count == 0){
            poolManager = null;
        }
    }

    private void InitGameobjectPool(){
        // init the prefab pool and gameobject pool
        for(int i=0; i<gameObjectsPrefab.Count; i++){
            GameObject reference = AddGameobject(gameObjectsPrefab[i]);

            this.gameObjectsPrefabPools.Add(this.gameObjectsPrefabKey[i]
            , this.gameObjectsPrefab[i]);

            this.gameObjectsPools.Add(this.gameObjectsPrefabKey[i], new List<GameObject>());
        }

        // add the gameObject pool using prefab pool
        for(int i=0; i<gameObjectsPrefab.Count; i++){
            for(int j=0; j<gameObjectsPrefabCount[i]; j++){
                AddGameobject(this.gameObjectsPrefabKey[i]);
            }
        }

    }

    // use this function to instantiate already indexed gameobject in prefab pool
    // and add gameobject into pool automatically
    public GameObject AddGameobject(string indexKey){
        // check the avaiblelity of pool
        List<GameObject> poolRef;
        this.gameObjectsPools.TryGetValue(indexKey, out poolRef);
        if(poolRef == null)
            return null;
        
        // instantiate gameObject and set to not active
        GameObject reference = Instantiate(this.gameObjectsPrefabPools[indexKey]
        ,transform);
        
        reference.SetActive(false);
        
        // add to pool
        poolRef.Add(reference);

        // Add properties into PooledGameObjectManager
        PooledGameObjectManager pooled = reference.GetComponent<PooledGameObjectManager>();
        pooled.SetProperties(indexKey, this.gameObjectsPrefabPools[indexKey]);

        return reference;
    }

    // use this function to add prefab into pool
    private GameObject AddGameobject(GameObject prefab){
        GameObject reference = Instantiate(prefab,transform);
        reference.SetActive(false);
        return reference;
    }

    ///<summary>Use this function to Instantiate Gameobject from pool
    ///<param name="indexKey">Pooled gameobject ID(string)</param>
    ///<param name="where">Where gameobject will be instantiated</param>
    ///<param name="rotation">Instantiated gameobject rotation</param>
    ///<returns>Return reference to instantiated gameobject
    ///, or return <c>null</c> of no pool found</returns>
    ///</summary>

    // use this function to instantiate gameobject from pool
    // don't use this function for non gameobject from pool
    // return null if no pool found
    // return reference to gameobject instantiated from pool if succeed
    public GameObject InstantiateGameobject(string indexKey, Vector3 where, Quaternion rotation){
        List<GameObject> poolRef;
        this.gameObjectsPools.TryGetValue(indexKey, out poolRef);
        if(poolRef == null){
            Debug.Log(indexKey+" not found");
            return null;
        }

        GameObject reference = null;
        bool notAvailable = true;
        
        // Search for available gameobject
        // available gameobject always not active
        foreach(GameObject available in poolRef){
            if(!available.activeSelf){
                reference = available;
                notAvailable = false;
                break;
            }
        }

        // if not available
        // create a new gameobject from prefabPool and add to pool
        // get the reference
        if(notAvailable){
            reference = AddGameobject(indexKey);
        }

        reference.SetActive(true);
        // detach from parent and Move and Rotate
        reference.transform.parent = null;
        reference.transform.position = where;
        reference.transform.rotation = rotation;

        // OnInstantiateEvent
        PooledGameObjectManager pooled = reference.GetComponent<PooledGameObjectManager>();
        pooled.OnInstantiate();

        return reference;
    }

    ///<summary>Use this function to remove Gameobject from scene into pool,
    ///and don't use this on non-pooled Gameobject
    ///<param name="reference">Gameobject that want to be destroyed</param>
    ///</summary>

    // use this function to deinstantiate gameobject from pool
    public void DestroyGameobject(GameObject reference){
        // OnDestroyEvent
        PooledGameObjectManager pooled = reference.GetComponent<PooledGameObjectManager>();
        pooled.OnDestroyed();

        reference.transform.parent = transform;
        reference.transform.localPosition = Vector3.zero;
        reference.transform.localRotation = Quaternion.identity;

        // set gameobject as disable(available for use)
        reference.SetActive(false);
    }

    ///<summary>Use this function to remove Gameobject from scene into pool with delay,
    ///and don't use this on non-pooled Gameobject.
    ///This process can be canceled by calling <see cref="CancelDestroyGameobject"/>
    ///<param name="reference">Gameobject that want to be destroyed</param>
    ///</summary>
    public void DestroyGameobject(GameObject reference, float delay){
        if(this.destroyRequests.ContainsKey(reference))
            return;
        
        Coroutine coroutine = StartCoroutine(DelayDestroyGameobject(reference, delay));
        this.destroyRequests.Add(reference, coroutine);
    }

    ///<summary>Use this function to cancel 
    ///<see cref="DestroyGameobject(GameObject reference, float delay)"/>
    ///<param name="reference">Gameobject that want to be destroyed</param>
    ///</summary>
    public void CancelDestroyGameobject(GameObject reference){
        if(!this.destroyRequests.ContainsKey(reference))
            return;
            
        StopCoroutine(this.destroyRequests[reference]);
        this.destroyRequests.Remove(reference);
    }

    IEnumerator DelayDestroyGameobject(GameObject reference, float delay){
        yield return new WaitForSeconds(delay);
        this.destroyRequests.Remove(reference);
        DestroyGameobject(reference);
        yield return null;
    }

    ///<summary>Use this function to reset pool by destroying pooled gameobject
    ///back into pool
    ///</summary>
    public void Reset(){
        foreach (KeyValuePair<string, List<GameObject>> pair in this.gameObjectsPools)
        {
            foreach(GameObject obj in pair.Value){
                if(obj.activeSelf){
                    obj.transform.parent = transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;

                    // set gameobject as disable(available for use)
                    obj.SetActive(false);
                }
            }
        }
    }

    ///<summary>Use this function to reset pool by destroying pooled gameobject
    ///back into pool. Also reseting pool state into default.
    ///</summary>
    public void ResetFully(){
        for (int i=0;i<this.gameObjectsPools.Count;i++)
        {
            List<GameObject> objs = 
            this.gameObjectsPools[this.gameObjectsPrefabKey[i]];

            for(int j=0; j<objs.Count; j++){
                GameObject obj = objs[j];
                int remaining = objs.Count - this.gameObjectsPrefabCount[i];
                if(remaining > 0){
                    objs.Remove(obj);
                    Destroy(obj);
                    j--;
                }
                else if(obj.activeSelf){
                    obj.transform.parent = transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;

                    // set gameobject as disable(available for use)
                    obj.SetActive(false);
                }
            }
        }
    }

    // Function to destroy all instantiate gameobject
    private void DestroyAll(){
        foreach (KeyValuePair<string, List<GameObject>> pair in this.gameObjectsPools)
        {
            foreach(GameObject obj in pair.Value){
                Destroy(obj);
            }
        }
    }
}
