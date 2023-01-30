//---------------------------------------
// Toon.shader
// アニメ風の見た目にするシェーダー
// 作成日 2022/12
// 作成者 佐藤光
//---------------------------------------
Shader "HIKALAB/Unlit/Toon"
{
    Properties
    {
        [Header(TEXTURE)]
        [Space(10)]
        //ベーステクスチャ
        _MainTex ("Main Texture", 2D) = "white" {}
        //二値化の影用テクスチャ
        [NOSCALEOFFSET]_RampTex ("Ramp Texture", 2D) = "white" {}

        [Header(RIM)]
        [Space(10)]
        //逆光の色
        _RimColor ("RimLight Color", Color) = (1,1,1,1)
        //逆光の強さ
        _RimPower ("RimLight Power", float) = 0.0

        [Header(OUTLINE)]
        [Space(10)]
        //フチの色
        _OutlineColor ("OutLine Color", Color) = (0,0,0,1)
        //フチの太さ
        _OutlineWidth ("OutLine Width", Range(0, 0.005)) = 0.003
    }
    SubShader
    {
    
        Pass
        {
            Name "ベースシェーダー"

            Tags { "RenderType" = "Opaque" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            //頂点シェーダへの入力定義
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            //vertexからfragmentに受け渡すデータ
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirection : TEXCOORD1;    //視線ベクトル 
                float3 normalDirection : TEXCOORD2;  //法線ベクトル
            };


            //プロパティで設定した変数をshader内で使用する宣言
            sampler2D _MainTex;
            sampler2D _RampTex;
            float4 _MainTex_ST;
            fixed4 _RimColor;
            half _RimPower;


            //頂点シェーダの処理
            v2f vert (appdata v)
            {
                v2f o;
                

                //３Ⅾ空間からカメラから見える２Ⅾ空間へ変換
                o.vertex = UnityObjectToClipPos(v.vertex);

                //現在のモデルの行列を代入
                float4x4 modelMatrix = unity_ObjectToWorld;

                //頂点の法線をワールド座標系に変換して正規化する
                o.normalDirection = normalize(UnityObjectToWorldNormal(v.normal));

                //カメラポジションから、モデルの座標(modelMatrix)と頂点を行列変換したものを引いて正規化する
                o.viewDirection = normalize(_WorldSpaceCameraPos - mul(modelMatrix, v.vertex).xyz);

                //テクスチャスケールとタイリング
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                return o;
            }

            //フラグメントシェーダの処理
            fixed4 frag(v2f i) : SV_Target
            {

                /*[[  MAINテクスチャ  ]]*/
                //Mainテクスチャからサンプリング
                fixed4 color = tex2D(_MainTex, i.uv);



                /*[[  RAMPテクスチャ  ]]*/
                //SceneのDirectionalLightの方向を取得
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

                //Lightと法線の内積から陰のかかり方を計算
                half Direction = dot(i.normalDirection, -lightDirection) * 0.5;
                
                //Rampテクスチャからサンプリング
                half3 ramp = tex2D(_RampTex, float2(Direction, Direction)).rgb;

                //Lightのカラーを乗算
                color.rgb *= _LightColor0.rgb * ramp;



                /*[[  リムライト  ]]*/
                //正規化したviewDirとnormalDirの内積取得（０～１の値を取る）
                half rimlight = 1.0 - abs(dot(i.viewDirection, i.normalDirection));

                //発光 = RimColor x 上記の絶対値の_RimPower乗を取得（発光も０～１の値を取る）その後_RimPowerを乗算
                fixed3 emission = _RimColor.rgb * pow(rimlight, _RimPower) * _RimPower;

                //color.rgbに発光を加算
                color.rgb += emission;

                //発光加算をcolorに代入
                color = fixed4(color.rgb, 1.0);


                return color;
            }
            ENDCG
        }

//===================================================================================================================================================//

        Pass
        {
            Name "アウトラインシェーダー"

            Tags { "RenderType" = "Opaque" }

            //表面を描画しない
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //頂点シェーダへの入力定義
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            //vertexからfragmentに受け渡すデータ
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            //プロパティで設定した変数をshader内で使用する宣言
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _OutlineColor;
            half _OutlineWidth;


            //頂点シェーダの処理
            v2f vert (appdata v, float2 uv : TEXCOORD0)
            {
                v2f o;


                //３Ⅾ空間からカメラから見える２Ⅾ空間へ変換
                o.vertex = UnityObjectToClipPos(v.vertex);

                //※初期化されてない構造体のパラメータがあるとwarningが出るので書いた(できれば無くしたい)
                o.uv = uv;

                //モデル座標系の法線をビュー座標系に変換し正規化する
                float3 viewnormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

                //ビュー座標系に変換した法線を投影座標系に変換
                float2 offset = TransformViewToProjection(viewnormal.xy);

                //法線方向に頂点位置を押し出す
                o.vertex.xy += offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(o.vertex.z) * _OutlineWidth;


                return o;
            }

            //フラグメントシェーダの処理
            fixed4 frag(v2f i) : SV_Target
            {

                /*[[  アウトライン  ]]*/
                //アウトラインカラーを表示
                return _OutlineColor;


            }
                ENDCG
        }

//===================================================================================================================================================//

        Pass
        {
            Name "シャドウイングシェーダー"

            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster //影描画のためのマクロ呼び出し

            #include "UnityCG.cginc"       

            //頂点シェーダへの入力定義
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            //vertexからfragmentに受け渡すデータ
            struct v2f
            {

                V2F_SHADOW_CASTER;

            };

            //頂点シェーダの処理
            v2f vert(appdata v)
            {
                v2f o;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

                return o;
            }

            //フラグメントシェーダの処理
            fixed4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
