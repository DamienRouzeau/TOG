// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:1,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:1,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:34696,y:33040,varname:node_2865,prsc:2|diff-282-OUT,spec-8766-OUT,gloss-9529-OUT,normal-4059-OUT,emission-9642-OUT,alpha-9171-OUT,refract-3510-OUT;n:type:ShaderForge.SFN_Tex2d,id:7736,x:32005,y:32078,ptovrint:True,ptlb:Albedo,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5964,x:31967,y:31427,ptovrint:True,ptlb:Normal Map,ptin:_BumpMap,varname:_BumpMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Slider,id:1813,x:31942,y:32547,ptovrint:False,ptlb:Roughness Intensity,ptin:_RoughnessIntensity,varname:_RoughnessIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5472615,max:1;n:type:ShaderForge.SFN_Slider,id:5239,x:27842,y:35305,ptovrint:False,ptlb:Stealth,ptin:_Stealth,varname:_Stealth,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7378641,max:1;n:type:ShaderForge.SFN_FragmentPosition,id:1496,x:27527,y:35395,varname:node_1496,prsc:2;n:type:ShaderForge.SFN_RemapRange,id:8695,x:28062,y:35076,varname:node_8695,prsc:2,frmn:0,frmx:1,tomn:-1.5,tomx:1.5|IN-5239-OUT;n:type:ShaderForge.SFN_Subtract,id:6214,x:27664,y:35541,varname:node_6214,prsc:2|A-1496-XYZ,B-2134-XYZ;n:type:ShaderForge.SFN_Transform,id:3416,x:28091,y:35488,varname:node_3416,prsc:2,tffrom:0,tfto:0|IN-3681-OUT;n:type:ShaderForge.SFN_ComponentMask,id:6633,x:28271,y:35512,varname:node_6633,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-3416-XYZ;n:type:ShaderForge.SFN_ObjectPosition,id:2134,x:27483,y:35655,varname:node_2134,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7633,x:28461,y:35223,varname:node_7633,prsc:2|A-8695-OUT,B-3000-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3000,x:28463,y:34296,ptovrint:False,ptlb:StealthScale,ptin:_StealthScale,varname:_StealthScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Append,id:1748,x:26795,y:34661,varname:node_1748,prsc:2|A-157-G,B-157-B;n:type:ShaderForge.SFN_Append,id:499,x:26792,y:34808,varname:node_499,prsc:2|A-157-B,B-157-R;n:type:ShaderForge.SFN_Append,id:3722,x:26792,y:34944,varname:node_3722,prsc:2|A-157-R,B-157-G;n:type:ShaderForge.SFN_Tex2d,id:2867,x:27774,y:34624,varname:node_2867,prsc:2,ntxv:0,isnm:False|UVIN-3852-UVOUT,TEX-1387-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:1387,x:27275,y:34446,ptovrint:False,ptlb:Pattern,ptin:_Pattern,varname:_Pattern,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2137,x:27774,y:34807,varname:node_2137,prsc:2,ntxv:0,isnm:False|UVIN-1326-UVOUT,TEX-1387-TEX;n:type:ShaderForge.SFN_Tex2d,id:2297,x:27774,y:34992,varname:node_2297,prsc:2,ntxv:0,isnm:False|UVIN-6826-UVOUT,TEX-1387-TEX;n:type:ShaderForge.SFN_ChannelBlend,id:6324,x:28100,y:34833,varname:node_6324,prsc:2,chbt:0|M-7265-OUT,R-2867-R,G-2137-R,B-2297-R;n:type:ShaderForge.SFN_NormalVector,id:552,x:27246,y:35191,prsc:2,pt:False;n:type:ShaderForge.SFN_Abs,id:9231,x:27454,y:35190,varname:node_9231,prsc:2|IN-552-OUT;n:type:ShaderForge.SFN_Multiply,id:7265,x:27695,y:35198,varname:node_7265,prsc:2|A-9231-OUT,B-9231-OUT;n:type:ShaderForge.SFN_Tex2d,id:8234,x:31977,y:32333,ptovrint:False,ptlb:Metallic(R),ptin:_MetallicR,varname:_MetallicR,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Set,id:7269,x:29110,y:35567,varname:VerticalGradient,prsc:2|IN-3884-OUT;n:type:ShaderForge.SFN_Set,id:3348,x:28665,y:34832,varname:Pattern,prsc:2|IN-5545-OUT;n:type:ShaderForge.SFN_Get,id:9302,x:30934,y:33504,varname:node_9302,prsc:2|IN-7269-OUT;n:type:ShaderForge.SFN_Get,id:9984,x:30515,y:34544,varname:node_9984,prsc:2|IN-3348-OUT;n:type:ShaderForge.SFN_Clamp01,id:9021,x:28285,y:34833,varname:node_9021,prsc:2|IN-6324-OUT;n:type:ShaderForge.SFN_Clamp01,id:2121,x:32181,y:33299,varname:node_2121,prsc:2|IN-1162-OUT;n:type:ShaderForge.SFN_Normalize,id:3681,x:27904,y:35488,varname:node_3681,prsc:2|IN-6214-OUT;n:type:ShaderForge.SFN_Multiply,id:3884,x:28694,y:35527,varname:node_3884,prsc:2|A-2880-OUT,B-8793-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:8793,x:28295,y:35753,ptovrint:False,ptlb:StartTop/Bottom,ptin:_StartTopBottom,varname:_StartTopBottom,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-1416-OUT,B-3909-OUT;n:type:ShaderForge.SFN_Vector1,id:3909,x:28061,y:35752,varname:node_3909,prsc:2,v1:-1;n:type:ShaderForge.SFN_Vector1,id:1416,x:28066,y:35832,varname:node_1416,prsc:2,v1:1;n:type:ShaderForge.SFN_ConstantClamp,id:9778,x:31296,y:34080,varname:node_9778,prsc:2,min:0,max:1|IN-9984-OUT;n:type:ShaderForge.SFN_ComponentMask,id:9015,x:34035,y:34697,varname:node_9015,prsc:2,cc1:0,cc2:0,cc3:-1,cc4:-1|IN-8539-OUT;n:type:ShaderForge.SFN_Multiply,id:9608,x:32436,y:34142,varname:node_9608,prsc:2|A-9778-OUT,B-1427-OUT;n:type:ShaderForge.SFN_Multiply,id:4736,x:32672,y:34237,varname:node_4736,prsc:2|A-9608-OUT,B-8551-OUT;n:type:ShaderForge.SFN_Vector1,id:9067,x:31434,y:34453,varname:node_9067,prsc:2,v1:2;n:type:ShaderForge.SFN_Set,id:6265,x:29112,y:35236,varname:Phase,prsc:2|IN-9260-OUT;n:type:ShaderForge.SFN_Get,id:3485,x:30884,y:33803,varname:node_3485,prsc:2|IN-6265-OUT;n:type:ShaderForge.SFN_OneMinus,id:9260,x:28663,y:35223,varname:node_9260,prsc:2|IN-7633-OUT;n:type:ShaderForge.SFN_Add,id:2129,x:32075,y:34430,varname:node_2129,prsc:2|A-9302-OUT,B-939-OUT;n:type:ShaderForge.SFN_Clamp01,id:8551,x:32309,y:34401,varname:node_8551,prsc:2|IN-2129-OUT;n:type:ShaderForge.SFN_Add,id:1162,x:31739,y:33436,varname:node_1162,prsc:2|A-3485-OUT,B-9778-OUT,C-9302-OUT;n:type:ShaderForge.SFN_Panner,id:3852,x:27528,y:34663,varname:node_3852,prsc:2,spu:1,spv:0|UVIN-280-OUT,DIST-6088-OUT;n:type:ShaderForge.SFN_Panner,id:1326,x:27518,y:34850,varname:node_1326,prsc:2,spu:1,spv:0|UVIN-9269-OUT,DIST-6088-OUT;n:type:ShaderForge.SFN_Panner,id:6826,x:27502,y:34990,varname:node_6826,prsc:2,spu:1,spv:0|UVIN-3886-OUT,DIST-6088-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3088,x:27312,y:34292,ptovrint:False,ptlb:PatternSpeed,ptin:_PatternSpeed,varname:_PatternSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Time,id:3326,x:27242,y:34094,varname:node_3326,prsc:2;n:type:ShaderForge.SFN_Multiply,id:6088,x:27450,y:34202,varname:node_6088,prsc:2|A-3326-T,B-3088-OUT;n:type:ShaderForge.SFN_Add,id:2310,x:31774,y:32934,varname:node_2310,prsc:2|A-18-OUT,B-9778-OUT,C-592-OUT;n:type:ShaderForge.SFN_OneMinus,id:1333,x:31994,y:33005,varname:node_1333,prsc:2|IN-2310-OUT;n:type:ShaderForge.SFN_Clamp01,id:4315,x:32921,y:33185,varname:node_4315,prsc:2|IN-2435-OUT;n:type:ShaderForge.SFN_Multiply,id:18,x:31365,y:33089,varname:node_18,prsc:2|A-9607-OUT,B-9302-OUT;n:type:ShaderForge.SFN_Vector1,id:9607,x:31070,y:33064,varname:node_9607,prsc:2,v1:10;n:type:ShaderForge.SFN_RemapRange,id:592,x:31538,y:32760,varname:node_592,prsc:2,frmn:0,frmx:1,tomn:-5,tomx:5|IN-1449-OUT;n:type:ShaderForge.SFN_Color,id:3545,x:32803,y:32763,ptovrint:False,ptlb:Pattern Color,ptin:_PatternColor,varname:_PatternColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:3049,x:33135,y:33145,varname:node_3049,prsc:2|A-3545-RGB,B-4315-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9155,x:33368,y:33274,ptovrint:False,ptlb:EmissiveIntensity,ptin:_EmissiveIntensity,varname:_EmissiveIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Add,id:9642,x:34207,y:33090,varname:node_9642,prsc:2|A-751-OUT,B-5652-OUT;n:type:ShaderForge.SFN_Tex2d,id:7320,x:33282,y:32281,ptovrint:False,ptlb:Emissive,ptin:_Emissive,varname:_Emissive,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Clamp01,id:1792,x:33546,y:33045,varname:node_1792,prsc:2|IN-3049-OUT;n:type:ShaderForge.SFN_Multiply,id:280,x:27116,y:34663,varname:node_280,prsc:2|A-1748-OUT,B-3784-OUT;n:type:ShaderForge.SFN_Multiply,id:9269,x:27116,y:34802,varname:node_9269,prsc:2|A-499-OUT,B-3784-OUT;n:type:ShaderForge.SFN_Multiply,id:3886,x:27116,y:34959,varname:node_3886,prsc:2|A-3722-OUT,B-3784-OUT;n:type:ShaderForge.SFN_ValueProperty,id:8496,x:26544,y:34505,ptovrint:False,ptlb:PatternScale,ptin:_PatternScale,varname:_PatternScale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ConstantClamp,id:3866,x:32969,y:34254,varname:node_3866,prsc:2,min:0,max:1|IN-4736-OUT;n:type:ShaderForge.SFN_Multiply,id:8348,x:30947,y:33217,varname:node_8348,prsc:2|A-9984-OUT,B-7834-OUT;n:type:ShaderForge.SFN_Vector1,id:7834,x:30704,y:33398,varname:node_7834,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Add,id:1449,x:31248,y:32937,varname:node_1449,prsc:2|A-8348-OUT,B-3485-OUT;n:type:ShaderForge.SFN_Add,id:2619,x:33729,y:33436,varname:node_2619,prsc:2|A-477-OUT,B-8264-OUT;n:type:ShaderForge.SFN_Fresnel,id:3398,x:32646,y:33455,varname:node_3398,prsc:2|EXP-3362-OUT;n:type:ShaderForge.SFN_Vector1,id:3362,x:32399,y:33502,varname:node_3362,prsc:2,v1:5;n:type:ShaderForge.SFN_Multiply,id:6761,x:32947,y:33474,varname:node_6761,prsc:2|A-3398-OUT,B-3802-OUT,C-4314-OUT;n:type:ShaderForge.SFN_Vector1,id:3802,x:32774,y:33533,varname:node_3802,prsc:2,v1:3;n:type:ShaderForge.SFN_OneMinus,id:4314,x:32399,y:33364,varname:node_4314,prsc:2|IN-2121-OUT;n:type:ShaderForge.SFN_Vector1,id:8105,x:32947,y:33414,varname:node_8105,prsc:2,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:3021,x:33171,y:33456,ptovrint:False,ptlb:VisibleEffect,ptin:_VisibleEffect,varname:_VisibleEffect,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-8105-OUT,B-6761-OUT;n:type:ShaderForge.SFN_Multiply,id:8264,x:33401,y:33490,varname:node_8264,prsc:2|A-3021-OUT,B-1835-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1835,x:33171,y:33593,ptovrint:False,ptlb:VisibleEffectIntensity,ptin:_VisibleEffectIntensity,varname:_VisibleEffectIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:5873,x:33731,y:33131,varname:node_5873,prsc:2|A-1792-OUT,B-9155-OUT;n:type:ShaderForge.SFN_Vector1,id:1072,x:32243,y:35151,varname:node_1072,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Multiply,id:7845,x:32815,y:35050,varname:node_7845,prsc:2|A-9923-OUT,B-1072-OUT;n:type:ShaderForge.SFN_Slider,id:9923,x:32164,y:35031,ptovrint:False,ptlb:Min Visibility,ptin:_MinVisibility,varname:_MinVisibility,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_ValueProperty,id:9584,x:31982,y:34275,ptovrint:False,ptlb:RefractionIntensity,ptin:_RefractionIntensity,varname:_RefractionIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:1427,x:32252,y:34202,varname:node_1427,prsc:2|A-5430-OUT,B-9584-OUT;n:type:ShaderForge.SFN_Vector1,id:5430,x:31982,y:34201,varname:node_5430,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Multiply,id:4373,x:33022,y:35009,varname:node_4373,prsc:2|A-9984-OUT,B-7845-OUT;n:type:ShaderForge.SFN_Clamp01,id:9156,x:33205,y:35009,varname:node_9156,prsc:2|IN-4373-OUT;n:type:ShaderForge.SFN_Multiply,id:9657,x:31850,y:33198,varname:node_9657,prsc:2|A-3046-OUT,B-1162-OUT;n:type:ShaderForge.SFN_Multiply,id:2797,x:32569,y:32924,varname:node_2797,prsc:2|A-1333-OUT,B-1362-OUT;n:type:ShaderForge.SFN_Vector1,id:3046,x:31701,y:33171,varname:node_3046,prsc:2,v1:10;n:type:ShaderForge.SFN_Add,id:4400,x:32109,y:33154,varname:node_4400,prsc:2|A-1461-OUT,B-9657-OUT;n:type:ShaderForge.SFN_Vector1,id:1461,x:31943,y:33119,varname:node_1461,prsc:2,v1:0;n:type:ShaderForge.SFN_Subtract,id:1362,x:32387,y:33103,varname:node_1362,prsc:2|A-1333-OUT,B-4400-OUT;n:type:ShaderForge.SFN_OneMinus,id:2435,x:32646,y:33137,varname:node_2435,prsc:2|IN-2797-OUT;n:type:ShaderForge.SFN_Clamp01,id:8539,x:33848,y:34697,varname:node_8539,prsc:2|IN-291-OUT;n:type:ShaderForge.SFN_Multiply,id:477,x:33433,y:33318,varname:node_477,prsc:2|A-2121-OUT,B-9664-OUT;n:type:ShaderForge.SFN_Vector1,id:9664,x:33274,y:33395,varname:node_9664,prsc:2,v1:1;n:type:ShaderForge.SFN_Clamp01,id:2569,x:33940,y:33425,varname:node_2569,prsc:2|IN-2619-OUT;n:type:ShaderForge.SFN_Add,id:5652,x:33995,y:33103,varname:node_5652,prsc:2|A-5873-OUT,B-9370-OUT;n:type:ShaderForge.SFN_Multiply,id:9370,x:33769,y:33254,varname:node_9370,prsc:2|A-3545-RGB,B-8264-OUT;n:type:ShaderForge.SFN_Add,id:291,x:33662,y:34717,varname:node_291,prsc:2|A-3866-OUT,B-9156-OUT;n:type:ShaderForge.SFN_Add,id:939,x:31710,y:34449,varname:node_939,prsc:2|A-3485-OUT,B-9067-OUT;n:type:ShaderForge.SFN_Tex2d,id:7498,x:32005,y:31905,ptovrint:False,ptlb:Ambient Oclussion,ptin:_AmbientOclussion,varname:_AmbientOclussion,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:282,x:32298,y:31997,varname:node_282,prsc:2|A-7498-RGB,B-7736-RGB,C-3702-RGB;n:type:ShaderForge.SFN_Color,id:3702,x:32001,y:31708,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:9529,x:32336,y:32579,varname:node_9529,prsc:2|A-1813-OUT,B-9159-R;n:type:ShaderForge.SFN_Tex2d,id:9159,x:32169,y:32790,ptovrint:False,ptlb:Roughness(R),ptin:_RoughnessR,varname:_RoughnessR,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:7578,x:33268,y:32509,ptovrint:False,ptlb:Emissive Color,ptin:_EmissiveColor,varname:_EmissiveColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:751,x:33537,y:32446,varname:node_751,prsc:2|A-7320-RGB,B-7578-RGB,C-4225-OUT;n:type:ShaderForge.SFN_Color,id:6616,x:31901,y:32165,ptovrint:False,ptlb:EmissiveColor,ptin:_EmissiveColor,varname:_EmissiveColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:8766,x:32612,y:32466,varname:node_8766,prsc:2|A-8234-R,B-916-OUT;n:type:ShaderForge.SFN_Slider,id:916,x:31727,y:32465,ptovrint:False,ptlb:Metallic Intensity,ptin:_MetallicIntensity,varname:_MetallicIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:3784,x:26755,y:34461,varname:node_3784,prsc:2|A-1906-OUT,B-8496-OUT;n:type:ShaderForge.SFN_Vector1,id:1906,x:26559,y:34337,varname:node_1906,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Multiply,id:9171,x:34207,y:33311,varname:node_9171,prsc:2|A-7877-OUT,B-2569-OUT;n:type:ShaderForge.SFN_Slider,id:7374,x:31844,y:31619,ptovrint:False,ptlb:Normal Intensity,ptin:_NormalIntensity,varname:_NormalIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:10;n:type:ShaderForge.SFN_ComponentMask,id:6627,x:32208,y:31485,varname:node_6627,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-5964-RGB;n:type:ShaderForge.SFN_Multiply,id:9788,x:32430,y:31535,varname:node_9788,prsc:2|A-6627-OUT,B-7374-OUT;n:type:ShaderForge.SFN_Append,id:4059,x:32610,y:31562,varname:node_4059,prsc:2|A-9788-OUT,B-5091-OUT;n:type:ShaderForge.SFN_Vector1,id:5091,x:32136,y:31310,varname:node_5091,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:8521,x:26059,y:34874,varname:node_8521,prsc:2|A-5577-XYZ,B-3804-XYZ;n:type:ShaderForge.SFN_FragmentPosition,id:5577,x:25827,y:34802,varname:node_5577,prsc:2;n:type:ShaderForge.SFN_ObjectPosition,id:3804,x:25827,y:34921,varname:node_3804,prsc:2;n:type:ShaderForge.SFN_ComponentMask,id:157,x:26482,y:34876,varname:node_157,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-8521-OUT;n:type:ShaderForge.SFN_TexCoord,id:4418,x:27726,y:35698,varname:node_4418,prsc:2,uv:1,uaff:False;n:type:ShaderForge.SFN_SwitchProperty,id:2880,x:28505,y:35428,ptovrint:False,ptlb:Triplanar/UV2,ptin:_TriplanarUV2,varname:_TriplanarUV2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-4418-V,B-6633-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:5545,x:28508,y:34819,ptovrint:False,ptlb:PatternTriplanar/UV1,ptin:_PatternTriplanarUV1,varname:_PatternTriplanarUV1,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6231-R,B-9021-OUT;n:type:ShaderForge.SFN_TexCoord,id:1881,x:27536,y:33986,varname:node_1881,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Tex2d,id:6231,x:28101,y:34410,varname:_node_6231,prsc:2,ntxv:0,isnm:False|UVIN-6494-UVOUT,TEX-1387-TEX;n:type:ShaderForge.SFN_Panner,id:6494,x:27955,y:34277,varname:node_6494,prsc:2,spu:1,spv:0|UVIN-7530-OUT,DIST-6088-OUT;n:type:ShaderForge.SFN_Multiply,id:7530,x:27812,y:34084,varname:node_7530,prsc:2|A-1881-UVOUT,B-3784-OUT;n:type:ShaderForge.SFN_Relay,id:7877,x:33995,y:33311,varname:node_7877,prsc:2|IN-7736-A;n:type:ShaderForge.SFN_Multiply,id:3510,x:34346,y:33473,varname:node_3510,prsc:2|A-2668-OUT,B-9015-OUT;n:type:ShaderForge.SFN_Multiply,id:2668,x:34146,y:33439,varname:node_2668,prsc:2|A-7877-OUT,B-8694-OUT;n:type:ShaderForge.SFN_Vector1,id:8694,x:33997,y:33581,varname:node_8694,prsc:2,v1:0.5;n:type:ShaderForge.SFN_ValueProperty,id:4225,x:33265,y:32769,ptovrint:False,ptlb:PBREmissiveIntensity,ptin:_PBREmissiveIntensity,varname:_PBREmissiveIntensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:5239-3000-7736-3702-7320-7578-4225-5964-7374-7498-8234-9159-1813-1387-9155-3545-3088-8496-8793-3021-1835-9923-9584-916-2880-5545;pass:END;sub:END;*/

Shader "Marc Sureda/Stealth(Metallic Setup)" {
    Properties {
        _Stealth ("Stealth", Range(0, 1)) = 0.7378641
        _StealthScale ("StealthScale", Float ) = 2
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Emissive ("Emissive", 2D) = "black" {}
        _EmissiveColor ("Emissive Color", Color) = (1,1,1,1)
        _PBREmissiveIntensity ("PBREmissiveIntensity", Float ) = 1
        _BumpMap ("Normal Map", 2D) = "black" {}
        _NormalIntensity ("Normal Intensity", Range(0, 10)) = 1
        _AmbientOclussion ("Ambient Oclussion", 2D) = "white" {}
        _MetallicR ("Metallic(R)", 2D) = "white" {}
        _RoughnessR ("Roughness(R)", 2D) = "white" {}
        _RoughnessIntensity ("Roughness Intensity", Range(0, 1)) = 0.5472615
        _Pattern ("Pattern", 2D) = "white" {}
        _EmissiveIntensity ("EmissiveIntensity", Float ) = 1
        _PatternColor ("Pattern Color", Color) = (1,1,1,1)
        _PatternSpeed ("PatternSpeed", Float ) = 0.1
        _PatternScale ("PatternScale", Float ) = 1
        [MaterialToggle] _StartTopBottom ("StartTop/Bottom", Float ) = 1
        [MaterialToggle] _VisibleEffect ("VisibleEffect", Float ) = 0
        _VisibleEffectIntensity ("VisibleEffectIntensity", Float ) = 1
        _MinVisibility ("Min Visibility", Range(0, 1)) = 0
        _RefractionIntensity ("RefractionIntensity", Float ) = 1
        _MetallicIntensity ("Metallic Intensity", Range(0, 1)) = 0
        [MaterialToggle] _TriplanarUV2 ("Triplanar/UV2", Float ) = 0
        [MaterialToggle] _PatternTriplanarUV1 ("PatternTriplanar/UV1", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "DisableBatching"="True"
        }
        GrabPass{ "Refraction" }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D Refraction;
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _RoughnessIntensity;
            uniform float _Stealth;
            uniform float _StealthScale;
            uniform sampler2D _Pattern; uniform float4 _Pattern_ST;
            uniform sampler2D _MetallicR; uniform float4 _MetallicR_ST;
            uniform fixed _StartTopBottom;
            uniform float _PatternSpeed;
            uniform float4 _PatternColor;
            uniform float _EmissiveIntensity;
            uniform sampler2D _Emissive; uniform float4 _Emissive_ST;
            uniform float _PatternScale;
            uniform fixed _VisibleEffect;
            uniform float _VisibleEffectIntensity;
            uniform float _MinVisibility;
            uniform float _RefractionIntensity;
            uniform sampler2D _AmbientOclussion; uniform float4 _AmbientOclussion_ST;
            uniform float4 _Color;
            uniform sampler2D _RoughnessR; uniform float4 _RoughnessR_ST;
            uniform float4 _EmissiveColor;
            uniform float _MetallicIntensity;
            uniform float _NormalIntensity;
            uniform fixed _TriplanarUV2;
            uniform fixed _PatternTriplanarUV1;
            uniform float _PBREmissiveIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                float4 screenPos : TEXCOORD6;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.screenPos = o.pos;
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 _BumpMap_var = tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap));
                float3 normalLocal = float3((_BumpMap_var.rgb.rg*_NormalIntensity),1.0);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_7877 = _MainTex_var.a;
                float4 node_3326 = _Time + _TimeEditor;
                float node_6088 = (node_3326.g*_PatternSpeed);
                float node_3784 = (0.1*_PatternScale);
                float2 node_6494 = ((i.uv0*node_3784)+node_6088*float2(1,0));
                float4 _node_6231 = tex2D(_Pattern,TRANSFORM_TEX(node_6494, _Pattern));
                float3 node_9231 = abs(i.normalDir);
                float3 node_7265 = (node_9231*node_9231);
                float3 node_157 = (i.posWorld.rgb-objPos.rgb).rgb;
                float2 node_3852 = ((float2(node_157.g,node_157.b)*node_3784)+node_6088*float2(1,0));
                float4 node_2867 = tex2D(_Pattern,TRANSFORM_TEX(node_3852, _Pattern));
                float2 node_1326 = ((float2(node_157.b,node_157.r)*node_3784)+node_6088*float2(1,0));
                float4 node_2137 = tex2D(_Pattern,TRANSFORM_TEX(node_1326, _Pattern));
                float2 node_6826 = ((float2(node_157.r,node_157.g)*node_3784)+node_6088*float2(1,0));
                float4 node_2297 = tex2D(_Pattern,TRANSFORM_TEX(node_6826, _Pattern));
                float Pattern = lerp( _node_6231.r, saturate((node_7265.r*node_2867.r + node_7265.g*node_2137.r + node_7265.b*node_2297.r)), _PatternTriplanarUV1 );
                float node_9984 = Pattern;
                float node_9778 = clamp(node_9984,0,1);
                float VerticalGradient = (lerp( i.uv1.g, normalize((i.posWorld.rgb-objPos.rgb)).rgb.g, _TriplanarUV2 )*lerp( 1.0, (-1.0), _StartTopBottom ));
                float node_9302 = VerticalGradient;
                float Phase = (1.0 - ((_Stealth*3.0+-1.5)*_StealthScale));
                float node_3485 = Phase;
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + ((node_7877*0.5)*saturate((clamp(((node_9778*(0.1*_RefractionIntensity))*saturate((node_9302+(node_3485+2.0)))),0,1)+saturate((node_9984*(_MinVisibility*0.1))))).rr);
                float4 sceneColor = tex2D(Refraction, sceneUVs);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _RoughnessR_var = tex2D(_RoughnessR,TRANSFORM_TEX(i.uv0, _RoughnessR));
                float gloss = 1.0 - (_RoughnessIntensity*_RoughnessR_var.r); // Convert roughness to gloss
                float perceptualRoughness = (_RoughnessIntensity*_RoughnessR_var.r);
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
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
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float4 _MetallicR_var = tex2D(_MetallicR,TRANSFORM_TEX(i.uv0, _MetallicR));
                float3 specularColor = (_MetallicR_var.r*_MetallicIntensity);
                float specularMonochrome;
                float4 _AmbientOclussion_var = tex2D(_AmbientOclussion,TRANSFORM_TEX(i.uv0, _AmbientOclussion));
                float3 diffuseColor = (_AmbientOclussion_var.rgb*_MainTex_var.rgb*_Color.rgb); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float4 _Emissive_var = tex2D(_Emissive,TRANSFORM_TEX(i.uv0, _Emissive));
                float node_1333 = (1.0 - ((10.0*node_9302)+node_9778+(((node_9984*0.1)+node_3485)*10.0+-5.0)));
                float node_1162 = (node_3485+node_9778+node_9302);
                float node_2121 = saturate(node_1162);
                float node_8264 = (lerp( 0.0, (pow(1.0-max(0,dot(normalDirection, viewDirection)),5.0)*3.0*(1.0 - node_2121)), _VisibleEffect )*_VisibleEffectIntensity);
                float3 emissive = ((_Emissive_var.rgb*_EmissiveColor.rgb*_PBREmissiveIntensity)+((saturate((_PatternColor.rgb*saturate((1.0 - (node_1333*(node_1333-(0.0+(10.0*node_1162))))))))*_EmissiveIntensity)+(_PatternColor.rgb*node_8264)));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(lerp(sceneColor.rgb, finalColor,(node_7877*saturate(((node_2121*1.0)+node_8264)))),1);
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
            #define UNITY_PASS_FORWARDADD
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D Refraction;
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _RoughnessIntensity;
            uniform float _Stealth;
            uniform float _StealthScale;
            uniform sampler2D _Pattern; uniform float4 _Pattern_ST;
            uniform sampler2D _MetallicR; uniform float4 _MetallicR_ST;
            uniform fixed _StartTopBottom;
            uniform float _PatternSpeed;
            uniform float4 _PatternColor;
            uniform float _EmissiveIntensity;
            uniform sampler2D _Emissive; uniform float4 _Emissive_ST;
            uniform float _PatternScale;
            uniform fixed _VisibleEffect;
            uniform float _VisibleEffectIntensity;
            uniform float _MinVisibility;
            uniform float _RefractionIntensity;
            uniform sampler2D _AmbientOclussion; uniform float4 _AmbientOclussion_ST;
            uniform float4 _Color;
            uniform sampler2D _RoughnessR; uniform float4 _RoughnessR_ST;
            uniform float4 _EmissiveColor;
            uniform float _MetallicIntensity;
            uniform float _NormalIntensity;
            uniform fixed _TriplanarUV2;
            uniform fixed _PatternTriplanarUV1;
            uniform float _PBREmissiveIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 tangentDir : TEXCOORD4;
                float3 bitangentDir : TEXCOORD5;
                float4 screenPos : TEXCOORD6;
                LIGHTING_COORDS(7,8)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 _BumpMap_var = tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap));
                float3 normalLocal = float3((_BumpMap_var.rgb.rg*_NormalIntensity),1.0);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_7877 = _MainTex_var.a;
                float4 node_3326 = _Time + _TimeEditor;
                float node_6088 = (node_3326.g*_PatternSpeed);
                float node_3784 = (0.1*_PatternScale);
                float2 node_6494 = ((i.uv0*node_3784)+node_6088*float2(1,0));
                float4 _node_6231 = tex2D(_Pattern,TRANSFORM_TEX(node_6494, _Pattern));
                float3 node_9231 = abs(i.normalDir);
                float3 node_7265 = (node_9231*node_9231);
                float3 node_157 = (i.posWorld.rgb-objPos.rgb).rgb;
                float2 node_3852 = ((float2(node_157.g,node_157.b)*node_3784)+node_6088*float2(1,0));
                float4 node_2867 = tex2D(_Pattern,TRANSFORM_TEX(node_3852, _Pattern));
                float2 node_1326 = ((float2(node_157.b,node_157.r)*node_3784)+node_6088*float2(1,0));
                float4 node_2137 = tex2D(_Pattern,TRANSFORM_TEX(node_1326, _Pattern));
                float2 node_6826 = ((float2(node_157.r,node_157.g)*node_3784)+node_6088*float2(1,0));
                float4 node_2297 = tex2D(_Pattern,TRANSFORM_TEX(node_6826, _Pattern));
                float Pattern = lerp( _node_6231.r, saturate((node_7265.r*node_2867.r + node_7265.g*node_2137.r + node_7265.b*node_2297.r)), _PatternTriplanarUV1 );
                float node_9984 = Pattern;
                float node_9778 = clamp(node_9984,0,1);
                float VerticalGradient = (lerp( i.uv1.g, normalize((i.posWorld.rgb-objPos.rgb)).rgb.g, _TriplanarUV2 )*lerp( 1.0, (-1.0), _StartTopBottom ));
                float node_9302 = VerticalGradient;
                float Phase = (1.0 - ((_Stealth*3.0+-1.5)*_StealthScale));
                float node_3485 = Phase;
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + ((node_7877*0.5)*saturate((clamp(((node_9778*(0.1*_RefractionIntensity))*saturate((node_9302+(node_3485+2.0)))),0,1)+saturate((node_9984*(_MinVisibility*0.1))))).rr);
                float4 sceneColor = tex2D(Refraction, sceneUVs);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _RoughnessR_var = tex2D(_RoughnessR,TRANSFORM_TEX(i.uv0, _RoughnessR));
                float gloss = 1.0 - (_RoughnessIntensity*_RoughnessR_var.r); // Convert roughness to gloss
                float perceptualRoughness = (_RoughnessIntensity*_RoughnessR_var.r);
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float4 _MetallicR_var = tex2D(_MetallicR,TRANSFORM_TEX(i.uv0, _MetallicR));
                float3 specularColor = (_MetallicR_var.r*_MetallicIntensity);
                float specularMonochrome;
                float4 _AmbientOclussion_var = tex2D(_AmbientOclussion,TRANSFORM_TEX(i.uv0, _AmbientOclussion));
                float3 diffuseColor = (_AmbientOclussion_var.rgb*_MainTex_var.rgb*_Color.rgb); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                float node_1162 = (node_3485+node_9778+node_9302);
                float node_2121 = saturate(node_1162);
                float node_8264 = (lerp( 0.0, (pow(1.0-max(0,dot(normalDirection, viewDirection)),5.0)*3.0*(1.0 - node_2121)), _VisibleEffect )*_VisibleEffectIntensity);
                return fixed4(finalColor * (node_7877*saturate(((node_2121*1.0)+node_8264))),0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "StealthMaterialInspector"
}
