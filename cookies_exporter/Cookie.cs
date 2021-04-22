namespace cookies_exporter
{
    public class Cookie
    {
        public long creation_utc { get; set; }
        public string host_key { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string path { get; set; }
        public long expires_utc { get; set; }
        public long is_secure { get; set; }
        public long is_httponly { get; set; }
        public long last_access_utc { get; set; }
        public long has_expires { get; set; }
        public long is_persistent { get; set; }
        public long priority { get; set; }
        public byte[] encrypted_value { get; set; }
        public string decrypted_value { get; set; }
        public long samesite { get; set; }
        public long source_scheme { get; set; }
    }
}