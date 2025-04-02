Shader "Ciconia Studio/Effects/Triplanar/Terrain/Simple Terrain (Mask Details)" {
    Properties {
        [Space(45)]_Color ("Color", Color) = (1,1,1,1)
        [Space(10)]_Emissive ("Emissive", Range(0, 1)) = 0
        _BlendValue ("Blend Value", Range(0.5, 0.7)) = 0.5
        [Space(10)]_GeneralTiling ("General Tiling", Float ) = 1

        [Space(45)][Header(Top Bottom)]
        [Space(10)]_DiffuseTopDown ("Diffuse Top/Down (Y)", 2D) = "white" {}
        [Space(10)]_TilingY ("Tiling Y", Float ) = 1
        _RotationY ("Rotation Y", Float ) = 0

        [Space(45)][Header(Sides)]
        [Space(10)]_DiffuseSidesXZ ("Diffuse Sides (XZ)", 2D) = "white" {}
        [Space(10)]_TilingX ("Tiling X", Float ) = 1
        _RotationX ("Rotation X ", Float ) = 0
        [Space(15)]_TilingZ ("Tiling Z", Float ) = 1
        _RotationZ ("Rotation Z", Float ) = 0

        [Space(45)][Header(Blend properties)]
        [Space(10)][MaterialToggle] _InvertMask ("Invert Mask", Float ) = 0
        [Space(10)]_DetailMask ("Blend Mask", 2D) = "black" {}
        [Space(15)]_MaskAmount ("Mask Amount", Float ) = 0.5
        _Contrast ("Contrast", Float ) = 1

        [Space(25)]_Diffuse2 ("Diffuse2", 2D) = "white" {}
        [Space(10)]_Tiling2 ("Tiling", Float ) = 1
        _Rotation2 ("Rotation ", Float ) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _DiffuseTopDown; uniform float4 _DiffuseTopDown_ST;
            uniform float _TilingX;
            uniform float _RotationX;
            uniform float _GeneralTiling;
            uniform float _TilingY;
            uniform float _RotationY;
            uniform float _TilingZ;
            uniform float _RotationZ;
            uniform float _Emissive;
            uniform sampler2D _DiffuseSidesXZ; uniform float4 _DiffuseSidesXZ_ST;
            uniform float _BlendValue;
            uniform sampler2D _DetailMask; uniform float4 _DetailMask_ST;
            uniform float _Contrast;
            uniform float _MaskAmount;
            uniform sampler2D _Diffuse2; uniform float4 _Diffuse2_ST;
            uniform float _Tiling2;
            uniform float _Rotation2;
            uniform fixed _InvertMask;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - 0;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float3 node_3480 = abs(mul( unity_WorldToObject, float4(i.normalDir,0) ).xyz.rgb);
                float3 NormalDirectionMask = saturate(( _BlendValue > 0.5 ? (1.0-(1.0-2.0*(_BlendValue-0.5))*(1.0-node_3480)) : (2.0*_BlendValue*node_3480) ));
                float3 node_2876 = NormalDirectionMask;
                float node_9302_ang = (((_RotationX+(-90.0))*3.141592654)/180.0);
                float node_9302_spd = 1.0;
                float node_9302_cos = cos(node_9302_spd*node_9302_ang);
                float node_9302_sin = sin(node_9302_spd*node_9302_ang);
                float2 node_9302_piv = float2(0.5,0.5);
                float3 node_7457 = (mul( unity_WorldToObject, float4((i.posWorld.rgb-objPos.rgb),0) ).xyz.rgb*_GeneralTiling*4.0);
                float2 node_9302 = (mul(((node_7457.gb/4.0)*_TilingX)-node_9302_piv,float2x2( node_9302_cos, -node_9302_sin, node_9302_sin, node_9302_cos))+node_9302_piv);
                float2 GB = node_9302;
                float2 node_9874 = GB;
                float4 _node_1257 = tex2D(_DiffuseSidesXZ,TRANSFORM_TEX(node_9874, _DiffuseSidesXZ)); // X Axis FrontBack
                float node_4319_ang = (((_RotationY+90.0)*3.141592654)/180.0);
                float node_4319_spd = 1.0;
                float node_4319_cos = cos(node_4319_spd*node_4319_ang);
                float node_4319_sin = sin(node_4319_spd*node_4319_ang);
                float2 node_4319_piv = float2(0.5,0.5);
                float2 node_4031 = node_7457.rb;
                float2 node_4319 = (mul(((node_4031/4.0)*_TilingY)-node_4319_piv,float2x2( node_4319_cos, -node_4319_sin, node_4319_sin, node_4319_cos))+node_4319_piv);
                float2 RB = node_4319;
                float2 node_7446 = RB;
                float4 _node_4330 = tex2D(_DiffuseTopDown,TRANSFORM_TEX(node_7446, _DiffuseTopDown)); // Y Axis TopBottom
                float node_5886_ang = (((_Rotation2+90.0)*3.141592654)/180.0);
                float node_5886_spd = 1.0;
                float node_5886_cos = cos(node_5886_spd*node_5886_ang);
                float node_5886_sin = sin(node_5886_spd*node_5886_ang);
                float2 node_5886_piv = float2(0.5,0.5);
                float2 node_5886 = (mul(((node_4031/4.0)*_Tiling2)-node_5886_piv,float2x2( node_5886_cos, -node_5886_sin, node_5886_sin, node_5886_cos))+node_5886_piv);
                float2 RBSecondarymap = node_5886;
                float2 node_6968 = RBSecondarymap;
                float4 _Diffuse2_var = tex2D(_Diffuse2,TRANSFORM_TEX(node_6968, _Diffuse2));
                float4 _DetailMask_var = tex2D(_DetailMask,TRANSFORM_TEX(i.uv0, _DetailMask));
                float node_9944 = 0.0;
                float node_5210 = (1.0+(-1*_Contrast));
                float Mask = saturate(((_MaskAmount*2.0+-1.0)+(node_5210 + ( (lerp( _DetailMask_var.r, (1.0 - _DetailMask_var.r), _InvertMask ) - node_9944) * (_Contrast - node_5210) ) / (1.0 - node_9944))));
                float node_8575_ang = ((_RotationZ*3.141592654)/180.0);
                float node_8575_spd = 1.0;
                float node_8575_cos = cos(node_8575_spd*node_8575_ang);
                float node_8575_sin = sin(node_8575_spd*node_8575_ang);
                float2 node_8575_piv = float2(0.5,0.5);
                float2 node_8575 = (mul(((node_7457.rg/4.0)*_TilingZ)-node_8575_piv,float2x2( node_8575_cos, -node_8575_sin, node_8575_sin, node_8575_cos))+node_8575_piv);
                float2 RG = node_8575;
                float2 node_4282 = RG;
                float4 _node_3650 = tex2D(_DiffuseSidesXZ,TRANSFORM_TEX(node_4282, _DiffuseSidesXZ)); // Z Axis LeftRight 
                float3 node_3746 = (_Color.rgb*(node_2876.r*(_node_1257.rgb*NormalDirectionMask.r) + node_2876.g*lerp((_node_4330.rgb*NormalDirectionMask.g),_Diffuse2_var.rgb,Mask) + node_2876.b*(_node_3650.rgb*NormalDirectionMask.b)));
                float3 diffuseColor = node_3746;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = (node_3746*_Emissive);
/// Final Color:
                float3 finalColor = diffuse + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _DiffuseTopDown; uniform float4 _DiffuseTopDown_ST;
            uniform float _TilingX;
            uniform float _RotationX;
            uniform float _GeneralTiling;
            uniform float _TilingY;
            uniform float _RotationY;
            uniform float _TilingZ;
            uniform float _RotationZ;
            uniform float _Emissive;
            uniform sampler2D _DiffuseSidesXZ; uniform float4 _DiffuseSidesXZ_ST;
            uniform float _BlendValue;
            uniform sampler2D _DetailMask; uniform float4 _DetailMask_ST;
            uniform float _Contrast;
            uniform float _MaskAmount;
            uniform sampler2D _Diffuse2; uniform float4 _Diffuse2_ST;
            uniform float _Tiling2;
            uniform float _Rotation2;
            uniform fixed _InvertMask;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 node_3480 = abs(mul( unity_WorldToObject, float4(i.normalDir,0) ).xyz.rgb);
                float3 NormalDirectionMask = saturate(( _BlendValue > 0.5 ? (1.0-(1.0-2.0*(_BlendValue-0.5))*(1.0-node_3480)) : (2.0*_BlendValue*node_3480) ));
                float3 node_2876 = NormalDirectionMask;
                float node_9302_ang = (((_RotationX+(-90.0))*3.141592654)/180.0);
                float node_9302_spd = 1.0;
                float node_9302_cos = cos(node_9302_spd*node_9302_ang);
                float node_9302_sin = sin(node_9302_spd*node_9302_ang);
                float2 node_9302_piv = float2(0.5,0.5);
                float3 node_7457 = (mul( unity_WorldToObject, float4((i.posWorld.rgb-objPos.rgb),0) ).xyz.rgb*_GeneralTiling*4.0);
                float2 node_9302 = (mul(((node_7457.gb/4.0)*_TilingX)-node_9302_piv,float2x2( node_9302_cos, -node_9302_sin, node_9302_sin, node_9302_cos))+node_9302_piv);
                float2 GB = node_9302;
                float2 node_9874 = GB;
                float4 _node_1257 = tex2D(_DiffuseSidesXZ,TRANSFORM_TEX(node_9874, _DiffuseSidesXZ)); // X Axis FrontBack
                float node_4319_ang = (((_RotationY+90.0)*3.141592654)/180.0);
                float node_4319_spd = 1.0;
                float node_4319_cos = cos(node_4319_spd*node_4319_ang);
                float node_4319_sin = sin(node_4319_spd*node_4319_ang);
                float2 node_4319_piv = float2(0.5,0.5);
                float2 node_4031 = node_7457.rb;
                float2 node_4319 = (mul(((node_4031/4.0)*_TilingY)-node_4319_piv,float2x2( node_4319_cos, -node_4319_sin, node_4319_sin, node_4319_cos))+node_4319_piv);
                float2 RB = node_4319;
                float2 node_7446 = RB;
                float4 _node_4330 = tex2D(_DiffuseTopDown,TRANSFORM_TEX(node_7446, _DiffuseTopDown)); // Y Axis TopBottom
                float node_5886_ang = (((_Rotation2+90.0)*3.141592654)/180.0);
                float node_5886_spd = 1.0;
                float node_5886_cos = cos(node_5886_spd*node_5886_ang);
                float node_5886_sin = sin(node_5886_spd*node_5886_ang);
                float2 node_5886_piv = float2(0.5,0.5);
                float2 node_5886 = (mul(((node_4031/4.0)*_Tiling2)-node_5886_piv,float2x2( node_5886_cos, -node_5886_sin, node_5886_sin, node_5886_cos))+node_5886_piv);
                float2 RBSecondarymap = node_5886;
                float2 node_6968 = RBSecondarymap;
                float4 _Diffuse2_var = tex2D(_Diffuse2,TRANSFORM_TEX(node_6968, _Diffuse2));
                float4 _DetailMask_var = tex2D(_DetailMask,TRANSFORM_TEX(i.uv0, _DetailMask));
                float node_9944 = 0.0;
                float node_5210 = (1.0+(-1*_Contrast));
                float Mask = saturate(((_MaskAmount*2.0+-1.0)+(node_5210 + ( (lerp( _DetailMask_var.r, (1.0 - _DetailMask_var.r), _InvertMask ) - node_9944) * (_Contrast - node_5210) ) / (1.0 - node_9944))));
                float node_8575_ang = ((_RotationZ*3.141592654)/180.0);
                float node_8575_spd = 1.0;
                float node_8575_cos = cos(node_8575_spd*node_8575_ang);
                float node_8575_sin = sin(node_8575_spd*node_8575_ang);
                float2 node_8575_piv = float2(0.5,0.5);
                float2 node_8575 = (mul(((node_7457.rg/4.0)*_TilingZ)-node_8575_piv,float2x2( node_8575_cos, -node_8575_sin, node_8575_sin, node_8575_cos))+node_8575_piv);
                float2 RG = node_8575;
                float2 node_4282 = RG;
                float4 _node_3650 = tex2D(_DiffuseSidesXZ,TRANSFORM_TEX(node_4282, _DiffuseSidesXZ)); // Z Axis LeftRight 
                float3 node_3746 = (_Color.rgb*(node_2876.r*(_node_1257.rgb*NormalDirectionMask.r) + node_2876.g*lerp((_node_4330.rgb*NormalDirectionMask.g),_Diffuse2_var.rgb,Mask) + node_2876.b*(_node_3650.rgb*NormalDirectionMask.b)));
                float3 diffuseColor = node_3746;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _DiffuseTopDown; uniform float4 _DiffuseTopDown_ST;
            uniform float _TilingX;
            uniform float _RotationX;
            uniform float _GeneralTiling;
            uniform float _TilingY;
            uniform float _RotationY;
            uniform float _TilingZ;
            uniform float _RotationZ;
            uniform float _Emissive;
            uniform sampler2D _DiffuseSidesXZ; uniform float4 _DiffuseSidesXZ_ST;
            uniform float _BlendValue;
            uniform sampler2D _DetailMask; uniform float4 _DetailMask_ST;
            uniform float _Contrast;
            uniform float _MaskAmount;
            uniform sampler2D _Diffuse2; uniform float4 _Diffuse2_ST;
            uniform float _Tiling2;
            uniform float _Rotation2;
            uniform fixed _InvertMask;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float3 node_3480 = abs(mul( unity_WorldToObject, float4(i.normalDir,0) ).xyz.rgb);
                float3 NormalDirectionMask = saturate(( _BlendValue > 0.5 ? (1.0-(1.0-2.0*(_BlendValue-0.5))*(1.0-node_3480)) : (2.0*_BlendValue*node_3480) ));
                float3 node_2876 = NormalDirectionMask;
                float node_9302_ang = (((_RotationX+(-90.0))*3.141592654)/180.0);
                float node_9302_spd = 1.0;
                float node_9302_cos = cos(node_9302_spd*node_9302_ang);
                float node_9302_sin = sin(node_9302_spd*node_9302_ang);
                float2 node_9302_piv = float2(0.5,0.5);
                float3 node_7457 = (mul( unity_WorldToObject, float4((i.posWorld.rgb-objPos.rgb),0) ).xyz.rgb*_GeneralTiling*4.0);
                float2 node_9302 = (mul(((node_7457.gb/4.0)*_TilingX)-node_9302_piv,float2x2( node_9302_cos, -node_9302_sin, node_9302_sin, node_9302_cos))+node_9302_piv);
                float2 GB = node_9302;
                float2 node_9874 = GB;
                float4 _node_1257 = tex2D(_DiffuseSidesXZ,TRANSFORM_TEX(node_9874, _DiffuseSidesXZ)); // X Axis FrontBack
                float node_4319_ang = (((_RotationY+90.0)*3.141592654)/180.0);
                float node_4319_spd = 1.0;
                float node_4319_cos = cos(node_4319_spd*node_4319_ang);
                float node_4319_sin = sin(node_4319_spd*node_4319_ang);
                float2 node_4319_piv = float2(0.5,0.5);
                float2 node_4031 = node_7457.rb;
                float2 node_4319 = (mul(((node_4031/4.0)*_TilingY)-node_4319_piv,float2x2( node_4319_cos, -node_4319_sin, node_4319_sin, node_4319_cos))+node_4319_piv);
                float2 RB = node_4319;
                float2 node_7446 = RB;
                float4 _node_4330 = tex2D(_DiffuseTopDown,TRANSFORM_TEX(node_7446, _DiffuseTopDown)); // Y Axis TopBottom
                float node_5886_ang = (((_Rotation2+90.0)*3.141592654)/180.0);
                float node_5886_spd = 1.0;
                float node_5886_cos = cos(node_5886_spd*node_5886_ang);
                float node_5886_sin = sin(node_5886_spd*node_5886_ang);
                float2 node_5886_piv = float2(0.5,0.5);
                float2 node_5886 = (mul(((node_4031/4.0)*_Tiling2)-node_5886_piv,float2x2( node_5886_cos, -node_5886_sin, node_5886_sin, node_5886_cos))+node_5886_piv);
                float2 RBSecondarymap = node_5886;
                float2 node_6968 = RBSecondarymap;
                float4 _Diffuse2_var = tex2D(_Diffuse2,TRANSFORM_TEX(node_6968, _Diffuse2));
                float4 _DetailMask_var = tex2D(_DetailMask,TRANSFORM_TEX(i.uv0, _DetailMask));
                float node_9944 = 0.0;
                float node_5210 = (1.0+(-1*_Contrast));
                float Mask = saturate(((_MaskAmount*2.0+-1.0)+(node_5210 + ( (lerp( _DetailMask_var.r, (1.0 - _DetailMask_var.r), _InvertMask ) - node_9944) * (_Contrast - node_5210) ) / (1.0 - node_9944))));
                float node_8575_ang = ((_RotationZ*3.141592654)/180.0);
                float node_8575_spd = 1.0;
                float node_8575_cos = cos(node_8575_spd*node_8575_ang);
                float node_8575_sin = sin(node_8575_spd*node_8575_ang);
                float2 node_8575_piv = float2(0.5,0.5);
                float2 node_8575 = (mul(((node_7457.rg/4.0)*_TilingZ)-node_8575_piv,float2x2( node_8575_cos, -node_8575_sin, node_8575_sin, node_8575_cos))+node_8575_piv);
                float2 RG = node_8575;
                float2 node_4282 = RG;
                float4 _node_3650 = tex2D(_DiffuseSidesXZ,TRANSFORM_TEX(node_4282, _DiffuseSidesXZ)); // Z Axis LeftRight 
                float3 node_3746 = (_Color.rgb*(node_2876.r*(_node_1257.rgb*NormalDirectionMask.r) + node_2876.g*lerp((_node_4330.rgb*NormalDirectionMask.g),_Diffuse2_var.rgb,Mask) + node_2876.b*(_node_3650.rgb*NormalDirectionMask.b)));
                o.Emission = (node_3746*_Emissive);
                
                float3 diffColor = node_3746;
                o.Albedo = diffColor;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
