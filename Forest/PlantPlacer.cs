using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest {

    public class PlantPlacer : MonoBehaviour {
        [SerializeField] float stationaryDistance = 5f;

        private Transform mainCharTransform;
        private CameraFollow cameraFollow;
        private ForestChunks forestChunks;
        private PlantPool plantPool;

        private HashSet<Chunk> lastVisibleChunks = new HashSet<Chunk>();

        private Vector2 lastPos;
        
        private void Start() {
            mainCharTransform = MainCharacter.current.transform;
            cameraFollow = CameraFollow.current;
            forestChunks = ForestChunks.current;
            plantPool = new PlantPool();

            lastPos = mainCharTransform.position;
            UpdatePlants(lastPos);
        }

        private void Update() {
            Vector2 currentPos = mainCharTransform.position;
            if((currentPos - lastPos).CompareLength(stationaryDistance) > 0) {
                UpdatePlants(currentPos);
                lastPos = currentPos;
            }
        }

        private void UpdatePlants(Vector2 currentPos) {
            Vector2 rectSize = cameraFollow.GetMaxRectSize();
            
            float additionalMargin = 5f;   // just 'safety' measure
            additionalMargin += stationaryDistance * 2; // x2 because stationaryDistance is kinda radius
            rectSize += Vector2.one * additionalMargin;
            var rect = new Rect(currentPos - rectSize * 0.5f, rectSize);

            var currentChunks = new HashSet<Chunk>(forestChunks.GetChunksTouchingRect(rect));

            var newlyVisible = new HashSet<Chunk>(currentChunks);
            newlyVisible.ExceptWith(lastVisibleChunks);
            var notVisibleAnymore = lastVisibleChunks;
            notVisibleAnymore.ExceptWith(currentChunks);

            foreach(var chunk in notVisibleAnymore) {
                RemovePlantsFromChunk(chunk);
            }
            foreach(var chunk in newlyVisible) {
                PlacePlantsOnChunk(chunk);
            }

            lastVisibleChunks = currentChunks;
        }

        private void PlacePlantsOnChunk(Chunk chunk) {
            for(int i = 0; i < chunk.slotDatas.Length; i++) {
                plantPool.PlacePlant(chunk.slotDatas[i]);
            }
        }

        private void RemovePlantsFromChunk(Chunk chunk) {
            for(int i = 0; i < chunk.slotDatas.Length; i++) {
                plantPool.RemovePlant(chunk.slotDatas[i]);
            }
        }
    }

}