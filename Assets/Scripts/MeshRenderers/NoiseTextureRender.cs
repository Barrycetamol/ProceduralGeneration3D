using UnityEngine;
using UnityEngine.UI;

public class NoiseTextureRender : MonoBehaviour{
        private Renderer RenderTarget{get; set;}
        private NoiseGenerator NoiseGenerator {get; set;}
        private ColorTextureRenderer ColorTextureRenderer{get; set;}

        [field: SerializeField] public bool AutoUpdate{get; set;} = false;
        [field: SerializeField] public bool UseTimeOffset {get; set;} = false;
        [field: SerializeField] public bool WriteToImage {get; set;} = false;
        [field: SerializeField] public Image NoiseTarget {get; set;}

        void Start(){
            
            if(AutoUpdate) InvokeRepeating("GeneratePlane", 0.0f, 1.0f);
            else GeneratePlane();
        }
        public void GeneratePlane(){
            NoiseGenerator = GetComponent<NoiseGenerator>();
            RenderTarget = GetComponent<MeshRenderer>();
        }

        public void SetRenderTarget(Renderer target){
            RenderTarget = target;
        }

        public void WritePixelsToRenderTarget(float[] noiseSamples, Vector2Int samepleSize){
            Texture2D texture = new Texture2D(samepleSize.x, samepleSize.y);

            Color[] pixels = new Color[samepleSize.x * samepleSize.y];
            for(int i = 0; i < samepleSize.y; i++){
                for(int j = 0; j < samepleSize.x; j++){
                    float noiseSample = noiseSamples[i * samepleSize.x + j];
                    pixels[i * samepleSize.x + j] = new Color(noiseSample, noiseSample, noiseSample);
                }
            }

            texture.filterMode = FilterMode.Point; 
            texture.wrapMode = TextureWrapMode.Clamp; 
            texture.SetPixels(pixels);
            texture.Apply(false);

            if(RenderTarget != null)RenderTarget.material.mainTexture = texture;
            if(WriteToImage) WriteTextureToImage(texture);

        }

        public void WriteTextureToImage(Texture2D texture){
            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height), // Full texture area
                new Vector2(0.5f, 0.5f) // Pivot at the center
            );

            // Assign the Sprite to the Image component
            NoiseTarget.sprite = newSprite;
        }
}