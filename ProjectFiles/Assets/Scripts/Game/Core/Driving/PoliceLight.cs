using UnityEngine;


namespace Game.Core.Driving {
    
}
public class PoliceLight : MonoBehaviour {
    // Serializable Fields
    [SerializeField] private float speed = 1f;

    // Start is called before the first frame update
    private void Start() {
        GetComponent<Animator>().SetFloat("LightSpeed", speed);
    }

    // Update is called once per frame
    private void Update() { }
}