using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Utils;

namespace Tests {
    public class RoundManagerTests {
        [Test]
        public void RoundManagerTestsSimplePasses() {
            var t = new Timer(10);
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator RoundManagerTestsWithEnumeratorPasses() {
            
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
    }
}