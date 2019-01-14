
using System;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using Microsoft.SqlServer.Server;

namespace Ak.CLR.BlobCompression
{

    public partial class UserDefinedFunctions
    {
        // Set the function characteristics.
        [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true,
          DataAccess = DataAccessKind.None)]
        // BEGIN CALLOUT A
        public static SqlBytes fn_compress(SqlBytes blob)
        // END CALLOUT A
        {
            if (blob.IsNull)
                return blob;
            // BEGIN CALLOUT B
            // Retrieve the BLOB's data.
            byte[] blobData = blob.Buffer;
            // END CALLOUT B
            // BEGIN CALLOUT C
            // Prepare for compression.
            MemoryStream compressedData = new MemoryStream();
            DeflateStream compressor = new DeflateStream(compressedData,
            CompressionMode.Compress, true);
            // Write the uncompressed data using a DeflateStream compressor.
            compressor.Write(blobData, 0, blobData.Length);
            // Close the compressor to allow all the compressed bytes to be written.
            compressor.Flush();
            compressor.Close();
            compressor = null;
            // END CALLOUT C
            // Return the compressed blob.
            return new SqlBytes(compressedData);
        }

        [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true,
    DataAccess = DataAccessKind.None)]
        public static SqlBytes fn_decompress(SqlBytes compressedBlob)
        {
            if (compressedBlob.IsNull)
                return compressedBlob;
            // Prepare to read the data from the compressed stream.
            DeflateStream decompressor = new DeflateStream(compressedBlob.Stream,
              CompressionMode.Decompress, true);
            // BEGIN CALLOUT A
            // Initialize the variables.
            int bytesRead = 1;
            int chunkSize = 10000;
            byte[] chunk = new byte[chunkSize];
            // Prepare the destination stream to hold the decompressed data.
            MemoryStream decompressedData = new MemoryStream();
            try
            {
                // Read from the compressed stream.
                while ((bytesRead = decompressor.Read(chunk, 0, chunkSize)) > 0)
                {
                    // Write the decompressed data.
                    decompressedData.Write(chunk, 0, bytesRead);
                }
            }
            // END CALLOUT A
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Clean up.
                decompressor.Close();
                decompressor = null;
            }
            // Return a decompressed BLOB.
            return new SqlBytes(decompressedData);
        }
    }
 
}
