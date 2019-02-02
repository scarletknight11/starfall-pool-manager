using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    public string[] keys;

    private Collider col;

    private List<PooledGameObjectManager> pooleds;

    void Awake(){
        this.col = GetComponent<Collider>();
        this.pooleds = new List<PooledGameObjectManager>();
    }

    public void GenerateRandom(){
        string key = this.keys[Random.Range(0,this.keys.Length)];
        Vector3 pos = Vector3.zero;
        for(int i=0; i<100; i++){
            pos = Random.insideUnitCircle * this.col.bounds.size.x;
            pos.y = 10;
            RaycastHit hit;
            Debug.DrawRay(pos, Vector3.down*50, Color.green, 0.5f);
            if(Physics.Raycast(pos, Vector3.down.normalized, out hit, 50)){
                if(hit.transform.name == "Area"){
                    break;
                }
            }
        }

        GameObject reference = GameObjectPoolManager.poolManager["Main"]
        .InstantiateGameobject(key, pos, Quaternion.identity);

        this.pooleds.Add(reference.GetComponent<PooledGameObjectManager>());
    }

    public void DeleteRandom(){
        if(this.pooleds.Count == 0)
            return;

        int randomIndex = Random.Range(0, this.pooleds.Count);
        PooledGameObjectManager reference = this.pooleds[randomIndex];

        GameObjectPoolManager.poolManager["Main"].DestroyGameobject(reference.gameObject);
        this.pooleds.RemoveAt(randomIndex);
    }

    public void Reset(bool fullReset){
        if(fullReset){
            GameObjectPoolManager.poolManager["Main"].ResetFully();
        }else{
            GameObjectPoolManager.poolManager["Main"].Reset();
        }
        
        this.pooleds.Clear();
    }

}
