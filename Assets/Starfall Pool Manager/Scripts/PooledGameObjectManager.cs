using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// compile with: -doc:PooledGameObjectManager.xml

///<summary>
/// <c>PooledGameObjectManager</c> control lifetime of pooled gameobject,
/// and store reference to default pooled prefab. 
/// <c>PooledGameObjectManager</c> will restore default properties according 
/// to pooled prefab. use inheritance to extend this class
///</summary>

public class PooledGameObjectManager : MonoBehaviour
{
    ///<summary><c>PoolManagerName</c> stores reference to belonging
    ///<see cref="GameObjectPoolManager.PoolName"/>
    ///</summary>
    public string PoolManagerName;

    ///<summary>Reference to belonging <c>GameObjectPoolManager</c>
    ///</summary>
    protected GameObjectPoolManager poolManager{
        get{
            return GameObjectPoolManager.poolManager[this.PoolManagerName];
        }
    }

    protected string _indexKey;
    public string indexKey{
        get{
            return this._indexKey;
        }
    }

    protected GameObject _prefab;
    public GameObject prefab{
        get{
            return this._prefab;
        }
    }

    ///<summary>
    /// Call this function to kill gameobject and will never be canceled.
    /// <para/>Will trigger <c>OnDie()</c> callback
    ///</summary>
    public void Die(){
        // OnDie event
        OnDie();

        // Destroy
        this.poolManager.DestroyGameobject(gameObject);
    }

    ///<summary>
    /// Call this function to kill gameobject with delay and will never be canceled.
    /// <para/>Will trigger <c>OnDie()</c> callback
    ///</summary>
    public void Die(float delay){
        // OnDie event
        OnDie();

        // Destroy
        this.poolManager.DestroyGameobject(gameObject, delay);
    }

    ///<summary>
    ///<c>OnDie()</c> callback.
    ///<para/>Called immediately after calling <c>Die()</c> function.
    ///<para/><seealso cref="PooledGameObjectManager.Die()"/>
    ///</summary>
    
    // OnDie event
    protected virtual void OnDie(){
        
    }

    ///<summary>
    ///<c>OnInstantiate()</c> callback.
    ///<para/>Called after gameobject is instantiated by 
    ///<c>GameObjectPoolManager.InstantiateGameobject()</c> function.
    ///<para/><seealso cref="GameObjectPoolManager.InstantiateGameobject(string, Vector3, Quaternion)"/>
    ///</summary>

    // OnInstantiate event
    public virtual void OnInstantiate(){

    }

    ///<summary>
    ///<c>OnDestroyed()</c> callback.
    ///<para/>Called after gameobject is destroyed by 
    ///<c>GameObjectPoolManager.DestroyGameobject()</c> function.
    ///<para/><seealso cref="GameObjectPoolManager.DestroyGameobject(GameObject)"/>
    ///</summary>

    // OnDestroy event
    public virtual void OnDestroyed(){
        
    }

    public void SetProperties(string indexKey, GameObject prefab){
        this._indexKey = indexKey;
        this._prefab = prefab;
    }
}
