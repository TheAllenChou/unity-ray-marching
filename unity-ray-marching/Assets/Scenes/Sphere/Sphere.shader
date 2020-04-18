Shader "Hidden/Sphere"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
  }
  SubShader
  {
    Cull Off ZWrite Off ZTest Always

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "../../Shader/SDF/SDF.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv     : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv     : TEXCOORD0;
        float4 vertex : SV_POSITION;
        float3 view   : TEXCOORD1;
      };

      sampler2D _MainTex;
      sampler2D _CameraDepthTexture;

      float map(float3 p)
      {
        const float3 kSphereCenter = float3(0.0f, 0.0f, 5.0f);
        const float3 kSphereRadius = 1.0f;

        /*
        return
          sdf_opu_cubic
          (
            sdf_sphere(p, kSphereCenter, kSphereRadius), 
            sdf_sphere(p + float3(1.0f, 0.0f, 0.0f), kSphereCenter, kSphereRadius), 
            0.1f
          );
        */

        return sdf_sphere(p, kSphereCenter, kSphereRadius);
      }

      float3 normal(float3 p)
      {
        return sdf_normal(p, map, 0.1f);
      }

      float3 ray_march(float3 ro, float3 rd)
      {
        const int kMaxSteps = 64;
        const float kHitDist = 0.01f;
        const float kMaxDist = 1000.0f;

        float dist = 0.0f;
        for (int i = 0; i < kMaxSteps; ++i)
        {
          float3 p = ro + dist * rd;
          float d = map(p);

          if (d < kHitDist)
            return normal(p);

          if (dist > kMaxDist)
            return 0.0f;

          dist += d;
        }

        return float3(1.0f, 0.0f, 0.0f);
      }

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;

        // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
        // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
        float3 view = mul(unity_CameraInvProjection, float4(v.uv * 2.0f - 1.0f, 0.0f, -1.0f));
        o.view = mul(unity_CameraToWorld, float4(view, 0.0f));

        return o;
      }

      fixed4 frag(v2f i) : SV_Target
      {
        fixed4 col = tex2D(_MainTex, i.uv);

        float3 ro = _WorldSpaceCameraPos;
        float3 rd = normalize(i.view);

        col.rgb = ray_march(ro, rd);

        return col;
      }
      ENDCG
    }
  }
}
