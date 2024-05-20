using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class PlanarReflectionCamera : MonoBehaviour
{
    private static readonly int ReflectionTex = Shader.PropertyToID("_ReflectionTex");
    private static readonly Matrix4x4 ScaleXMinusOne = Matrix4x4.Scale(new Vector3(-1, 1, 1));
    
    private enum ReflectionNormal
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }
    
    [SerializeField]
    private Material _material;
    
    [SerializeField]
    private ReflectionNormal _reflectionNormal = ReflectionNormal.Up;
    
    [SerializeField]
    private LayerMask _reflectionMask = -1;
    
    [SerializeField]
    private float _clipPlaneOffset = 0;
    
    [SerializeField]
    [Range(0.01f, 1f)]
    private float _farScale = 1;
    
    [SerializeField]
    [Range(0.01f, 1f)]
    private float _renderTextureScale = 1f;

    [SerializeField] 
    private bool _showHiddenCamera = false;

    private Camera _reflectionCamera;
    private Renderer _renderer;

    private int ScaledWidth => Mathf.Max(1, (int)(Screen.width * _renderTextureScale));
    
    private int ScaledHeight => Mathf.Max(1, (int)(Screen.height * _renderTextureScale));

    private bool HasReflectionCameraChanged => _reflectionCamera == null || _reflectionCamera.targetTexture == null;
    
    private bool IsScreenSizeChanged => _reflectionCamera.targetTexture.width != ScaledWidth || _reflectionCamera.targetTexture.height != ScaledHeight;

    private Vector3 ReflectionNormalVector
    {
        get
        {
            switch (_reflectionNormal)
            {
                case ReflectionNormal.Up:
                    return -transform.up;//reversed
                
                case ReflectionNormal.Down:
                    return transform.up;//reversed
                
                case ReflectionNormal.Left:
                    return -transform.right;
                
                case ReflectionNormal.Right:
                    return transform.right;
                
                case ReflectionNormal.Forward:
                    return transform.forward;
                
                case ReflectionNormal.Back:
                    return -transform.forward;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    private void Reset()
    {
        if (_reflectionCamera != null)
        {
            DestroyImmediate(_reflectionCamera.gameObject);
        }
        
        _renderer = GetComponent<Renderer>();
        _material = _renderer.sharedMaterial;
    }

    private void Awake()
    {
        Init();
    }
    
    private void OnValidate()
    {
        //reinitialize
        if (Application.isPlaying == false)
        {
            Init();
        }
    }

    private void Init()
    {
        if (_reflectionCamera == null)
        {
            var cameraObject = new GameObject(nameof(PlanarReflectionCamera));
            cameraObject.transform.SetParent(transform);

            _reflectionCamera = cameraObject.AddComponent<Camera>();
            _renderer = GetComponent<Renderer>();
        }
        
        var oldRenderTexture = _reflectionCamera.targetTexture;
        var renderTexture = new RenderTexture(ScaledWidth, ScaledHeight, 16)
        {
            name = $"{nameof(PlanarReflectionCamera)}-{name}",
            useMipMap = false,
            autoGenerateMips = false,
            anisoLevel = 0,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat,
        };

        _reflectionCamera.targetTexture = renderTexture;
        _reflectionCamera.gameObject.hideFlags = _showHiddenCamera ? HideFlags.NotEditable | HideFlags.DontSave : HideFlags.HideAndDontSave;
            
        if (oldRenderTexture != null)
        {
            DestroyImmediate(oldRenderTexture);
        }
            
        if (_material != null)
        {
            if (_material.HasProperty(ReflectionTex))
            {
                _material.SetTexture(ReflectionTex, _reflectionCamera.targetTexture);
            }
            else
            {
                Debug.LogError("Material does not have _ReflectionTex property. Please add it to the shader.");
            }
        }
    }
    
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        
        #if UNITY_EDITOR
        UnityEditor.PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnEditorSceneManagerOnSceneSaved;
        #endif
    }


    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        
#if UNITY_EDITOR
        UnityEditor.PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdated;
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnEditorSceneManagerOnSceneSaved;
#endif
    }

    private void OnBeginCameraRendering(ScriptableRenderContext arg1, Camera cam)
    {
        if (_reflectionCamera == cam)
            GL.invertCulling = true;
    }
    
    private void OnEndCameraRendering(ScriptableRenderContext arg1, Camera cam)
    {
        if (_reflectionCamera == cam)
            GL.invertCulling = false;
    }
    
    private void OnEditorSceneManagerOnSceneSaved(Scene _)
    {
        Init();
    }
    
    private void OnPrefabInstanceUpdated(GameObject _)
    {
        Init();
    }
    
    private void LateUpdate()
    {
        //reinitialize if needed
        if (HasReflectionCameraChanged || IsScreenSizeChanged)
        {
            Init();
        }

        UpdateReflection(Camera.main, _reflectionCamera);
    }

    private void UpdateReflection(Camera main, Camera reflectionCamera)
    {
        if (main == null || reflectionCamera == null)
        {
            return;
        }
        
        //copy camera settings
        reflectionCamera.clearFlags = main.clearFlags;
        reflectionCamera.backgroundColor = main.backgroundColor;
        reflectionCamera.cullingMask = main.cullingMask & _reflectionMask;
        reflectionCamera.orthographic = main.orthographic;
        reflectionCamera.fieldOfView = main.fieldOfView;
        reflectionCamera.aspect = main.aspect;
        reflectionCamera.orthographicSize = main.orthographicSize;
        reflectionCamera.nearClipPlane = main.nearClipPlane;
        reflectionCamera.farClipPlane = main.farClipPlane * _farScale;
        reflectionCamera.renderingPath = main.renderingPath;
        reflectionCamera.allowHDR = main.allowHDR;
        reflectionCamera.allowMSAA = main.allowMSAA;
        reflectionCamera.allowDynamicResolution = main.allowDynamicResolution;

        Vector3 normal = ReflectionNormalVector;
        Vector3 pos = transform.position;
        float d = -Vector3.Dot(normal, pos);
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);

        reflectionCamera.worldToCameraMatrix = ScaleXMinusOne * main.worldToCameraMatrix * reflection;
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        reflectionCamera.projectionMatrix = ScaleXMinusOne * main.CalculateObliqueMatrix(clipPlane);
    }
    
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * _clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = -m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }
}
