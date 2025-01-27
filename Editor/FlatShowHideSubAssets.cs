using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Flat.subtools;

namespace Flat.subtools{
    public class FlatShowHideSubAssets:MonoBehaviour{
        [MenuItem("Assets/FlatSubtools/FlatShowHideSubAssets")]
        private static void ShowHideSubAssets(){
            Object selectedAsset = Selection.activeObject;
            if(selectedAsset != null) {
                //EditorWindow.GetWindow<FlatHideAssetsWindow>("FlatSubTools-ShowHideSubAssets");
                FlatHideAssetsWindow.ShowWindow(selectedAsset);
            }else{
                Debug.LogWarning("No asset selected.");
            }
        }
    }
    public class FlatHideAssetsWindow : EditorWindow{
        private Object rootObj;
        private Object[] subAssets;
        public static void ShowWindow(Object obj){
            FlatHideAssetsWindow window = GetWindow<FlatHideAssetsWindow>("FlatSubTools-ShowHideSubAssets");
            window.rootObj = obj;
            window.subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj));
            window.Show();
        }
        private void OnGUI(){
            EditorGUILayout.LabelField("SubAssetは" + (subAssets.Length-1) + "あります。");
            for(int i=0;i<subAssets.Length;i++){
                if(subAssets[i]==rootObj){
                    EditorGUILayout.LabelField(subAssets[i].name + ":" + subAssets[i].GetType() + "<----Main Asset");
                }else{
                    EditorGUILayout.LabelField(subAssets[i].name + ":" + subAssets[i].GetType() + "<----Sub Asset");
                }
            }
        }
    }
}
