/// Сервис
/// 
/// Создатель: Андрей Стельмах
/// 2012 год
///-----------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;

namespace BuroStoreService
{
    //TODO: сделан public в целях отладки
    public class BuroStoreService : ServiceBase, IDisposable
    {
		/// <summary>
		/// список подключившихся клиентов telnet, всем им раздаются логи
		/// </summary>
		List<Socket> _telnetClients = new List<Socket>();
        System.Threading.ManualResetEvent _readyToUseTelnetClientsList = new System.Threading.ManualResetEvent(true);

        public static void Main()
        {
            ServiceBase.Run(new BuroStoreService());

        }

		/// <summary>
		/// счетчик входящих запросов для нумерации их в логах
		/// </summary>
        int _requestCounter = 0;

        public class BuroStoreServiceEventArgs : EventArgs
        {
            public enum EventType { Error, Info, Warning };

            public EventType Type;

            public string Message;

        }

        public event EventHandler<BuroStoreService.BuroStoreServiceEventArgs> OnEvent;

        //int _maxEventMessageLength = 1000;

        void FireEvent(BuroStoreServiceEventArgs.EventType type, string message)
        {
            if (this.OnEvent != null)
            {
                this.OnEvent(null, new BuroStoreServiceEventArgs() { Type = type, Message = message });

            }

        }

        //void FireEvent(BuroStoreServiceEventArgs.EventType type, byte[] bytes, int length)
        //{
        //    if (OnEvent != null)
        //    {
        //        OnEvent
        //        (
        //            null,
        //            new BuroStoreServiceEventArgs()
        //            {
        //                Type = type,
        //                Message
        //                    = this._encoding1251.GetString
        //                    (
        //                        bytes,
        //                        0,
        //                        this._maxEventMessageLength > length
        //                            ? length
        //                            : this._maxEventMessageLength
        //                    )
        //            }
        //        );

        //        return;

        //        StringBuilder sb = new StringBuilder();

        //        for(int i = 0; i < length; i++)
        //        {
        //            if (0x20 <= bytes[i] && bytes[i] <= 0xFF)
        //            {
        //                sb.Append(this._encoding1251.GetString(bytes, i, 1));

        //            }
        //            else
        //            {
        //                sb.Append("<");
        //                sb.Append(bytes[i].ToString("X2"));
        //                sb.Append(">");

        //            }

        //        }

        //        OnEvent(null, new BuroStoreServiceEventArgs() { Type = type, Message = sb.ToString() });

        //    }

        //}


        string _connectionString
        {
            get
            {
                //TODO: рефакторинг
                //System.Data.SqlClient.SqlConnectionStringBuilder cb = new System.Data.SqlClient.SqlConnectionStringBuilder();
                //cb.ApplicationName = AppDomain.CurrentDomain.FriendlyName;
                //cb.IntegratedSecurity = true;
                //cb.DataSource = "B2";
                //cb.InitialCatalog = "TradeCS";

                //return cb.ConnectionString;

                return Properties.Settings.Default.ConnectionString;

            }

        }

        //сделан public чтобы можно было управлять из консольного проекта, а не только из сервиса
        /// <summary>
		/// фоновой поток в котором непрерывно прослушиваются порты (рабочий и для логов)
		/// </summary>
        public System.ComponentModel.BackgroundWorker _tcpServer
            = new System.ComponentModel.BackgroundWorker()
            { WorkerSupportsCancellation = true };

		/// <summary>
		/// как только сокет прослушиватель ставится в режим BeginAccept, данный флаг сбрасывается,
		/// чтобы не происходило повторных вызовов BeginAccept, после EndAccept флаг поднимается
		/// как знак того что фоновой поток может вызвать BeginAccept
		/// </summary>
		System.Threading.ManualResetEvent _readyToAccept = new System.Threading.ManualResetEvent(true);

		/// <summary>
		/// то же что и ReadyToAccept, но для сокета по которому подключаются Telnet-ы
		/// </summary>
		System.Threading.ManualResetEvent _readyToAcceptTelnet = new System.Threading.ManualResetEvent(true);

        Encoding _encoding1251 = Encoding.GetEncoding(1251);
        Encoding _encoding866 = Encoding.GetEncoding(866);

		/// <summary>
		/// ресурсы которые публикует сервис
		/// </summary>
        //Dictionary<string, byte[]> Resources = new Dictionary<string, byte[]>();

        SqlCommandManager _sqlCommandManager;

        SharedFileResourceManager _resourceCache = new SharedFileResourceManager();

        public BuroStoreService()
        {
            this.ServiceName = "BuroService";

            #region
            this.AutoLog = false;

            //TODO: требуется повторный запуск после создания источника
            //нужно это как-то донести до пользователя

            if (!System.Diagnostics.EventLog.SourceExists("Бюрократ WMS 2"))
            {
                System.Diagnostics.EventLog.CreateEventSource("Бюрократ WMS 2", "Бюрократ WMS 2");

            }

            #endregion

            this._sqlCommandManager
                = new SqlCommandManager()
                {
                    ConnectionString = this._connectionString
                };

            _tcpServer.DoWork += new System.ComponentModel.DoWorkEventHandler(TcpServer_Listener);

            this.OnEvent += new EventHandler<BuroStoreServiceEventArgs>(BuroStoreService_TelnetLogging);

            this.OnEvent += new EventHandler<BuroStoreServiceEventArgs>(BuroStoreService_WindowsEventLog);

        }

		/// <summary>
		/// обработчик события, раздает событие всем подключенным сессиям telnet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BuroStoreService_TelnetLogging(object sender, BuroStoreService.BuroStoreServiceEventArgs e)
		{
			//окна windows показывают в кодировке 866
			//http://ru.wikipedia.org/wiki/%D0%90%D0%BB%D1%8C%D1%82%D0%B5%D1%80%D0%BD%D0%B0%D1%82%D0%B8%D0%B2%D0%BD%D0%B0%D1%8F_%D0%BA%D0%BE%D0%B4%D0%B8%D1%80%D0%BE%D0%B2%D0%BA%D0%B0
			byte [] buffer
                = this._encoding866.GetBytes
                (
                    (e.Type != BuroStoreServiceEventArgs.EventType.Info
                        ? e.Type.ToString()
                        : ""
                    )
                    + e.Message
                );

			//сокеты telnet при обращении к которым возникли проблемы
			//нельзя удалять элементы из списка в процессе прохода по нему в цикле foreach
			//поэтому я сперва аккумулирую проблемные сокеты в этом списке
			List<Socket> brokenSockets = new List<Socket>();

            //почему 3000? А почему бы и нет?
            if (_readyToUseTelnetClientsList.WaitOne(3000))
            {
                //публикация в telnet и еще что-нибудь
                foreach (Socket s in this._telnetClients)
                {
                    try
                    {
                        s.Send(buffer);

                        s.Send(this._encoding866.GetBytes("\r\n"));

                    }
                    catch
                    {
                        brokenSockets.Add(s);

                    }

                }

                foreach (Socket s in brokenSockets)
                {
                    this._telnetClients.Remove(s);

                }

                _readyToUseTelnetClientsList.Set();

            }

		}

        void BuroStoreService_WindowsEventLog(object sender, BuroStoreService.BuroStoreServiceEventArgs e)
        {
            if (e.Type == BuroStoreServiceEventArgs.EventType.Error
                || e.Type == BuroStoreServiceEventArgs.EventType.Warning)
            {
                System.Diagnostics.EventLog log = new System.Diagnostics.EventLog("Бюрократ WMS 2", ".", "Бюрократ WMS 2");

                if (e.Type == BuroStoreServiceEventArgs.EventType.Error)
                {

                    log.WriteEntry(e.Message, System.Diagnostics.EventLogEntryType.Error);

                }
                else if (e.Type == BuroStoreServiceEventArgs.EventType.Warning)
                {
                    log.WriteEntry(e.Message, System.Diagnostics.EventLogEntryType.Warning);

                }

                log.Close();

                log.Dispose();

            }

        }

        protected override void OnStart(string[] args)
		{
			base.OnStart(args);

			_tcpServer.RunWorkerAsync();

		}

		protected override void OnStop()
		{
			base.OnStop();

			_tcpServer.CancelAsync();

		}

		/// <summary>
		/// вертится в фоновом потоке,
		/// по мере необходимости переводит прослушивающие сокеты (всего их два)
		/// в режим BeginAccept
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void TcpServer_Listener(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
            try
            {
                using (Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    using (Socket telnet_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        listener.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, Properties.Settings.Default.ClientPort));

                        telnet_listener.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, Properties.Settings.Default.TelnetPort));

                        //TODO: хардкод
                        listener.Listen(1000);

                        //TODO: хардкод
                        telnet_listener.Listen(1000);

                        listener.BeginAccept(new AsyncCallback(this.TcpServer_RequestServer), listener);
                        telnet_listener.BeginAccept(new AsyncCallback(this.TcpServer_TelnetRequestServer), telnet_listener);


                        while (!_tcpServer.CancellationPending)
                        {
                            if (this._readyToAccept.WaitOne(2))
                            //if (this.ReadyToAccept)
                            {
                                this._readyToAccept.Reset();

                                listener.BeginAccept(new AsyncCallback(this.TcpServer_RequestServer), listener);

                            }

                            if (this._readyToAcceptTelnet.WaitOne(2))
                            //if (this.ReadyToAcceptTelnet)
                            {
                                this._readyToAcceptTelnet.Reset();

                                telnet_listener.BeginAccept(new AsyncCallback(this.TcpServer_TelnetRequestServer), telnet_listener);

                            }

                            //периодически запускаю очистку кеша
                            this._resourceCache.RemoveObsolete();

                            //TODO: хардкод
                            //System.Threading.Thread.Sleep(200);

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                this.FireEvent
                (
                    BuroStoreService.BuroStoreServiceEventArgs.EventType.Error,
                    ex.Message + " " + ex.StackTrace
                );

            }

		}

		/// <summary>
		/// обработчик входящих запросов терминалов. Здесь "бизнес-логика"
		/// </summary>
		/// <param name="ar"></param>
        void TcpServer_RequestServer(IAsyncResult ar)
        {
            string rawCommandString = "[не установлено]";

            LogMessage logMessage = new LogMessage();

            try
            {
                Socket listen_socket = (Socket)ar.AsyncState;

                Socket local = listen_socket.EndAccept(ar);
                this._readyToAccept.Set();

                //сокет будет закрыт с задержкой, чтобы все отправляемые данные были отправлены
                //размер задержки см. ниже local.Close(..)
                local.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, true);

                int requestNumber = System.Threading.Interlocked.Increment(ref _requestCounter);

                using (NetworkStream networkStream = new NetworkStream(local, false))
                {
                    //                if (d.DataAvailable)
                    //try
                    //{
                    //TODO: магическое число
                    byte[] buffer = new byte[2000];

                    int nbytes = networkStream.Read(buffer, 0, buffer.Length);

                    //TODO: как быть если одним единственным чтением не обойдется
                    //FireEvent
                    //(
                    //    BuroStoreServiceEventArgs.EventType.Info,
                    //    "------------------------------"
                    //);

                    //FireEvent
                    //(
                    //    BuroStoreServiceEventArgs.EventType.Info,
                    //    buffer,
                    //    nbytes
                    //);

                    //FireEvent
                    //(
                    //    BuroStoreServiceEventArgs.EventType.Info,
                    //    ""
                    //);

                    rawCommandString = this._encoding1251.GetString(buffer, 0, nbytes);

                    logMessage.AppendLine(rawCommandString);

                    XmlTranslator clientTranslator;

                    //TODO: модифай
                    //if (!rawCommandString.Contains("<session>"))
                    //{
                    //    rawCommandString = "<session>" + rawCommandString + "</session>";

                    //}

                    //TODO: пока сосуществуют два способа обращения к сервису: команда с разделителями, команда в xml-форматировании
                    //try
                    {
                        clientTranslator = new XmlTranslator(rawCommandString.Replace("\0", ""), this._sqlCommandManager, this._resourceCache);

                    }
                    //catch
                    //{   //похоже запрос пришел в старом виде
                    //    clientTranslator = new OldTranslator(rawCommandString, this._sqlCommandManager);

                    //}

                    byte[] rawResponse;
                    long responseLength;
                    Translator.ResponseType responseType;

                    try
                    {
                        rawResponse = clientTranslator.GetBytes(out responseLength, out responseType);

                        if (responseType == Translator.ResponseType.Binary)
                        {
                            logMessage.AppendLine("[Двоичные данные]");

                        }
                        else if (responseType == Translator.ResponseType.string1251)
                        {
                            logMessage.AppendLine(this._encoding1251.GetString(rawResponse, 0, (int)responseLength));

                        }

                        //FireEvent
                        //(
                        //    BuroStoreServiceEventArgs.EventType.Info,
                        //    rawResponse,
                        //    (int)responseLength
                        //);

                        //если используется XmlTranslator, то предварять длиной и контрольной суммой
                        if (clientTranslator is XmlTranslator)
                        {
                            networkStream.Write(BitConverter.GetBytes((Int32)responseLength), 0, sizeof(Int32));
                            //TODO: зарезервировал место под контрольную сумму
                            networkStream.Write(BitConverter.GetBytes((Int32)0x0E0E), 0, sizeof(Int32));

                        }

                        networkStream.Write(rawResponse, 0, (int)responseLength);

                        #region Логирование процедурой term.ScanerSaveCommand2
                        SqlCommand cmdLog = this._sqlCommandManager[Properties.Settings.Default.LogProcedureName];

                        cmdLog.Connection = new SqlConnection(this._sqlCommandManager.ConnectionString);

                        try
                        {
                            cmdLog.Connection.Open();

                            cmdLog.Parameters["@procedurename"].Value = clientTranslator.Name;
                            cmdLog.Parameters["@procedureresult"].Value = clientTranslator.Return;
                            //cmdLog.Parameters["@termid"].Value = clientTranslator.TermID;
                            cmdLog.Parameters["@terminalserial"].Value = clientTranslator.TerminalSerial;
                            cmdLog.Parameters["@userid"].Value = clientTranslator.UserID;
                            cmdLog.Parameters["@count"].Value = clientTranslator.QueryCounterValue;

                            clientTranslator.PlaceParametersInSqlCommand(cmdLog);

                            cmdLog.ExecuteNonQuery();

                        }
                        finally
                        {
                            cmdLog.Connection.Close();
                            cmdLog.Connection.Dispose();

                        }

                        #endregion

                        FireEvent
                        (
                            BuroStoreServiceEventArgs.EventType.Info,
                            logMessage.ToString()

                        );

                    }
                    catch (Exception e)
                    {
                        this.FireEvent
                        (
                            BuroStoreService.BuroStoreServiceEventArgs.EventType.Error,
                            string.Format
                            (
                                "{0}\r\n---------\r\n{1}\r\n{2}",
                                rawCommandString,
                                e.Message,
                                e.StackTrace
                            )
                        );

                    }


                        #region Может пригодится когда-нибудь
                        //TODO: магическое число
                        //byte[] memBuffer = new byte[10000];

                        //StringBuilder xmlResponse = new StringBuilder();

                        //начальный размер 1000 байт
                        //using (MemoryStream memStream = new MemoryStream(1000))
                        //{
                        //    StreamWriter responseWriter
                        //        = new StreamWriter(memStream, this._encoding1251) { AutoFlush = true };
                        //    {
                        //        ClientResponse response = new ClientResponse(clientCommand, this._sqlCommandManager);

                        //        response.GetBytes();

                        //        //----------

                        //        SqlCommand cmd = this._sqlCommandManager[clientCommand.Name];

                        //        cmd.Connection = new SqlConnection(this._connectionString);
                        //        cmd.Connection.Open();

                        //        try
                        //        {

                        //            foreach (string paramName in clientCommand.Parameters.Keys)
                        //            {
                        //                if (cmd.Parameters.Contains(paramName))
                        //                {
                        //                    cmd.Parameters[paramName].Value = clientCommand.Parameters[paramName];

                        //                }

                        //            }

                        //            //TODO: проверить в каком режиме пришел запрос: в старом или в новом
                        //            if (!clientCommand.ПрежнийРежим)
                        //            {
                        //                SqlDataReader reader = cmd.ExecuteReader();

                        //                if (reader.HasRows)
                        //                {
                        //                    responseWriter.Write("<recordset>");

                        //                    while (reader.Read())
                        //                    {
                        //                        responseWriter.Write("<r>");

                        //                        for (int i = 0; i < reader.FieldCount; i++)
                        //                        {
                        //                            responseWriter.Write(string.Format("<{0}>{1}</{0}>", reader.GetName(i), reader.GetValue(i).ToString()));

                        //                        }

                        //                        responseWriter.Write("</r>");

                        //                    }

                        //                    responseWriter.Write("</recordset>");

                        //                }

                        //                reader.Close();

                        //                cmd.Connection.Close();
                        //                cmd.Connection.Dispose();

                        //                //TODO: эту секцию можно вовсе не выводить,
                        //                //если out-параметры отсутствуют
                        //                responseWriter.Write("<outparams>");

                        //                for (int i = 0; i < cmd.Parameters.Count; i++)
                        //                {
                        //                    if
                        //                    (cmd.Parameters[i].Direction == System.Data.ParameterDirection.InputOutput
                        //                        || cmd.Parameters[i].Direction == System.Data.ParameterDirection.Output
                        //                    )
                        //                    {
                        //                        responseWriter.Write("<p>");

                        //                        responseWriter.Write(cmd.Parameters[i].Value.ToString());

                        //                        responseWriter.Write("</p>");

                        //                    }

                        //                }

                        //                responseWriter.Write("</outparams>");

                        //            }
                        //            else
                        //            {
                        //                cmd.Connection.Close();
                        //                cmd.Connection.Dispose();

                        //                //флажок нужен чтобы перед первым параметром 0x1E не вставился
                        //                bool firstParam = true;
                        //                for (int i = 0; i < cmd.Parameters.Count; i++)
                        //                {
                        //                    if
                        //                    (cmd.Parameters[i].Direction == System.Data.ParameterDirection.InputOutput
                        //                        || cmd.Parameters[i].Direction == System.Data.ParameterDirection.Output
                        //                    )
                        //                    {
                        //                        if (!firstParam)
                        //                        {
                        //                            //разделитель
                        //                            responseWriter.Write(0x1E);

                        //                        }

                        //                        responseWriter.Write(cmd.Parameters[i].Value.ToString());

                        //                        firstParam = false;

                        //                    }

                        //                }

                        //                // ноль конца строки (для прежнего способа)
                        //                responseWriter.Write(0);

                        //            }

                        //        }
                        //        catch (SqlException sqlException)
                        //        {
                        //            //сообщаю подписчикам тельнета
                        //            FireEvent
                        //            (
                        //                BuroStoreServiceEventArgs.EventType.Error,
                        //                sqlException.Message + "\r\nStackTrace:\r\n" + sqlException.StackTrace
                        //            );

                        //            responseWriter.Write
                        //            (
                        //                string.Format
                        //                (
                        //                    "<err><cod>{0}</cod><msg>{1}</msg></err>",
                        //                    //TODO:как-то добыть сюда правильный код
                        //                    //для пользовательских ошибок,
                        //                    //который Толик возвращает в @ret
                        //                    sqlException.Number,
                        //                    sqlException.Message
                        //                )

                        //            );

                        //        }

                        //        //responseWriter.Flush();

                        //    }
                        //    //byte[] bytes = this._encoding1251.GetBytes(xmlResponse.ToString());

                        //    //после закрытия потока свойство Length будет недоступно
                        //    long bytesWritten = memStream.Length;

                        //    byte[] bytes = memStream.GetBuffer();

                        //    FireEvent
                        //    (
                        //        BuroStoreServiceEventArgs.EventType.Info,
                        //        _encoding1251.GetString(bytes)
                        //    );
                            
                        //    networkStream.Write(bytes, 0, (int)bytesWritten);

                        //}

                        //}
                        //else
                        //{
                        //    switch (clientCommand.Name)
                        //    {
                        //                                                                    #region Get barcode info
                        //    //ответ на запрос информации о штрихкоде
                        //    case "Get barcode info":

                        //        using (SqlConnection cnn = new SqlConnection(ConnectionString))
                        //        {
                        //            cnn.Open();

                        //            SqlCommand cmd = cnn.CreateCommand();

                        //            cmd.CommandType = System.Data.CommandType.Text;
                        //            cmd.CommandText = string.Format("SELECT P.Name FROM Products P INNER JOIN Barcode B ON P.CounterID = B.CounterID WHERE B.Code = '{0}'", ClientCommand.ParameterStrings(rawCommandString)[0].ToString());

                        //            string result = cmd.ExecuteScalar() as string;

                        //            cnn.Close();

                        //            byte[] bytes = this._encoding.GetBytes(result);

                        //            networkStream.Write(bytes, 0, bytes.Length);

                        //        }

                        //        break;
                        //    #endregion
                        //        //#region Determine the need for updates
                        //        //                            //ответ на запрос обновления базы штрихкодов и прошивки
                        //        //                            case "Determine the need for updates":
                        //        //                                //writer.Write("<Firmware><ResourceId>8</ResourceId><BlocksNumber>1</BlocksNumber></Firmware>");
                        //        //                                //writer.Write("<DataBase><ResourceId>9</ResourceId><BlocksNumber>9</BlocksNumber></DataBase>");
                        //        //                                break;
                        //        //#endregion
                        //        //#region Get resource info
                        //        //                            case "Get resource info":
                        //        //                                string resource_length;
                        //        //                                if(this.Resources.ContainsKey(ClientCommand.ParameterValues(rawCommandString)[0]))
                        //        //                                {
                        //        //                                    resource_length = this.Resources[ClientCommand.ParameterValues(rawCommandString)[0]].Length.ToString();

                        //        //                                }
                        //        //                                else
                        //        //                                {
                        //        //                                    resource_length = "0";

                        //        //                                }

                        //        //                                byte[] byts = this._encoding.GetBytes(resource_length);

                        //        //                                d.Write(byts, 0, byts.Length);

                        //        //                                // ноль конца строки
                        //        //                                d.WriteByte(0);

                        //        //                                break;
                        //        //#endregion
                        //        //#region Get resource
                        //        //                            case "Get resource":
                        //        //                                {
                        //        //                                    string resourceUri = ClientCommand.ParameterValues(rawCommandString)[0];
                        //        //                                    int blockStart = int.Parse(ClientCommand.ParameterValues(rawCommandString)[1]);
                        //        //                                    int blockLength = int.Parse(ClientCommand.ParameterValues(rawCommandString)[2]);

                        //        //                                    d.Write(this.Resources[resourceUri], blockStart, blockLength);

                        //        //                                }

                        //        //                                break;
                        //        //#endregion

                        //                                                                                                                                                                                                                                                            #region Make database snapshot
                        //    case "Make database snapshot":
                        //        {
                        //            //string resourceUri = ClientCommand.ParameterValues(rawCommandString)[0];
                        //            //int blockStart = int.Parse(ClientCommand.ParameterValues(rawCommandString)[1]);
                        //            //int blockLength = int.Parse(ClientCommand.ParameterValues(rawCommandString)[2]);

                        //            int db_key;
                        //            int descr_key;

                        //            //TODO: на время отладки
                        //            #region

                        //            //HACK: весь этот блок
                        //            using (SqlConnection cnn = new SqlConnection(ConnectionString))
                        //            {
                        //                cnn.Open();

                        //                SqlCommand cmd = cnn.CreateCommand();
                        //                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        //                cmd.CommandText = "dbo.[GoodsForOfflineScanning]";
                        //                SqlDataReader reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);

                        //                Table_ArtikulInfo t = new Table_ArtikulInfo();

                        //                int n_Barcode = reader.GetOrdinal("barcode");
                        //                int n_ProductId = reader.GetOrdinal("artikul");
                        //                int n_Name = reader.GetOrdinal("description");

                        //                while (reader.Read())
                        //                {
                        //                    t.AddRow(reader[n_Barcode] as string, reader[n_ProductId] as string, reader[n_Name] as string);

                        //                }

                        //                cnn.Close();

                        //                db_key = this._resourceCache.AddResource(t.GetBytes());
                        //                descr_key = this._resourceCache.AddResource(t.GetBytes_Descriptions());

                        //                int breakpoint_002 = 0;

                        //            }

                        //            #endregion

                        //            string xml_response
                        //                = string.Format
                        //                    (
                        //                        "<databaseFileKey>{0}</databaseFileKey><databaseFileSize>{1}</databaseFileSize><databaseFileChecksum>{5}</databaseFileChecksum><descriptionFileKey>{2}</descriptionFileKey><descriptionFileSize>{3}</descriptionFileSize><descriptionFileChecksum>{6}</descriptionFileChecksum><currentTime>{4}</currentTime>",
                        //                        db_key,
                        //                        this._resourceCache.GetResource(db_key).Length,
                        //                        descr_key,
                        //                        this._resourceCache.GetResource(descr_key).Length,
                        //                        DateTime.Now.ToString(),
                        //                        this._resourceCache.GetResourceChecksum(db_key),
                        //                        this._resourceCache.GetResourceChecksum(descr_key)
                        //                    );

                        //            byte[] bytes = this._encoding.GetBytes(xml_response);

                        //            FireEvent
                        //            (
                        //                BuroStoreServiceEventArgs.EventType.Info,
                        //                string.Format
                        //                (
                        //                    "\r\n>>ответ на [{0}] {1}>> {2}",
                        //                    requestNumber,
                        //                    DateTime.Now.ToString(),
                        //                    xml_response
                        //                )
                        //            );

                        //            networkStream.Write(bytes, 0, bytes.Length);

                        //            // ноль конца строки
                        //            networkStream.WriteByte(0);

                        //        }

                        //        break;
                        //    #endregion
                        //                                            #region Get resource by id
                        //    case "Get resource by id":
                        //        {
                        //            int resourceId = int.Parse(ClientCommand.ParameterStrings(rawCommandString)[0]);
                        //            int blockStart = int.Parse(ClientCommand.ParameterStrings(rawCommandString)[1]);
                        //            int blockLength = int.Parse(ClientCommand.ParameterStrings(rawCommandString)[2]);

                        //            networkStream.Write(this._resourceCache.GetResource(resourceId), blockStart, blockLength);

                        //        }

                        //        break;

                        //    #endregion
                        //                        #region Команда неизвестна сервису
                        //    default:
                        //        FireEvent(BuroStoreServiceEventArgs.EventType.Warning, "Команда не распознана");

                        //        break;
                        //    #endregion

                        //    }

                        //}
#endregion
                    //}
                    #region Может пригодится когда-нибудь
                    //catch (SqlException sqlException)
                    //{
                    //    //сообщаю подписчикам тельнета
                    //    FireEvent
                    //    (
                    //        BuroStoreServiceEventArgs.EventType.Error,
                    //        sqlException.Message + "\r\nStackTrace:\r\n" + sqlException.StackTrace
                    //    );

                    //    //FireEvent
                    //    //(
                    //    //    BuroStoreServiceEventArgs.EventType.Info,
                    //    //    string.Format
                    //    //    (
                    //    //        "<err><cod>{0}</cod><msg>{1}</msg></err>",
                    //    //    //TODO:как-то добыть сюда правильный код, который Толик возвращает в @ret
                    //    //        sqlException.Number,
                    //    //        "**" + sqlException.Message
                    //    //    )
                    //    //);

                    //    //формирую ответ терминалу
                    //    byte[] bytes
                    //        = this._encoding1251.GetBytes
                    //        (
                    //            string.Format
                    //            (
                    //                "<err><cod>{0}</cod><msg>{1}</msg></err>",
                    //                //TODO:как-то добыть сюда правильный код, который Толик возвращает в @ret
                    //                sqlException.Number,
                    //                sqlException.Message
                    //            )
                            
                    //        );

                    //    networkStream.Write(bytes, 0, bytes.Length);

                    //    // ноль конца строки
                    //    networkStream.WriteByte(0);

                    //}
#endregion
                }
                #region Может пригодится когда-нибудь
                //shutdown перед закрытием гарантирует отправку всех данных
                //local.Shutdown(SocketShutdown.Both);
                //local.Close();
                #endregion

                //другой вариант
                local.Close(240);

            }
            catch (Exception e)
            {
                this.FireEvent
                (
                    BuroStoreService.BuroStoreServiceEventArgs.EventType.Error,
                    string.Format
                    (
                        "{0}\r\n---------\r\n{1}\r\n{2}",
                        rawCommandString,
                        e.Message,
                        e.StackTrace
                    )
                );


            }

        }

		/// <summary>
		/// обработчик входящих запросов telnet. Просто подписывает сессии telnet на логи
		/// </summary>
		/// <param name="ar"></param>
		void TcpServer_TelnetRequestServer(IAsyncResult ar)
		{
			try
			{
				Socket listen_socket = (Socket)ar.AsyncState;

				Socket s = listen_socket.EndAccept(ar);

				this._readyToAcceptTelnet.Set();

                //TODO: 5000 магическое число
                if (this._readyToUseTelnetClientsList.WaitOne(5000))
                {
                    this._telnetClients.Add(s);

                    this._readyToUseTelnetClientsList.Set();

                    byte[] welcomeMessage
                        = this._encoding866.GetBytes
                        (
                            string.Format
                            (
                                "Herzlich willkommen :)\r\n\r\nстрока соединения:\t{3}\r\n\r\nоткрыто тельнетов:\t{0}\r\nзакешировано ресурсов:\t{1}\r\n-из них именных:\t{2}\r\n",
                                this._telnetClients.Count,
                                this._resourceCache.Count,
                                this._resourceCache.NamedResourceCount,
                                this._connectionString
                            )
                        );

                    s.Send(welcomeMessage);

                }

			}
			catch (Exception e)
			{
                //TODO: удалить клиента из списка?

                FireEvent
				(
					BuroStoreServiceEventArgs.EventType.Error,
					e.Message + " StackTrace: " + e.StackTrace
				);

			}

		}

		protected override void Dispose(bool disposing)
		{
			_tcpServer.CancelAsync();

			base.Dispose(disposing);

        }

    }

	/// <summary>
	/// вспомогательный класс, преобразующий наборы данных полученные от SQL сервера
	/// в вид пригодный для использования на ТСД
	/// </summary>
    class Table_ArtikulInfo
    {
        Encoding _encoding = Encoding.GetEncoding(1251);
        MemoryStream m = new MemoryStream();
        MemoryStream descriptions = new MemoryStream();

        //TODO: магические числа
        byte[] _b_barcode = new byte[14 + 1];
        byte[] _b_i = new byte[20 + 1];
        byte[] _b_offset = new byte[4];
        byte[] _b_name = new byte[150 + 1];

        public void AddRow(string barcode, string i, string name)
        {
            this._encoding.GetBytes(barcode, 0, barcode.Length, _b_barcode, 0);
            this._encoding.GetBytes(i, 0, i.Length, _b_i, 0);
            this._encoding.GetBytes(name, 0, name.Length, _b_name, 0);

            //TODO: расставить концы строки
            _b_barcode[barcode.Length] = 0;
            _b_i[i.Length] = 0;
            _b_name[name.Length] = 0;

			byte[] description_offset = BitConverter.GetBytes((int)descriptions.Position);
			descriptions.Write(_b_name, 0, name.Length + 1);

            m.Write(_b_barcode, 0, _b_barcode.Length);
            m.Write(_b_i, 0, _b_i.Length);
            m.Write(description_offset, 0, description_offset.Length);

        }

        public byte[] GetBytes()
        {
            return this.m.ToArray();

        }

        public byte[] GetBytes_Descriptions()
        {
            return this.descriptions.ToArray();

        }

    }

    class LogMessage
    {
        //TODO: максимальная длина чего?
        int _maxLength = 1000;
        StringBuilder _stringBuilder = new StringBuilder();

        public LogMessage()
        {
            this._stringBuilder.AppendLine("-------------------------------------------");

        }

        public void AppendLine(string message)
        {
            if (message.Length > this._maxLength)
            {
                this._stringBuilder.AppendLine(message.Substring(0, this._maxLength));

                this._stringBuilder.AppendLine("<..>");

            }
            else
            {
                this._stringBuilder.AppendLine(message);

            }

            this._stringBuilder.AppendLine("");

        }

        public override string ToString()
        {
            return this._stringBuilder.ToString();

        }

    }

}
