namespace MostlyLucid.EufySecurity.P2P;

/// <summary>
/// Metadata for P2P video/audio stream
/// </summary>
public class StreamMetadata
{
    /// <summary>
    /// Video codec
    /// </summary>
    public VideoCodec VideoCodec { get; set; }

    /// <summary>
    /// Audio codec
    /// </summary>
    public AudioCodec AudioCodec { get; set; }

    /// <summary>
    /// Video width in pixels
    /// </summary>
    public int VideoWidth { get; set; }

    /// <summary>
    /// Video height in pixels
    /// </summary>
    public int VideoHeight { get; set; }

    /// <summary>
    /// Video frames per second
    /// </summary>
    public int VideoFps { get; set; }

    /// <summary>
    /// Audio sample rate
    /// </summary>
    public int AudioSampleRate { get; set; }

    /// <summary>
    /// Audio channels
    /// </summary>
    public int AudioChannels { get; set; }
}

/// <summary>
/// Video codec types
/// </summary>
public enum VideoCodec
{
    Unknown = 0,
    H264 = 1,
    H265 = 2
}

/// <summary>
/// Audio codec types
/// </summary>
public enum AudioCodec
{
    Unknown = 0,
    AAC = 1,
    PCM = 2,
    G711 = 3
}
