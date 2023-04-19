Shader "Custom/GrassComputeVertexFrag"
{
    Properties
    {
        [Toggle(BLEND)] _BlendFloor("Blend with floor", Float) = 0
        _Fade("Top Fade Offset", Range(-1,10)) = 1
        _AmbientAdjustment("Ambient Adjustment", Range(-1,10)) = 0
    }

        CGINCLUDE
#include "UnityCG.cginc" 
#include "Lighting.cginc"
#include "AutoLight.cginc"
#pragma multi_compile _SHADOWS_SCREEN
#pragma multi_compile_fwdbase_fullforwardshadows
#pragma multi_compile_fog
#pragma shader_feature BLEND
        //
        struct DrawVertex
    {
        float3 positionWS; // The position in world space 
        float2 uv;
        float3 diffuseColor;
    };

    // A triangle on the generated mesh
    struct DrawTriangle
    {
        float3 normalOS;
        DrawVertex vertices[3]; // The three points on the triangle
    };

    StructuredBuffer<DrawTriangle> _DrawTriangles;

    struct v2f
    {
        float4 pos : SV_POSITION; // Position in clip space
        float2 uv : TEXCOORD0;          // The height of this vertex on the grass blade
        float3 positionWS : TEXCOORD1; // Position in world space
        float3 normalWS : TEXCOORD2;   // Normal vector in world space
        float3 diffuseColor : COLOR;
        LIGHTING_COORDS(3, 4)
            UNITY_FOG_COORDS(5)
    };
    // Properties
    float4 _TopTint;
    float4 _BottomTint;
    float _Fade;
    float4 _PositionMoving;
    float _OrthographicCamSize;
    float3 _OrthographicCamPos;
    uniform sampler2D _TerrainDiffuse;
    float _AmbientAdjustment;

    // Vertex function
    struct unityTransferVertexToFragmentSucksHack
    {
        float3 vertex : POSITION;
    };

    // -- retrieve data generated from compute shader
    v2f vert(uint vertexID : SV_VertexID)
    {
        // Initialize the output struct
        v2f output = (v2f)0;

        // Get the vertex from the buffer
        // Since the buffer is structured in triangles, we need to divide the vertexID by three
        // to get the triangle, and then modulo by 3 to get the vertex on the triangle
        DrawTriangle tri = _DrawTriangles[vertexID / 3];
        DrawVertex input = tri.vertices[vertexID % 3];

        output.pos = UnityObjectToClipPos(input.positionWS);
        output.positionWS = input.positionWS;
        float3 faceNormal = tri.normalOS;
        output.normalWS = faceNormal;

        output.uv = input.uv;

        output.diffuseColor = input.diffuseColor;

        // making pointlights work requires v.vertex
        unityTransferVertexToFragmentSucksHack v;
        v.vertex = output.pos;

        TRANSFER_VERTEX_TO_FRAGMENT(output);
        UNITY_TRANSFER_FOG(output, output.pos);

        return output;
    }


    ENDCG
        SubShader
    {
        Cull Off
        Pass // basic color with directional lights
        {
            Tags
            {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag            

            float4 frag(v2f i) : SV_Target
            {
            // rendertexture UV for terrain blending
            float2 uv = i.positionWS.xz - _OrthographicCamPos.xz;
            uv = uv / (_OrthographicCamSize * 2);
            uv += 0.5;

            // get ambient color from environment lighting
            float4 ambient = float4(ShadeSH9(float4(0,0,1,1)),0);

            float shadow = 0;
            #if BLEND
                shadow = 1;
            #endif
            #if defined(SHADOWS_SCREEN)
                shadow = (SAMPLE_DEPTH_TEXTURE_PROJ(_ShadowMapTexture, UNITY_PROJ_COORD(i._ShadowCoord)).r);
            #endif  

                // fade over the length of the grass
                float verticalFade = saturate(i.uv.y + _Fade);
                // colors from the tool with tinting from the grass script
                float4 baseColor = lerp(_BottomTint , _TopTint,verticalFade) * float4(i.diffuseColor, 1);
                // get the floor map
                float4 terrainForBlending = tex2D(_TerrainDiffuse, uv);

                #if BLEND
                // fade toptint with ambient if there is no main light   
                _TopTint = _TopTint * ambient;
            #endif

            float4 final = float4(0,0,0,0);
            #if BLEND      
            // tint the top blades and add in light color             
            terrainForBlending = lerp(terrainForBlending,terrainForBlending + (_TopTint * float4(i.diffuseColor, 1)) , verticalFade);
            final = lerp((terrainForBlending)*shadow , terrainForBlending, shadow);
            // add in ambient and attempt to blend in with the shadows
            final += lerp((ambient * terrainForBlending) * _AmbientAdjustment,0, shadow);
        #else
            final = baseColor;
            // add in shadows
            final *= shadow;
            // if theres a main light, multiply with its color and intensity 
            #if SHADOWS_SCREEN
                final *= _LightColor0;
            #endif  
                // add in ambient
                final += (ambient * baseColor);
            #endif

                // add fog
                UNITY_APPLY_FOG(i.fogCoord, final);
                return final;
            }
            ENDCG
        }

        Pass
                // point lights
                {
                    Tags
                    {
                        "LightMode" = "ForwardAdd"
                    }
                    Blend OneMinusDstColor One
                    ZWrite Off
                    Cull Off

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag                                   
                    #pragma multi_compile_fwdadd_fullshadows 

                    float4 frag(v2f i) : SV_Target
                    {
                        UNITY_LIGHT_ATTENUATION(atten, i, i.positionWS);

                        float3 pointlights = (atten * _LightColor0.rgb);
                        // fade light over length for a nicer look
                        float alpha = lerp(0.2, 1, saturate(i.uv.y + _Fade));
                        pointlights *= alpha;
                        return float4(pointlights, 1);
                    }
                    ENDCG
                }

                Pass // shadow pass
                {

                    Tags
                    {
                        "LightMode" = "ShadowCaster"
                    }

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma multi_compile_shadowcaster

                    float4 frag(v2f i) : SV_Target
                    {

                        SHADOW_CASTER_FRAGMENT(i)
                    }
                    ENDCG
                }


    }     Fallback "VertexLit"
}
