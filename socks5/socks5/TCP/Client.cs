public class Client : System.IO.Stream
    {
        public event EventHandler<ClientEventArgs> onClientDisconnected;

        public event EventHandler<DataEventArgs> onDataReceived = delegate { };
        public event EventHandler<DataEventArgs> onDataSent = delegate { };

        public Socket Sock { get; set; }

        public override bool CanRead
        {
            get
            {
                return Sock.Connected;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return Sock.Connected;
            }
        }

        public override long Length
        {
            get
            {
                return 0;
            }
        }

        public override long Position
        {
            get
            {
                return 0;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        private byte[] buffer;
        private int packetSize = 4096;
        public bool Receiving = false;

        public Client(Socket sock, int PacketSize)
        {
            //start the data exchange.
            Sock = sock;
            onClientDisconnected = delegate { };
            buffer = new byte[PacketSize];
            packetSize = PacketSize;
            sock.ReceiveBufferSize = PacketSize;
        }

        private void DataReceived(IAsyncResult res)
        {
            Receiving = false;
            try
            {
                SocketError err = SocketError.Success;
                if(disposed)
                    return;
                int received = ((Socket)res.AsyncState).EndReceive(res, out err);
                if (received <= 0 || err != SocketError.Success)
                {
                    this.Disconnect();
                    return;
                }
                DataEventArgs data = new DataEventArgs(this, buffer, received);
                this.onDataReceived(this, data);
            }
            catch (Exception ex)
            {
                #if DEBUG
 #if DEBUG
 Console.WriteLine(ex.ToString()); 
#endif 
#endif
                this.Disconnect();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Receive(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Send(buffer, offset, count);
        }

        public int Receive(byte[] data, int offset, int count)
        {
            try
            {
                int received = this.Sock.Receive(data, offset, count, SocketFlags.None);
                if (received <= 0)
                {
                    this.Disconnect();
                    return -1;
                }
                DataEventArgs dargs = new DataEventArgs(this, data, received);
                //this.onDataReceived(this, dargs);
                return received;
            }
            catch (Exception ex)
            {
                #if DEBUG
  Console.WriteLine(ex.ToString()); 
#endif 
                this.Disconnect();
                return -1;
            }
        }
        
        public void ReceiveAsync(int buffersize = -1)
        {
            try
            {
                if (buffersize > -1)
                {
                    buffer = new byte[buffersize];
                }
                Receiving = true;
                Sock.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(DataReceived), Sock);
            }
            catch(Exception ex)
            {
                #if DEBUG
 Console.WriteLine(ex.ToString()); 
#endif
                this.Disconnect();
            }
        }


        public void Disconnect()
        {
            try
            {
                //while (Receiving) Thread.Sleep(10);
                if (!this.disposed)
                {
                    if (this.Sock != null && this.Sock.Connected)
                    {
                        onClientDisconnected(this, new ClientEventArgs(this));
                        this.Sock.Close();
                        //this.Sock = null;
                        return;
                    }
                    else
                        onClientDisconnected(this, new ClientEventArgs(this));
                    this.Dispose();
                }
            }
            catch { }
        }

        private void DataSent(IAsyncResult res)
        {
            try
            {
                int sent = ((Socket)res.AsyncState).EndSend(res);
                if (sent < 0)
                {
                    this.Sock.Shutdown(SocketShutdown.Both);
                    this.Sock.Close();
                    return;
                }
                DataEventArgs data = new DataEventArgs(this, new byte[0] {}, sent);
                this.onDataSent(this, data);
            }
            catch (Exception ex) {
#if DEBUG
 Console.WriteLine(ex.ToString()); 
#endif 
            }
        }

        public bool Send(byte[] buff)
        {
            return Send(buff, 0, buff.Length);
        }

        public void SendAsync(byte[] buff, int offset, int count)
        {
            try
            {
                if (this.Sock != null && this.Sock.Connected)
                {
                    this.Sock.BeginSend(buff, offset, count, SocketFlags.None, new AsyncCallback(DataSent), this.Sock);
                }
            }
            catch (Exception ex)
            {
                #if DEBUG
 Console.WriteLine(ex.ToString()); 
#endif
                this.Disconnect();
            }
        }

        public bool Send(byte[] buff, int offset, int count)
        {
            try
            {
                if (this.Sock != null)
                {
                    if (this.Sock.Send(buff, offset, count, SocketFlags.None) <= 0)
                    {
                        this.Disconnect();
                        return false;
                    }
                    DataEventArgs data = new DataEventArgs(this, buff, count);
                    this.onDataSent(this, data);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                #if DEBUG
 #if DEBUG
 Console.WriteLine(ex.ToString()); 
#endif 
#endif
                this.Disconnect();
                return false;
            }
        }
        bool disposed = false;

        // Protected implementation of Dispose pattern. 
        protected override void Dispose(bool disposing)
        {

            if (disposed)
                return;

            disposed = true;

            if (disposing)
            {
                // Free any other managed objects here. 
                //
                Sock = null;
                buffer = null;
                onClientDisconnected = null;
                onDataReceived = null;
                onDataSent = null;
            }

            // Free any unmanaged objects here. 
            //
            
        }

        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
