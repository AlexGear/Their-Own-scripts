using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armament
{
    public class BulletMagazine : CommonMagazine
    {
        [SerializeField] GameObject _bulletPrefab;
        [SerializeField] GameObject _heavyBulletPrefab;

        public GameObject bulletPrefab => _bulletPrefab;
        public GameObject heavyBulletPrefab => _heavyBulletPrefab;

        private GameObjectPool _bulletPool;
        public GameObjectPool bulletPool => _bulletPool ?? (_bulletPool = GameObjectPool.Get(_bulletPrefab));

        private GameObjectPool _heavyBulletPool;
        public GameObjectPool heavyBulletPool => _heavyBulletPool ?? (_heavyBulletPool = GameObjectPool.Get(_heavyBulletPrefab));
    }
}