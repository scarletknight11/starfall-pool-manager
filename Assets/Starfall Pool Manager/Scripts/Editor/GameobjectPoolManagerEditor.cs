using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameObjectPoolManager))]
[CanEditMultipleObjects]
public class GameobjectPoolManagerEditor : Editor
{
    GameObjectPoolManager poolManager;

    public Texture CloseTexture;

    void OnEnable()
    {
        poolManager = (GameObjectPoolManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        this.poolManager.PoolName = 
        EditorGUILayout.TextField("Pool Name", this.poolManager.PoolName);

        EditorGUILayout.Space();

        // Loop Gameobject and show it in inspector
        EditorGUILayout.LabelField("Pooled Gameobjects");
        string count = "Count: " + this.poolManager.gameObjectsPrefab.Count;
        EditorGUILayout.LabelField(count);

        for(int i = 0; i < this.poolManager.gameObjectsPrefab.Count; i++){
            Rect baseRect = EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Prefab", GUILayout.Width(50));
            
            this.poolManager.gameObjectsPrefab[i] = (GameObject)
            EditorGUILayout.ObjectField(this.poolManager.gameObjectsPrefab[i]
            , typeof(GameObject), false, GUILayout.MinWidth(50), GUILayout.MaxWidth(200));

            EditorGUILayout.LabelField("Key", GUILayout.Width(30));
            
            this.poolManager.gameObjectsPrefabKey[i] = 
            EditorGUILayout.TextField(this.poolManager.gameObjectsPrefabKey[i]);

            EditorGUILayout.LabelField("Count", GUILayout.Width(45));

            this.poolManager.gameObjectsPrefabCount[i] = 
            EditorGUILayout.IntField(this.poolManager.gameObjectsPrefabCount[i]
            , GUILayout.MaxWidth(50), GUILayout.MinWidth(30));

            if(GUILayout.Button(this.CloseTexture, 
            GUILayout.Width(15), GUILayout.Height(15))){
                DeleteGameobject(i);
            }

            EditorGUILayout.EndHorizontal();
            
            // if playing
            // draw bar about used gameobject count and maximal gameobject count
            if(EditorApplication.isPlaying){
                Rect layout = EditorGUILayout.BeginHorizontal();
                layout.height = baseRect.height;
                int used = 0;
                string indexKey = this.poolManager.gameObjectsPrefabKey[i];
                foreach (GameObject obj in 
                    this.poolManager.gameObjectsPools[indexKey])
                {
                    if(obj.activeSelf){
                        used++;
                    }
                }

                string text = used + " / " + this.poolManager.gameObjectsPools[indexKey].Count;
                float ratio = (float)used/(float)this.poolManager.gameObjectsPools[indexKey].Count;

                EditorGUI.ProgressBar(layout, ratio, text);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();
        }

        if(GUILayout.Button("Add New Gameobject")){
            AddGameobject();
        }
    }

    void AddGameobject(){
        this.poolManager.gameObjectsPrefab.Add(null);
        this.poolManager.gameObjectsPrefabCount.Add(0);
        this.poolManager.gameObjectsPrefabKey.Add("");
    }

    void DeleteGameobject(int index){
        this.poolManager.gameObjectsPrefab.RemoveAt(index);
        this.poolManager.gameObjectsPrefabCount.RemoveAt(index);
        this.poolManager.gameObjectsPrefabKey.RemoveAt(index);
    }
}
