﻿namespace MVsDotNetAMSIClient.Contracts.Enums
{
    public enum ContentType
    {
        /// <summary>
        /// Content is of different type than file, string or byte array
        /// </summary>
        Other,
        /// <summary>
        /// Scanned content is sequence of characters
        /// </summary>
        String, 
        /// <summary>
        /// Scanned content is array of bytes
        /// </summary>
        ByteArray,
        /// <summary>
        /// Scanned content is file
        /// </summary>
        File
    }
}
