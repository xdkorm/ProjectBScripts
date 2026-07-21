using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace ZigdarkS.ProjectB.Infrastructure.Rendering.Viewmodel
{
    [System.Serializable]
    class ViewmodelCustomPass : CustomPass
    {
        public float weaponFOV = 65f;
        public LayerMask weaponLayer = 1 << 6;
        public Shader compositeShader; // создать шейдер см. ниже, перетащить сюда

        Camera m_WeaponCamera;
        GameObject m_WeaponCameraGO;
        RTHandle m_WeaponColor;
        RTHandle m_WeaponDepth;
        Material m_CompositeMaterial;

        // Добавляет слой оружия в culling ТОЛЬКО для этого пасса,
        // не трогая Culling Mask основной камеры (иначе будет двойной рендер).
        protected override void AggregateCullingParameters(ref ScriptableCullingParameters cullingParameters, HDCamera hdCamera)
        {
            cullingParameters.cullingMask |= (uint)weaponLayer.value;
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            m_WeaponCameraGO = new GameObject("ViewmodelPass Hidden Camera") { hideFlags = HideFlags.HideAndDontSave };
            m_WeaponCamera = m_WeaponCameraGO.AddComponent<Camera>();
            m_WeaponCamera.enabled = false;

            m_WeaponColor = RTHandles.Alloc(
                Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
                useDynamicScale: true, name: "Weapon Color"
            );

            m_WeaponDepth = RTHandles.Alloc(
                Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                depthBufferBits: DepthBits.Depth32,
                useDynamicScale: true, name: "Weapon Depth"
            );

            if (compositeShader != null)
                m_CompositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);
            else
                Debug.LogWarning("ViewmodelCustomPass: compositeShader не назначен.");
        }

        protected override void Execute(CustomPassContext ctx)
        {
            var mainCamera = ctx.hdCamera.camera;

            m_WeaponCamera.CopyFrom(mainCamera);
            m_WeaponCamera.enabled = false;
            m_WeaponCamera.fieldOfView = weaponFOV;
            m_WeaponCamera.cullingMask = weaponLayer;
            m_WeaponCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);

            // Нормальный depth-тест против НАШЕГО чистого буфера — не полагаемся
            // на предположение HDRP о том, что перед этим уже был depth pre-pass.
            var depthState = new RenderStateBlock(RenderStateMask.Depth)
            {
                depthState = new DepthState(true, CompareFunction.LessEqual)
            };

            // Рисуем оружие в свои приватные, полностью чистые буферы.
            CustomPassUtils.RenderFromCamera(
                ctx,
                m_WeaponCamera,
                m_WeaponColor,
                m_WeaponDepth,
                ClearFlag.All,
                weaponLayer,
                CustomPass.RenderQueueType.All,
                overrideRenderState: depthState
            );

            // Компонуем поверх сцены (цвет + реальная глубина) только там, где есть оружие.
            if (m_CompositeMaterial != null)
            {
                ctx.cmd.SetGlobalTexture("_WeaponColor", m_WeaponColor);
                ctx.cmd.SetGlobalTexture("_WeaponDepth", m_WeaponDepth);
                CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer);
                CoreUtils.DrawFullScreen(ctx.cmd, m_CompositeMaterial, shaderPassId: 0);
            }
        }

        protected override void Cleanup()
        {
            m_WeaponColor?.Release();
            m_WeaponDepth?.Release();
            CoreUtils.Destroy(m_CompositeMaterial);
            if (m_WeaponCameraGO != null)
                CoreUtils.Destroy(m_WeaponCameraGO);
        }
    }
}