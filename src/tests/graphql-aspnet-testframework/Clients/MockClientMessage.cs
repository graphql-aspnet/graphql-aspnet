// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.Clients
{
    using System;
    using System.Text;
    using GraphQL.AspNet.Connections.Clients;

    /// <summary>
    /// A fake message mimicing what would be generating when
    /// a client connection recieves data form its underlying implementaiton.
    /// </summary>
    public class MockClientMessage
    {
        private int _lastIndexRead = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientMessage" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="isEndOfMessage">The is end of message.</param>
        /// <param name="closeStatus">The close status.</param>
        /// <param name="closeDescription">The close description.</param>
        public MockClientMessage(
                    byte[] data,
                    ClientMessageType messageType,
                    bool isEndOfMessage = false,
                    ClientConnectionCloseStatus? closeStatus = null,
                    string closeDescription = null)
        {
            this.Data = data;
            this.MessageType = messageType;
            this.CloseStatus = closeStatus;
            this.CloseStatusDescription = closeDescription;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientMessage" /> class.
        /// </summary>
        /// <param name="data">The text data to encode for the message.</param>
        public MockClientMessage(string data)
        {
            this.Data = Encoding.UTF8.GetBytes(data);
            this.MessageType = ClientMessageType.Text;
            this.CloseStatus = null;
            this.CloseStatusDescription = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockClientMessage" /> class.
        /// </summary>
        /// <param name="data">The data to serialize as text for the message content.</param>
        public MockClientMessage(object data)
        {
            var messageSerialized = System.Text.Json.JsonSerializer.Serialize(data, data.GetType());
            this.Data = Encoding.UTF8.GetBytes(messageSerialized);
            this.MessageType = ClientMessageType.Text;
            this.CloseStatus = null;
            this.CloseStatusDescription = null;
        }

        /// <summary>
        /// Converts the data raw data stream to a string, with the assumption the raw data
        /// is UTF8 encoded.
        /// </summary>
        /// <returns>System.String.</returns>
        public string ConvertDataToUTF8()
        {
            if (this.Data == null)
                return null;

            return Encoding.UTF8.GetString(this.Data);
        }

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake, if it was closed. Null otherwise.
        /// </summary>
        /// <value>The close status that was provided, if any.</value>
        public ClientConnectionCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets an optional description that describes why the close handshake has been
        /// initiated by the remote endpoint.
        /// </summary>
        /// <value>The close status description.</value>
        public string CloseStatusDescription { get; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        public byte[] Data { get; }

        /// <summary>
        /// Gets a value indicating whether the current message is a UTF-8 message or a binary message.
        /// </summary>
        /// <value>The type of the message that was recieved.</value>
        public ClientMessageType MessageType { get; }

        /// <summary>
        /// Reads the next bytes from the data array.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="bytesRead">The total bytes read.</param>
        /// <returns><c>true</c> if there are more bytes to read, <c>false</c> otherwise.</returns>
        internal bool ReadNextBytes(ArraySegment<byte> buffer, out int bytesRead)
        {
            var lengthRemaining = this.Data.Length - _lastIndexRead;
            if (lengthRemaining <= buffer.Count)
            {
                Array.Copy(this.Data, _lastIndexRead, buffer.Array, 0, lengthRemaining);
                _lastIndexRead = this.Data.Length;
                bytesRead = lengthRemaining;
                return false;
            }
            else
            {
                lengthRemaining = buffer.Count;
                Array.Copy(this.Data, _lastIndexRead, buffer.Array, 0, lengthRemaining);
                _lastIndexRead += lengthRemaining;
                bytesRead = lengthRemaining;
                return true;
            }
        }
    }
}