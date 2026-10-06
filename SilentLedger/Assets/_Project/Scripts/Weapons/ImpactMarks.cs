using System.Collections.Generic;
using UnityEngine;

namespace SilentLedger.Weapons
{
    /// <summary>Small pooled markers where shots land. Placeholder until real decals and effects.</summary>
    public static class ImpactMarks
    {
        const int MaxMarks = 40;
        const float Size = 0.05f;

        static readonly Queue<Transform> Marks = new Queue<Transform>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetPool() => Marks.Clear();

        public static void Spawn(Vector3 point, Vector3 normal)
        {
            Transform mark = null;
            while (Marks.Count >= MaxMarks && mark == null)
                mark = Marks.Dequeue();
            if (mark == null)
                mark = CreateMark();

            mark.SetPositionAndRotation(point + normal * 0.005f, Quaternion.LookRotation(normal));
            Marks.Enqueue(mark);
        }

        static Transform CreateMark()
        {
            // A cube, not a sphere: player builds strip engine classes no scene uses, and the
            // levels only use box colliders, so CreatePrimitive(Sphere) fails on device.
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ImpactMark";
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.localScale = new Vector3(Size, Size, 0.005f);
            return go.transform;
        }
    }
}
