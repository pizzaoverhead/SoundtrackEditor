// Sourced from here: http://answers.unity3d.com/questions/380838/is-there-any-converter-mp3-to-ogg-.html
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;
using System.IO;

public class MP3Import
{
    public IntPtr handle_mpg;
    public IntPtr errPtr;
    public IntPtr rate;
    public IntPtr channels;
    public IntPtr encoding;
    public IntPtr id3v1;
    public IntPtr id3v2;
    public IntPtr done;

    public string mPath;
    public int x;
    public int intRate;
    public int intChannels;
    public int intEncoding;
    public int FrameSize;
    public int lengthSamples;
    public AudioClip myClip;
    public AudioSource audioSource;

    #region Consts: Standard values used in almost all conversions.
    private const float const_1_div_128_ = 1.0f / 128.0f;  // 8 bit multiplier
    private const float const_1_div_32768_ = 1.0f / 32768.0f; // 16 bit multiplier
    private const double const_1_div_2147483648_ = 1.0 / 2147483648.0; // 32 bit
    #endregion

    public struct TrackInfo
    {
        public int Rate;
        public int Channels;
        public int Encoding;
        public string Album;
        public string Artist;
        public string Title;
        public string Year;
        public string Genre;
        public string Comment;
        public string Tag;
    }

    public TrackInfo GetTrackInfo(string path)
    {
        MPGImport.mpg123_init();
        handle_mpg = MPGImport.mpg123_new(null, errPtr);
        x = MPGImport.mpg123_open(handle_mpg, path);
        MPGImport.mpg123_getformat(handle_mpg, out rate, out channels, out encoding);
        intRate = rate.ToInt32();
        intChannels = channels.ToInt32();
        intEncoding = encoding.ToInt32();

        MPGImport.mpg123_id3(handle_mpg, out id3v1, out id3v2);
        MPGImport.mpg123_format_none(handle_mpg);
        MPGImport.mpg123_format(handle_mpg, intRate, intChannels, 208);

        if (id3v1 != IntPtr.Zero)
        {
            MPGImport.mpg123_id3v1 v1 = (MPGImport.mpg123_id3v1)Marshal.PtrToStructure(id3v1, typeof(MPGImport.mpg123_id3v1));
            return new TrackInfo
            {
                Album = new String(v1.album),	    // "Runnin' Wild"
                Artist = new String(v1.artist),	    // "Airbourne"
                Title = new String(v1.title),	    // "Stand Up For Rock N Roll"
                Year = new String(v1.year), 	    // "2007"
                Genre = (v1.genre < GenreText.Length ? GenreText[v1.genre] : string.Empty), // "Rock"
                Comment = new String(v1.comment),    // "Comment"
                Tag = new String(v1.tag),    // "Comment"
                Rate = intRate,
                Channels = intChannels,
                Encoding = intEncoding
            };
        }
        MPGImport.mpg123_close(handle_mpg);

        // TODO: ID3v2 support is more complex.
        return new TrackInfo();
    }

    public AudioClip StartImport(string mPath)
    {
        MPGImport.mpg123_init();
        handle_mpg = MPGImport.mpg123_new(null, errPtr);
        try
        {
            x = MPGImport.mpg123_open(handle_mpg, mPath);
            MPGImport.mpg123_getformat(handle_mpg, out rate, out channels, out encoding);
            intRate = rate.ToInt32();
            intChannels = channels.ToInt32();
            intEncoding = encoding.ToInt32();
            MPGImport.mpg123_id3(handle_mpg, out id3v1, out id3v2);
            MPGImport.mpg123_format_none(handle_mpg);
            MPGImport.mpg123_format(handle_mpg, intRate, intChannels, 208);

            string title;
            if (id3v1 != IntPtr.Zero)
            {
                Debug.Log("Getting ID3 info");
                MPGImport.mpg123_id3v1 v1 = (MPGImport.mpg123_id3v1)Marshal.PtrToStructure(id3v1, typeof(MPGImport.mpg123_id3v1));
                title = new String(v1.title);
            }
            else
            {
                title = Path.GetFileNameWithoutExtension(mPath);
            }

            FrameSize = MPGImport.mpg123_outblock(handle_mpg);
            byte[] Buffer = new byte[FrameSize];
            lengthSamples = MPGImport.mpg123_length(handle_mpg);

            Debug.Log("Creating audio clip");
            myClip = AudioClip.Create(title, lengthSamples, intChannels, intRate, false);

            int importIndex = 0;

            while (0 == MPGImport.mpg123_read(handle_mpg, Buffer, FrameSize, out done))
            {
                float[] fArray;
                fArray = ByteToFloat(Buffer);
                float offset = (importIndex * fArray.Length) / 2;
                if (offset > lengthSamples)
                {
                    Debug.LogWarning("[STED] MP3 file " + mPath + " is of an unexpected length and was truncated.");
                    break; // File was reported as shorter than it is. Salvage what we have and return.
                }
                myClip.SetData(fArray, (int)offset);
                importIndex++;
            }
        }
        catch (Exception ex)
        {
            // Attempt to dump any used memory before continuing.
            // TODO: Still holds onto memory when repeatedy failing.
            myClip.UnloadAudioData();
            myClip = null;
            throw ex;
        }
        finally
        {
            MPGImport.mpg123_close(handle_mpg);
        }
        return myClip;
    }

    public float[] IntToFloat(Int16[] from)
    {
        float[] to = new float[from.Length];

        for (int i = 0; i < from.Length; i++)
            to[i] = (float)(from[i] * const_1_div_32768_);

        return to;
    }

    public Int16[] ByteToInt16(byte[] buffer)
    {
        Int16[] result = new Int16[1];
        int size = buffer.Length;
        if ((size % 2) != 0)
        {
            /* Error here */
            Console.WriteLine("error");
            return result;
        }
        else
        {
            result = new Int16[size / 2];
            IntPtr ptr_src = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr_src, size);
            Marshal.Copy(ptr_src, result, 0, result.Length);
            Marshal.FreeHGlobal(ptr_src);
            return result;
        }
    }

    public float[] ByteToFloat(byte[] bArray)
    {
        Int16[] iArray;
        iArray = ByteToInt16(bArray);
        return IntToFloat(iArray);
    }

    // Source: https://de.wikipedia.org/wiki/Liste_der_ID3v1-Genres
    public static readonly string[] GenreText = new string[] {
        "Blues",
        "Classic Rock",
        "Country",
        "Dance",
        "Disco",
        "Funk",
        "Grunge",
        "Hip-Hop",
        "Jazz",
        "Metal",
        "New Age",
        "Oldies",
        "Other",
        "Pop",
        "Rhythm and Blues",
        "Rap",
        "Reggae",
        "Rock",
        "Techno",
        "Industrial",
        "Alternative",
        "Ska",
        "Death Metal",
        "Pranks",
        "Soundtrack",
        "Euro-Techno",
        "Ambient",
        "Trip-Hop",
        "Vocal",
        "Jazz & Funk",
        "Fusion",
        "Trance",
        "Classical",
        "Instrumental",
        "Acid",
        "House",
        "Game",
        "Sound Clip",
        "Gospel",
        "Noise",
        "Alternative Rock",
        "Bass",
        "Soul",
        "Punk",
        "Space",
        "Meditative",
        "Instrumental Pop",
        "Instrumental Rock",
        "Ethnic",
        "Gothic",
        "Darkwave",
        "Techno-Industrial",
        "Electronic",
        "Pop-Folk",
        "Eurodance",
        "Dream",
        "Southern Rock",
        "Comedy",
        "Cult",
        "Gangsta",
        "Top 40",
        "Christian Rap",
        "Pop/Funk",
        "Jungle",
        "Native US",
        "Cabaret",
        "New Wave",
        "Psychedelic",
        "Rave",
        "Showtunes",
        "Trailer",
        "Lo-Fi",
        "Tribal",
        "Acid Punk",
        "Acid Jazz",
        "Polka",
        "Retro",
        "Musical",
        "Rock ’n’ Roll",
        "Hard Rock"
    };
}
