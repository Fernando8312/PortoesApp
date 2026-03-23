using System;
using System.IO.Ports;

namespace PortoesApp
{
    public class SerialManager
    {
        private SerialPort _serialPort;

        // Verifica se a porta está devidamente aberta para comunicação
        public bool IsOpen { get { return _serialPort != null && _serialPort.IsOpen; } }

        public SerialManager()
        {
            _serialPort = new SerialPort();
            
            // Configurações padrão conforme seus requisitos (8, N, 1)
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            
            // Timeouts para evitar que o aplicativo trave caso haja problemas de comunicação
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
        }

        // Abre a conexão com o Arduino usando os parâmetros da interface
        public void Connect(string portName, int baudRate)
        {
            // Fecha caso já exista uma conexão antes de iniciar a nova
            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            
            // Garante que o Arduino será resetado ao conectar
            _serialPort.DtrEnable = true;
            
            _serialPort.Open();
            
            // Aguarda 2 segundos para o bootloader do Arduino finalizar
            // Isso previne que os primeiros comandos sejam perdidos
            System.Threading.Thread.Sleep(2000);
        }

        // Desconecta a comunicação de forma segura
        public void Disconnect()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        // Envia o comando configurado ao Arduino para o pulso de acionamento do relé
        public void SendCommand(string command)
        {
            if (IsOpen)
            {
                _serialPort.WriteLine(command);
            }
            else
            {
                throw new InvalidOperationException("Falha na execução: A porta serial não está conectada ou não está acessível.");
            }
        }
    }
}
