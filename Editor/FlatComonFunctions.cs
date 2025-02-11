using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Flat.subtools;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.Dynamics;

namespace Flat.subtools{
    public class flatCommonFunctions{
        static public AnimatorControllerLayer makeSimpleToggleLayer(AnimatorController controller,string layerName,string paramName, AnimationClip defaultAnim, AnimationClip changedAnim, bool default_on){
            //レイヤーをAnimatorControllerに追加します。
            AnimatorControllerLayer layer = new AnimatorControllerLayer{
                name = layerName,
                stateMachine = new AnimatorStateMachine(),
                defaultWeight = 1.0f
            };

            //AnimatorControllerにパラメーターを追加
            controller.AddParameter(paramName,AnimatorControllerParameterType.Bool);
            
            //AnimationControllerにlayerを追加し、そのlayerにstateMachineを作成してstateを追加
            controller.AddLayer(layer);
            int lastLayer = controller.layers.Length;
            var stateMachine = controller.layers[lastLayer-1].stateMachine;
            var state_toggle_default = stateMachine.AddState(paramName+"_default");
            state_toggle_default.motion = defaultAnim;

            var state_toggle_changed = stateMachine.AddState(paramName+"_changed");
            state_toggle_changed.motion = changedAnim;

            //ギミックdefault＞changedのTransitionを作成
            AnimatorStateTransition transition_default_changed = state_toggle_default.AddTransition(state_toggle_changed);
            transition_default_changed.hasExitTime = false;
            transition_default_changed.exitTime = 0.0f;
            transition_default_changed.duration = 0.0f;
            if(default_on){
                transition_default_changed.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);
            }else{
                transition_default_changed.AddCondition(AnimatorConditionMode.If, 0, paramName);
            }

            //ギミックchanged＞defaultのTransitionを作成
            AnimatorStateTransition transition_changed_default = state_toggle_changed.AddTransition(state_toggle_default);
            transition_changed_default.hasExitTime = false;
            transition_changed_default.exitTime = 0.0f;
            transition_changed_default.duration = 0.0f;
            if(default_on){
                transition_changed_default.AddCondition(AnimatorConditionMode.If, 0, paramName);
            }else{
                transition_changed_default.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);
            }

            stateMachine.defaultState = state_toggle_default;

            AssetDatabase.AddObjectToAsset(stateMachine, controller);
            stateMachine.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(transition_default_changed, controller);
            AssetDatabase.AddObjectToAsset(transition_changed_default, controller);
            AssetDatabase.AddObjectToAsset(state_toggle_default, controller);
            AssetDatabase.AddObjectToAsset(state_toggle_changed, controller);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            return layer;
        }

        static public VRCParentConstraint makeSimpleFollowSetting(GameObject target,GameObject bindBone){
            //MA bone proxyの作成とかを含めて実装する。
            VRCParentConstraint constraint = target.AddComponent<VRCParentConstraint>();
            VRCConstraintSource source = new VRCConstraintSource();
            source.SourceTransform = bindBone.transform;
            source.Weight = 1.0f;
            constraint.Sources.Add(source);
            constraint.IsActive = true;

            return constraint;
        }

        static public GameObject getRootObject(GameObject target){
            if (target == null) return null;
            Transform currentParent = target.transform;
            while (currentParent.parent != null){
                currentParent = currentParent.parent;
            }
            return currentParent.gameObject; 
        }

        static public GameObject makePositionProxy(GameObject root,GameObject target){
            GameObject proxyObject = new GameObject(target.name);
            proxyObject.transform.parent = target.transform.parent;
            proxyObject.transform.localPosition = Vector3.zero;
            proxyObject.transform.localRotation = Quaternion.identity;
            target.transform.parent = proxyObject.transform;
            return proxyObject;
        }
    }
}
