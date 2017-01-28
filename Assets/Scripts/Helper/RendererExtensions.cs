using UnityEngine;

public static class RendererExtensions {

    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        try
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        } catch (System.NullReferenceException e) {
            return false;
        }
    }
}
