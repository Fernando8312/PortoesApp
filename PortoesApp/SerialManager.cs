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
        }

        // Abre a conexão com o Arduino usando os parâmetros da interface
        public void Connect(string portName, int baudRate)
        {
            // Fecha caso já exista uma conexão antes de iniciar a nova
            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Open();
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
                _serialPort.Write(command);
            }
            else
            {
                throw new InvalidOperationException("Falha na execução: A porta serial não está conectada ou não está acessível.");
            }
        }
    }
}
