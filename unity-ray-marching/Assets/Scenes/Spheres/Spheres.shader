Shader "Hidden/Spheres"
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
      #include "../../Shader/Math/Math.cginc"
      #include "../../Shader/Ray Marching/SDF/SDF.cginc"

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
        const float t = _Time.y;

        const float a = sdf_sphere(p, kSphereCenter, kSphereRadius);
        const float b = sdf_sphere(p, kSphereCenter - float3(2.0f * sin(2.0f * t), 0.0f, 0.0f), 0.7f * kSphereRadius * (0.8f * abs(sin(2.0f * t)) + 0.2f));
        const float c = sdf_sphere(p, kSphereCenter - float3(0.0f, 2.0f * cos(2.0f * t), 0.0f), 0.7f * kSphereRadius * (0.8f * abs(cos(2.0f * t)) + 0.2f));

        float k = 0.5f;
        float res = sdf_uni_smooth(a, b, k);
        res = sdf_uni_smooth(c, res, k);

        return res;
      }


      float3 normal(float3 p)
      {
        return sdf_normal(p, map, 0.01f);
      }

      float3 march(float3 ro, float3 rd)
      {
        const int kMaxSteps = 256;
        const float kHitDist = 0.005f;
        const float kMaxDist = 1000.0f;
        const float3 kBackground = float3(0.0f, 0.07f, 0.15f);
        const float3 kDiffuse = float3(1.0f, 0.65f, 0.05f);
        const float3 kAmbient = 0.1f * kDiffuse;
        const float t = _Time.y;

        float dist = 0.0f;
        for (int i = 0; i < kMaxSteps; ++i)
        {
          float3 p = ro + dist * rd;
          float d = map(p);

          if (d < kHitDist)
          {
            float3 n = normal(p);
            float3 lightPos = ro + float3(5.0f * sin(2.0f * t), 5.0f * cos(2.0f * t), 0.0f);
            float3 lightDir = normalize(p - lightPos);
            float3 shaded = max(pow(dot(n, -lightDir), 1.0f), kAmbient) * kDiffuse;
            float3 fresnel = 0.3f * pow(saturate(1.0f - dot(n, -rd)), 2.0f);
            float3 specular = 0.2f * pow(saturate(dot(n, -normalize(rd + lightDir))), 100.0f);
            return shaded + fresnel + specular;
          }

          if (dist > kMaxDist)
            return kBackground;

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

        col.rgb = march(ro, rd);

        return col;
      }
      ENDCG
    }
  }
}
