namespace raceday_api.database
{//start of namespace
    //checks an uploaded image before it is stored, returns null when it is fine
    public static class image_rules
    {//start of class

        public const long max_bytes = 5 * 1024 * 1024; //5 MB

        public static string? check(IFormFile? file)
        {//start of method
            if (file == null || file.Length == 0) return "Please choose an image to upload.";
            if (file.Length > max_bytes) return "The image is too big (max 5 MB).";

            string type = (file.ContentType ?? "").ToLowerInvariant();
            if (type != "image/jpeg" && type != "image/png" && type != "image/gif" && type != "image/webp")
                return "Only JPG, PNG, GIF or WebP images are allowed.";

            //look at the first bytes of the file to make sure it really is an image
            byte[] head = new byte[12];
            int read;
            using (Stream stream = file.OpenReadStream())
            {
                read = stream.Read(head, 0, head.Length);
            }
            if (read < 12) return "That file is not a valid image.";

            bool is_jpeg = head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
            bool is_png = head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
            bool is_gif = head[0] == 0x47 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x38;
            bool is_webp = head[0] == 0x52 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x46
                        && head[8] == 0x57 && head[9] == 0x45 && head[10] == 0x42 && head[11] == 0x50;

            bool matches_type = (type == "image/jpeg" && is_jpeg)
                             || (type == "image/png" && is_png)
                             || (type == "image/gif" && is_gif)
                             || (type == "image/webp" && is_webp);
            if (!matches_type) return "That file is not a valid image.";
            return null;
        }//end of method

        public static string extension(string content_type)
        {//start of method
            switch (content_type.ToLowerInvariant())
            {
                case "image/jpeg": return ".jpg";
                case "image/png": return ".png";
                case "image/gif": return ".gif";
                case "image/webp": return ".webp";
                default: return ".img";
            }
        }//end of method

    }//end of class
}//end of namespace
