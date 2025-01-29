using UnityEngine;
using UnityEditor;
using VRC.SDK3;
using VRC.SDKBase;
using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using Flat.subtools;

namespace Flat.subtools{
    public class FlatVRCParameterUtil{
        public static VRCExpressionParameters MargeParameter(VRCExpressionParameters params1,VRCExpressionParameters params2){
            var paramList = new List<VRCExpressionParameters.Parameter>();
            for(int i=0;i<params1.parameters.Length;i++){
                paramList.Add(params1.parameters[i]);
            }
            for(int i=0;i<params2.parameters.Length;i++){
                paramList.Add(params2.parameters[i]);
            }
            VRCExpressionParameters exports = new VRCExpressionParameters();
            exports.parameters = paramList.ToArray();
            return exports;
        }

        public static void AddParameter(GameObject targetAvatar ,VRCExpressionParameters addParams){
            VRCExpressionParameters exports = MargeParameter(targetAvatar.GetComponentsInChildren<VRCAvatarDescriptor>()[0].expressionParameters,addParams);
            targetAvatar.GetComponentsInChildren<VRCAvatarDescriptor>()[0].expressionParameters = exports;
        }
    }
}