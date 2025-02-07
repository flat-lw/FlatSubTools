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
    public class FlatMakeWorldFixAnimation : MonoBehaviour
    {
        [MenuItem("FlatSubTools/MakeWorldFixAnimation")]
        private static void makeComponents(){
            EditorWindow.GetWindow<MakeWorldFixAnimationWindow>("FlatSubTools-MakeWorldFixAnimation");
        }
    }

    public class MakeWorldFixAnimationWindow : EditorWindow{
        GameObject target;
        GameObject bindBone;
        List<GameObject> worldFixObjects = new List<GameObject>();
        string rootMenuTitle;
        string enableMenuTitle;
        string fixMenuTitle;
        string enableParamName;
        string fixParamName;
        bool renameParam = true;
        bool margeMA;
        bool abs;
        Texture2D rootMenuIcon;
        Texture2D enableMenuIcon;
        Texture2D fixMenuIcon;
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
            EditorGUILayout.LabelField("追従ボーン");
            bindBone = (GameObject)EditorGUILayout.ObjectField(bindBone, typeof(GameObject), true);
            EditorGUILayout.LabelField("ワールド固定対象");
            for(int i=0;i<worldFixObjects.Count; i++){
                worldFixObjects[i] = (GameObject)EditorGUILayout.ObjectField(worldFixObjects[i], typeof(GameObject), true);
            }
            if(GUILayout.Button("Add")){
                worldFixObjects.Add(null);
            }
            if(GUILayout.Button("Delete")){
                worldFixObjects.RemoveAt(worldFixObjects.Count-1);
            }
            rootMenuTitle = EditorGUILayout.TextField("RootMenuのタイトル", rootMenuTitle);
            rootMenuIcon = (Texture2D)EditorGUILayout.ObjectField("RootMenuのアイコン", rootMenuIcon, typeof(Texture2D), false);
            enableMenuTitle = EditorGUILayout.TextField("ON/OFF Menuのタイトル", enableMenuTitle);
            enableMenuIcon = (Texture2D)EditorGUILayout.ObjectField("ON/OFF Menuのアイコン", enableMenuIcon, typeof(Texture2D), false);
            fixMenuTitle = EditorGUILayout.TextField("ワールド固定Menuのタイトル", fixMenuTitle);
            fixMenuIcon = (Texture2D)EditorGUILayout.ObjectField("ワールド固定Menuのアイコン", fixMenuIcon, typeof(Texture2D), false);

            enableParamName = EditorGUILayout.TextField("ON/OFF Parameter名", enableParamName);
            fixParamName = EditorGUILayout.TextField("ワールド固定Parameter名", fixParamName);
            renameParam = EditorGUILayout.Toggle("Parameterを自動リネーム", renameParam);
            if(renameParam){
                EditorGUILayout.LabelField("", alartTextStyle);
            }else{
                EditorGUILayout.LabelField("Parameterを他のMAから操作しない場合はONにしてください。", alartTextStyle);
            }
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
                for(int i=0;i<worldFixObjects.Count; i++){
                    if(worldFixObjects[i]==null){
                        displayAlart.Add("Toggle対象の" + (i+1) + "番目にオブジェクトを指定してください。");
                        nullFlag = true;
                    }
                }
                if(rootMenuTitle==""){
                    displayAlart.Add("Menuのタイトルを指定してください。");
                    nullFlag = true;
                }
                if(enableMenuTitle==""){
                    displayAlart.Add("Menuのタイトルを指定してください。");
                    nullFlag = true;
                }
                if(fixMenuTitle==""){
                    displayAlart.Add("Menuのタイトルを指定してください。");
                    nullFlag = true;
                }
                if(enableParamName==""){
                    displayAlart.Add("Parameter名を指定してください。");
                    nullFlag = true;
                }
                if(fixParamName==""){
                    displayAlart.Add("Parameter名を指定してください。");
                    nullFlag = true;
                }
                if(abs==false){
                    for(int i=0;i<worldFixObjects.Count;i++){
                        if(!IsChildOf(worldFixObjects[i],target)){
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

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            //AnimationController内のlayerのリストを削除
            if(margeMA){
            }else{
                controller.layers = new AnimatorControllerLayer[0];
            }


            // ON/OFFのレイヤーを作成
            //AnimationClipを作成し、worldFixObjectsの中身のOn、Offをそれぞれ順番に追加していく
            AnimationClip toggle_on = new AnimationClip();
            foreach(GameObject worldFixObject in worldFixObjects){
                AnimationCurve curve_on = AnimationCurve.Constant(0,0,1);
                if(abs){
                    toggle_on.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, null), typeof(GameObject), "m_IsActive", curve_on);
                }else{
                    toggle_on.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, target.transform), typeof(GameObject), "m_IsActive", curve_on);
                }
            }
            AssetDatabase.CreateAsset(toggle_on,folderPath+"/"+enableParamName+"_on.Anim");

            AnimationClip toggle_off = new AnimationClip();
            foreach(GameObject worldFixObject in worldFixObjects){
                AnimationCurve curve_off = AnimationCurve.Constant(0,0,0);
                if(abs){
                    toggle_off.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, null), typeof(GameObject), "m_IsActive", curve_off);
                }else{
                    toggle_off.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, target.transform), typeof(GameObject), "m_IsActive", curve_off);
                }
            }
            AssetDatabase.CreateAsset(toggle_off,folderPath+"/"+enableParamName+"_off.Anim");
            
            flatCommonFunctions.makeSimpleToggleLayer(controller, enableMenuTitle, enableParamName, toggle_off, toggle_on, false);

            // ワールド固定のレイヤーを作成
            //AnimationClipを作成し、worldFixObjectsの中身のOn、Offをそれぞれ順番に追加していく
            AnimationClip bind_bone = new AnimationClip();
            foreach(GameObject worldFixObject in worldFixObjects){
                AnimationCurve curve_bone = AnimationCurve.Constant(0,0,1);
                if(abs){
                    bind_bone.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, null), typeof(GameObject), "m_IsActive", curve_bone);
                }else{
                    bind_bone.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, target.transform), typeof(GameObject), "m_IsActive", curve_bone);
                }
            }
            AssetDatabase.CreateAsset(bind_bone,folderPath+"/"+enableParamName+"_bone.Anim");

            AnimationClip bind_world = new AnimationClip();
            foreach(GameObject worldFixObject in worldFixObjects){
                AnimationCurve curve_world = AnimationCurve.Constant(0,0,0);
                if(abs){
                    bind_world.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, null), typeof(GameObject), "m_IsActive", curve_world);
                }else{
                    bind_world.SetCurve(AnimationUtility.CalculateTransformPath(worldFixObject.transform, target.transform), typeof(GameObject), "m_IsActive", curve_world);
                }
            }
            AssetDatabase.CreateAsset(bind_world,folderPath+"/"+enableParamName+"_world.Anim");
            
            flatCommonFunctions.makeSimpleToggleLayer(controller, fixMenuTitle, fixParamName, bind_bone, bind_world, false);

            //作成したAnimatorControllerをMAに追加
            target.GetComponent<ModularAvatarMergeAnimator>().animator = controller;

            //Menuを作成
            VRCExpressionsMenu menu;
            if(margeMA&&target.GetComponent<ModularAvatarMenuInstaller>().menuToAppend!=null){
                menu = target.GetComponent<ModularAvatarMenuInstaller>().menuToAppend;
            }else{
                menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                AssetDatabase.CreateAsset(menu, folderPath+"/"+menuName+".asset");
            }

            VRCExpressionsMenu.Control menu_enable = new VRCExpressionsMenu.Control{
            name = menuName + "ON/OFF",
            icon = rootMenuIcon,
            type = VRCExpressionsMenu.Control.ControlType.Toggle,
            parameter = new VRCExpressionsMenu.Control.Parameter { name = enableParamName },
            value = 1.0f
            };
            menu.controls.Add(menu_enable);

            VRCExpressionsMenu.Control menu_toggle = new VRCExpressionsMenu.Control{
            name = menuName + "ワールド固定",
            icon = rootMenuIcon,
            type = VRCExpressionsMenu.Control.ControlType.Toggle,
            parameter = new VRCExpressionsMenu.Control.Parameter { name = fixParamName },
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
            ParameterConfig enableParam = new ParameterConfig();
            enableParam.nameOrPrefix = enableParamName;
            enableParam.syncType = ParameterSyncType.Bool;
            enableParam.defaultValue = 1;
            enableParam.internalParameter = true;
            parameters.Add(enableParam);

            ParameterConfig fixParam = new ParameterConfig();
            fixParam.nameOrPrefix = fixParamName;
            fixParam.syncType = ParameterSyncType.Bool;
            fixParam.defaultValue = 1;
            fixParam.internalParameter = true;
            parameters.Add(fixParam);

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
