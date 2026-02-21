using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    // Copy and paste for basic organization of inspector variables:
    // [Header("Inscribed References")]
    // [Header("Inscribed Settings")]
    // [Header("Dynamic References - Don't Modify In Inspector")]
    // [Header("Dynamic Settings - Don't Modify In Inspector")]
    
    #region Linear Interpolation #######################################################################################

    // The standard Vector Lerp functions in Unity don't allow for extrapolation
    //   (which is input u values <0 or >1), so we need to write our own functions
    public static Vector3 Lerp (Vector3 vFrom, Vector3 vTo, float u) {
        Vector3 res = (1-u)*vFrom + u*vTo;
        return( res );
    }
    // The same function for Vector2
    public static Vector2 Lerp (Vector2 vFrom, Vector2 vTo, float u) {
        Vector2 res = (1-u)*vFrom + u*vTo;
        return( res );
    }
    // The same function for float
    public static float Lerp (float vFrom, float vTo, float u) {
        float res = (1-u)*vFrom + u*vTo;
        return( res );
    }

    #endregion

    #region Bezier Curves ##############################################################################################

    /// <summary>
    /// While most Bézier curves are 3 or 4 points, it is possible to have
    ///   any number of points using this recursive function.
    /// LerpUnclamped is used to allow extrapolation.
    /// </summary>
    /// <param name="u">The amount of interpolation [0..1]</param>
    /// <param name="list">A List<Vector3> of points to interpolate</param>
    /// <param name="i0">The index of the left extent of the used part of the list. 
    ///   Defaults to 0.</param>
    /// <param name="i1">The index of the right extent of the used part of the list. 
    ///   Defaults to -1, which is then changed to the final element of the List.</param>
    public static Vector3 Bezier( float u, List<Vector3> list, int i0=0, int i1=-1 ) {
        // Set i1 to the last element in list
        if (i1 == -1) i1 = list.Count-1;
        // If we are only looking at one element of list, return it
        if (i0 == i1) {
            return( list[i0] );
        }
        // Otherwise, call Bezier again with all but the leftmost used element of list
        Vector3 l = Bezier(u, list, i0, i1-1);
        // And call Bezier again with all but the rightmost used element of list
        Vector3 r = Bezier(u, list, i0+1, i1);
        // The result is the Lerp of these two recursive calls to Bezier
        Vector3 res = Vector3.LerpUnclamped( l, r, u );
        return( res );
    }

    // This version allows an Array or a series of Vector3s as input
    public static Vector3 Bezier( float u, params Vector3[] vecs ) {
        return( Bezier( u, new List<Vector3>(vecs) ) );
    }


    // The same two functions for Vector2
    public static Vector2 Bezier( float u, List<Vector2> list, int i0=0, int i1=-1 ) {
        // Set i1 to the last element in list
        if (i1 == -1) i1 = list.Count-1;
        // If we are only looking at one element of list, return it
        if (i0 == i1) {
            return( list[i0] );
        }
        // Otherwise, call Bezier again with all but the leftmost used element of list
        Vector2 l = Bezier(u, list, i0, i1-1);
        // And call Bezier again with all but the rightmost used element of list
        Vector2 r = Bezier(u, list, i0+1, i1);
        // The result is the Lerp of these two recursive calls to Bezier
        Vector2 res = Vector2.LerpUnclamped( l, r, u );
        return( res );
    }

    // This version allows an Array or a series of Vector2s as input
    public static Vector2 Bezier( float u, params Vector2[] vecs ) {
        return( Bezier( u, new List<Vector2>(vecs) ) );
    }


    // The same two functions for float
    public static float Bezier( float u, List<float> list, int i0=0, int i1=-1 ) {
        // Set i1 to the last element in list
        if (i1 == -1) i1 = list.Count-1;
        // If we are only looking at one element of list, return it
        if (i0 == i1) {
            return( list[i0] );
        }
        // Otherwise, call Bezier again with all but the leftmost used element of list
        float l = Bezier(u, list, i0, i1-1);
        // And call Bezier again with all but the rightmost used element of list
        float r = Bezier(u, list, i0+1, i1);
        // The result is the Lerp of these two recursive calls to Bezier
        float res = (1-u)*l + u*r;
        return( res );
    }

    // This version allows an Array or a series of floats as input
    public static float Bezier( float u, params float[] vecs ) {
        return( Bezier( u, new List<float>(vecs) ) );
    }


    /// <summary>
    /// While most Bézier curves are 3 or 4 points, it is possible to have
    ///   any number of points using this recursive function.
    /// This uses the Utils.Lerp function rather than the built-in Vector3.Lerp 
    ///   because it needs to allow extrapolation.
    /// The 
    /// </summary>
    /// <param name="u">The amount of interpolation [0..1]</param>
    /// <param name="list">A List<Quaternion> of points to interpolate</param>
    /// <param name="i0">The index of the left extent of the used part of the list. 
    ///   Defaults to 0.</param>
    /// <param name="i1">The index of the right extent of the used part of the list. 
    ///   Defaults to -1, which is then changed to the final element of the List.</param>
    public static Quaternion Bezier( float u, List<Quaternion> list, int i0=0, int i1=-1 ) {
        // Set i1 to the last element in list
        if (i1 == -1) i1 = list.Count-1;

        // If we are only looking at one element of list, return it
        if (i0 == i1) {
            return( list[i0] );
        }

        // Otherwise, call Bezier again with all but the leftmost used element of list
        Quaternion l = Bezier(u, list, i0, i1-1);
        // And call Bezier again with all but the rightmost used element of list
        Quaternion r = Bezier(u, list, i0+1, i1);
        // The result is the Slerp (spherical lerp) of these two recursive calls to Bezier
        Quaternion res = Quaternion.SlerpUnclamped( l, r, u );

        return( res );
    }

    // This version allows an Array or a series of Quaternions as input
    public static Quaternion Bezier( float u, params Quaternion[] arr ) {
        return( Bezier( u, new List<Quaternion>(arr) ) );
    }

    #endregion

    #region Easing #####################################################################################################
    [System.Serializable]
    public class EasingCachedCurve {
        public List<string>     curves =    new List<string>();
        public List<float>      mods =      new List<float>();
    }

    public class Easing {
        public static string Linear =       ",Linear|";
        public static string In =           ",In|";
        public static string Out =          ",Out|";
        public static string InOut =        ",InOut|";
        public static string Sin =          ",Sin|";
        public static string SinIn =        ",SinIn|";
        public static string SinOut =       ",SinOut|";

        public static Dictionary<string,EasingCachedCurve> cache;
        // This is a cache for the information contained in the complex strings
        //   that can be passed into the Ease function. The parsing of these
        //   strings is most of the effort of the Ease function, so each time one
        //   is parsed, the result is stored in the cache to be recalled much 
        //   faster than a parse would take.
        // Need to be careful of memory leaks, which could be a problem if several
        //   million unique easing parameters are called

        public static float Ease( float u, params string[] curveParams ) {
            // Set up the cache for curves
            if (cache == null) {
                cache = new Dictionary<string, EasingCachedCurve>();
            }

            float u2 = u;
            foreach ( string curve in curveParams ) {
                // Check to see if this curve is already cached
                if (!cache.ContainsKey(curve)) {
                    // If not, parse and cache it
                    EaseParse(curve);
                } 
                // Call the cached curve
                u2 = EaseP( u2, cache[curve] );
            }
            return( u2 );
        }
        
        private static void EaseParse( string curveIn ) {
            EasingCachedCurve ecc = new EasingCachedCurve();
            // It's possible to pass in several comma-separated curves
            string[] curves = curveIn.Split(',');
            foreach (string curve in curves) {
                if (curve == "") continue;
                // Split each curve on | to find curve and mod
                string[] curveA = curve.Split('|');
                ecc.curves.Add(curveA[0]);
                if (curveA.Length == 1 || curveA[1] == "") {
                    ecc.mods.Add(float.NaN);
                } else {
                    float parseRes;
                    if ( float.TryParse(curveA[1], out parseRes) ) {
                        ecc.mods.Add( parseRes );
                    } else {
                        ecc.mods.Add( float.NaN );
                    }
                }   
            }
            cache.Add(curveIn, ecc);
        }
        
        
        public static float Ease( float u, string curve, float mod ) {
            return( EaseP( u, curve, mod ) );
        }
        
        private static float EaseP( float u, EasingCachedCurve ec ) {
            float u2 = u;
            for (int i=0; i<ec.curves.Count; i++) {
                u2 = EaseP( u2, ec.curves[i], ec.mods[i] );
            }
            return( u2 );
        }
        
        private static float EaseP( float u, string curve, float mod ) {
            float u2 = u;
            
            switch (curve) {
                case "In":
                    if (float.IsNaN(mod)) mod = 2;
                    u2 = Mathf.Pow(u, mod);
                    break;
                    
                case "Out":
                    if (float.IsNaN(mod)) mod = 2;
                    u2 = 1 - Mathf.Pow( 1-u, mod );
                    break;
                    
                case "InOut":
                    if (float.IsNaN(mod)) mod = 2;
                    if ( u <= 0.5f ) {
                        u2 = 0.5f * Mathf.Pow( u*2, mod );
                    } else {
                        u2 = 0.5f + 0.5f * (  1 - Mathf.Pow( 1-(2*(u-0.5f)), mod )  );
                    }
                    break;
                    
                case "Sin":
                    if (float.IsNaN(mod)) mod = 0.15f;
                    u2 = u + mod * Mathf.Sin( 2*Mathf.PI*u );
                    break;
                    
                case "SinIn":
                    // mod is ignored for SinIn
                    u2 = 1 - Mathf.Cos( u * Mathf.PI * 0.5f );
                    break;
                    
                case "SinOut":
                    // mod is ignored for SinOut
                    u2 = Mathf.Sin( u * Mathf.PI * 0.5f );
                    break;
                    
                case "Linear":
                default:
                    // u2 already equals u
                    break;
            }
            
            return( u2 );
        }
        
    }

    #endregion
    
    #region Positions ##################################################################################################
    
    public static Vector3 WorldToLocal(Vector3 worldPos, Transform parent)
    {
        return parent.InverseTransformPoint(worldPos);
    }
    
    public static Vector3 LocalToWorld(Vector3 localPos, Transform parent)
    {
        return parent.TransformPoint(localPos);
    }
    
    public enum EBoundsPos
    {
        TopLeft,
        TopCenter,
        TopRight,
        RightCenter,
        BottomRight,
        BottomCenter,
        BottomLeft,
        LeftCenter,
        Center
    }
    
    // This method is used to determine the placement of a button based on given bounds and a button rect
    // Assumes anchor is in the middle of the button rect for desired placement
    public static Vector2 BoundsButtonPlacement(Vector2 min, Vector2 max, Rect buttonRect, EBoundsPos placement)
    {
        Vector2 targetPos = Vector2.zero;
        
        switch (placement)
        {
            case EBoundsPos.TopLeft:
                targetPos = new Vector2(min.x + buttonRect.width / 2, max.y - buttonRect.height / 2);
                break;
            case EBoundsPos.TopCenter:
                targetPos = new Vector2((min.x + max.x) / 2, max.y - buttonRect.height / 2);
                break;
            case EBoundsPos.TopRight:
                targetPos = new Vector2(max.x - buttonRect.width / 2, max.y - buttonRect.height / 2);
                break;
            case EBoundsPos.RightCenter:
                targetPos = new Vector2(max.x - buttonRect.width / 2, (min.y + max.y) / 2);
                break;
            case EBoundsPos.BottomRight:
                targetPos = new Vector2(max.x - buttonRect.width / 2, min.y + buttonRect.height / 2);
                break;
            case EBoundsPos.BottomCenter:
                targetPos = new Vector2((min.x + max.x) / 2, min.y + buttonRect.height / 2);
                break;
            case EBoundsPos.BottomLeft:
                targetPos = new Vector2(min.x + buttonRect.width / 2, min.y + buttonRect.height / 2);
                break;
            case EBoundsPos.LeftCenter:
                targetPos = new Vector2(min.x + buttonRect.width / 2, (min.y + max.y) / 2);
                break;
            case EBoundsPos.Center:
                targetPos = new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2);
                break;
        }
        
        return targetPos;
    }
    
    public static bool CanConnect(Vector3 pos1, Vector3 pos2, float maxDistance, LayerMask obstacleMask)
    {
        // Check if the distance between the two positions is within the maximum distance
        if (Vector3.Distance(pos1, pos2) > maxDistance)
        {
            return false;
        }

        // Perform a raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(pos1, (pos2 - pos1).normalized, out hit, maxDistance, obstacleMask))
        {
            return false; // An obstacle was hit
        }

        return true; // No obstacles in the way
    }

    #endregion
    
    #region Vectors ####################################################################################################
    
    public static Vector3 QuatToDir(Quaternion rotation)
    {
        return rotation * Vector3.forward;
    }
    
    public static Vector3 EulerToDir(Vector3 euler)
    {
        return Quaternion.Euler(euler) * Vector3.forward;
    }

    #endregion

    #region Rotations ##################################################################################################

    public static Quaternion DirToQuat(Vector3 direction)
    {
        return Quaternion.LookRotation(direction);
    }
    
    public static Vector3 DirToEuler(Vector3 direction)
    {
        return Quaternion.LookRotation(direction).eulerAngles;
    }

    #endregion
    
    public static string FloatToStatString(float number)
        {
            if (number < 0f)
            {
                return "--:--.--";
            }
            
            int minutes = Mathf.FloorToInt(number / 60f);
                    
            int seconds = Mathf.FloorToInt(number % 60f);
                
            // showing 3 digits past decimal point
            int deciSeconds = Mathf.FloorToInt((number * 1000f) % 1000f);
                
            string minutesString = minutes < 10 ? $"0{minutes}" : minutes.ToString();
                
            string secondsString = seconds < 10 ? $"0{seconds}" : seconds.ToString();
                
            string withMinutesString = $"{minutesString}:{secondsString}.{deciSeconds:D3}";
                
            string withoutMinutesString = $"{secondsString}.{deciSeconds:D3}";
                
            if (minutes > 0)
            {
                return withMinutesString;
            }
            else
            {
                return withoutMinutesString;
            }
        }
}
