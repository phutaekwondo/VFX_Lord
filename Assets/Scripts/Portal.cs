using System;
using UnityEngine;
using UnityEngine.Rendering;
using RenderPipelineManager = UnityEngine.Rendering.RenderPipelineManager;

// [ExecuteInEditMode]
public class Portal : MonoBehaviour
{
    [SerializeField] private Portal m_connectedPortal;
    [SerializeField] private Camera m_camera; //the camera that will render the portal view for the connected portal
    [SerializeField] private Material m_portalMat;
    [SerializeField] private GameObject m_plane;
    float currentResolutionWidth;
    float currentResolutionHeight;

    private void Start() 
    {
        currentResolutionHeight = Screen.height;
        currentResolutionWidth = Screen.width;
        SetupTextureSize();
    }

    private void Update() 
    {
        if (currentResolutionHeight != Screen.height || currentResolutionWidth != Screen.width)
        {
            currentResolutionHeight = Screen.height;
            currentResolutionWidth = Screen.width;
            SetupTextureSize();
        }
    }

    void SetupTextureSize()
    {
        if (m_camera.targetTexture != null)
        {
            m_camera.targetTexture.Release();
        }
        m_camera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        m_connectedPortal.m_portalMat.SetTexture("_PortalTexture", m_camera.targetTexture);
    }

    private void OnEnable() 
    {
        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
    }

    private void OnDisable() 
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        UpdateCamera(camera);
    }

    private void UpdateCamera(Camera sourceCamera)
    {
        if ((sourceCamera.cameraType == CameraType.Game || sourceCamera.cameraType == CameraType.SceneView) && sourceCamera.tag != "PortalCamera" )
        {
            UpdateCameraPosition(     sourceCamera);
            UpdateCameraRotation(     sourceCamera);
            UpdateCameraClipingPlanes(sourceCamera);
        }
    }

    private void UpdateCameraClipingPlanes(Camera playerCamera)
    {
        //distance from camera to plane
        float distance = Vector3.Distance(m_plane.transform.position, m_camera.transform.position);
        m_camera.nearClipPlane = distance-0.3f;
    }

    private void UpdateCameraPosition( Camera playerCamera )
    {
        Transform connectedPortalTransform = m_connectedPortal.transform;

        Vector2 A = new Vector2(connectedPortalTransform.position.x, connectedPortalTransform.position.z);
        // Vector2 C = new Vector2(m_playerCamera.transform.position.x, m_playerCamera.transform.position.z);
        Vector2 C = new Vector2(playerCamera.transform.position.x, playerCamera.transform.position.z);
        Vector2 F = new Vector2(connectedPortalTransform.forward.x, connectedPortalTransform.forward.z);
        F.Normalize();

        Vector2 portalPosition2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 thisPortalForward2D = new Vector2(transform.forward.x, transform.forward.z);
        thisPortalForward2D.Normalize();

        // Vector2 relativePosition2D = portalPosition2D + a * thisPortalForward2D + b * thisPortalForwardPerpendicular2D;

        //angle to rotate from F to (C-A)
        float angle = Vector2.SignedAngle(F, C - A);
        Vector2 dir = Quaternion.Euler(0, 0, angle) * - thisPortalForward2D;
        Vector2 relativePosition2D = portalPosition2D + (dir * Vector2.Distance(A, C));
        

        //set the camera position
        m_camera.transform.position = new Vector3(relativePosition2D.x, playerCamera.transform.position.y, relativePosition2D.y);
    }
    
    private void UpdateCameraRotation( Camera playerCamera )
    {
        Vector2 thisPortalForward2D = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 connectedPortalForward2D = new Vector2(m_connectedPortal.transform.forward.x, m_connectedPortal.transform.forward.z);
        Vector2 playerCameraForward2D = new Vector2(playerCamera.transform.forward.x, playerCamera.transform.forward.z);

        float angle = Vector2.SignedAngle(connectedPortalForward2D, playerCameraForward2D);
        Vector2 dir = Quaternion.Euler(0, 0, angle) * (- thisPortalForward2D);
        dir *= playerCameraForward2D.magnitude / dir.magnitude;

        Vector3 relativeForward = new Vector3(dir.x, playerCamera.transform.forward.y, dir.y);
        m_camera.transform.forward = relativeForward;
    }
}
