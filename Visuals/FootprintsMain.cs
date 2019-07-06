using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Foot { Left, Right }

public class FootprintsMain : MonoBehaviour {
    [System.Serializable]
    private class FootprintsSystems {
        [SerializeField] string _name;
        [SerializeField] ParticleSystem left;
        [SerializeField] ParticleSystem right;

        public string name => _name;

        public ParticleSystem GetParticleSystem(Foot foot) {
            return foot == Foot.Left ? left : right;
        }
    }

    [SerializeField] FootprintsSystems[] footprintsSystems = new FootprintsSystems[0];

    public static FootprintsMain current;

    private void Awake() {
        current = this;
    }

    public ParticleSystem GetParticleSystem(string name, Foot foot) {
        foreach(var fs in footprintsSystems) {
            if(fs.name == name)
                return fs.GetParticleSystem(foot);
        }
        return null;
    }
}
