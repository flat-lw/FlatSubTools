using UnityEngine;
using UnityEditor;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
using VRC.SDK3;
using VRC.SDKBase;
using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
using Flat.subtools;

[assembly: ExportsPlugin(typeof(FlatMargeParametersPlugin))]

namespace Flat.subtools{
    public class FlatMargeParametersPlugin : Plugin<FlatMargeParametersPlugin>{
        protected override void Configure(){
            InPhase(BuildPhase.Resolving)
                .Run("Multi Shader Effector Setting...", ctx => {
                    var objs = ctx.AvatarRootObject.GetComponentsInChildren<FlatMargeParameters>(true);
                    foreach(var obj in objs){
                        VRCExpressionParameters parameters = obj.parameters;
                        FlatVRCParameterUtil.AddParameter(ctx.AvatarRootObject,parameters);
                    }
                });
        }
    }
}