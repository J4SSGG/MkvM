namespace DataLayer.Models;

public class MkvMetadata
{
    // Removing all properties except the ones used in the application.
    // public List<object> attachments { get; set; }
    // public List<Chapter> chapters { get; set; }
    // public Container container { get; set; }
    // public List<object> errors { get; set; }
    // public string file_name { get; set; }
    // public List<object> global_tags { get; set; }
    // public ulong identification_format_version { get; set; }
    // public List<object> track_tags { get; set; }
    public List<Track> tracks { get; set; }
    // public List<object> warnings { get; set; }
}

public class Chapter
{
    public ulong num_entries { get; set; }
}

public class Container
{
    public Properties properties { get; set; }
    public bool recognized { get; set; }
    public bool supported { get; set; }
    public string type { get; set; }
}

public class Properties
{
    public ulong container_type { get; set; }
    public DateTime date_local { get; set; }
    public DateTime date_utc { get; set; }
    public ulong duration { get; set; }
    public bool is_providing_timestamps { get; set; }
    public string muxing_application { get; set; }
    public string segment_uid { get; set; }
    public ulong timestamp_scale { get; set; }
    public string title { get; set; }
    public string writing_application { get; set; }
}

public class Track
{
    public string codec { get; set; }
    public ulong id { get; set; }
    public TrackProperties properties { get; set; }
    public string type { get; set; }
}

public class TrackProperties
{
    public ulong audio_channels { get; set; }
    public ulong audio_sampling_frequency { get; set; }
    public string codec_id { get; set; }
    public string codec_private_data { get; set; }
    public ulong codec_private_length { get; set; }
    public ulong default_duration { get; set; }
    public bool default_track { get; set; }
    public string display_dimensions { get; set; }
    public ulong display_unit { get; set; }
    public bool enabled_track { get; set; }
    public bool forced_track { get; set; }
    public string language { get; set; }
    public ulong minimum_timestamp { get; set; }
    public ulong num_index_entries { get; set; }
    public ulong number { get; set; }
    public string packetizer { get; set; }
    public string pixel_dimensions { get; set; }
    public string tag__statistics_tags { get; set; }
    public string tag__statistics_writing_app { get; set; }
    public string tag__statistics_writing_date_utc { get; set; }
    public string tag_bps { get; set; }
    public string tag_duration { get; set; }
    public string tag_number_of_bytes { get; set; }
    public string tag_number_of_frames { get; set; }
    public string track_name { get; set; }
    public ulong uid { get; set; }
    public bool text_subtitles { get; set; }
    public string encoding { get; set; }
    public string content_encoding_algorithms { get; set; }
}