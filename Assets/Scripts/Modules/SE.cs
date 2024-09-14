// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// public class SE : Singleton<SE>
// {
//     public Volume volume;
//     private void Awake()
//     {
//         volume = GetComponent<Volume>();
//     }
//     public void SetVignette(float intensity)
//     {
//         Vignette v;
//         if (volume.profile.TryGet<Vignette>(out v))
//         {
//             v.intensity.SetValue(new ClampedFloatParameter(intensity, 0, 1, true));
//         }
//     }
//     public void SetBloom(float intensity)
//     {
//         Bloom bloom;
//         if (volume.profile.TryGet<Bloom>(out bloom))
//         {
//             bloom.intensity.SetValue(new ClampedFloatParameter(intensity, 0, 10, true));
//         }
//     }
// }
