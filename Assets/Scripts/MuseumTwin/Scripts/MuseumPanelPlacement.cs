using UnityEngine;

// Select a fixed, comfortable anchor for the existing world-space menu and its previews.
// No updates to the rig, collider layers, raycasters or input bindings.
public static class MuseumPanelPlacement
{
    static readonly float[] Distances = { .85f, .70f, .60f, 1.05f, 1.15f };
    static readonly float[] Angles = { 0f, -20f, 20f, -35f, 35f, -55f, 55f, -80f, 80f };
    static readonly float[] Lifts = { 0f, .15f, .30f };
    static readonly Vector3[] Samples = {
        new Vector3(0,0,0), new Vector3(-.39f,.18f,0), new Vector3(.39f,.18f,0),
        new Vector3(-.39f,-.39f,0), new Vector3(.39f,-.39f,0),
        new Vector3(0,.34f,-.08f), new Vector3(0,-.39f,0)
    };

    public static void FindAnchor(Camera camera, out Vector3 position, out Quaternion rotation)
    {
        Vector3 eye = camera.transform.position;
        Vector3 forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < .0001f) forward = Vector3.Cross(camera.transform.right, Vector3.up);
        if (forward.sqrMagnitude < .0001f) forward = Vector3.forward;
        forward.Normalize();
        Transform rig = camera.transform.root;
        Physics.SyncTransforms();
        float best = float.PositiveInfinity;
        position = eye + forward * .70f;
        rotation = Quaternion.LookRotation(forward, Vector3.up);

        foreach (float angle in Angles)
        foreach (float lift in Lifts)
        foreach (float distance in Distances)
        {
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
            Vector3 p = eye + dir * distance + Vector3.up * lift;
            Quaternion r = Quaternion.LookRotation(dir, Vector3.up);
            // Reserve space for the lower View/Explore controls and the upper preview.
            int blocked = 0;
            var hits = Physics.OverlapBox(p + Vector3.up * -.04f,
                new Vector3(.43f,.45f,.06f), r, ~0, QueryTriggerInteraction.Ignore);
            foreach (var h in hits) if (!IsRig(h,rig)) blocked += 4;
            foreach (Vector3 sample in Samples)
            {
                Vector3 ray = p + r * sample - eye;
                foreach (var h in Physics.RaycastAll(eye, ray.normalized, ray.magnitude, ~0, QueryTriggerInteraction.Ignore))
                    if (!IsRig(h.collider, rig)) { blocked++; break; }
            }
            // Prefer forward, near eye height; never move the menu once placed.
            float score = blocked * 100f + Mathf.Abs(angle)*.035f + lift*6f + Mathf.Abs(distance-.85f)*3f;
            if (score < best) { best = score; position = p; rotation = r; }
            if (blocked == 0) { position = p; rotation = r; return; }
        }
    }

    static bool IsRig(Collider c, Transform root)
    {
        return c == null || c.transform == root || c.transform.IsChildOf(root);
    }
}
