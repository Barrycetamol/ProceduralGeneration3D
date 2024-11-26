

using UnityEngine;

class NoiseTextureRender : MonoBehaviour{
        private Renderer RenderTarget{get; set;}
        private NoiseGenerator NoiseGenerator {get; set;}
        private ColorTextureRenderer ColorTextureRenderer{get; set;}

        [field: SerializeField] public bool AutoUpdate{get; set;} = false;
        [field: SerializeField] public bool UseTimeOffset {get; set;} = false;

        void Start(){
            
            if(AutoUpdate) InvokeRepeating("GeneratePlane", 0.0f, 1.0f);
            else GeneratePlane();
        }
        public void GeneratePlane(){
            NoiseGenerator = GetComponent<NoiseGenerator>();
            RenderTarget = GetComponent<MeshRenderer>();
            ColorTextureRenderer = GetComponent<ColorTextureRenderer>();
            if(NoiseGenerator != null) NoiseGenerator.GenerateNoise(UseTimeOffset);
            if(RenderTarget != null && ColorTextureRenderer != null) WritePixelsToRenderTarget(NoiseGenerator.GetNoiseSamples(), NoiseGenerator.NoiseSampleSize);
        }

        public void SetRenderTarter(Renderer target){
            RenderTarget = target;
        }

        public void WritePixelsToRenderTarget(float[] noiseSamples, Vector2Int samepleSize){
            Texture2D texture = new Texture2D(samepleSize.x, samepleSize.y);

            Color[] pixels = new Color[samepleSize.x * samepleSize.y];
            for(int i = 0; i < samepleSize.y; i++){
                for(int j = 0; j < samepleSize.x; j++){
                    float noiseSample = noiseSamples[i * samepleSize.x + j];
                    pixels[i * samepleSize.x + j] = ColorTextureRenderer.GenerateColor(noiseSample);
                }
            }

            texture.filterMode = FilterMode.Point; 
            texture.wrapMode = TextureWrapMode.Clamp; 
            texture.SetPixels(pixels);
            texture.Apply(false);

            RenderTarget.material.mainTexture = texture;

        }
}