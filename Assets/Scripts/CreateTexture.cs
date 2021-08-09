using UnityEditor;
using UnityEngine;

public class CreateTexture : MonoBehaviour
{
    [MenuItem("MyTools/CreateTexture")]
    static void DoCreateTexture()
    {
        var tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

        var pixels = new Color32[256 * 256];

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                pixels[y * 256 + x] = new Color32((byte)x, (byte)y, 0, 255);
            }
        }

        tex.SetPixels32(pixels);

        var data = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes("Assets/Textures/MyTexture.png", data);
    }

    [MenuItem("MyTools/CreateSimpleNoiseTexture")]
    static void DoCreateNoiseTexture()
    {
        var tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

        var pixels = new Color32[256 * 256];

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                byte g = ((byte)Random.Range(0, 256));
                pixels[y * 256 + x] = new Color32(g, g, g, 255);
            }
        }

        tex.SetPixels32(pixels);

        var data = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes("Assets/Textures/MySimpleNoiseTexture.png", data);
    }

    [MenuItem("MyTools/CreateGaussianBlurredSimpleNoiseTexture")]
    static void DoCreateGaussianBlurredNoiseTexture()
    {
        var tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

        var pixels = new Color32[256 * 256];

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                byte g = ((byte)Random.Range(0, 256));
                pixels[y * 256 + x] = new Color32(g, g, g, 255);
            }
        }

        Color32[] blurredPix = ApproxGaussBlur(pixels, 256, 256, 2);

        tex.SetPixels32(blurredPix);

        var data = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes("Assets/Textures/MyBlurredSimpleNoiseTexture.png", data);
    }

    [MenuItem("MyTools/CreatePerlinNoiseTexture")]
    static void DoCreatePerlinNoiseTexture()
    {
        var tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

        var pixels = new Color32[256 * 256];
        float scale = 8f;
        float xOrg = 0f;
        float yOrg = 0f;

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float xCoord = xOrg + x / 256f * scale;
                float yCoord = yOrg + y / 256f * scale;
                byte g = ((byte)(Mathf.PerlinNoise(xCoord, yCoord) * 255));
                pixels[y * 256 + x] = new Color32(g, g, g, 255);
            }
        }

        tex.SetPixels32(pixels);

        var data = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes("Assets/Textures/MyPerlinNoiseTexture.png", data);
    }

    // Based on Ivan Kutskir
    // http://blog.ivank.net/fastest-gaussian-blur.html
    static int[] BoxesForGauss(float sigma, int n)
    {
        float wIdeal = Mathf.Sqrt((12 * sigma * sigma / n) + 1);  // Ideal averaging filter width 
        int wl = Mathf.FloorToInt(wIdeal); if (wl % 2 == 0) wl--;
        int wu = wl + 2;

        float mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
        int m = Mathf.RoundToInt(mIdeal);
        // var sigmaActual = Math.sqrt( (m*wl*wl + (n-m)*wu*wu - n)/12 );
        int[] sizes = new int[n];
        for (var i = 0; i < n; i++) sizes[i] = i < m ? wl : wu;
        return sizes;
    }

    static Color32[] ApproxGaussBlur(Color32[] source, int w, int h, int r)
    {
        Color32[] result = new Color32[source.Length];
        var bxs = BoxesForGauss(r, 3);
        BoxBlur(in source, ref result, w, h, (bxs[0] - 1) / 2);
        BoxBlur(in source, ref result, w, h, (bxs[1] - 1) / 2);
        BoxBlur(in source, ref result, w, h, (bxs[2] - 1) / 2);
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                int idx = y * 256 + x;
                result[idx].g = result[idx].r;
                result[idx].b = result[idx].r;
                result[idx].a = 255;
            }
        }

        return result;
    }

    static void BoxBlur(in Color32[] scl, ref Color32[] tcl, int w, int h, int r)
    {
        for (var i = 0; i < scl.Length; i++) tcl[i] = scl[i];
        BoxBlurH(in scl, ref tcl, w, h, r);
        BoxBlurT(in scl, ref tcl, w, h, r);
    }
    static void BoxBlurH(in Color32[] scl, ref Color32[] tcl, int w, int h, int r)
    {
        float iarr = 1 / (float)(r + r + 1);
        for (var i = 0; i < h; i++)
        {
            int ti = i * w;
            int li = ti;
            int ri = ti + r;
            byte fv = scl[ti].r;
            byte lv = scl[ti + w - 1].r;
            int val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += scl[ti + j].r;
            for (var j = 0; j <= r; j++)
            {
                val += scl[ri++].r - fv;
                tcl[ti++].r = (byte)Mathf.RoundToInt(val * iarr);
            }
            for (var j = r + 1; j < w - r; j++)
            {
                val += scl[ri++].r - scl[li++].r;
                tcl[ti++].r = (byte)Mathf.RoundToInt(val * iarr);
            }
            for (var j = w - r; j < w; j++)
            {
                val += lv - scl[li++].r;
                tcl[ti++].r = (byte)Mathf.RoundToInt(val * iarr);
            }
        }
    }
    static void BoxBlurT(in Color32[] scl, ref Color32[] tcl, int w, int h, int r)
    {
        float iarr = 1 / (float)(r + r + 1);
        for (var i = 0; i < w; i++)
        {
            int ti = i;
            int li = ti;
            int ri = ti + r * w;
            byte fv = scl[ti].r;
            byte lv = scl[ti + w * (h - 1)].r;
            var val = (r + 1) * fv;
            for (var j = 0; j < r; j++) val += scl[ti + j * w].r;
            for (var j = 0; j <= r; j++)
            {
                val += scl[ri].r - fv;
                tcl[ti].r = (byte)Mathf.RoundToInt(val * iarr);
                ri += w; ti += w;
            }
            for (var j = r + 1; j < h - r; j++)
            {
                val += scl[ri].r - scl[li].r;
                tcl[ti].r = (byte)Mathf.RoundToInt(val * iarr);
                li += w; ri += w; ti += w;
            }
            for (var j = h - r; j < h; j++)
            {
                val += lv - scl[li].r;
                tcl[ti].r = (byte)Mathf.RoundToInt(val * iarr);
                li += w; ti += w;
            }
        }
    }
}
