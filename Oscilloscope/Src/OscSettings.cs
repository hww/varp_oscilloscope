using UnityEngine;

namespace VARP.OSC
{
    [CreateAssetMenu(menuName = "VARP/OscSettings")]
    public class OscSettings : ScriptableObject
    {
        // =============================================================================================================
        // Global constants
        // =============================================================================================================

        /// <summary>Size of buffer should be 2^N</summary>
        public const int BUFFER_SIZE = 1024;
        /// <summary>Size of buffer should be 2^N / 2</summary>
        public const int BUFFER_HALF_SIZE = BUFFER_SIZE / 2; 
        /// <summary>Size of buffer should be 2^N-1</summary>
        public const int BUFFER_INDEX_MASK = BUFFER_SIZE - 1;
        /// <summary>How many samples per single draw</summary>
        public const int SAMPLES_PER_DRAW = 10;

        // =============================================================================================================
        // Fiels for designer
        // =============================================================================================================

        public int pixelsPerDivision = 50;	//< How many pixels in single division (recomend 10,20,30,...)
        public int divisionsX = 11;			//< Horizontal divisions (Recomend odd value)
        public int divisionsY = 11;			//< Vertical divisions (Recomend odd value)
        public int subdivisions = 5;		//< Subdivisions in the division (Recomend 5 or 10)
        [Header("Grid options")]
        public bool drawGrid = true;		//< Draw grid
        public bool drawRullerX = true;		//< Draw horizontal ruller in center
        public bool drawRullerY = true;		//< Draw vertical ruller in center
        [Header("Calculated")]
        public int pixelsPerSubdivision;	//< (Calculated) How many pixels in subdivision
        public Vector2Int textureSize;		//< (Calculated) Texture size in pixels
        public Vector2Int textureCenter;	//< (Calculated) Center pixel position
        public Rect rectangle;				//< (Calculated) Screen rectangle divisions
        public float markersPositionX;		//< (Calculated) Coordinate of markers (Grid Divisions)
        public float timePerSample;         //< (Injected)  
  
        // =============================================================================================================
        // Initialization
        // =============================================================================================================
		
        /// <summary>
        /// Initialize object
        /// </summary>
        public void Initialize(float timePerSamp)
        {
            timePerSample = timePerSamp;
            pixelsPerSubdivision = pixelsPerDivision / subdivisions;
            textureSize = new Vector2Int(pixelsPerDivision * divisionsX, pixelsPerDivision * divisionsY);
            textureCenter = new Vector2Int(pixelsPerDivision * divisionsX / 2, pixelsPerDivision * divisionsY / 2);
            markersPositionX = -Mathf.RoundToInt(divisionsX/2);
            rectangle.Set(-(float)divisionsX / 2, -(float)divisionsY / 2,(float)divisionsX,(float)divisionsY);
        }
		
        // =============================================================================================================
        // INFO
        // 
        // The screen coordinates in the grid cells. The center of screen 0,0
        // Left bottom corner -N,-N. And top right corner is N,N.
        //
        // The pixels coordinate in the texture coordinate space.
        // If the pixel coordinates are out of bounds (larger than width/height or small than 0),
        // they will be clamped or repeated based on the texture's wrap mode.
        // Texture coordinates start at lower left corner.
        // =============================================================================================================
		
        // =============================================================================================================
        // Floating point pixel coordinate
        // =============================================================================================================
		
        /// <summary>Get pixel coordinate for the screen division coordinate</summary>
        public Vector2 GetPixelPosition(float x, float y)
        {
            return new Vector2(x * pixelsPerDivision + textureCenter.x, y * pixelsPerDivision + textureCenter.y);
        }
        /// <summary>Get X pixel coordinate for the screen division coordinate</summary>
        public float GetPixelPositionX(float x) { return x * pixelsPerDivision + textureCenter.x; }
        /// <summary>Get Y pixel coordinate for the screen division coordinate</summary>
        public float GetPixelPositionY(float y) { return y * pixelsPerDivision + textureCenter.y; }
		
        // =============================================================================================================
        // Integer pixel coordinate
        // =============================================================================================================
		
        /// <summary>Get pixel coordinate for the screen division coordinate</summary>
        public Vector2Int GetPixelPositionInt(float x, float y)
        {
            return new Vector2Int(
                Mathf.RoundToInt(x * pixelsPerDivision) + textureCenter.x, 
                Mathf.RoundToInt(y * pixelsPerDivision) + textureCenter.y);
        }
        /// <summary>Get X pixel coordinate for the screen division coordinate</summary>
        public int GetPixelPositionIntX(float x) { return Mathf.RoundToInt(x * pixelsPerDivision) + textureCenter.x; }
        /// <summary>Get Y pixel coordinate for the screen division coordinate</summary>
        public int GetPixelPositionIntY(float y) { return Mathf.RoundToInt(y * pixelsPerDivision) + textureCenter.y; }

        // =============================================================================================================
        // Clamped screen coordinates
        // =============================================================================================================

        /// <summary>Test if X pixel coordinate inside of screen</summary>
        public bool TestPixelInsideScreenX(int x) { return x>=0 && x<=textureSize.x; }
        /// <summary>Test if Y pixel coordinate inside of screen</summary>
        public bool TestPixelInsideScreenY(int y) { return y>=0 && y<=textureSize.y; }
        /// <summary>Clamp X pixel coordinate inside of screen</summary>
        public int ClampPixelInsideScreenX(int x) { return x<0 ? 0 : (x<textureSize.x ? x : textureSize.x-1); }
        /// <summary>Clamp Y pixel coordinate inside of screen</summary>
        public int ClampPixelInsideScreenY(int y) { return y<0 ? 0 : (y<textureSize.y ? y : textureSize.y-1); }
        
        public Vector2Int GetPixelPositionClamped(float x, float y)
        {
            var ix = Mathf.RoundToInt(x * pixelsPerDivision) + textureCenter.x;
            var iy = Mathf.RoundToInt(y * pixelsPerDivision) + textureCenter.y;
            ix = ix < 0 ? 0 : (ix < textureSize.x ? ix : textureSize.x - 1);
            iy = iy < 0 ? 0 : (iy < textureSize.y ? iy : textureSize.y - 1);
            return new Vector2Int(ix,iy);
        }
    }
}