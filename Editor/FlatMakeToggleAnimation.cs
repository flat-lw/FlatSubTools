using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Flat.subtools;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Flat.subtools{
    public class FlatMakeToggleAnimation : MonoBehaviour
    {
        [MenuItem("FlatSubTools/MakeToggleAnimation")]
        private static void makeComponents(){
            EditorWindow.GetWindow<MakeToggleAnimationWindow>("FlatSubTools-MakeToggleAnimation");
        }
    }

    public class MakeToggleAnimationWindow : EditorWindow{
        GameObject target;
        List<GameObject> toggleObjects = new List<GameObject>();
        string menuTitle;
        string paramName;
        bool renameParam = true;
        bool margeMA;
        bool abs;
        Texture2D menuIcon;
        string folderPath;
        string controllerName;
        string menuName;
        List<string> displayAlart = new List<string>();
        void OnGUI(){
            GUIStyle alartTextStyle = new GUIStyle(GUI.skin.label);
            alartTextStyle.normal.textColor = Color.red;
            //テキスト
            EditorGUILayout.LabelField("MAを付けるオブジェクト");
            target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
            EditorGUILayout.LabelField("Toggle対象");
            for(int i=0;i<toggleObjects.Count; i++){
                toggleObjects[i] = (GameObject)EditorGUILayout.ObjectField(toggleObjects[i], typeof(GameObject), true);
            }
            if(GUILayout.Button("Add")){
                toggleObjects.Add(null);
            }
            if(GUILayout.Button("Delete")){
                toggleObjects.RemoveAt(toggleObjects.Count-1);
            }
            menuTitle = EditorGUILayout.TextField("Menuのタイトル", menuTitle);
            paramName = EditorGUILayout.TextField("Parameter名", paramName);
            renameParam = EditorGUILayout.Toggle("Parameterを自動リネーム", renameParam);
            if(renameParam){
                EditorGUILayout.LabelField("", alartTextStyle);
            }else{
                EditorGUILayout.LabelField("Parameterを他のMAから操作しない場合はONにしてください。", alartTextStyle);
            }
            menuIcon = (Texture2D)EditorGUILayout.ObjectField("Menuのアイコン", menuIcon, typeof(Texture2D), false);
            margeMA = EditorGUILayout.Toggle("既に存在するMAに追加する", margeMA);
            if(margeMA){
                EditorGUILayout.LabelField("MAを付ける対象に既に存在するMAに追加されます。足りないConponentは作成します。", alartTextStyle);
            }else{
                EditorGUILayout.LabelField("新規作成します。既に対象に付いているComponentは上書きします。", alartTextStyle);
            }
            abs = EditorGUILayout.Toggle("絶対パスを使う", abs);
            if(GUILayout.Button("Asset保存先を指定")){
                folderPath = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if(!string.IsNullOrEmpty(folderPath)){
                    int assetsIndex = folderPath.IndexOf("Assets");
                    if(assetsIndex >=0){
                        folderPath = folderPath.Substring(assetsIndex);
                    }else{
                        folderPath = null;
                    }
                }
            }
            EditorGUILayout.LabelField("保存先:",folderPath);
            if(margeMA){
                EditorGUILayout.LabelField("AnimatorController名は既存のものに追加されるので指定できません。");
            }else{
                controllerName = EditorGUILayout.TextField("AnimatorController名", controllerName);
            }
            menuName = EditorGUILayout.TextField("VRCExpressionMenu名", menuName);
            if(GUILayout.Button("MAを作成")){
                bool nullFlag = false;
                displayAlart.Clear();
                if(target==null){
                    displayAlart.Add("MAを付けるオブジェクトを指定してください。");
                    nullFlag = true;
                }
                for(int i=0;i<toggleObjects.Count; i++){
                    if(toggleObjects[i]==null){
                        displayAlart.Add("Toggle対象の" + (i+1) + "番目にオブジェクトを指定してください。");
                        nullFlag = true;
                    }
                }
                if(menuTitle==""){
                    displayAlart.Add("Menuのタイトルを指定してください。");
                    nullFlag = true;
                }
                if(paramName==""){
                    displayAlart.Add("Parameter名を指定してください。");
                    nullFlag = true;
                }
                if(abs==false){
                    for(int i=0;i<toggleObjects.Count;i++){
                        if(!IsChildOf(toggleObjects[i],target)){
                            displayAlart.Add("一つでもToggle対象がMAを付けるオブジェクトの子で無い場合、絶対パスを指定する必要があります。");
                            nullFlag = true;
                        }
                    }
                }
                if(folderPath==null){
                    displayAlart.Add("Asset保存先を指定してください。");
                    nullFlag = true;
                }
                if((controllerName==""||controllerName==null)&&margeMA==false){
                    displayAlart.Add("AnimatorController名を指定してください。");
                    nullFlag = true;
                }
                if(menuName==""||menuName==null){
                    displayAlart.Add("VRCExpressionMenu名を指定してください。");
                    nullFlag = true;
                }
                if(nullFlag!=true){
                    makeMAComponents();
                }
            }
            for(int i=0;i<displayAlart.Count;i++){
                EditorGUILayout.LabelField(displayAlart[i], alartTextStyle);
            }
        }

        void makeMAComponents(){
            if(target.GetComponent<ModularAvatarMergeAnimator>()==null){
                target.AddComponent<ModularAvatarMergeAnimator>();
            }
            if(target.GetComponent<ModularAvatarMenuInstaller>()==null){
                target.AddComponent<ModularAvatarMenuInstaller>();
            }
            if(target.GetComponent<ModularAvatarParameters>()==null){
                target.AddComponent<ModularAvatarParameters>();
            }

            //AnimatorControllerを作成
            AnimatorController controller;
            if(margeMA&&target.GetComponent<ModularAvatarMergeAnimator>().animator!=null){
                controller = (AnimatorController)target.GetComponent<ModularAvatarMergeAnimator>().animator;
            }else{
                controller = AnimatorController.CreateAnimatorControllerAtPath(folderPath+"/"+controllerName+".controller");
            }
            controller.AddParameter(paramName,AnimatorControllerParameterType.Bool);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            //AnimationClipを作成し、toggleObjectsの中身のOn、Offをそれぞれ順番に追加していく
            AnimationClip toggle_on = new AnimationClip();
            foreach(GameObject toggleObject in toggleObjects){
                AnimationCurve curve_on = AnimationCurve.Constant(0,0,1);
                if(abs){
                    toggle_on.SetCurve(AnimationUtility.CalculateTransformPath(toggleObject.transform, null), typeof(GameObject), "m_IsActive", curve_on);
                }else{
                    toggle_on.SetCurve(AnimationUtility.CalculateTransformPath(toggleObject.transform, target.transform), typeof(GameObject), "m_IsActive", curve_on);
                }
            }
            AssetDatabase.CreateAsset(toggle_on,folderPath+"/"+paramName+"_on.Anim");

            AnimationClip toggle_off = new AnimationClip();
            foreach(GameObject toggleObject in toggleObjects){
                AnimationCurve curve_off = AnimationCurve.Constant(0,0,0);
                if(abs){
                    toggle_off.SetCurve(AnimationUtility.CalculateTransformPath(toggleObject.transform, null), typeof(GameObject), "m_IsActive", curve_off);
                }else{
                    toggle_off.SetCurve(AnimationUtility.CalculateTransformPath(toggleObject.transform, target.transform), typeof(GameObject), "m_IsActive", curve_off);
                }

            }
            AssetDatabase.CreateAsset(toggle_off,folderPath+"/"+paramName+"_off.Anim");


            //レイヤーをAnimatorControllerに追加します。
            AnimatorControllerLayer layer = new AnimatorControllerLayer{
                name = menuName,
                stateMachine = new AnimatorStateMachine(),
                defaultWeight = 1.0f
            };
            
            //AnimationController内のlayerのリストを削除
            if(margeMA){
            }else{
                controller.layers = new AnimatorControllerLayer[0];
            }

            //AnimationControllerにlayerを追加し、そのlayerにstateMachineを作成してstateを追加
            controller.AddLayer(layer);
            int lastLayer = controller.layers.Length;
            var stateMachine = controller.layers[lastLayer-1].stateMachine;
            var state_toggle_on = stateMachine.AddState(paramName+"_on");
            state_toggle_on.motion = toggle_on;

            var state_toggle_off = stateMachine.AddState(paramName+"_off");
            state_toggle_off.motion = toggle_off;

            //ギミックOn＞OffのTransitionを作成
            AnimatorStateTransition transition_on_off = state_toggle_on.AddTransition(state_toggle_off);
            transition_on_off.hasExitTime = false;
            transition_on_off.exitTime = 0.0f;
            transition_on_off.duration = 0.0f;
            transition_on_off.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);

            //ギミックOff＞OnのTransitionを作成
            AnimatorStateTransition transition_off_on = state_toggle_off.AddTransition(state_toggle_on);
            transition_off_on.hasExitTime = false;
            transition_off_on.exitTime = 0.0f;
            transition_off_on.duration = 0.0f;
            transition_off_on.AddCondition(AnimatorConditionMode.If, 0, paramName);

            stateMachine.defaultState = state_toggle_on;

            AssetDatabase.AddObjectToAsset(stateMachine, controller);
            stateMachine.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(transition_on_off, controller);
            AssetDatabase.AddObjectToAsset(transition_off_on, controller);
            AssetDatabase.AddObjectToAsset(state_toggle_on, controller);
            AssetDatabase.AddObjectToAsset(state_toggle_off, controller);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            target.GetComponent<ModularAvatarMergeAnimator>().animator = controller;

            //Menuを作成
            VRCExpressionsMenu menu;
            if(margeMA&&target.GetComponent<ModularAvatarMenuInstaller>().menuToAppend!=null){
                menu = target.GetComponent<ModularAvatarMenuInstaller>().menuToAppend;
            }else{
                menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                AssetDatabase.CreateAsset(menu, folderPath+"/"+menuName+".asset");
            }
            VRCExpressionsMenu.Control menu_toggle = new VRCExpressionsMenu.Control{
            name = menuName,
            icon = menuIcon,
            type = VRCExpressionsMenu.Control.ControlType.Toggle,
            parameter = new VRCExpressionsMenu.Control.Parameter { name = paramName },
            value = 1.0f
            };
            menu.controls.Add(menu_toggle);

            EditorUtility.SetDirty(menu);
            target.GetComponent<ModularAvatarMenuInstaller>().menuToAppend = menu;
            
            List<ParameterConfig> parameters;
            if(margeMA&&target.GetComponent<ModularAvatarParameters>().parameters!=null){
                parameters = target.GetComponent<ModularAvatarParameters>().parameters;
            }else{
                parameters = new List<ParameterConfig>();
            }

            //パラメーターを作成
            ParameterConfig param = new ParameterConfig();
            param.nameOrPrefix = paramName;
            param.syncType = ParameterSyncType.Bool;
            param.defaultValue = 1;
            param.internalParameter = true;
            parameters.Add(param);

            target.GetComponent<ModularAvatarParameters>().parameters = parameters;

            AssetDatabase.SaveAssets();
        }


        bool IsChildOf(GameObject child, GameObject parent)
        {
            Transform current = child.transform;
            while (current != null)
            {
                if (current == parent.transform)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
    }
}
