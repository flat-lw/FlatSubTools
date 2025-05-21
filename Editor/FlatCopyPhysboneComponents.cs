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
    public class FlatCopyPhysboneComponents : MonoBehaviour
    {
        [MenuItem("FlatSubTools/CopyPhysboneComponents")]
        private static void makeComponents(){
            EditorWindow.GetWindow<CopyPhysboneComponentsWindow>("FlatSubTools-CopyPhysboneComponents");
        }
    }
    public class CopyPhysboneComponentsWindow : EditorWindow{
        GameObject source;
        GameObject target;
        List<CopyObjectPair> physbonePairs = new List<CopyObjectPair>();
        List<CopyObjectPair> colliderPairs = new List<CopyObjectPair>();
        private Vector2 physboneScrollPosition;
        private Vector2 colliderScrollPosition;
        bool colliderMakeWithObject;

        void OnGUI(){
            source = (GameObject)EditorGUILayout.ObjectField("Source",source,typeof(GameObject),true);
            target = (GameObject)EditorGUILayout.ObjectField("Target",target,typeof(GameObject),true);
            if(GUILayout.Button("Get")){
                GetGameObjectPair();
            }
            if(physbonePairs.Count != 0){
                EditorGUILayout.LabelField("Physbone");
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField("souece");
                EditorGUILayout.LabelField("target");
                EditorGUILayout.EndHorizontal ();
                
                physboneScrollPosition = EditorGUILayout.BeginScrollView(physboneScrollPosition);
                foreach(CopyObjectPair pair in physbonePairs){
                    EditorGUILayout.BeginHorizontal ();
                    pair.source = (GameObject)EditorGUILayout.ObjectField("",pair.source,typeof(GameObject),true);
                    pair.target = (GameObject)EditorGUILayout.ObjectField("",pair.target,typeof(GameObject),true);
                    EditorGUILayout.EndHorizontal ();
                }
                EditorGUILayout.EndScrollView();
            }
            if(colliderPairs.Count != 0){
                EditorGUILayout.LabelField("Collider");
                colliderMakeWithObject = EditorGUILayout.Toggle("Objectごと生成",colliderMakeWithObject);
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField("souece");
                EditorGUILayout.LabelField("target");
                EditorGUILayout.EndHorizontal ();
                colliderScrollPosition = EditorGUILayout.BeginScrollView(colliderScrollPosition);
                foreach(CopyObjectPair pair in colliderPairs){
                    EditorGUILayout.BeginHorizontal ();
                    pair.source = (GameObject)EditorGUILayout.ObjectField("",pair.source,typeof(GameObject),true);
                    pair.target = (GameObject)EditorGUILayout.ObjectField("",pair.target,typeof(GameObject),true);
                    EditorGUILayout.EndHorizontal ();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.Space();
            if(GUILayout.Button("Copy")){
                CopyColliderComponents();
                CopyPhysboneComponents();
            }
        }
        private void GetGameObjectPair(){
            physbonePairs.Clear();
            colliderPairs.Clear();
            foreach(VRCPhysBone pb in source.GetComponentsInChildren<VRCPhysBone>()){
                Debug.Log(pb.gameObject.name);
                GameObject targetGameObject = flatCommonFunctions.findChildByName(target,pb.gameObject.name);
                physbonePairs.Add(new CopyObjectPair{source=pb.gameObject,target=targetGameObject});
            }
            foreach(VRCPhysBoneCollider pb in source.GetComponentsInChildren<VRCPhysBoneCollider>()){
                Debug.Log(pb.gameObject.name);
                GameObject targetGameObject;
                if(colliderMakeWithObject){
                    targetGameObject = flatCommonFunctions.findChildByName(target,pb.gameObject.transform.parent.name);
                }else{
                    targetGameObject = flatCommonFunctions.findChildByName(target,pb.gameObject.name);
                }
                colliderPairs.Add(new CopyObjectPair{source=pb.gameObject,target=targetGameObject});
            }
        }

        private void CopyPhysboneComponents(){
            foreach(CopyObjectPair physbonePair in physbonePairs){
                if(physbonePair.target != null){
                    VRCPhysBone pb = physbonePair.target.AddComponent<VRCPhysBone>();
                    pb.integrationType = physbonePair.source.GetComponent<VRCPhysBone>().integrationType;
                    pb.rootTransform = physbonePair.source.GetComponent<VRCPhysBone>().rootTransform;
                    List<Transform> ignoreTransforms = physbonePair.source.GetComponent<VRCPhysBone>().ignoreTransforms;
                    if(ignoreTransforms != null){
                        pb.ignoreTransforms = ignoreTransforms;
                    }
                    pb.endpointPosition = physbonePair.source.GetComponent<VRCPhysBone>().endpointPosition;
                    pb.multiChildType = physbonePair.source.GetComponent<VRCPhysBone>().multiChildType;
                    pb.pull = physbonePair.source.GetComponent<VRCPhysBone>().pull;
                    pb.pullCurve = physbonePair.source.GetComponent<VRCPhysBone>().pullCurve;
                    pb.spring = physbonePair.source.GetComponent<VRCPhysBone>().spring;
                    pb.springCurve = physbonePair.source.GetComponent<VRCPhysBone>().springCurve;
                    pb.stiffness = physbonePair.source.GetComponent<VRCPhysBone>().stiffness;
                    pb.stiffnessCurve = physbonePair.source.GetComponent<VRCPhysBone>().stiffnessCurve;
                    pb.gravity = physbonePair.source.GetComponent<VRCPhysBone>().gravity;
                    pb.gravityFalloff = physbonePair.source.GetComponent<VRCPhysBone>().gravityFalloff;
                    pb.gravityFalloffCurve = physbonePair.source.GetComponent<VRCPhysBone>().gravityFalloffCurve;

                    pb.immobileType = physbonePair.source.GetComponent<VRCPhysBone>().immobileType;
                    pb.immobile = physbonePair.source.GetComponent<VRCPhysBone>().immobile;
                    pb.immobileCurve = physbonePair.source.GetComponent<VRCPhysBone>().immobileCurve;

                    pb.allowCollision = physbonePair.source.GetComponent<VRCPhysBone>().allowCollision;
                    pb.collisionFilter.allowSelf = physbonePair.source.GetComponent<VRCPhysBone>().collisionFilter.allowSelf;
                    pb.collisionFilter.allowOthers = physbonePair.source.GetComponent<VRCPhysBone>().collisionFilter.allowOthers;
                    pb.radius = physbonePair.source.GetComponent<VRCPhysBone>().radius;
                    pb.radiusCurve = physbonePair.source.GetComponent<VRCPhysBone>().radiusCurve;
                    List<VRCPhysBoneColliderBase> colliders = physbonePair.source.GetComponent<VRCPhysBone>().colliders;
                    List<VRCPhysBoneColliderBase> targetColliders = new List<VRCPhysBoneColliderBase>();
                    foreach(VRCPhysBoneColliderBase collider in colliders){
                        targetColliders.Add(flatCommonFunctions.findChildByName(target,collider.name).GetComponent<VRCPhysBoneCollider>());
                    }
                    if(targetColliders != null){
                        pb.colliders = targetColliders;
                    }

                    pb.limitType = physbonePair.source.GetComponent<VRCPhysBone>().limitType;
                    pb.maxAngleX = physbonePair.source.GetComponent<VRCPhysBone>().maxAngleX;
                    pb.maxAngleXCurve = physbonePair.source.GetComponent<VRCPhysBone>().maxAngleXCurve;
                    pb.maxAngleZ = physbonePair.source.GetComponent<VRCPhysBone>().maxAngleZ;
                    pb.maxAngleZCurve = physbonePair.source.GetComponent<VRCPhysBone>().maxAngleZCurve;
                    pb.limitRotation = physbonePair.source.GetComponent<VRCPhysBone>().limitRotation;
                    pb.limitRotationXCurve = physbonePair.source.GetComponent<VRCPhysBone>().limitRotationXCurve;
                    pb.limitRotationYCurve = physbonePair.source.GetComponent<VRCPhysBone>().limitRotationYCurve;
                    pb.limitRotationZCurve = physbonePair.source.GetComponent<VRCPhysBone>().limitRotationZCurve;

                    pb.allowGrabbing = physbonePair.source.GetComponent<VRCPhysBone>().allowGrabbing;
                    pb.grabFilter.allowSelf = physbonePair.source.GetComponent<VRCPhysBone>().grabFilter.allowSelf;
                    pb.grabFilter.allowOthers = physbonePair.source.GetComponent<VRCPhysBone>().grabFilter.allowOthers;

                    pb.allowPosing = physbonePair.source.GetComponent<VRCPhysBone>().allowPosing;
                    pb.poseFilter.allowSelf = physbonePair.source.GetComponent<VRCPhysBone>().poseFilter.allowSelf;
                    pb.poseFilter.allowOthers = physbonePair.source.GetComponent<VRCPhysBone>().poseFilter.allowOthers;

                    pb.snapToHand = physbonePair.source.GetComponent<VRCPhysBone>().snapToHand;
                    pb.grabMovement = physbonePair.source.GetComponent<VRCPhysBone>().grabMovement;
                    pb.maxStretch = physbonePair.source.GetComponent<VRCPhysBone>().maxStretch;
                    pb.maxStretchCurve = physbonePair.source.GetComponent<VRCPhysBone>().maxStretchCurve;
                    pb.maxSquish = physbonePair.source.GetComponent<VRCPhysBone>().maxSquish;
                    pb.maxSquishCurve = physbonePair.source.GetComponent<VRCPhysBone>().maxSquishCurve;
                    pb.stretchMotion = physbonePair.source.GetComponent<VRCPhysBone>().stretchMotion;
                    pb.stretchMotionCurve = physbonePair.source.GetComponent<VRCPhysBone>().stretchMotionCurve;
                    pb.isAnimated = physbonePair.source.GetComponent<VRCPhysBone>().isAnimated;
                    pb.resetWhenDisabled = physbonePair.source.GetComponent<VRCPhysBone>().resetWhenDisabled;
                    pb.parameter = physbonePair.source.GetComponent<VRCPhysBone>().parameter;
                    pb.showGizmos = physbonePair.source.GetComponent<VRCPhysBone>().showGizmos;
                    pb.boneOpacity = physbonePair.source.GetComponent<VRCPhysBone>().boneOpacity;
                    pb.limitOpacity = physbonePair.source.GetComponent<VRCPhysBone>().limitOpacity;
                }
            }
        }
        private void CopyColliderComponents(){
            if(colliderMakeWithObject){
                foreach(CopyObjectPair colliderPair in colliderPairs){
                    if(colliderPair.target != null){
                        GameObject go = new GameObject();
                        go.name = colliderPair.source.name;
                        go.transform.parent = colliderPair.target.transform;
                        go.transform.localPosition = colliderPair.source.transform.localPosition;
                        go.transform.localRotation = colliderPair.source.transform.localRotation;
                        go.transform.localScale = colliderPair.source.transform.localScale;
                        VRCPhysBoneCollider pbc = go.AddComponent<VRCPhysBoneCollider>();
                        pbc.rootTransform = colliderPair.source.GetComponent<VRCPhysBoneCollider>().rootTransform;
                        pbc.shapeType = colliderPair.source.GetComponent<VRCPhysBoneCollider>().shapeType;
                        pbc.radius = colliderPair.source.GetComponent<VRCPhysBoneCollider>().radius;
                        pbc.height = colliderPair.source.GetComponent<VRCPhysBoneCollider>().height;
                        pbc.position = colliderPair.source.GetComponent<VRCPhysBoneCollider>().position;
                        pbc.insideBounds = colliderPair.source.GetComponent<VRCPhysBoneCollider>().insideBounds;
                        pbc.rotation = colliderPair.source.GetComponent<VRCPhysBoneCollider>().rotation;
                        pbc.bonesAsSpheres = colliderPair.source.GetComponent<VRCPhysBoneCollider>().bonesAsSpheres;
                    }
                }

            }else{
                foreach(CopyObjectPair colliderPair in colliderPairs){
                    if(colliderPair.target != null){
                        VRCPhysBoneCollider pbc = colliderPair.target.AddComponent<VRCPhysBoneCollider>();
                        pbc.rootTransform = colliderPair.source.GetComponent<VRCPhysBoneCollider>().rootTransform;
                        pbc.shapeType = colliderPair.source.GetComponent<VRCPhysBoneCollider>().shapeType;
                        pbc.radius = colliderPair.source.GetComponent<VRCPhysBoneCollider>().radius;
                        pbc.height = colliderPair.source.GetComponent<VRCPhysBoneCollider>().height;
                        pbc.position = colliderPair.source.GetComponent<VRCPhysBoneCollider>().position;
                        pbc.insideBounds = colliderPair.source.GetComponent<VRCPhysBoneCollider>().insideBounds;
                        pbc.rotation = colliderPair.source.GetComponent<VRCPhysBoneCollider>().rotation;
                        pbc.bonesAsSpheres = colliderPair.source.GetComponent<VRCPhysBoneCollider>().bonesAsSpheres;
                    }
                }
            }
        }
    }

    public class CopyObjectPair{
        public GameObject source;
        public GameObject target;
    }
}
