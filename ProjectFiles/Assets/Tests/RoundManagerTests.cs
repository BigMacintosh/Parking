using System.Collections;
using Game.Core.Parking;
using Game.Core.Rounds;
using Game.Entity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utils;

namespace Tests {
    public class RoundManagerTests {
        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator RoundManagerTestsWithEnumeratorPasses() {
            yield return new MonoBehaviourTest<TestGame>();
        }
    }
}