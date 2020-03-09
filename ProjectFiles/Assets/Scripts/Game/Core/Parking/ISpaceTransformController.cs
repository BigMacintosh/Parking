using UnityEngine;
using Utils;

namespace Game.Core.Parking {
    public interface ISpaceTransformController {
        ObjectTransform GetTransform();
    }
}