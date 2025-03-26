using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Flat.subtools;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using VRC.SDK3;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using VRC.Dynamics;

namespace Flat.subtools{
    public class FlatColorPresetBuilder  : MonoBehaviour
    {
        [MenuItem("FlatSubTools/ColorPresetBuilder")]
        private static void makeComponents(){
            EditorWindow.GetWindow<ColorPresetBuilderWindow>("FlatSubTools-ColorPresetBuilder");
        }
    }
    public class ColorPresetBuilderWindow : EditorWindow{
        GameObject source;
        GameObject prefab;
        string commonName;
        string sourceName = "";
        string targetName = "";

        List<changeMaterialPair> matreialPairs = new List<changeMaterialPair>();
        private Vector2 materialScrollPosition;

        void OnGUI(){
            source = (GameObject)EditorGUILayout.ObjectField("Source",source,typeof(GameObject),true);
            if(GUILayout.Button("Get")){
                GetMaterialPair();
            }
            if(matreialPairs.Count != 0){
                EditorGUILayout.LabelField("Materials");
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField("souece");
                EditorGUILayout.LabelField("target");
                EditorGUILayout.EndHorizontal ();
                
                materialScrollPosition = EditorGUILayout.BeginScrollView(materialScrollPosition);
                foreach(changeMaterialPair pair in matreialPairs){
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.ObjectField("",pair.source,typeof(Material),true);
                    EditorGUILayout.ObjectField("",pair.target,typeof(Material),true);
                    EditorGUILayout.EndHorizontal ();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.Space();
            //EditorGUILayout.TextField("Common Name", commonName);
            EditorGUILayout.BeginHorizontal ();
            sourceName = EditorGUILayout.TextField("source Name", sourceName);
            targetName = EditorGUILayout.TextField("target Name", targetName);
            EditorGUILayout.EndHorizontal ();
            if(GUILayout.Button("GetTarget")){
               SetMaterials();
            }


            EditorGUILayout.Space();
            if(GUILayout.Button("Create New Prefab")){
                clonePedfab(source);
            }
        }

        //Sourceに含まれるマテリアルを一覧にしてそれをsourceに持つMateiralPairsを作成
        private void GetMaterialPair(){
            matreialPairs.Clear();
            foreach(SkinnedMeshRenderer smr in source.GetComponentsInChildren<SkinnedMeshRenderer>()){
                Debug.Log(smr.gameObject.name);
                foreach(Material mat in smr.sharedMaterials){
                    if(matreialPairs.Find(x => x.source == mat) == null){
                        matreialPairs.Add(new changeMaterialPair{source=mat,target=null});
                    }
                }
            }
        }

        //souceMaterialを引数にtargetMaterialを返す
        private Material GetTargetMaterial(Material sourceMat){
            string targetPath = AssetDatabase.GetAssetPath(sourceMat).Replace(sourceName,targetName);
            Debug.Log("[FlatSubtools]Finding " + targetPath + "...");
            Material targetMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GetAssetPath(sourceMat).Replace(sourceName,targetName));
            if(targetMat != null){
                Debug.Log("[FlatSubtools]Found " + targetPath);
                return targetMat;
            }else{
                Debug.Log("[FlatSubtools]Not Found " + targetPath);
                return null;
            }
        }

        //
        private void SetMaterials(){
            foreach(changeMaterialPair pair in matreialPairs){
                pair.target = GetTargetMaterial(pair.source);
            }
        }

        //prefabのマテリアルを変更
        private void ChangeMaterials(GameObject pf){
            foreach(SkinnedMeshRenderer smr in pf.GetComponentsInChildren<SkinnedMeshRenderer>()){
                foreach(Material mat in smr.sharedMaterials){
                    List<Material> mats = new List<Material>();
                    foreach(changeMaterialPair cmp in matreialPairs){
                        if(cmp.source == mat){
                            mats.Add(cmp.target);
                            break;
                        }
                    }
                    smr.sharedMaterials = mats.ToArray();
                }
            }
        }


        //指定したprefabをcloneしてsourceName部分をtargetNameに変更して保存する
        private void clonePedfab(GameObject go){
            GameObject newGo = PrefabUtility.InstantiatePrefab(go) as GameObject;
            newGo.name = go.name.Replace(sourceName,targetName);
            ChangeMaterials(newGo);
            PrefabUtility.SaveAsPrefabAsset(newGo,AssetDatabase.GetAssetPath(go).Replace(sourceName,targetName));
            DestroyImmediate(newGo);
        }
    }

    public class changeMaterialPair{
        public Material source;
        public Material target;
    }
}
